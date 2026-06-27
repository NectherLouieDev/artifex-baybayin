using UnityEngine;

public class Splat : MonoBehaviour
{
    [SerializeField] private float _lifetime = 2.0f;

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }
}
