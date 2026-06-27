using UnityEngine;

public class LazyYLerp : MonoBehaviour
{
    [SerializeField] private float speed = 0.25f;
    [SerializeField] private float startY = 0;
    [SerializeField] private float endY = 0;
    [SerializeField] private bool _playOnStart = false;
    
    private bool isLerping = false;
    private float progress = 0f;

    private void Start()
    {
        if (_playOnStart)
        {
            StartLerp();
        }
    }

    public void SetLerpTargets(float start, float end)
    {
        startY = start;
        endY = end;
    }

    void Update()
    {
        if (isLerping)
        {
            progress += Time.deltaTime * speed;
            progress = Mathf.Clamp01(progress);

            float newY = Mathf.Lerp(startY, endY, progress);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);

            if (progress >= 1f)
            {
                isLerping = false;
            }
        }
    }

    public void StartLerp()
    {
        progress = 0f;
        isLerping = true;
    }
}
