using UnityEngine;

public class Lazy_Follow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, 0.5f);

    [Header("Movement")]
    public float followSpeed = 6f;

    [Header("Angle Delay System")]
    public float allowedAngle = 20f;
    public float delaySeconds = 5f;

    float timer = 0f;
    bool waiting = false;

    Vector3 delayedPosition;
    Quaternion delayedRotation;

    void Start()
    {
        if (target == null)
            target = Camera.main.transform;

        delayedPosition = transform.position;
        delayedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        float angle = Vector3.Angle(transform.forward, target.forward);

        // Calculate the target position (with locked Y)
        Vector3 newPos = target.position + target.TransformVector(offset);
        newPos.y = transform.position.y;

        Quaternion newRot = Quaternion.Euler(0f, target.eulerAngles.y, 0f);

        // ------ CAMERA IS WITHIN ALLOWED ANGLE ------
        if (angle < allowedAngle)
        {
            // Reset timer but DO NOT update delayedPosition
            waiting = false;
            timer = 0f;

            // Just smoothly go to the last saved delayedPosition
            transform.position = Vector3.Lerp(transform.position, delayedPosition, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, delayedRotation, Time.deltaTime * followSpeed);
            return;
        }

        // ------ CAMERA OUTSIDE ALLOWED ANGLE ------
        if (!waiting)
        {
            waiting = true;
            timer = 0f;
        }

        timer += Time.deltaTime;

        if (timer >= delaySeconds)
        {
            // After delay, update delayed position
            delayedPosition = newPos;
            delayedRotation = newRot;

            waiting = false; // reset waiting so delay is not repeated
        }

        // Smooth move to delayed target
        transform.position = Vector3.Lerp(transform.position, delayedPosition, Time.deltaTime * followSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, delayedRotation, Time.deltaTime * followSpeed);
    }
}
