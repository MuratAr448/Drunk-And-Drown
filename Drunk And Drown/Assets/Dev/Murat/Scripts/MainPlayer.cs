using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayer : MonoBehaviour, IDamageable
{
    public GameObject Weapon;
    public bool pause = false;
    public GameObject PauseScreen;
    private Movement movement;
    public GameObject rayOrigin;
    [SerializeField] private UIWeapons weapon;
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
        SwitchWeapon();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
            
    }
    private void SwitchWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            weapon.SwitchWeaponUI(0);
        }else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            weapon.SwitchWeaponUI(1);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            weapon.SwitchWeaponUI(2);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            weapon.SwitchWeaponUI(3);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            weapon.SwitchWeaponUI(4);
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
            if (Weapon.GetComponent<Gun>() != null)
            {
                Gun Gun = Weapon.GetComponent<Gun>();
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Gun.Schoot();
                }
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    Gun.SecondDairy();
                }
            }
            else
            {
                Melee Melee = Weapon.GetComponent<Melee>();
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Melee.Swing();
                }
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    Melee.SecondDairy();
                }
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
