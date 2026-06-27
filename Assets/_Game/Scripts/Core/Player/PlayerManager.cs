using Unity.Cinemachine;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private CinemachineConfiner3D _confinerComponent;

    public PlayerIdentity Identity;

    public void SetConfinerVolume(Collider boundingVolume)
    {
        _confinerComponent.BoundingVolume = boundingVolume;
    }
}
