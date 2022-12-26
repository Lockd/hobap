using UnityEngine;
using Unity.Netcode;
public class ZoneBehaviour : NetworkBehaviour
{
    [SerializeField] private float startShrinkingAfter = 30f;
    [Range(0f, 1f), SerializeField] private float shrinkingSpeed = 5f;
    bool shouldStopShrinking = false;

    void Start()
    {
        startShrinkingAfter += Time.time;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (transform.localScale.x <= 0.01f) shouldStopShrinking = true;

        if (Time.time > startShrinkingAfter && !shouldStopShrinking && IsOwnedByServer)
        {
            shrinkZoneServerRpc();
        }
    }

    [ServerRpc]
    void shrinkZoneServerRpc()
    {
        float shrinkFor = shrinkingSpeed * Time.fixedDeltaTime;
        float newScale = transform.localScale.x - shrinkFor;

        transform.localScale = new Vector3(
            newScale,
            newScale,
            1f
        );
    }
}
