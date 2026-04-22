using UnityEngine;

public class SmartHUDFollow : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody; // XR Origin (the thing that moves in the world)
    public Transform playerHead; // Main Camera

    [Header("Position Settings")]
    public float distance = 1.2f;
    public float heightOffset = -0.3f; // Below eye level

    [Header("Follow Behavior")]
    public FollowMode followMode = FollowMode.BodyWithHeadCatchup;
    public float headRotationThreshold = 45f; // Degrees before HUD repositions
    public float movementSmoothSpeed = 5f;
    public float rotationSmoothSpeed = 5f;

    [Header("Rotation Snapping (Optional)")]
    public bool snapToAngles = false;
    public float snapAngle = 45f; // HUD only appears at 0°, 45°, 90°, etc.

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float currentSnapAngle = 0f;

    public enum FollowMode
    {
        AlwaysFollowHead,           // Moves with every head movement
        BodyWithHeadCatchup,        // Follows body, but moves if head turns too far
        BodyOnly                    // Only follows body rotation, ignores head completely
    }

    void Start()
    {
        // Auto-find references if not assigned
        if (playerHead == null)
            playerHead = Camera.main.transform;

        if (playerBody == null)
        {
            playerBody = playerHead.parent.parent; // Main Camera -> Camera Offset -> XR Origin
        }
    }

    void LateUpdate()
    {
        if (playerHead == null || playerBody == null) return;

        CalculateTargetTransform();

        // Smoothly move to target
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSmoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
    }

    void CalculateTargetTransform()
    {
        Transform referenceTransform = playerBody;

        switch (followMode)
        {
            case FollowMode.AlwaysFollowHead:
                referenceTransform = playerHead;
                break;

            case FollowMode.BodyWithHeadCatchup:
                // Check if head has turned significantly from body
                float headBodyAngleDiff = Vector3.Angle(
                    Vector3.ProjectOnPlane(playerHead.forward, Vector3.up),
                    Vector3.ProjectOnPlane(playerBody.forward, Vector3.up)
                );

                // If head turned too far, follow the head instead
                if (headBodyAngleDiff > headRotationThreshold)
                {
                    referenceTransform = playerHead;
                }
                else
                {
                    referenceTransform = playerBody;
                }
                break;

            case FollowMode.BodyOnly:
                referenceTransform = playerBody;
                break;
        }

        // Calculate forward direction (flattened to horizontal)
        Vector3 forward = referenceTransform.forward;
        forward.y = 0; // Keep on horizontal plane
        forward.Normalize();

        // Apply angle snapping if enabled
        if (snapToAngles && forward.magnitude > 0.001f)
        {
            float currentAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(currentAngle / snapAngle) * snapAngle;

            // Only update if we've crossed a snap threshold
            if (Mathf.Abs(snappedAngle - currentSnapAngle) > snapAngle * 0.9f)
            {
                currentSnapAngle = snappedAngle;
            }

            forward = new Vector3(
                Mathf.Sin(currentSnapAngle * Mathf.Deg2Rad),
                0,
                Mathf.Cos(currentSnapAngle * Mathf.Deg2Rad)
            );
        }

        // Calculate target position (moves with player)
        targetPosition = playerHead.position
            + forward * distance
            + Vector3.up * heightOffset;

        // Calculate target rotation (always faces player, kept level)
        if (forward.magnitude > 0.001f)
        {
            targetRotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }

    public void RecenterHUD()
    {
        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }
}