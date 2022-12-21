using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "characterClass", menuName = "ScriptableObjects/characterClass", order = 1)]
public class characterClassSO : ScriptableObject
{
    public List<BulletPattern> spells;
    public GameObject weapon;
    public GameObject hat;
    public string className;

    // TODO add description and spells
}
