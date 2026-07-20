using UnityEngine;

public class ShadowTarget : MonoBehaviour
{
    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player!= null)
        {
            transform.position = player.transform.position + Vector3.up;
        }
    }
}
