using UnityEngine;

public class MainPlayer : MonoBehaviour, IDamageable
{
    public GameObject gun;
    public GameObject melee;
    public bool pause = false;
    public GameObject PauseScreen;
    private Movement movement;

    [SerializeField] private float health;
    private float maxHealth;

    public float CurrentHealth => health;
    public float BaseHealth => maxHealth;
    void Start()
    {
        movement = GetComponent<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        Shoot();
        if (Input.GetKeyDown(KeyCode.Escape))
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
                Gun Gun = gun.GetComponent<Gun>();
                Gun.Schoot();
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Melee Melee = melee.GetComponent<Melee>();
                Melee.Swing();
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    public void ResetHealth()
    {
        health = maxHealth;
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
