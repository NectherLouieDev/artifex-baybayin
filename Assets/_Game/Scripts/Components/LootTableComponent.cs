using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class LootTableComponent : MonoBehaviour
{
    [SerializeField] private GameData_LootTable _lootTable;

    public void SpawnLoot(ECreepType creepType)
    {
        List<LootData> loots = _lootTable.GetLootDataFromCreepType(creepType);

        int randomIndex = Random.Range(0, loots.Count - 1);

        GameObject g = Instantiate(loots[randomIndex].prefab, transform.position + Vector3.up, Quaternion.identity);
    }
}
