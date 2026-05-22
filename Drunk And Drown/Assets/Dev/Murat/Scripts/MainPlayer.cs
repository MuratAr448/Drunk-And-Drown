using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayer : MonoBehaviour, IDamageable
{
    public GameObject Weapon;
    public List<GameObject> Weapons;
    public bool pause = false;
    public GameObject PauseScreen;
    private Movement movement;
    public GameObject rayOrigin;
    [SerializeField] private UIWeapons weaponUI;
    [SerializeField] private int weaponCurrentSwitch = 0;
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
        int current = weaponCurrentSwitch;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            weaponCurrentSwitch = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            weaponCurrentSwitch = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            weaponCurrentSwitch = 2;
            weaponUI.SwitchWeaponUI(2);
            //Weapon = Weapons[2];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {   
            //weaponCurrentSwitch = 3;
            weaponUI.SwitchWeaponUI(3);
            //Weapon = Weapons[3];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            //weaponCurrentSwitch = 4;
            weaponUI.SwitchWeaponUI(4);
            //Weapon = Weapons[4];
        }
        if (current!=weaponCurrentSwitch)
        {
            weaponUI.SwitchWeaponUI(weaponCurrentSwitch);
            for (int i = 0; i < Weapons.Count; i++)
            {
                Weapons[i].SetActive(false);
            }
            Weapons[weaponCurrentSwitch].SetActive(true);
            Weapon = Weapons[weaponCurrentSwitch];
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
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    Gun.Shoot();
                }
                if (Input.GetKey(KeyCode.Mouse1))
                {
                    Gun.Secondary();
                }
            }
            else
            {
                Melee Melee = Weapon.GetComponent<Melee>();
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    Melee.Swing();
                }
                if (Input.GetKey(KeyCode.Mouse1))
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
