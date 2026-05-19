using UnityEngine;

public class Looking : MonoBehaviour
{
    [SerializeField] private GameObject middleScreen;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.LookAt(middleScreen.transform.position);
    }

}
