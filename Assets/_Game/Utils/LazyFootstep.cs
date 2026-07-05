using UnityEngine;

public class LazyFootstep : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f;
    public float volume = 0.7f;

    private PlayerController controller;
    private float stepTimer;
    private Vector3 lastPosition;

    void Start()
    {
        controller = GetComponent<PlayerController>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        // Check if character is moving and grounded
        bool isMoving = controller.IsMoving;
        bool isGrounded = controller.IsGrounded;

        if (isMoving && isGrounded)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayRandomFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = stepInterval; // Reset to play immediately when moving starts
        }

        lastPosition = transform.position;
    }

    void PlayRandomFootstep()
    {
        if (AudioManager.Instance == null)
            return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        float pitch = Random.Range(0.9f, 1.1f);

        AudioManager.Instance.PlaySFX(clip, 0.2f, pitch);
        //AudioManager.Instance.PlaySFXAtPosition(clip, transform.position, volume, pitch);
    }
}
