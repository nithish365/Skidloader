using UnityEngine;

public class Follow_panel : MonoBehaviour
  {

    public Transform target;
    public float smoothTime = 0.08f;   // Lower = faster, Higher = smoother
    public float fixedZDistance = 0.6f;

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
        // 1️⃣ Forward direction without Y (no pitch)
        Vector3 camForward = target.forward;
        camForward.y = 0;
        camForward.Normalize();

        // 2️⃣ Desired position
        Vector3 targetPos = target.position + camForward * fixedZDistance;
        targetPos.y = transform.position.y;  // lock height

        // 3️⃣ SUPER SMOOTH follow (Shake removed)
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        // 4️⃣ Correct facing direction (no backside)
        Vector3 lookDirection = transform.position - target.position;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            Quaternion yRotation = Quaternion.LookRotation(lookDirection);

            Quaternion finalRotation = Quaternion.Euler(
                rotationX,                // public X
                yRotation.eulerAngles.y, // Y follows camera
                0f                       // no Z rotation
            );

            transform.rotation = Quaternion.Lerp(transform.rotation, finalRotation, 10f * Time.deltaTime);
        }
    }
}