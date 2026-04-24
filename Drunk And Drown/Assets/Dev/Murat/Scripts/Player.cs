using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject gun1;
    public GameObject gun2;
    public bool pause = false;
    public GameObject PauseScreen;
    private Movement movement;
    void Start()
    {
        movement = GetComponent<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        Shoot();
        if (Input.GetKeyDown(KeyCode.P))
        {
            Pause();
        }
            
    }
    public void Pause()
    {
        pause = !pause;
        if (pause)
        {
            PauseScreen.SetActive(true);
            movement.canMove = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
            
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
            PauseScreen.SetActive(false);
            movement.canMove = true;
        }
    }
    private void Shoot()
    {
        if (movement.canMove)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Gun gun = gun1.GetComponent<Gun>();
                gun.Schoot();
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Gun gun = gun2.GetComponent<Gun>();
                gun.Schoot();
            }
        }
    }
}
