using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data_ArtifactSpawnPoint", menuName = "Scriptable Objects/Data_ArtifactSpawnPoint")]
public class Data_ArtifactSpawnPoint : ScriptableObject
{
    public List<Transform> spawnPoints = new List<Transform>();

    private List<int> previousSpawnIndices = new List<int>();

    public void Reset()
    {
        previousSpawnIndices.Clear();
    }

    public int SelectSpawnIndex()
    {
        // Use weighted selection to avoid repetition
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!previousSpawnIndices.Contains(i))
                availableIndices.Add(i);
        }

        int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];

        return randomIndex;
    }

    public Transform SelectSpawnPoint()
    {
        // Use weighted selection to avoid repetition
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!previousSpawnIndices.Contains(i))
                availableIndices.Add(i);
        }

        int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];

        return spawnPoints[randomIndex];
    }

    public void AddPreviousSpawnIndex(int index)
    {
        previousSpawnIndices.Add(index);
    }
}
