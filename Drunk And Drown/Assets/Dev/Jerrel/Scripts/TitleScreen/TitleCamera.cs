using UnityEngine;

public class TitleCamera : MonoBehaviour
{
    [Header("Movement (Translation) Settings")]
    [SerializeField] private Vector2 maxPositionOffset = new Vector2(0.5f, 0.3f);
    [SerializeField] private float positionSmoothSpeed = 2f;

    [Header("Rotation Settings")]
    [SerializeField] private Vector2 maxRotationOffset = new Vector2(2f, 2f); // in degrees
    [SerializeField] private float rotationSmoothSpeed = 2f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        // Get normalized mouse position relative to the center of the screen [-1, 1]
        float mouseX = (Input.mousePosition.x - (Screen.width / 2f)) / (Screen.width / 2f);
        float mouseY = (Input.mousePosition.y - (Screen.height / 2f)) / (Screen.height / 2f);

        // Clamp in case mouse goes slightly outside the game window limits
        mouseX = Mathf.Clamp(mouseX, -1f, 1f);
        mouseY = Mathf.Clamp(mouseY, -1f, 1f);

        // Calculate target position based on translation offsets (relative to camera's local axes)
        Vector3 targetOffset = (transform.right * mouseX * maxPositionOffset.x) + (transform.up * mouseY * maxPositionOffset.y);
        Vector3 targetPosition = startPosition + targetOffset;

        // Calculate target rotation (yaw and pitch offsets)
        // Pitch (up/down) rotates around X-axis (influenced by vertical mouseY)
        // Yaw (left/right) rotates around Y-axis (influenced by horizontal mouseX)
        Quaternion targetRotation = startRotation * Quaternion.Euler(-mouseY * maxRotationOffset.y, mouseX * maxRotationOffset.x, 0f);

        // Smoothly interpolate position and rotation
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionSmoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
    }
}
