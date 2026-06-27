using UnityEngine;

public class SpawnObjectStep : SequenceStep
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _spawnPoint;

    public override void Execute()
    {
        GameObject g = Instantiate(_prefab, Vector3.zero, Quaternion.identity);
        g.transform.SetParent(_spawnPoint);
        g.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        Complete();
    }
}
