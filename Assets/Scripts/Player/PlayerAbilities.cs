using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject firePoint;
    [SerializeField] private List<BulletPattern> spells;

    void Start()
    {
        foreach (BulletPattern pattern in spells)
        {
            pattern.Start();
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        for (int i = 0; i < spells.Count; i++)
        {
            BulletPattern pattern = spells[i];
            if (
                Input.GetKeyDown(pattern.triggerButton) &&
                Time.time > pattern.canShootAfter
            )
            {
                testServerRpc(i);
            }
        }
    }

    [ServerRpc]
    void testServerRpc(int patternIdx)
    {
        BulletPattern pattern = spells[patternIdx];
        pattern.onShoot(firePoint, transform);
    }
}
