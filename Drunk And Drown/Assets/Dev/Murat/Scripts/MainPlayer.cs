using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPlayer : MonoBehaviour, IDamageable
{
    public GameObject Weapon;
    public static List<GameObject> Weapons = new List<GameObject>();
    public bool pause = false;
    public GameObject pauseScreen;
    public List<GameObject> deathScreen;
    public bool dead = false;
    private Movement movement;
    public GameObject rayOrigin;
    private UIWeapons weaponUI;
    [SerializeField] private int weaponCurrentSwitch = 0;
    [SerializeField] private float health;
    private PlayerEffects hitEffect;
    [SerializeField] private Color hitColor;
    private float maxHealth;
    public Action OnHealthChanged { get; set; }
    public float CurrentHealth => health;
    public float BaseHealth => maxHealth;
    [SerializeField] private List<GameObject> BottleFaces;

    private Transform hotbar;
    public static Transform HotbarInstance { get; private set; }

    void Start()
    {
        movement = GetComponent<Movement>();
        hitEffect = GetComponent<PlayerEffects>();
        maxHealth = health;
        weaponUI = FindFirstObjectByType<UIWeapons>();
        hotbar = transform.Find("Hand 1");
        HotbarInstance = hotbar;
    }

    void Update()
    {
        if (movement.canMove)
        {
            Shoot();
            SwitchWeapon();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }
        }
    }

    private void SwitchWeapon()
    {
        if (Weapons == null || Weapons.Count == 0) return;

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
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            weaponUI.SwitchWeaponUI(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            weaponUI.SwitchWeaponUI(4);
        }

        if (current != weaponCurrentSwitch && weaponCurrentSwitch < Weapons.Count)
        {
            for (int i = 0; i < Weapons.Count; i++)
            {
                if (Weapons[i] != null) Weapons[i].SetActive(false);
            }

            if (Weapons[weaponCurrentSwitch] != null)
            {
                Weapons[weaponCurrentSwitch].SetActive(true);
                Weapon = Weapons[weaponCurrentSwitch];
            }

            if (weaponUI != null)
            {
                weaponUI.SwitchWeaponUI(weaponCurrentSwitch);
            }
        }
    }

    public void Pause()
    {
        pause = !pause;
        if (pause)
        {
            pauseScreen.SetActive(true);
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
            pauseScreen.SetActive(false);
            movement.canMove = true;
        }
    }

    private void Shoot()
    {
        if (Weapon == null) return;

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
        else if (Weapon.GetComponent<Melee>() != null)
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

    public void TakeDamage(float damage)
    {
        health -= damage;
        OnHealthChanged?.Invoke();
        hitEffect.TakeDamageFlash(hitColor);
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    private void BottleFaceChange()
    {
        float hpProcent = 100 / maxHealth * health;
        int bottleface = 0;

        for (int i = 0; i < BottleFaces.Count; i++)
        {
            BottleFaces[i].SetActive(false);
        }
        BottleFaces[bottleface].SetActive(true);
    }

    public void ResetHealth()
    {
        health = maxHealth;
        OnHealthChanged?.Invoke();
    }

    public void Die()
    {
        movement.canMove = false;
        dead = true;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator DeathScreen()
    {
        deathScreen[0].SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        int x = 150;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < deathScreen.Count; j++)
            {
                if (deathScreen[j].GetComponent<Image>())
                {
                    Image Deathcurrent = deathScreen[j].GetComponent<Image>();
                    Deathcurrent.color = Deathcurrent.color + new UnityEngine.Color(0, 0, 0, Time.deltaTime);
                }
                else if (deathScreen[j].GetComponent<TextMeshProUGUI>())
                {
                    TextMeshProUGUI Deathcurrent = deathScreen[j].GetComponent<TextMeshProUGUI>();
                    Deathcurrent.color = Deathcurrent.color + new UnityEngine.Color(0, 0, 0, Time.deltaTime);
                }
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public static void AddWeaponToList(GameObject weapon)
    {
        if (Weapons == null)
        {
            Weapons = new List<GameObject>();
        }

        Weapons.Add(weapon);

        if (HotbarInstance != null)
        {
            weapon.transform.SetParent(HotbarInstance);
        }
    }
}