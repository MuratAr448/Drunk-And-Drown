using UnityEngine;
using UnityEngine.InputSystem;

public class CameraNav : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float sprintMultiplier = 2.5f;
    public float lookSensitivity = 0.5f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            rotationY += mouseDelta.x * lookSensitivity;
            rotationX -= mouseDelta.y * lookSensitivity;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);
            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        }

        if (Keyboard.current != null)
        {
            Vector3 direction = Vector3.zero;

            if (Keyboard.current.wKey.isPressed) direction += transform.forward;
            if (Keyboard.current.sKey.isPressed) direction -= transform.forward;
            if (Keyboard.current.aKey.isPressed) direction -= transform.right;
            if (Keyboard.current.dKey.isPressed) direction += transform.right;

            float currentSpeed = moveSpeed;
            if (Keyboard.current.leftShiftKey.isPressed)
            {
                currentSpeed *= sprintMultiplier;
            }

            transform.position += direction * currentSpeed * Time.deltaTime;
        }
    }
}