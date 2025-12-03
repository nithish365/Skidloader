using UnityEngine;

public class Follow_panel : MonoBehaviour
{
    public Transform target;

    [Header("Follow Settings")]
    public float smoothTime = 0.1f;
    public float fixedZDistance = 0.45f;

    [Header("Vertical Offset")]
    public float heightOffset = 0f;   // <--- PUBLIC Y OFFSET

    [Header("Panel Rotation")]
    public float rotationX = 0f;

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        if (target == null)
            target = Camera.main.transform;
    }

    void LateUpdate()
    {
        // 1️⃣ Forward direction ignoring Y
        Vector3 camForward = target.forward;
        camForward.y = 0;
        camForward.Normalize();

        // 2️⃣ Target Position
        Vector3 targetPos = target.position + camForward * fixedZDistance;

        // 3️⃣ Follow camera Y + offset
        targetPos.y = target.position.y + heightOffset;  // follows up/down, adjustable

        // 4️⃣ Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        // 5️⃣ Rotation only on Y + your custom X
        Vector3 lookDirection = transform.position - target.position;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            Quaternion yRotation = Quaternion.LookRotation(lookDirection);

            Quaternion finalRotation = Quaternion.Euler(
                rotationX,
                yRotation.eulerAngles.y,
                0f
            );

            transform.rotation = Quaternion.Lerp(transform.rotation, finalRotation, 10f * Time.deltaTime);
        }
    }
}
