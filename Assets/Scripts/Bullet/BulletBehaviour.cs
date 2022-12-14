using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletBehaviour : NetworkBehaviour
{
    [SerializeField] private float maxTravelDistance = 20f;
    Vector3 initialPosition;
    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (Vector3.Distance(initialPosition, transform.position) > maxTravelDistance)
        {
            destroyBulletServerRpc();
        }
    }

    [ServerRpc]
    void destroyBulletServerRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
}
