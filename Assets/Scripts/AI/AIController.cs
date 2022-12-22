using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AIController : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] float selectNewTargetCooldown = 5f;
    [SerializeField] float distanceToStopMovingForward = 8f;
    Rigidbody2D rb;
    float checkForTargetAfter = 0f;
    Transform target = null;
    bool isAbleToRotate = true;
    bool isFacingRight = true;
    Vector3 moveVector;
    AIAbilities aiAbilities;

    void Start()
    {
        onSelectTarget();
        checkForTargetAfter = 0f;
        rb = GetComponent<Rigidbody2D>();
        aiAbilities = GetComponent<AIAbilities>();
    }

    public void onChangeRotationAbility(bool shouldBeAbleToRotate)
    {
        isAbleToRotate = shouldBeAbleToRotate;
    }

    private void onSelectTarget()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        float maxDistance = 0f;
        GameObject newTarget = null;
        foreach (GameObject player in allPlayers)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer > maxDistance)
            {
                maxDistance = distanceToPlayer;
                newTarget = player;
            }
        }

        checkForTargetAfter = Time.time + selectNewTargetCooldown;

        if (newTarget != null) target = newTarget.transform;
    }

    void FixedUpdate()
    {
        if (Time.time > checkForTargetAfter || target == null) onSelectTarget();

        if (target == null) return;

        moveVector = (target.position - transform.position).normalized;

        float horizontalDirection = moveVector.x;
        float verticalDirection = moveVector.y;

        bool shouldRotate = false;
        if (
            (horizontalDirection > 0 && !isFacingRight || horizontalDirection < 0 && isFacingRight) &&
            isAbleToRotate
        )
        {
            shouldRotate = true;
            isFacingRight = horizontalDirection > 0;
        }

        onMoveServerRpc(horizontalDirection, verticalDirection, shouldRotate);
    }

    [ServerRpc]
    void onMoveServerRpc(float horizontalDirection, float verticalDirection, bool shouldRotate)
    {
        if (isAbleToRotate)
        {
            float rotationAngle = Mathf.Atan2(verticalDirection, horizontalDirection) * Mathf.Rad2Deg;
            if (!isFacingRight) rotationAngle -= 180;

            Quaternion rotationQuaternion = Quaternion.AngleAxis(rotationAngle, Vector3.forward);

            if (horizontalDirection != 0 || verticalDirection != 0)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rotationQuaternion, Time.fixedDeltaTime * rotationSpeed);
            }

            if (shouldRotate)
            {
                transform.localScale = new Vector3(
                    transform.localScale.x * -1,
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }

        if (Vector3.Distance(transform.position, target.position) > distanceToStopMovingForward)
        {
            Vector2 direction = new Vector2(horizontalDirection, verticalDirection).normalized;
            rb.velocity = direction * speed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
}