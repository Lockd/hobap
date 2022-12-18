using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    // it's displayed in unity for debugging purposes
    [SerializeField] private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void onChangeCurrentHealth(int amount, bool isHealing = false)
    {
        if (isHealing)
        {
            currentHealth += amount;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
        if (!isHealing)
        {
            currentHealth -= amount;
        }

        if (currentHealth <= 0) onDeathServerRpc();
    }

    [ServerRpc]
    void onDeathServerRpc()
    {
        transform.parent.GetComponent<NetworkObject>()?.Despawn(true);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Projectile")
        {
            Debug.Log("Should destroy bullet on collision with Player");
            BulletBehaviour bulletBehaviour = collider.GetComponent<BulletBehaviour>();
            if (bulletBehaviour != null)
            {
                onChangeCurrentHealth(bulletBehaviour.damage);
                bulletBehaviour.destroyBulletServerRpc();
            }
        }
    }
}
