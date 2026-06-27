using UnityEngine;

public class SineWaveBob : MonoBehaviour
{
    [Header("Bob Settings")]
    public float amplitude = 0.5f;
    public float frequency = 1f;
    public bool useWorldSpace = false;

    [Header("Rotation Bob (Optional)")]
    public bool enableRotationBob = false;
    public float rotationAmplitude = 10f;

    private Vector3 startPosition;
    private Vector3 startRotation;
    private float randomOffset;

    void Start()
    {
        if (useWorldSpace)
        {
            startPosition = transform.position;
        }
        else
        {
            startPosition = transform.localPosition;
        }

        startRotation = transform.localEulerAngles;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        BobMovement();

        if (enableRotationBob)
        {
            BobRotation();
        }
    }

    void BobMovement()
    {
        float newY = startPosition.y + Mathf.Sin((Time.time * frequency + randomOffset) * 2 * Mathf.PI) * amplitude;

        Vector3 newPosition = new Vector3(startPosition.x, newY, startPosition.z);

        if (useWorldSpace)
        {
            transform.position = newPosition;
        }
        else
        {
            transform.localPosition = newPosition;
        }
    }

    void BobRotation()
    {
        float newRotationZ = startRotation.z + Mathf.Cos((Time.time * frequency + randomOffset) * 2 * Mathf.PI) * rotationAmplitude;

        Vector3 newRotation = new Vector3(startRotation.x, startRotation.y, newRotationZ);
        transform.localEulerAngles = newRotation;
    }

    // Optional: Reset to original position when disabled
    void OnDisable()
    {
        if (useWorldSpace)
        {
            transform.position = startPosition;
        }
        else
        {
            transform.localPosition = startPosition;
        }

        if (enableRotationBob)
        {
            transform.localEulerAngles = startRotation;
        }
    }
}