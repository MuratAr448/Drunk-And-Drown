using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float lookSpeed = 1.5f;
    [SerializeField] private float lookXLimit = 60f;

    private Vector3 moveDirection = Vector3.zero;
    private Rigidbody rb;
    private float rotationX = 0;
    public CharacterController characterController;
    [SerializeField] private bool doubleJump = true;
    public bool canMove = true;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        //bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX;
        float curSpeedY;
        if (canMove)
        {
            curSpeedX = walkSpeed;
            curSpeedY = walkSpeed;
            curSpeedX = curSpeedX * Input.GetAxis("Vertical");
            curSpeedY = curSpeedY * Input.GetAxis("Horizontal");
        }
        else
        {
            curSpeedX = 0;
            curSpeedY = 0;
        }

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);


        if (Input.GetKeyDown(KeyCode.Space) && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }
        if (Input.GetKeyDown(KeyCode.Space) && canMove && !characterController.isGrounded && doubleJump)
        {
            characterController.enabled = true;
            rb.Equals(false);
            moveDirection.y = jumpPower;
            doubleJump = false;
        }

        if (!characterController.isGrounded)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                moveDirection.y -= gravity * Time.deltaTime * 1.2f;
            }
            else
            {
                moveDirection.y -= gravity * Time.deltaTime*1.5f;
            }
        }
        else
        {
            doubleJump = true;
        }

        if (characterController.enabled)
        {
            characterController.Move(moveDirection * Time.deltaTime);
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            rb.MovePosition(transform.position+ moveDirection *Time.deltaTime);
        }
        

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        characterController.enabled = true;
        rb.Equals(false);
    }
    public void Exposion()
    {
        characterController.enabled = false;
        rb.Equals(true);
    }
}
