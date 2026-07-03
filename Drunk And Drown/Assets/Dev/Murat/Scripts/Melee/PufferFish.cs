using UnityEngine;

public class PufferFish : MonoBehaviour
{
    [SerializeField] private MorningStar morningStar;
    private void OnTriggerEnter(Collider other)
    {
        morningStar.HitEnemy(other);
    }
}
