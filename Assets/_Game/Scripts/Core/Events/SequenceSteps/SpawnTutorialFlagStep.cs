using UnityEngine;

public class SpawnTutorialFlagStep : SequenceStep
{
    [SerializeField] private GameObject _greenFlagPrefab;
    [SerializeField] private GameObject _redFlagPrefab;
    [SerializeField] private Transform _spawnPoint;

    private bool _success = false;

    public void SetSuccess(bool success)
    {
        _success = success;
    }

    public override void Execute()
    {
        GameObject g = Instantiate(
            _success ? _greenFlagPrefab : _redFlagPrefab, 
            Vector3.zero, Quaternion.identity);
        
        g.transform.SetParent(_spawnPoint);
        g.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        Complete();
    }
}
