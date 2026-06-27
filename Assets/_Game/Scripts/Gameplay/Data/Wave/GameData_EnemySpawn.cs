using UnityEngine;

[CreateAssetMenu(fileName = "GameData_EnemySpawn", menuName = "Scriptable Objects/GameData_EnemySpawn")]
public class GameData_EnemySpawn : ScriptableObject
{
    [Header("Enemy Type")]
    public GameObject _prefab;

    [Header("Spawn Behavior")]
    public int defaultCount = 1;
    public float spawnDelayOffset = 0f;
    public bool spawnAtRandomPathPoint = false;

    [Header("Enemy Properties")]
    public string _enemyName = "Enemy";
    public Sprite _icon;
    [TextArea(2, 3)]
    public string _description;
}
