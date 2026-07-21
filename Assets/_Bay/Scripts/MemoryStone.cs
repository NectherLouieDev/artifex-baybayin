// MemoryStone.cs
using System.Collections.Generic;
using UnityEngine;

public class MemoryStone : MonoBehaviour
{
    [SerializeField] private EchoComponent echoComponent;
    [SerializeField] private SafeAreaTrigger _safeAreaTrigger;
    [SerializeField] private SphereCollider _sphereCollider;
    [SerializeField] private float _safeZoneRadius = 3.5f;
    [SerializeField] private float activationCost = 5f;
    [SerializeField] private Transform _respawnTranfsorm;
    [SerializeField] private List<GameObject> lightGlows = new List<GameObject>();
    
    private bool isActivated = false;
    public bool IsActivated => isActivated;

    public float SafeZoneRadius => _safeZoneRadius;

    private void Start()
    {
        _sphereCollider.radius = _safeZoneRadius;

        foreach (GameObject lightGlow in lightGlows)
        {
            lightGlow.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out PlayerIdentity identity))
        {
            if (isActivated)
                return;

            if (LanternManager.Instance.TrySpendFuel(activationCost))
            {
                ActivateStone();
            }
            else
            {
                UIManager.Instance.ShowMessage("Need more Emberlight!");
            }
        }
    }

    void ActivateStone()
    {
        isActivated = true;

        foreach (GameObject lightGlow in lightGlows)
        {
            lightGlow.SetActive(true);
        }

        _safeAreaTrigger.Activate();

        echoComponent.SpawnEcho();

        GameManager.Instance.ActivateMemoryStone(_respawnTranfsorm.position, gameObject.name);
        //FogManager.Instance.PushBackFog(transform.position, 20f);
        UIManager.Instance.ShowMessage("Memory Stone Activated!");
    }
}