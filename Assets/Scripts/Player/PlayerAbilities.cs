using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject firePoint;
    [SerializeField] private List<BulletPattern> spells = new List<BulletPattern>();
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private float reflectionDuration = 1.2f;
    [SerializeField] private float reflectionCooldown = 5f;
    public bool isReflecting = false;
    float shouldStopReflectingAfter = 0f;
    float canReflectAfter = 0f;
    PlayerController playerController;
    LineRenderer laserLine;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        laserLine = GetComponent<LineRenderer>();
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
            Input.GetKeyDown(KeyCode.Q) &&
            Time.time > canReflectAfter &&
            !isReflecting
        )
        {
            canReflectAfter = Time.time + reflectionCooldown;
            shouldStopReflectingAfter = Time.time + reflectionDuration;
            isReflecting = true;
            playerController.onChangeRotationAbility(false);

            changeShieldActivityServerRpc(true);
        }

        if (
            Time.time > shouldStopReflectingAfter &&
            isReflecting
        )
        {
            isReflecting = false;
            playerController.onChangeRotationAbility(true);
            changeShieldActivityServerRpc(false);
        }
    }

    public void onAddSpells(List<BulletPattern> spellsToAdd)
    {
        List<BulletPattern> InstantiatedSpells = new List<BulletPattern>();
        spellsToAdd.ForEach((BulletPattern spell) =>
        {
            BulletPattern spellToAdd = Instantiate(spell);
            InstantiatedSpells.Add(spellToAdd);
            spellToAdd.onAddSpell(laserLine);
        });
        spells = InstantiatedSpells;
    }

    [ServerRpc]
    void shotBulletServerRpc(int patternIdx)
    {
        BulletPattern pattern = spells[patternIdx];
        StartCoroutine(
            pattern.onShoot(
                firePoint,
                transform,
                playerController.onChangeRotationAbility
            )
        );
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
