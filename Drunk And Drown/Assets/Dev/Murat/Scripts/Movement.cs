using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState
{
    Running,
    Crouched
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Movement : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionAsset inputActions;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float crouchSpeedTarget = 3f;
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float lookSpeed = 0.1f;
    [SerializeField] private float lookXLimit = 60f;

    [SerializeField] private float maxForce = 15f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool isOnSlope;

    private float curSpeed;
    private float rotationX = 0;
    private bool doubleJumpAvailable = true;
    private bool isGrounded;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool crouchHeld;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction crouchAction;

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
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.useGravity = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        curSpeed = walkSpeed;
    }

    void Update()
    {
        HandleInputs();

        if (canMove)
        {
            HandleLook();
        }
    }

    void FixedUpdate()
    {
        CheckGround();

        if (canMove)
        {
            HandleMovementState();
            HandlePhysicsMovement();
            HandleJumping();
        }
    }

    private void HandleInputs()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
        crouchHeld = crouchAction.IsPressed();

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

    private void HandleMovementState()
    {
        movementState = crouchHeld ? MovementState.Crouched : MovementState.Running;

        switch (movementState)
        {
            case MovementState.Running:
                if (capsuleCollider.height != 2f)
                {
                    capsuleCollider.height = 2f;
                    capsuleCollider.center = Vector3.zero;
                }
                curSpeed = walkSpeed;
                break;

            case MovementState.Crouched:
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
        }
    }

    private void HandlePhysicsMovement()
    {
        Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
        Vector3 targetVelocity = moveDirection * curSpeed;

        Vector3 currentVelocity = rb.linearVelocity;

        if (isOnSlope && isGrounded)
        {
            Vector3 slopeDirection = Vector3.ProjectOnPlane(targetVelocity, slopeHit.normal);
            Vector3 slopeVelocityChange = slopeDirection - currentVelocity;

            slopeVelocityChange.x = Mathf.Clamp(slopeVelocityChange.x, -maxForce, maxForce);
            slopeVelocityChange.y = Mathf.Clamp(slopeVelocityChange.y, -maxForce, maxForce);
            slopeVelocityChange.z = Mathf.Clamp(slopeVelocityChange.z, -maxForce, maxForce);

            rb.useGravity = false;

            rb.AddForce(slopeVelocityChange, ForceMode.Force);
            return;
        }

        rb.useGravity = true;
        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange.y = 0;

        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxForce, maxForce);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxForce, maxForce);

        rb.AddForce(velocityChange, ForceMode.Force);
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

    public void ApplyKnockback(Vector3 forceDirection)
    {
        rb.AddForce(forceDirection, ForceMode.Impulse);
    }
}