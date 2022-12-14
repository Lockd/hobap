using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private Rigidbody2D rb;
    // [SerializeField] private Animator anim;
    bool moving;
    bool canMove = true;

    public static Vector3 mousePosition;
    public Vector3 objectPosition;
    float rotationAngle;

    void Update()
    {
        if (!IsOwner) return;

        if (canMove)
        {
            float horizontalDirection = Input.GetAxisRaw("Horizontal");
            float verticalDirection = Input.GetAxisRaw("Vertical");

            if (new Vector2(horizontalDirection, verticalDirection).Equals(new Vector2(0, 0))) moving = false;
            else moving = true;

            // if (anim != null) anim.SetBool("moving", moving);
            // if (horizontalDirection != 0)
            //     transform.localScale = new Vector3(horizontalDirection / Mathf.Abs(horizontalDirection), transform.localScale.y, transform.localScale.z);

            onMoveServerRpc(horizontalDirection, verticalDirection);
        }

        mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.z;
        objectPosition = Camera.main.WorldToScreenPoint(transform.position);
        mousePosition.x = mousePosition.x - objectPosition.x;
        mousePosition.y = mousePosition.y - objectPosition.y;
        rotationAngle = Mathf.Atan2(mousePosition.y, mousePosition.x) * Mathf.Rad2Deg;
        // TODO if transform is different
        onRotateServerRpc(rotationAngle);
    }

    public bool CanMove
    {
        set
        {
            canMove = value;
        }
    }

    [ServerRpc]
    void onMoveServerRpc(float horizontalDirection, float verticalDirection)
    {
        // There's a delay in this thing?
        rb.velocity = new Vector2(horizontalDirection, verticalDirection).normalized * speed;
    }

    [ServerRpc]
    void onRotateServerRpc(float rotationAngle)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, rotationAngle));
    }
}