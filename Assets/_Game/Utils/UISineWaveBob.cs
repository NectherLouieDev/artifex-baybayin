using UnityEngine;

public class UISineWaveBob : MonoBehaviour
{
    [Header("Scale Bob Settings")]
    public float scaleAmplitude = 0.2f;
    public float scaleFrequency = 1f;
    public bool uniformScale = true;

    [Header("Position Bob Settings")]
    public bool enablePositionBob = false;
    public Vector2 positionAmplitude = new Vector2(10f, 10f);
    public float positionFrequency = 1f;

    [Header("RectTransform Reference")]
    public RectTransform targetRectTransform;

    private Vector3 startScale;
    private Vector2 startAnchoredPosition;
    private float randomOffset;

    void Start()
    {
        if (targetRectTransform == null)
        {
            targetRectTransform = GetComponent<RectTransform>();
        }

        if (targetRectTransform == null)
        {
            Debug.LogError("UISineWaveBob: No RectTransform found!");
            enabled = false;
            return;
        }

        // Store initial values
        startScale = targetRectTransform.localScale;
        startAnchoredPosition = targetRectTransform.anchoredPosition;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        BobScale();

        if (enablePositionBob)
        {
            BobPosition();
        }
    }

    void BobScale()
    {
        float scaleFactor = 1f + Mathf.Sin((Time.time * scaleFrequency + randomOffset) * 2 * Mathf.PI) * scaleAmplitude;

        Vector3 newScale;
        if (uniformScale)
        {
            newScale = startScale * scaleFactor;
        }
        else
        {
            float xFactor = 1f + Mathf.Sin((Time.time * scaleFrequency + randomOffset) * 2 * Mathf.PI) * scaleAmplitude;
            float yFactor = 1f + Mathf.Sin((Time.time * scaleFrequency + randomOffset + 0.5f) * 2 * Mathf.PI) * scaleAmplitude;
            newScale = new Vector3(startScale.x * xFactor, startScale.y * yFactor, startScale.z);
        }

        targetRectTransform.localScale = newScale;
    }

    void BobPosition()
    {
        float xOffset = Mathf.Sin((Time.time * positionFrequency + randomOffset) * 2 * Mathf.PI) * positionAmplitude.x;
        float yOffset = Mathf.Cos((Time.time * positionFrequency + randomOffset * 1.3f) * 2 * Mathf.PI) * positionAmplitude.y;

        Vector2 newPosition = startAnchoredPosition + new Vector2(xOffset, yOffset);
        targetRectTransform.anchoredPosition = newPosition;
    }

    public void SetScaleBob(float amplitude, float frequency)
    {
        scaleAmplitude = amplitude;
        scaleFrequency = frequency;
    }

    public void SetPositionBob(bool enabled, Vector2 amplitude, float frequency)
    {
        enablePositionBob = enabled;
        positionAmplitude = amplitude;
        positionFrequency = frequency;
    }

    public void ResetToOriginal()
    {
        targetRectTransform.localScale = startScale;
        targetRectTransform.anchoredPosition = startAnchoredPosition;
    }

    void OnDisable()
    {
        ResetToOriginal();
    }

    void OnEnable()
    {
        if (targetRectTransform != null && startScale == Vector3.zero)
        {
            startScale = targetRectTransform.localScale;
            startAnchoredPosition = targetRectTransform.anchoredPosition;
        }
    }
}