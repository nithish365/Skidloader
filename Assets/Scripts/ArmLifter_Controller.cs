using UnityEngine;

public class Arm_Controller : MonoBehaviour
{
    [Header("1️⃣ Rotation Target")]
    public Transform rotationTarget;
    public Vector3 rotationUp;
    public Vector3 rotationDown;

    [Header("2️⃣ Position Target")]
    public Transform positionTarget;
    public Vector3 positionUp;
    public Vector3 positionDown;

    [Header("3️⃣ Rotation + Scale Target")]
    public Transform rotScaleTarget;
    public Vector3 rotScaleRotationUp;
    public Vector3 rotScaleRotationDown;
    public Vector3 scaleUp = Vector3.one;
    public Vector3 scaleDown = Vector3.one;

    [Header("Smooth Settings")]
    public float smoothSpeed = 3f;

    // Internal targets
    Quaternion targetRotation;
    Vector3 targetPosition;

    Quaternion targetRotScaleRotation;
    Vector3 targetScale;

    void Start()
    {
        if (rotationTarget != null)
            targetRotation = rotationTarget.localRotation;

        if (positionTarget != null)
            targetPosition = positionTarget.localPosition;

        if (rotScaleTarget != null)
        {
            targetRotScaleRotation = rotScaleTarget.localRotation;
            targetScale = rotScaleTarget.localScale;
        }
    }

    void Update()
    {
        // Rotation only
        if (rotationTarget != null)
        {
            rotationTarget.localRotation = Quaternion.Lerp(
                rotationTarget.localRotation,
                targetRotation,
                Time.deltaTime * smoothSpeed
            );
        }

        // Position only
        if (positionTarget != null)
        {
            positionTarget.localPosition = Vector3.Lerp(
                positionTarget.localPosition,
                targetPosition,
                Time.deltaTime * smoothSpeed
            );
        }

        // Rotation + Scale
        if (rotScaleTarget != null)
        {
            rotScaleTarget.localRotation = Quaternion.Lerp(
                rotScaleTarget.localRotation,
                targetRotScaleRotation,
                Time.deltaTime * smoothSpeed
            );

            rotScaleTarget.localScale = Vector3.Lerp(
                rotScaleTarget.localScale,
                targetScale,
                Time.deltaTime * smoothSpeed
            );
        }
    }

    // XR Select Entered → UP
    public void Up()
    {
        if (rotationTarget != null)
            targetRotation = Quaternion.Euler(rotationUp);

        if (positionTarget != null)
            targetPosition = positionUp;

        if (rotScaleTarget != null)
        {
            targetRotScaleRotation = Quaternion.Euler(rotScaleRotationUp);
            targetScale = scaleUp;
        }
    }

    // XR Select Entered → DOWN
    public void Down()
    {
        if (rotationTarget != null)
            targetRotation = Quaternion.Euler(rotationDown);

        if (positionTarget != null)
            targetPosition = positionDown;

        if (rotScaleTarget != null)
        {
            targetRotScaleRotation = Quaternion.Euler(rotScaleRotationDown);
            targetScale = scaleDown;
        }
    }
}
