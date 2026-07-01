using UnityEngine;

public class RotationRandomizer : MonoBehaviour
{
    void Start()
    {
        // Applies a completely random 3D rotation
        transform.rotation = Random.rotation;
    }
}