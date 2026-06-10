using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPlayer : MonoBehaviour, IDamageable
{
    public static MainPlayer Instance { get; private set; }

    public Weapons weapon;
    public static List<Weapons> weapons;
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

    [SerializeField] private Transform hotbar;
    public static Transform HotbarInstance { get; private set; }

    [Header("Sounds")]
    private AudioSource audioSource;
    [SerializeField] private AudioEvent playerHurt;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        Instance = this;
        weapons = new List<Weapons>();
        weaponCurrentSwitch = 0;
        if (weapon != null)
        {
            weapons.Add(weapon);
        }

        movement = GetComponent<Movement>();
        hitEffect = GetComponent<PlayerEffects>();
        maxHealth = health;
        weaponUI = FindFirstObjectByType<UIWeapons>();
        if (hotbar == null)
        {
            hotbar = transform.Find("Hand 1");
        }
        HotbarInstance = hotbar;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        if (movement.canMove)
        {
            Shoot();
            if (AlowedToSwitch())
            {
                SwitchWeapon();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }
        }
    }
    private bool AlowedToSwitch()
    {
        if (weapon.TryGetComponent(out Gun gun))
        {
            switch (gun.Kind)
            {
                case KindofGun.ParrotGun:
                    if (gun.cooldown1 >= gun.shootRate1 && gun.cooldown2 >= gun.shootRate2)
                    {
                        return true;
                    }
                    break;
                case KindofGun.BunderBuss:
                    if (gun.cooldown1 >= gun.shootRate1 && gun.cooldown2 >= gun.shootRate2)
                    {
                        return true;
                    }
                    break;
                case KindofGun.SquidRayGun:
                    if (gun.cooldown1 <= 0&&gun.GetComponent<SquidRayGun>().enemy==null)
                    {
                        return true;
                    }
                    break;
                default:
                    return false;
            }
        }
        else if (weapon.TryGetComponent(out Melee Melee))
        {
            switch (Melee.Kind)
            {
                case KindofMelee.MorningStar:
                    if (!Melee.GetComponent<MorningStar>().Attacking)
                    {
                        return true;
                    }
                    break;
                case KindofMelee.SeaShellsBoxingGloves:
                    if (Melee.cooldown1 >= Melee.swingRate1 && !Melee.GetComponent<SeaShellsBoxingGloves>().Lunging)
                    {
                        return true;
                    }
                    break;
                default:
                    return false;

            }
        }
        return false;
    }

    private void SwitchWeapon()
    {
        if (weapons == null || weapons.Count == 0) return;
        
        int targetSwitch = weaponCurrentSwitch;
        if (Input.GetKeyDown(KeyCode.Alpha1)) targetSwitch = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) targetSwitch = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) targetSwitch = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) targetSwitch = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) targetSwitch = 4;
        if (targetSwitch != weaponCurrentSwitch && targetSwitch < weapons.Count)
        {

            weaponCurrentSwitch = targetSwitch;
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i] != null) weapons[i].gameObject.SetActive(false);
            }

            if (weapons[weaponCurrentSwitch] != null)
            {
                weapons[weaponCurrentSwitch].gameObject.SetActive(true);
                weapon = weapons[weaponCurrentSwitch];
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
        if (weapon == null) return;

        if (weapon.GetComponent<Gun>() != null)
        {
            Gun Gun = weapon.GetComponent<Gun>();
            if (Input.GetKey(KeyCode.Mouse0))
            {
                Gun.Shoot();
            }
            if (Input.GetKey(KeyCode.Mouse1))
            {
                Gun.Secondary();
            }
        }
        else if (weapon.GetComponent<Melee>() != null)
        {
            Melee Melee = weapon.GetComponent<Melee>();
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
        playerHurt.PlayOneShot(audioSource);
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

    public void ModifyMaxHealth(float amount)
    {
        maxHealth += amount;
        health += amount;
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
        Weapons Weapons = weapon.GetComponent<Weapons>();
        if (weapons == null)
        {
            weapons = new List<Weapons>();
        }

        if (HotbarInstance != null)
        {
            weapon.transform.SetParent(HotbarInstance, false);
        }

        weapons.Add(Weapons);

        // Deactivate all other weapons
        for (int i = 0; i < weapons.Count - 1; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].gameObject.SetActive(false);
            }
        }

        // Auto-equip the new weapon
        weapon.gameObject.SetActive(true);

        if (Instance != null)
        {
            Instance.weapon = Weapons;
            Instance.weaponCurrentSwitch = weapons.Count - 1;
            if (Instance.weaponUI != null)
            {
                Instance.weaponUI.SwitchWeaponUI(Instance.weaponCurrentSwitch);
            }
        }
    }
}