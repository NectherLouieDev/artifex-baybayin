using UnityEngine;

public class EchoTarget : MonoBehaviour
{
    private void Update()
    {
        BaybayinStone baybayinStone = FindFirstObjectByType<BaybayinStone>();

        if (baybayinStone != null)
        {
            transform.position = baybayinStone.transform.position + Vector3.up;
        }
    }
}
