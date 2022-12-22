using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private Rigidbody2D rb;
    bool isAbleToRotate = true;
    bool isFacingRight = true;

    void Start()
    {
        if (IsOwner)
        {
            CinemachineVirtualCamera vcam = Object.FindObjectOfType<CinemachineVirtualCamera>();
            vcam.Follow = transform;
        }
    }

    public void onChangeRotationAbility(bool shouldBeAbleToRotate)
    {
        isAbleToRotate = shouldBeAbleToRotate;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        float horizontalDirection = Input.GetAxisRaw("Horizontal");
        float verticalDirection = Input.GetAxisRaw("Vertical");

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
        Vector2 direction = new Vector2(horizontalDirection, verticalDirection).normalized;
        rb.velocity = direction * speed;

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
    }
}