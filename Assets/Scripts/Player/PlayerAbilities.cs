using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject firePoint;
    [SerializeField] private List<BulletPattern> spells;
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private float reflectionDuration = 1.2f;
    [SerializeField] private float reflectionCooldown = 5f;
    public bool isReflecting = false;
    float shouldStopReflectingAfter = 0f;
    float canReflectAfter = 0f;
    Collider2D reflectorCollider;
    SpriteRenderer reflectorRenderer;

    void Start()
    {
        reflectorRenderer = shieldPrefab.GetComponent<SpriteRenderer>();
        reflectorCollider = shieldPrefab.GetComponent<Collider2D>();

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
                shotBulletServerRpc(i);
            }
        }

        if (
            Input.GetKeyDown(KeyCode.LeftShift) &&
            Time.time > canReflectAfter &&
            !isReflecting
        )
        {
            canReflectAfter = Time.time + reflectionCooldown;
            shouldStopReflectingAfter = Time.time + reflectionDuration;
            isReflecting = true;

            changeShieldActivityServerRpc(true);
        }

        if (
            Time.time > shouldStopReflectingAfter &&
            isReflecting
        )
        {
            isReflecting = false;
            changeShieldActivityServerRpc(false);
        }
    }

    [ServerRpc]
    void shotBulletServerRpc(int patternIdx)
    {
        BulletPattern pattern = spells[patternIdx];
        pattern.onShoot(firePoint, transform);
    }

    // TODO Do I really need both of em?
    [ServerRpc]
    void changeShieldActivityServerRpc(bool newActivity)
    {
        shieldPrefab.SetActive(newActivity);
        changeShieldActivityClientRpc(newActivity);
    }
    [ClientRpc]
    void changeShieldActivityClientRpc(bool newActivity)
    {
        shieldPrefab.SetActive(newActivity);
    }
}
