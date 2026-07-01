using UnityEngine;

public class RotateFish : MonoBehaviour
{
    private void Update()
    {
        float speed = 5f;
        transform.Rotate(Random.value * speed, Random.value * speed, Random.value * speed);
    }
}
