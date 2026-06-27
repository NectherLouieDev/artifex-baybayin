using UnityEngine;

public enum EPlayerTeam
{
    Red, Green, Blue
}


public class PlayerIdentity : MonoBehaviour
{
    [SerializeField] private EPlayerTeam _team;
}
