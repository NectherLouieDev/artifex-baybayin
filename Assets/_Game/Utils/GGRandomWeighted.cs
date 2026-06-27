using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class GGRandomWeighted : MonoBehaviour
{
    public T GetWeightedRandomNumber<T>(List<T> values, List<int> weights)
    {
        // Check if the numbers and weights arrays have the same length
        if (values.Count != weights.Count || values.Count == 0)
        {
            throw new ArgumentException("Numbers and weights arrays must have the same non-zero length.");
        }

        // Calculate the total weight
        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        // Generate a random number within the total weight
        int randomValue = Random.Range(0, totalWeight);

        // Find the index corresponding to the random value
        int index = -1;
        while (randomValue >= 0)
        {
            index++;
            randomValue -= weights[index];
        }

        // Return the corresponding number from the numbers array
        return values[Math.Min(index, values.Count - 1)];
    }
}
