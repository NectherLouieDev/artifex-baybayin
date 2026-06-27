using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class LootData
{
    public ELootType lootType;
    public int lootValue = 0;
    public GameObject prefab;
}

[Serializable]
public class CreepLootData
{
    public ECreepType creepType;
    public List<LootData> lootTypes = new List<LootData>();
}

[CreateAssetMenu(fileName = "GameData_LootTable", menuName = "Scriptable Objects/GameData_LootTable")]
public class GameData_LootTable : ScriptableObject
{
    public List<CreepLootData> _lootData = new List<CreepLootData>();

    public List<LootData> GetLootDataFromCreepType(ECreepType type)
    {
        List<LootData> output = null;

        foreach (CreepLootData lootData in _lootData)
        {
            if (lootData.creepType == type)
            {
                output = lootData.lootTypes;
                break;
            }
        }

        return output;
    }
}
