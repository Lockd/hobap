using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class characterClassManager : MonoBehaviour
{
    [SerializeField] private List<characterClassSO> classes;
    PlayerAbilities playerAbilities;

    // TODO make it so that class is selected from UI
    // class should be selected in the first few seconds of joining and creating lobby
    // this script is responsible for invoking UI object for class selection

    void Start()
    {
        playerAbilities = GetComponent<PlayerAbilities>();
        onSelectClassServerPrc(0);
    }

    [ServerRpc]
    public void onSelectClassServerPrc(int classIdx)
    {
        characterClassSO selectedClass = classes[classIdx];
        spawnGameObject(selectedClass.hat);
        spawnGameObject(selectedClass.weapon);

        playerAbilities.onAddSpells(selectedClass.spells);
    }

    void spawnGameObject(GameObject prefab)
    {
        GameObject spawnedPrefab = Instantiate(prefab, transform, false);
    }
}
