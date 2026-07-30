using UnityEngine;

public class ArtifactSpawnPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 4f);
    }
}
