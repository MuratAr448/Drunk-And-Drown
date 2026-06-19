using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MainPlayer : MonoBehaviour, IDamageable
{
    public static MainPlayer Instance { get; private set; }

    public Weapons weapon;
    public static List<Weapons> weapons;
    public bool pause = false;
    public GameObject pauseScreen;
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

    [SerializeField] private Volume _pauseVolume;

    [Header("Luck Settings")]
    [SerializeField] private float luck = 1f;
    public float Luck
    {
        get => luck;
        set => luck = value;
    }

    [Header("Stats Screen Settings")]
    [SerializeField] private TextMeshProUGUI statsText;

    [SerializeField] private Transform hotbar;
    public static Transform HotbarInstance { get; private set; }

    [Header("Death Screen Settings")]
    public GameObject deathScreen;
    [SerializeField] private TextMeshProUGUI finalScore;

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

        // Auto-discover weapons under the hotbar (Hand 1) at start
        if (hotbar != null)
        {
            foreach (Transform child in hotbar)
            {
                if (child.TryGetComponent(out Weapons weapon))
                {
                    if (!weapons.Contains(weapon))
                    {
                        weapons.Add(weapon);
                    }
                }
            }
        }

        // Set the active weapon
        if (weapon == null && weapons.Count > 0)
        {
            weapon = weapons[0];
            weaponCurrentSwitch = 0;
        }

        // Ensure only the active weapon is enabled, and others are disabled
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].gameObject.SetActive(weapons[i] == weapon);
            }
        }
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }

        if (movement.canMove)
        {
            Shoot();
            if (AlowedToSwitch())
            {
                SwitchWeapon();
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
                    return true;
                case KindofGun.BunderBuss:
                    return true;
                case KindofGun.SquidRayGun:
                    SquidRayGun squidGun = gun.GetComponent<SquidRayGun>();
                    if (squidGun != null)
                    {
                        if (squidGun.enemy == null)
                        {
                            return true;
                        }
                    }
                    else
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
                    if (!Melee.GetComponent<SeaShellsBoxingGloves>().Lunging)
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
            UpdateStatsText();
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
        TogglePauseVolume(pause);
    }

    private void TogglePauseVolume(bool active)
    {
        // Automatically ensure the Main Camera has URP Post Processing enabled
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            var cameraData = mainCam.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData != null && !cameraData.renderPostProcessing)
            {
                cameraData.renderPostProcessing = true;
            }
        }

        if (_pauseVolume == null)
        {
            // Try to find an existing Volume in the scene named "PauseVolume"
            Volume[] allVolumes = FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var vol in allVolumes)
            {
                if (vol.name.Contains("Pause"))
                {
                    _pauseVolume = vol;
                    break;
                }
            }
        }

        if (_pauseVolume == null)
        {
            // Create a dynamic volume parented to the player
            GameObject volumeGo = new GameObject("PauseVolumeDynamic");
            volumeGo.transform.SetParent(transform);
            _pauseVolume = volumeGo.AddComponent<Volume>();
            _pauseVolume.isGlobal = true;
            _pauseVolume.priority = 100f; // High priority to override other volume profiles
            
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _pauseVolume.profile = profile;

            // Always configure the Depth of Field settings for our dynamic fallback volume
            DepthOfField dof;
            if (!_pauseVolume.profile.TryGet<DepthOfField>(out dof))
            {
                dof = _pauseVolume.profile.Add<DepthOfField>(true);
            }

            if (dof != null)
            {
                dof.active = true;
                dof.mode.Override(DepthOfFieldMode.Gaussian);
                dof.gaussianStart.Override(0.1f);
                dof.gaussianEnd.Override(1.0f);
                dof.gaussianMaxRadius.Override(1.5f);
                dof.highQualitySampling.Override(true);
            }
        }

        if (_pauseVolume != null)
        {
            _pauseVolume.priority = 100f;
            _pauseVolume.weight = active ? 1f : 0f;
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
        if (dead) return;

        health -= damage;
        OnHealthChanged?.Invoke();
        hitEffect.TakeDamageFlash(hitColor);
        if (playerHurt != null) playerHurt.PlayOneShot(audioSource);
        if (health <= 0)
        {
            Die();
        }
    }


    public void ResetHealth()
    {
        health = maxHealth;
        OnHealthChanged?.Invoke();
    }

    public void Heal(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
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
        deathScreen.SetActive(true);
        UpdateStatsText();
        finalScore.text = "Score: " + UIUtils.FormatNumber(ScoreSystem.Instance._totalScore);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
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

    public static bool HasWeapon(GameObject weaponPrefab)
    {
        if (weapons == null || weaponPrefab == null) return false;

        Weapons prefabComp = weaponPrefab.GetComponent<Weapons>();
        if (prefabComp == null) return false;

        System.Type targetType = prefabComp.GetType();
        foreach (var ownedWeapon in weapons)
        {
            if (ownedWeapon != null && ownedWeapon.GetType() == targetType)
            {
                return true;
            }
        }
        return false;
    }

    private void GetRarityChances(float luckVal, out float commonPct, out float uncommonPct, out float rarePct, out float epicPct, out float legendaryPct)
    {
        float wCommon = 40f / Mathf.Max(0.1f, luckVal);
        float wUncommon = 25f;
        float wRare = 18f * Mathf.Sqrt(luckVal);
        float wEpic = 12f * luckVal;
        float wLegendary = 5f * luckVal * luckVal;

        float totalWeight = wCommon + wUncommon + wRare + wEpic + wLegendary;
        
        commonPct = (wCommon / totalWeight) * 100f;
        uncommonPct = (wUncommon / totalWeight) * 100f;
        rarePct = (wRare / totalWeight) * 100f;
        epicPct = (wEpic / totalWeight) * 100f;
        legendaryPct = (wLegendary / totalWeight) * 100f;
    }

    private void UpdateStatsText()
    {
        if (statsText == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Header Title
        sb.AppendLine("<size=140%><color=#FFFFFF><b>CHARACTER STATS</b></color></size>");
        sb.AppendLine();

        // 1. Attributes
        sb.AppendLine("<color=#FFFFFF><b>Attributes:</b></color>");
        float currentMaxHP = BaseHealth;
        float currentSpeed = movement != null ? movement.WalkSpeed : 6f;
        float currentCoinMultiplier = CoinSystem.Instance != null ? CoinSystem.Instance.CoinMultiplier : 1f;
        int currentCoins = CoinSystem.Instance != null ? CoinSystem.Instance.GetCoinAmount() : 0;

        // Health: Green if full, Red if damaged. Max health: Green if >= baseline 100, Red otherwise.
        string hpColor = health < currentMaxHP ? "#FF5555" : "#55FF55";
        string maxHpColor = currentMaxHP >= 100f ? "#55FF55" : "#FF5555";
        sb.AppendLine($"- Health: <color={hpColor}>{UIUtils.FormatNumber(health)}</color> / <color={maxHpColor}>{UIUtils.FormatNumber(currentMaxHP)}</color>");

        // Speed: Green if >= baseline 6.0, Red otherwise.
        string speedColor = currentSpeed >= 6f ? "#55FF55" : "#FF5555";
        sb.AppendLine($"- Speed: <color={speedColor}>{UIUtils.FormatNumber(currentSpeed)}</color>");

        // Coin multiplier: Green if >= baseline 1.0, Red otherwise.
        string multColor = currentCoinMultiplier >= 1f ? "#55FF55" : "#FF5555";
        sb.AppendLine($"- Coin Mult: <color={multColor}>{UIUtils.FormatNumber(currentCoinMultiplier)}x</color>");

        // Coins: Green if you have dabloons
        string coinColor = currentCoins > 0 ? "#55FF55" : "#FFFFFF";
        sb.AppendLine($"- Dabloons: <color={coinColor}>{UIUtils.FormatNumber(currentCoins)}</color>");

        // Luck: Green if >= baseline 1.0, Red otherwise.
        string luckColor = luck >= 1f ? "#55FF55" : "#FF5555";
        sb.AppendLine($"- Luck: <color={luckColor}>{UIUtils.FormatNumber(luck)}</color>");
        sb.AppendLine();

        // 2. Rarity Chances
        sb.AppendLine("<color=#FFFFFF><b>Shop Rarity Chances:</b></color>");
        float common, uncommon, rare, epic, legendary;
        GetRarityChances(luck, out common, out uncommon, out rare, out epic, out legendary);

        sb.AppendLine($"- <color=#BBBBBB>Common: {UIUtils.FormatNumber(common)}%</color>");
        sb.AppendLine($"- <color=#55FF55>Uncommon: {UIUtils.FormatNumber(uncommon)}%</color>");
        sb.AppendLine($"- <color=#5555FF>Rare: {UIUtils.FormatNumber(rare)}%</color>");
        sb.AppendLine($"- <color=#AA00FF>Epic: {UIUtils.FormatNumber(epic)}%</color>");
        sb.AppendLine($"- <color=#FF9900>Legendary: {UIUtils.FormatNumber(legendary)}%</color>");
        sb.AppendLine();

        // 3. Weapons
        sb.AppendLine("<color=#FFFFFF><b>Equipped Weapons:</b></color>");
        if (weapons != null && weapons.Count > 0)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i] == null) continue;
                string wName = weapons[i].gameObject.name.Replace("(Clone)", "").Trim();
                string activeText = (weapons[i] == weapon) ? " <color=#55FF55>[Active]</color>" : "";
                
                float dmg = weapons[i].GetDamage();
                string rates = "";
                float r1 = weapons[i].GetRate1();
                if (r1 > 0f) rates += $", {weapons[i].GetRate1Name()}: {UIUtils.FormatNumber(r1)}s";
                float r2 = weapons[i].GetRate2();
                if (r2 > 0f) rates += $", {weapons[i].GetRate2Name()}: {UIUtils.FormatNumber(r2)}s";

                sb.AppendLine($"- Slot {i+1}: {wName} (Dmg: <color=#55FF55>{UIUtils.FormatNumber(dmg)}</color>{rates}){activeText}");
            }
        }
        else
        {
            sb.AppendLine("- No weapons equipped");
        }

        statsText.text = sb.ToString();
    }
}