using UnityEngine;

public class LazyCogRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.forward;
    public float rotationSpeed = 90f;
    public bool randomizeDirection = false;

    private float actualSpeed;

    void Start()
    {
        actualSpeed = randomizeDirection ?
            rotationSpeed * (Random.value > 0.5f ? 1f : -1f) :
            rotationSpeed;
    }

    void Update()
    {
        transform.Rotate(rotationAxis * actualSpeed * Time.deltaTime);
    }
}