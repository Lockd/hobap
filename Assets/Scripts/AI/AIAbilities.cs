using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AIAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject firePoint;
    [SerializeField] private List<BulletPattern> spells = new List<BulletPattern>();
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private float reflectionDuration = 1.2f;
    [SerializeField] private float reflectionCooldown = 5f;
    public bool isReflecting = false;
    float canReflectAfter = 0f;
    AIController aiController;
    LineRenderer laserLine;

    void Start()
    {
        aiController = GetComponent<AIController>();
        laserLine = GetComponent<LineRenderer>();
        canReflectAfter = 0f;
    }

    public IEnumerator onReflect()
    {
        if (isReflecting || Time.time < canReflectAfter) yield break;

        canReflectAfter = Time.time + reflectionCooldown;
        isReflecting = true;
        aiController.onChangeRotationAbility(false);

        changeShieldActivityServerRpc(true);

        yield return new WaitForSeconds(reflectionDuration);

        isReflecting = false;
        aiController.onChangeRotationAbility(true);
        changeShieldActivityServerRpc(false);
    }

    // TODO add cooldowns
    // TODO rework cooldowns to work from bullet pattern script
    public void usePrimarySpell()
    {
        shotBulletServerRpc(0);
    }

    public void useSecondarySpell()
    {
        shotBulletServerRpc(1);
    }

    [ServerRpc]
    void shotBulletServerRpc(int patternIdx)
    {
        BulletPattern pattern = spells[patternIdx];
        StartCoroutine(pattern.onShoot(firePoint, transform));
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