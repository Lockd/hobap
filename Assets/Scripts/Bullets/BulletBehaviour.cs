using UnityEngine;
using Unity.Netcode;

public class BulletBehaviour : NetworkBehaviour
{
    public GameObject parent;
    public float projectileSpeed;
    public float maxTravelDistance;
    public int damage = 1;
    Vector3 initialPosition;
    Rigidbody2D rb;

    void Start()
    {
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Vector3.Distance(initialPosition, transform.position) > maxTravelDistance && IsOwner)
        {
            destroyBulletServerRpc();
        }
    }

    [ServerRpc]
    public void destroyBulletServerRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Reflector" && collider.transform.parent != null)
        {
            Debug.Log("Bullet should be reflected");
            if (collider.transform.parent.gameObject != parent)
            {
                Vector2 curDir = transform.TransformDirection(Vector2.right);

                Vector2 newDir = Vector2.Reflect(curDir, collider.transform.right);
                rb.velocity = newDir.normalized * projectileSpeed;
                // TODO fix that so that long bullets are rotated properly 
            }
        }
    }
}
