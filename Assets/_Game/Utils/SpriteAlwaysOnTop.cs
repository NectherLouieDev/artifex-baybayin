using UnityEngine;

public class SpriteAlwaysOnTop : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        // Get the Sprite Renderer component
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null && _spriteRenderer.material != null)
        {
            // Set the renderQueue value to 4000 (Overlay)
            Debug.Log("RenderQueue 4000");
            _spriteRenderer.material.renderQueue = 4000;
        }
    }

    private void Update()
    {
        if (_spriteRenderer != null && _spriteRenderer.material != null)
        {
            Debug.Log("r");
            _spriteRenderer.material.renderQueue = 4000;
        }
    }
}
