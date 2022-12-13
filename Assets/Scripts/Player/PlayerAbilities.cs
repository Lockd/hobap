using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
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
        foreach (BulletPattern pattern in spells)
        {
            if (
                Input.GetKeyDown(pattern.triggerButton) &&
                Time.time > pattern.canShootAfter
            )
            {
                pattern.onShoot(firePoint, transform);
            }
        }
    }
}
