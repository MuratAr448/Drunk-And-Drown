using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquidRayGun : Gun
{
    [Header("Squid Ray Settings")]
    [SerializeField] private float Basedamage = 1;
    [SerializeField] private float size = 0.01f;
    public float squidOverheatProcent;
    private bool squidOverheated;
    private MainPlayer playerSquid;
    [SerializeField] private GameObject inkPrefab;
    private GameObject inkRay;
    [SerializeField] private LayerMask layerMask;
    private GameObject previousTarget;
    private bool ableToHit = false;
    public Enemy enemy;

    [Header("Grapple Target & Layer settings")]
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private float maxGrappleDistance = 50f;
    [SerializeField] public Color grappleReadyColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] public Color grappleCooldownColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Grapple Physics & Speeds")]
    [SerializeField] private float grappleForce = 150f; // Velocity change acceleration rate
    [SerializeField] private float initialGrappleSpeed = 15f;
    [SerializeField] private float launchUpwardForce = 12f;

    [Header("Collision & Contact Thresholds")]
    [Tooltip("Distance threshold between player and enemy pivot points to trigger launch.")]
    [SerializeField] private float reachThreshold = 3.0f;
    [Tooltip("Distance threshold between player center and enemy collider surface to trigger launch.")]
    [SerializeField] private float contactThreshold = 0.8f;

    [Header("Damage settings")]
    [SerializeField] private float speedDamageMultiplier = 1f;

    [Header("References")]
    [SerializeField] private Transform transformPivot; // Muzzle position where the rope originates
    [SerializeField] private UnityEngine.UI.Image grappleIndicator;

    // Private cache and state variables
    private Movement player;
    private MainPlayer mainPlayer;
    private Rigidbody playerRb;
    private Collider playerCollider;
    
    private LineRenderer lineRenderer;
    private Transform activeGrapplePoint;
    private Collider activeEnemyCollider;
    private Vector3 staticGrappleWorldPos;
    private bool isStaticGrapple = false;
    protected bool isGrappling = false;
    protected bool isInputBlocked = false;
    protected KeyCode grappleKey = KeyCode.Mouse1;

    public override float GetDamage() { return Basedamage; }
    public override void ApplyRarityScaling(float multiplier)
    {
        base.ApplyRarityScaling(multiplier);
        Basedamage *= multiplier;
        speedDamageMultiplier *= multiplier;
    }

    protected virtual void Start()
    {
        playerSquid = FindFirstObjectByType<MainPlayer>();
        Kind = KindofGun.SquidRayGun;

        InitializePlayerReferences();
        InitializeGunSettings();
        InitializeLineRenderer();
        InitializeGrappleIndicator();
    }

    protected override void OnEnable()
    {
        if (lastDisableTime > 0f)
        {
            float timePassed = Time.time - lastDisableTime;
            // Cooldown 1 is heat, so it decreases while unequipped! (5f per second is the cooling rate)
            cooldown1 = Mathf.Max(cooldown1 - (5f * timePassed), 0f);
            
            // Cooldown 2 is a normal cooldown, so it increases!
            cooldown2 = Mathf.Min(cooldown2 + timePassed, shootRate2);
        }
        // Deliberately NOT calling base.OnEnable() because it would increase cooldown1!
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopGrapple();
        if (grappleIndicator != null)
        {
            grappleIndicator.gameObject.SetActive(false);
        }
    }

    public override void Shoot()
    {
        // Primary attack logic
        if (Input.GetKey(KeyCode.Mouse0)&& !squidOverheated)
        {
            cooldown1 += Time.deltaTime*2;
            squidOverheatProcent += Time.deltaTime;
        }
        if (squidOverheatProcent > 7.5f)
        {
            squidOverheated = true;
        }
    }

    private void CoolingDown()
    {
        if(!Input.GetKey(KeyCode.Mouse0)|| squidOverheated)
        {
            cooldown1 -= Time.deltaTime;
            ableToHit = false;

            if (cooldown1 < 0f)
            {
                cooldown1 = 0;
                if (inkRay != null)
                {
                    Destroy(inkRay);
                    inkRay = null;
                }
            }
            squidOverheatProcent -= Time.deltaTime*1.5f;
            if (squidOverheatProcent < 0f)
            {
                squidOverheatProcent = 0;
                squidOverheated=false;
            }
        }
        else
        {
            if (inkRay == null)
            {
                inkRay = Instantiate(inkPrefab, gameObject.transform);
            }
            ableToHit = true;
            if (cooldown1>= shootRate1)
            {
                cooldown1 = shootRate1;
            }
        }
        size = cooldown1 / shootRate1;
        RaycastHit hit;
        GameObject Origin = playerSquid.rayOrigin;
        
        if (Physics.Raycast(Origin.transform.position, Origin.transform.forward, out hit, 20, layerMask) && inkRay != null)
        {
            if (hit.collider != inkRay.GetComponent<Collider>())
            {
                float distance = Vector3.Distance(transform.position, hit.point);
                transform.LookAt(hit.point);
                inkRay.transform.rotation = transform.rotation;
                inkRay.transform.localScale = new Vector3(size, size, distance);
                inkRay.transform.localPosition = Vector3.forward * (distance * 0.3f);
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null && ableToHit)
                {
                    GameObject targetObj = (damageable as MonoBehaviour).gameObject;
                    if (targetObj.TryGetComponent(out RayDamage RD))
                    {
                        if (RD.Iframe <= 0)
                        {
                            RD.RayIframe();
                            if (previousTarget != targetObj)
                            {
                                RD.Multiplyer = 0;
                            }
                            RD.Multiplyer += Basedamage;
                            damageable.TakeDamage(RD.Multiplyer);
                        }
                    }
                    else
                    {
                        targetObj.AddComponent<RayDamage>();
                        damageable.TakeDamage(Basedamage);
                    }
                    previousTarget = targetObj;
                }
            }
        }
        else if(inkRay != null)
        {
            transform.LookAt(player.transform.position + Origin.transform.forward* 20);
            inkRay.transform.localPosition = Vector3.forward * (20 * 0.3f);
            inkRay.transform.rotation = transform.rotation;
            inkRay.transform.localScale = new Vector3(size, size, 20);
            inkRay.transform.localPosition = Vector3.forward * (20f * 0.3f);
        }
    }

    public override void Secondary()
    {
        if (isInputBlocked) return;

        if (shootRate2 <= cooldown2 && !isGrappling)
        {
            if (StartGrapple())
            {
                cooldown2 = 0f;
            }
        }
    }

    protected virtual void Update()
    {
        HandleCooldowns();

        if (isInputBlocked && !Input.GetKey(grappleKey))
        {
            isInputBlocked = false;
        }
        
        if (isGrappling)
        {
            HandleActiveGrapple();
        }
        else
        {
            UpdateRangeIndicator();
        }

        CoolingDown(); // Squid ray gun specific cooling down logic
    }

    protected virtual void FixedUpdate()
    {
        if (isGrappling && playerRb != null)
        {
            ApplyGrappleForce();
        }
    }

    // --- Helper Initialization Methods ---

    private void InitializePlayerReferences()
    {
        player = FindFirstObjectByType<Movement>();
        if (player != null)
        {
            mainPlayer = player.GetComponent<MainPlayer>();
            playerRb = player.GetComponent<Rigidbody>();
            playerCollider = player.GetComponent<Collider>();
        }
    }

    private void InitializeGunSettings()
    {
        // Set default shoot rates if left at 0 to avoid lockouts
        if (shootRate1 == 0f) shootRate1 = 0.2f;
        if (shootRate2 == 0f) shootRate2 = 0.2f;
    }

    private void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.positionCount = 2;
            
            Shader defaultShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply") ?? Shader.Find("Sprites/Default");
            if (defaultShader != null)
            {
                lineRenderer.material = new Material(defaultShader);
            }
            lineRenderer.startColor = new Color(0.8f, 0.8f, 0.8f, 1f); 
            lineRenderer.endColor = new Color(0f, 0.8f, 1f, 1f);       
        }
        
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;
    }

    private void InitializeGrappleIndicator()
    {
        if (grappleIndicator == null)
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                Transform t = FindChildRecursive(root.transform, "GrappleIndicator");
                if (t != null)
                {
                    grappleIndicator = t.GetComponent<UnityEngine.UI.Image>();
                    break;
                }
            }

            if (grappleIndicator == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject indicatorGo = new GameObject("GrappleIndicator");
                    indicatorGo.transform.SetParent(canvas.transform, false);
                    indicatorGo.transform.SetAsFirstSibling();
                    
                    grappleIndicator = indicatorGo.AddComponent<UnityEngine.UI.Image>();
                    RectTransform rect = indicatorGo.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(20f, 20f);
                    rect.anchoredPosition = Vector2.zero;
                    grappleIndicator.color = grappleReadyColor; 
                    
                    Sprite knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
                    if (knob != null)
                    {
                        grappleIndicator.sprite = knob;
                    }
                }
            }
        }

        if (grappleIndicator != null)
        {
            grappleIndicator.gameObject.SetActive(false);
        }
    }

    // --- Core Grapple Logic ---

    protected bool StartGrapple()
    {
        if (mainPlayer == null || mainPlayer.rayOrigin == null) return false;

        GameObject origin = mainPlayer.rayOrigin;
        Vector3 fireDirection = origin.transform.forward;

        int mask = grappleLayer.value == 0 ? ~0 : grappleLayer.value;
        Vector3 startPos = origin.transform.position + fireDirection * 0.2f;

        RaycastHit[] hits = Physics.RaycastAll(startPos, fireDirection, maxGrappleDistance, mask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RaycastHit hit = default;
        bool hitSomething = false;
        Enemy hitEnemy = null;

        foreach (var h in hits)
        {
            if (player != null && h.collider.transform.root == player.transform.root)
            {
                continue;
            }
            
            hitEnemy = h.collider.GetComponentInParent<Enemy>();
            if (hitEnemy != null)
            {
                hit = h;
                hitSomething = true;
                break;
            }
        }

        if (hitSomething && hitEnemy != null)
        {
            activeEnemyCollider = hit.collider;
            Transform grapplePoint = hitEnemy.transform.Find("GrapplePoint");
            activeGrapplePoint = grapplePoint != null ? grapplePoint : hitEnemy.transform;
            isStaticGrapple = false;
            isGrappling = true;
            this.enemy = hitEnemy;
            this.enemy.SetStunned(true);

            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
            }

            // Snappy redirection on frame 1
            if (playerRb != null && player != null)
            {
                Vector3 targetPos = isStaticGrapple ? staticGrappleWorldPos : (activeGrapplePoint != null ? activeGrapplePoint.position : player.transform.position);
                Vector3 pullDirection = (targetPos - player.transform.position).normalized;
                float currentSpeed = playerRb.linearVelocity.magnitude;
                float launchSpeed = Mathf.Max(currentSpeed, initialGrappleSpeed);
                playerRb.linearVelocity = pullDirection * launchSpeed;
            }
            
            return true;
        }
        
        return false;
    }

    protected void StopGrapple()
    {
        if (this.enemy != null)
        {
            this.enemy.SetStunned(false);
        }
        isGrappling = false;
        activeGrapplePoint = null;
        activeEnemyCollider = null;
        isStaticGrapple = false;
        this.enemy = null;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        if (Input.GetKey(grappleKey))
        {
            isInputBlocked = true;
        }
    }

    public Enemy GetActiveEnemy()
    {
        if (isGrappling && !isStaticGrapple && activeEnemyCollider != null)
        {
            return activeEnemyCollider.GetComponentInParent<Enemy>();
        }
        return null;
    }

    public void StopGrappleExternal()
    {
        StopGrapple();
    }

    private void HandleCooldowns()
    {
        // cooldown1 is managed by CoolingDown() uniquely for the primary laser
        if (shootRate2 >= cooldown2) cooldown2 += Time.deltaTime;
    }

    private void HandleActiveGrapple()
    {
        bool isHoldingGrapple = Input.GetKey(grappleKey);
        Vector3 targetPos = isStaticGrapple ? staticGrappleWorldPos : (activeGrapplePoint != null ? activeGrapplePoint.position : Vector3.zero);
        float currentDistance = Vector3.Distance(player.transform.position, targetPos);

        if (!isHoldingGrapple || (!isStaticGrapple && activeGrapplePoint == null) || currentDistance > maxGrappleDistance * 1.5f)
        {
            StopGrapple();
            return;
        }

        UpdateLineRenderer(targetPos);

        if (!isStaticGrapple && activeGrapplePoint != null)
        {
            CheckEnemyCollision(currentDistance);
        }
    }

    private void CheckEnemyCollision(float currentDistance)
    {
        bool shouldTriggerLaunch = false;

        // 1. Pivot distance check
        if (currentDistance <= reachThreshold)
        {
            shouldTriggerLaunch = true;
        }
        // 2. Proximity/surface contact check
        else if (activeEnemyCollider != null && playerCollider != null)
        {
            Vector3 playerCenter = playerCollider.bounds.center;
            Vector3 closestPointOnEnemy = activeEnemyCollider.ClosestPoint(playerCenter);
            float distanceToEnemySurface = Vector3.Distance(playerCenter, closestPointOnEnemy);
            
            if (distanceToEnemySurface <= contactThreshold)
            {
                shouldTriggerLaunch = true;
            }
        }

        if (shouldTriggerLaunch)
        {
            TriggerLaunch();
        }
    }

    private void TriggerLaunch()
    {
        // 1. Cache values before stopping grapple clears them
        float speed = player != null ? player.GetVelocity() : 0f;
        float damage = speed * speedDamageMultiplier;
        if (damage < 5f) damage = 5f;

        Enemy enemy = activeGrapplePoint != null ? activeGrapplePoint.GetComponentInParent<Enemy>() : null;
        Collider enemyCollider = activeEnemyCollider;

        // 2. Stop the grapple hook immediately (so physics pull ceases)
        StopGrapple();

        // 3. Deal damage and launch player
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        if (playerRb != null)
        {
            if (playerCollider != null && enemyCollider != null)
            {
                StartCoroutine(TempIgnoreCollision(playerCollider, enemyCollider, 0.25f));
            }

            // Nudge player upward slightly to avoid stuck states
            player.transform.position += Vector3.up * 0.5f;
            playerRb.linearVelocity = Vector3.up * launchUpwardForce;
        }
    }

    private void ApplyGrappleForce()
    {
        Vector3 targetPos = isStaticGrapple ? staticGrappleWorldPos : (activeGrapplePoint != null ? activeGrapplePoint.position : player.transform.position);
        
        if (!isStaticGrapple && activeGrapplePoint == null)
        {
            StopGrapple();
            return;
        }

        Vector3 direction = (targetPos - player.transform.position).normalized;
        playerRb.AddForce(direction * grappleForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    private void UpdateLineRenderer(Vector3 targetPos)
    {
        if (lineRenderer != null)
        {
            Vector3 originPos = transformPivot != null ? transformPivot.position : transform.position;
            lineRenderer.SetPosition(0, originPos);
            lineRenderer.SetPosition(1, targetPos);
        }
    }

    private void UpdateRangeIndicator()
    {
        if (grappleIndicator == null) return;

        if (mainPlayer != null && mainPlayer.rayOrigin != null)
        {
            GameObject origin = mainPlayer.rayOrigin;
            Vector3 fireDirection = origin.transform.forward;
            int mask = grappleLayer.value == 0 ? ~0 : grappleLayer.value;
            Vector3 startPos = origin.transform.position + fireDirection * 0.2f;

            RaycastHit[] hits = Physics.RaycastAll(startPos, fireDirection, maxGrappleDistance, mask);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            bool inRange = false;
            foreach (var h in hits)
            {
                if (player != null && h.collider.transform.root == player.transform.root)
                {
                    continue;
                }
                
                if (h.collider.GetComponentInParent<Enemy>() != null)
                {
                    inRange = true;
                    break;
                }
            }

            if (inRange)
            {
                grappleIndicator.gameObject.SetActive(true);
                if (cooldown2 >= shootRate2)
                {
                    grappleIndicator.color = grappleReadyColor; // White (Ready)
                }
                else
                {
                    grappleIndicator.color = grappleCooldownColor; // Dark Grey (On Cooldown)
                }
            }
            else
            {
                grappleIndicator.gameObject.SetActive(false);
            }
        }
        else
        {
            grappleIndicator.gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator TempIgnoreCollision(Collider col1, Collider col2, float duration)
    {
        if (col1 == null || col2 == null) yield break;
        
        Physics.IgnoreCollision(col1, col2, true);
        yield return new WaitForSeconds(duration);
        
        if (col1 != null && col2 != null)
        {
            Physics.IgnoreCollision(col1, col2, false);
        }
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
