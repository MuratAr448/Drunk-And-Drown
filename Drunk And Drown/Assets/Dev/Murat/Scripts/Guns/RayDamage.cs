using UnityEngine;

public class RayDamage : MonoBehaviour
{
    public float Multiplyer = 0;
    public float Iframe = 0;
    public void RayIframe()
    {
        Iframe = 0.2f;
    }
    private void Update()
    {
        if (Iframe>0)
        {
            Iframe -= Time.deltaTime;
        }

    }
}
