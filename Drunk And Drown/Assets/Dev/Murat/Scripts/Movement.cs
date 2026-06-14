using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState
{
    Running,
    Crouched,
    Sliding
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Movement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionAsset inputActions;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    [Header("Player Stats")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float crouchSpeedTarget = 3f;
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float lookSpeed = 0.1f;
    [SerializeField] private float lookXLimit = 60f;
    [SerializeField] private float maxForce = 15f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 16f;
    [SerializeField] private float airControl = 2f;

    [Header("Ground Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Slide Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float flatGroundSlideForce = 5f;
    [SerializeField] private float slideCooldownDuration = 1f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private bool canSlide = true;
    [SerializeField] private Color slideColor;
    [SerializeField] private ParticleSystem slideVFX;
    [SerializeField] private AudioEvent dashSound;
    private RaycastHit slopeHit;
    private bool isOnSlope;
    private float nextSlideTime;
    private bool isCooldownActive;
    private Coroutine slideVFXCoroutine;
    private AudioSource audioSource;
    private float slideStartTime;
    private bool isBoostedSlide;
    [SerializeField] private PhysicsMaterial normalMaterial;
    [SerializeField] private PhysicsMaterial slideMaterial;

    [Header("Fast Fall Settings")]
    [SerializeField] private float fastFallForce = 10f;

    [Header("Camera Height Settings")]
    [SerializeField] private float cameraStandHeight = 0.8f;
    [SerializeField] private float cameraCrouchHeight = 0.2f;
    [SerializeField] private float cameraLerpSpeed = 10f;
    private float targetCameraY;

    private float curSpeed;
    private float rotationX = 0;
    private bool doubleJumpAvailable = true;
    private bool isGrounded;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool crouchHeld;
    private bool slideBuffered;
    private bool fastFallBuffered;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction crouchAction;

    private PlayerEffects playerEffects;

    public bool canMove = true;
    public MovementState movementState;

    void Awake()
    {
        var playerMap = inputActions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");
        jumpAction = playerMap.FindAction("Jump");
        crouchAction = playerMap.FindAction("Crouch");
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
        if (slideVFXCoroutine != null)
        {
            StopCoroutine(slideVFXCoroutine);
            slideVFXCoroutine = null;
        }
        if (slideVFX != null)
        {
            slideVFX.Stop();
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerEffects = GetComponent<PlayerEffects>();
        audioSource = GetComponent<AudioSource>();

        rb.freezeRotation = true;
        rb.useGravity = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        curSpeed = walkSpeed;
        targetCameraY = cameraStandHeight;
    }

    void Update()
    {
        HandleInputs();

        if (canMove)
        {
            HandleLook();
        }

        HandleCameraHeight();
    }

    void FixedUpdate()
    {
        CheckGround();

        if (canMove)
        {
            HandleMovementState();
            HandlePhysicsMovement();
            HandleJumping();
            HandleAirMechanics();
        }
    }

    private void HandleInputs()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
        crouchHeld = crouchAction.IsPressed();

        if (crouchAction.WasPressedThisFrame())
        {
            if (isGrounded)
            {
                if (moveInput.sqrMagnitude > 0.01f)
                {
                    slideBuffered = true;
                }
            }
            else
            {
                fastFallBuffered = true;
                if (moveInput.sqrMagnitude > 0.01f)
                {
                    slideBuffered = true;
                }
            }
        }

        if (jumpAction.WasPressedThisFrame())
        {
            jumpPressed = true;
        }
    }

    private void HandleLook()
    {
        transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed, 0);

        rotationX -= lookInput.y * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    private void HandleCameraHeight()
    {
        Vector3 localPos = playerCamera.transform.localPosition;
        localPos.y = Mathf.Lerp(localPos.y, targetCameraY, Time.deltaTime * cameraLerpSpeed);
        playerCamera.transform.localPosition = localPos;
    }

    private void HandleMovementState()
    {
        if (slideBuffered && isGrounded)
        {
            movementState = MovementState.Sliding;
            slideBuffered = false;
            slideStartTime = Time.time;
            if (Time.time >= nextSlideTime && !isCooldownActive)
            {
                isBoostedSlide = true;
                if (slideVFX != null)
                {
                    if (slideVFXCoroutine != null)
                    {
                        StopCoroutine(slideVFXCoroutine);
                    }
                    slideVFXCoroutine = StartCoroutine(PlaySlideVFXForDuration(0.5f));
                }
                if (dashSound != null && audioSource != null)
                {
                    dashSound.Play(audioSource);
                }
            }
            else
            {
                isBoostedSlide = false;
            }
        }
        else if (movementState == MovementState.Sliding)
        {
            bool durationEnded = isBoostedSlide && (Time.time - slideStartTime >= slideDuration);
            if (durationEnded || !isGrounded || !crouchHeld)
            {
                if (isBoostedSlide)
                {
                    StartCoroutine(SlideCooldown());
                }
                movementState = crouchHeld ? MovementState.Crouched : MovementState.Running;
            }
        }
        else
        {
            movementState = crouchHeld ? MovementState.Crouched : MovementState.Running;
        }

        switch (movementState)
        {
            case MovementState.Running:
                targetCameraY = cameraStandHeight;
                capsuleCollider.material = normalMaterial;
                if (capsuleCollider.height != 2f)
                {
                    capsuleCollider.height = 2f;
                    capsuleCollider.center = Vector3.zero;
                }
                curSpeed = walkSpeed;
                break;

            case MovementState.Crouched:
                targetCameraY = cameraCrouchHeight;
                capsuleCollider.material = normalMaterial;
                if (capsuleCollider.height != 1f)
                {
                    capsuleCollider.height = 1f;
                    capsuleCollider.center = new Vector3(0, -0.5f, 0);
                }

                if (curSpeed > crouchSpeedTarget)
                {
                    curSpeed -= Time.fixedDeltaTime * 5f;
                }
                else
                {
                    curSpeed = crouchSpeedTarget;
                }
                break;

            case MovementState.Sliding:
                targetCameraY = cameraCrouchHeight;
                capsuleCollider.material = slideMaterial;
                if (capsuleCollider.height != 1f)
                {
                    capsuleCollider.height = 1f;
                    capsuleCollider.center = new Vector3(0, -0.5f, 0);
                }
                break;
        }
    }

    private void HandlePhysicsMovement()
    {
        if (movementState == MovementState.Sliding)
        {
            rb.useGravity = true;

            if (!canSlide)
            {
                if (isBoostedSlide)
                {
                    if (playerEffects != null)
                    {
                        playerEffects.TakeDamageFlash(slideColor);
                    }

                    Vector3 flatSlideDirection = transform.forward;
                    rb.AddForce(flatSlideDirection * flatGroundSlideForce, ForceMode.VelocityChange);
                }

                canSlide = true;
            }

            // Apply a small sustained movement force if holding inputs, so they don't stop completely
            if (moveInput.sqrMagnitude > 0.01f)
            {
                Vector3 slideMoveDir = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
                Vector3 slideCurrentVel = rb.linearVelocity;
                slideCurrentVel.y = 0;

                float targetSpeed = isBoostedSlide ? walkSpeed : crouchSpeedTarget;
                float currentSpeedInMoveDir = Vector3.Dot(slideCurrentVel, slideMoveDir);

                Vector3 slideForce = Vector3.zero;

                // If moving slower than target speed in the input direction, accelerate up to it
                if (currentSpeedInMoveDir < targetSpeed)
                {
                    float speedDiff = targetSpeed - currentSpeedInMoveDir;
                    slideForce += slideMoveDir * speedDiff * 3f;
                }

                // Apply some lateral friction/steering to align velocity with move direction
                Vector3 lateralVel = slideCurrentVel - (slideMoveDir * currentSpeedInMoveDir);
                if (lateralVel.sqrMagnitude > 0.01f)
                {
                    slideForce += -lateralVel * 2f;
                }

                slideForce *= Time.fixedDeltaTime;
                slideForce.x = Mathf.Clamp(slideForce.x, -maxForce, maxForce);
                slideForce.z = Mathf.Clamp(slideForce.z, -maxForce, maxForce);

                rb.AddForce(slideForce, ForceMode.VelocityChange);
            }
            return;
        }

        canSlide = false;

        Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
        Vector3 targetVelocity = moveDirection * curSpeed;

        Vector3 currentVelocity = rb.linearVelocity;
        float speedChangeRate = isGrounded ? ((targetVelocity.sqrMagnitude > 0.01f) ? acceleration : deceleration) : airControl;

        if (isOnSlope && isGrounded)
        {
            Vector3 slopeDirection = Vector3.ProjectOnPlane(targetVelocity, slopeHit.normal);
            Vector3 slopeVelocityChange = slopeDirection - currentVelocity;

            slopeVelocityChange *= speedChangeRate * Time.fixedDeltaTime;

            slopeVelocityChange.x = Mathf.Clamp(slopeVelocityChange.x, -maxForce, maxForce);
            slopeVelocityChange.y = Mathf.Clamp(slopeVelocityChange.y, -maxForce, maxForce);
            slopeVelocityChange.z = Mathf.Clamp(slopeVelocityChange.z, -maxForce, maxForce);

            rb.useGravity = false;

            rb.AddForce(slopeVelocityChange, ForceMode.VelocityChange);
            return;
        }

        rb.useGravity = true;
        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange.y = 0;

        velocityChange *= speedChangeRate * Time.fixedDeltaTime;

        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxForce, maxForce);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxForce, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void HandleJumping()
    {
        if (isGrounded)
        {
            doubleJumpAvailable = true;
        }

        if (jumpPressed)
        {
            rb.useGravity = true;

            if (isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
            }
            else if (doubleJumpAvailable)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                doubleJumpAvailable = false;
            }

            jumpPressed = false;
        }
    }

    private void HandleAirMechanics()
    {
        if (!isGrounded && fastFallBuffered)
        {
            rb.AddForce(Vector3.down * fastFallForce, ForceMode.VelocityChange);
            fastFallBuffered = false;
        }
    }

    private void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, 0.3f, groundLayer);
        }
        else
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
        }

        if (isGrounded)
        {
            fastFallBuffered = false;
        }

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, (capsuleCollider.height / 2f) + 0.3f, groundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            isOnSlope = angle > 0.1f && angle <= maxSlopeAngle;
        }
        else
        {
            isOnSlope = false;
        }
    }

    private IEnumerator SlideCooldown()
    {
        isCooldownActive = true;
        nextSlideTime = Time.time + slideCooldownDuration;
        yield return new WaitForSeconds(slideCooldownDuration);
        isCooldownActive = false;
    }

    private IEnumerator PlaySlideVFXForDuration(float duration)
    {
        slideVFX.Play();
        yield return new WaitForSeconds(duration);
        slideVFX.Stop();
    }

    public float GetVelocity()
    {
        return rb.linearVelocity.magnitude;
    }

    public void ModifySpeed(float multiplier)
    {
        walkSpeed *= multiplier;
        curSpeed = walkSpeed;
    }

    public void ApplyKnockback(Vector3 forceDirection)
    {
        rb.AddForce(forceDirection, ForceMode.Impulse);
    }
}