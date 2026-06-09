using UnityEngine;

public class GrappleGun : Gun
{
    [Header("Grapple Settings")]
    [SerializeField] private float grappleForce = 150f; // Velocity change acceleration rate
    [SerializeField] private float maxGrappleDistance = 50f;
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private Transform transformPivot; // Muzzle position where the rope originates

    private Movement player;
    private MainPlayer mainPlayer;
    private Rigidbody playerRb;
    
    private LineRenderer lineRenderer;
    private Transform activeGrapplePoint;
    private Vector3 staticGrappleWorldPos;
    private bool isStaticGrapple = false;
    private bool isGrappling = false;

    void Start()
    {
        player = FindFirstObjectByType<Movement>();
        if (player != null)
        {
            mainPlayer = player.GetComponent<MainPlayer>();
            playerRb = player.GetComponent<Rigidbody>();
        }
        
        Kind = KindofGun.GrappleGun;

        // Set default shoot rates if they are left at 0 to avoid lockouts
        if (shootRate1 == 0f) shootRate1 = 0.2f;
        if (shootRate2 == 0f) shootRate2 = 0.2f;

        // Automatically set up LineRenderer if not already assigned
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
        
        lineRenderer.enabled = false;
        Debug.Log("GrappleGun: Started and Initialized.");
    }

    public override void Shoot()
    {
        base.Shoot();
        Debug.Log("GrappleGun: Shoot() invoked.");

        if (shootRate1 <= cooldown1 && !isGrappling)
        {
            cooldown1 = 0f;
            StartGrapple();
        }
    }

    private void StartGrapple()
    {
        if (mainPlayer == null)
        {
            Debug.LogWarning("GrappleGun: mainPlayer reference is missing!");
            return;
        }
        if (mainPlayer.rayOrigin == null)
        {
            Debug.LogWarning("GrappleGun: mainPlayer.rayOrigin is missing!");
            return;
        }

        GameObject origin = mainPlayer.rayOrigin;
        Vector3 fireDirection = origin.transform.forward;

        int playerLayer = player != null ? player.gameObject.layer : 0;
        int mask = grappleLayer.value == 0 ? ~(1 << playerLayer) : grappleLayer.value;
        Vector3 startPos = origin.transform.position + fireDirection * 0.2f;

        Debug.Log($"GrappleGun: Raycasting from {startPos} along direction {fireDirection} (Range: {maxGrappleDistance}, Mask: {mask})");

        if (Physics.Raycast(startPos, fireDirection, out RaycastHit hit, maxGrappleDistance, mask))
        {
            Debug.Log($"GrappleGun: Raycast hit object '{hit.collider.gameObject.name}' on layer '{LayerMask.LayerToName(hit.collider.gameObject.layer)}'");

            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
            
            if (enemy != null)
            {
                Transform grapplePoint = enemy.transform.Find("GrapplePoint");
                activeGrapplePoint = grapplePoint != null ? grapplePoint : enemy.transform;
                isStaticGrapple = false;
                isGrappling = true;
                
                Debug.Log($"GrappleGun: Grappling moving enemy '{enemy.name}' at grapple point '{activeGrapplePoint.name}'");
            }
            else
            {
                // Grapple to the exact hit point on static environments (walls, ceilings, etc.)
                staticGrappleWorldPos = hit.point;
                isStaticGrapple = true;
                isGrappling = true;
                
                Debug.Log($"GrappleGun: Grappling static surface at world position {staticGrappleWorldPos}");
            }

            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
            }
        }
        else
        {
            Debug.Log("GrappleGun: Raycast hit nothing.");
        }
    }

    private void StopGrapple()
    {
        Debug.Log("GrappleGun: Stopping grapple.");
        isGrappling = false;
        activeGrapplePoint = null;
        isStaticGrapple = false;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (shootRate1 >= cooldown1)
        {
            cooldown1 += Time.deltaTime;
        }
        if (shootRate2 >= cooldown2)
        {
            cooldown2 += Time.deltaTime;
        }

        if (isGrappling)
        {
            bool isHoldingGrapple = Input.GetKey(KeyCode.Mouse0);
            
            Vector3 targetPos = isStaticGrapple ? staticGrappleWorldPos : (activeGrapplePoint != null ? activeGrapplePoint.position : Vector3.zero);
            float currentDistance = Vector3.Distance(player.transform.position, targetPos);

            if (!isHoldingGrapple || (!isStaticGrapple && activeGrapplePoint == null) || currentDistance > maxGrappleDistance * 1.5f)
            {
                StopGrapple();
                return;
            }

            if (lineRenderer != null)
            {
                Vector3 originPos = transformPivot != null ? transformPivot.position : transform.position;
                lineRenderer.SetPosition(0, originPos);
                lineRenderer.SetPosition(1, targetPos);
            }
        }
    }

    void FixedUpdate()
    {
        if (isGrappling && playerRb != null)
        {
            Vector3 targetPos = isStaticGrapple ? staticGrappleWorldPos : (activeGrapplePoint != null ? activeGrapplePoint.position : player.transform.position);
            
            if (!isStaticGrapple && activeGrapplePoint == null)
            {
                StopGrapple();
                return;
            }

            Vector3 playerPos = player.transform.position;
            Vector3 direction = (targetPos - playerPos).normalized;

            Vector3 velocityIncrement = direction * grappleForce * Time.fixedDeltaTime;
            playerRb.AddForce(velocityIncrement, ForceMode.VelocityChange);
        }
    }

    private void OnDisable()
    {
        StopGrapple();
    }
}
