using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AIController : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] float selectNewTargetCooldown = 5f;
    [SerializeField] float shootingDistance = 8f;
    [SerializeField] float distanceToMoveBack = 6f;
    [SerializeField] private Rigidbody2D rb;
    float checkForTargetAfter = 0f;
    Transform target = null;
    bool isAbleToRotate = true;
    bool isFacingRight = true;
    Vector3 moveVector;
    AIAbilities aiAbilities;
    public List<Rigidbody2D> rbToAvoid = new List<Rigidbody2D>();

    enum botControllerStates { Chasing, Attacking, Dodging, Avoid };
    [SerializeField] private botControllerStates state = botControllerStates.Chasing;

    void Start()
    {
        onSelectTarget();
        checkForTargetAfter = 0f;
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

    bool checkIfShouldContinueDodging()
    {
        List<Rigidbody2D> filteredRbToAvoid = new List<Rigidbody2D>();
        foreach (Rigidbody2D _rb in rbToAvoid)
        {
            if (_rb == null) continue;

            RaycastHit2D[] hits = Physics2D.RaycastAll(_rb.transform.position, _rb.velocity);
            bool shouldRemoveRb = true;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform == transform) shouldRemoveRb = false;
            }

            if (!shouldRemoveRb) filteredRbToAvoid.Add(_rb);
        }

        rbToAvoid = filteredRbToAvoid;

        return rbToAvoid.Count != 0;
    }

    void checkDistanceToTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget < shootingDistance && distanceToTarget > distanceToMoveBack)
            state = botControllerStates.Attacking;

        if (distanceToTarget > shootingDistance) state = botControllerStates.Chasing;

        if (distanceToTarget <= distanceToMoveBack) state = botControllerStates.Avoid;
    }

    bool checkIfShouldReflect()
    {
        return rbToAvoid.Count >= 2;
    }

    void FixedUpdate()
    {
        if (Time.time > checkForTargetAfter || target == null) onSelectTarget();

        // TODO add logic for AI to walk around? 
        if (target == null) return;

        switch (state)
        {
            case botControllerStates.Avoid:
                moveVector = (transform.position - target.position).normalized;
                checkDistanceToTarget();
                break;
            case botControllerStates.Attacking:
                checkDistanceToTarget();

                for (int i = 0; i < aiAbilities.spells.Count; i++)
                {
                    BulletPattern spell = aiAbilities.spells[i];
                    if (Time.time > spell.canShootAfter)
                    {
                        aiAbilities.shotBulletServerRpc(i);
                        break;
                    }
                }
                moveVector = (target.position - transform.position).normalized;
                break;
            case botControllerStates.Chasing:
                moveVector = (target.position - transform.position).normalized;
                checkDistanceToTarget();
                break;
            case botControllerStates.Dodging:
                bool shouldContinueDodge = checkIfShouldContinueDodging();
                if (!shouldContinueDodge)
                {
                    state = botControllerStates.Chasing;
                    break;
                }

                moveVector = Vector2.Perpendicular(rbToAvoid[0].velocity).normalized;
                break;
            default:
                break;
        }

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

        if (
            state == botControllerStates.Chasing ||
            state == botControllerStates.Dodging ||
            state == botControllerStates.Avoid
        )
        {
            Vector2 direction = new Vector2(horizontalDirection, verticalDirection).normalized;
            rb.velocity = direction * speed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Projectile")
        {
            BulletBehaviour bulletBehaviour = collider.GetComponent<BulletBehaviour>();
            // AI should not react to it's own bullets
            if (gameObject == bulletBehaviour.parent)
                return;

            Rigidbody2D bulletRb = collider.gameObject.GetComponent<Rigidbody2D>();
            Vector2 bulletDirection = bulletRb.velocity;
            RaycastHit2D[] hits = Physics2D.RaycastAll(collider.transform.position, bulletDirection);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform == transform)
                {
                    state = botControllerStates.Dodging;
                    rbToAvoid.Add(bulletRb);
                }
            }
            if (checkIfShouldReflect())
            {
                StartCoroutine(aiAbilities.onReflect());
            }
        }
    }
}