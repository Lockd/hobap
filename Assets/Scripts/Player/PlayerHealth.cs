using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;
    [SerializeField] private float zoneDamageCooldown = 1.5f;
    [SerializeField] private Transform healthUI;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Color32 activeUIColor;
    [SerializeField] private Color32 inactiveUIColor;
    [SerializeField] private NetworkObject networkObject;
    [SerializeField] private bool isInsideAllowedArea = false;
    int lastColoredHealthBarIdx;
    float takeDamageFromZoneAfter = 0f;

    void Start()
    {
        currentHealth = maxHealth;

        // This is here so that it's easier to set up UI from editor
        foreach (Transform child in healthUI)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < maxHealth; i++)
        {
            Instantiate(healthBar, healthUI, false);
        }
        lastColoredHealthBarIdx = maxHealth - 1;
    }

    void Update()
    {
        if (!isInsideAllowedArea && Time.time > takeDamageFromZoneAfter)
        {
            onChangeCurrentHealth(1);
            takeDamageFromZoneAfter = Time.time + zoneDamageCooldown;
        }
    }

    void repaintHealthbars(int amount, bool isHealing)
    {
        for (int i = 0; i < amount; i++)
        {
            int idx = isHealing ? currentHealth - i - 1 : currentHealth + i;
            Color32 colorToPaint = isHealing ? activeUIColor : inactiveUIColor;

            SpriteRenderer healthbarRenderer = healthUI.GetChild(idx).GetComponent<SpriteRenderer>();
            if (healthbarRenderer != null) healthbarRenderer.color = colorToPaint;
        }
    }

    void onChangeCurrentHealth(int amount, bool isHealing = false)
    {
        if (isHealing)
        {
            currentHealth += amount;

            if (currentHealth > maxHealth) currentHealth = maxHealth;
        }
        if (!isHealing)
        {
            currentHealth -= amount;
        }

        if (currentHealth <= 0)
        {
            onDeathServerRpc();
            return;
        }

        repaintHealthbars(amount, isHealing);
    }

    [ServerRpc]
    void onDeathServerRpc()
    {
        networkObject.Despawn(true);
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
        if (collider.tag == "Allowed Area")
        {
            isInsideAllowedArea = true;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag == "Allowed Area")
        {
            isInsideAllowedArea = false;
            takeDamageFromZoneAfter = Time.time + zoneDamageCooldown;
        }
    }
}
