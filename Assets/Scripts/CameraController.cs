using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float followSpeed = 4f;
    public Transform target;

    public float yOffset = -1f;

    [Header("Vertical Deadzone")]
    public float yDeadzone = 1.5f;

    private float targetCameraY;

    private void Start()
    {
        targetCameraY = target.position.y + yOffset;
    }

    private void LateUpdate()
    {
        float playerY = target.position.y + yOffset;

        // Player has moved far enough upward
        if (playerY > targetCameraY + yDeadzone)
        {
            targetCameraY = playerY - yDeadzone;
        }

        // Always allow camera to come back down
        if (playerY < targetCameraY)
        {
            targetCameraY = playerY;
        }

        Vector3 newPos = new Vector3(
            target.position.x,
            targetCameraY,
            -10f
        );

        transform.position = Vector3.Lerp(
            transform.position,
            newPos,
            followSpeed * Time.deltaTime
        );
    }
    public void SnapToTarget()
    {
        targetCameraY = target.position.y + yOffset;

        transform.position = new Vector3(
            target.position.x,
            targetCameraY,
            -10f
        );
    }
}