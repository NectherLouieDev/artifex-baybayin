using UnityEngine;

public class ConditionalSpawnObjectStep : SequenceStep
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private int _maxSpawnCount = 1;

    private bool _conditional = false;
    private int _spawnCount = 0;

    public void SetConditional(bool conditional)
    {
        _conditional = conditional;
    }

    public override void Execute()
    {
        if (_conditional)
        {
            if (_spawnCount < _maxSpawnCount)
            {
                GameObject g = Instantiate(_prefab, Vector3.zero, Quaternion.identity);
                g.transform.SetParent(_spawnPoint);
                g.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                _spawnCount++;
            }
        }

        Complete();
    }
}
