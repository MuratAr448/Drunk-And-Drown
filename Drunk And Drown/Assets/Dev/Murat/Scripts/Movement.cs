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

    private bool launch = false;
    private Vector3 moveDirection = Vector3.zero;
    private Rigidbody rb;
    private Vector3 explosionpos = Vector3.zero;
    private float power = 0;
    private float rotationX = 0;
    public CharacterController characterController;

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

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX;
        float curSpeedY;
        if (canMove)
        {
            if (isRunning)
            {
                curSpeedX = runSpeed;
                curSpeedY = runSpeed;
            }
            else
            {
                curSpeedX = walkSpeed;
                curSpeedY = walkSpeed;
            }
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

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        if (canMove)
        {
            characterController.enabled = true;
            characterController.Move(moveDirection * Time.deltaTime);
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
        }
        else if (launch)
        {
            //characterController.enabled = false;
            //rb.AddForce(force);
            Vector3 direction = transform.position - explosionpos;
            float distance = direction.magnitude;
            float force = 1f - (power / distance);
            characterController.Move(direction.normalized * force);
            launch = false;
        }

        

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
    public void Impact(Vector3 explosionPos,float Power)
    {
        power = Power;
        explosionpos = explosionPos;
        canMove = false;
        launch = true;
        StartCoroutine(ReturnMovement());
    }
    private IEnumerator ReturnMovement()
    {
        yield return new WaitForSeconds(0.2f);
        canMove = true;
    }
}
