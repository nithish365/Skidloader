using UnityEngine;

public class ArmLifter_Controller : MonoBehaviour


{ 
    [Header("Target Object")]
    public Transform target;

    [Header("Rotation Targets (Local Euler Angles)")]
    public Vector3 rotationUp;     // Target rotation when clicking UP
    public Vector3 rotationDown;   // Target rotation when clicking DOWN

    [Header("Smooth Settings")]
    public float smoothSpeed = 3f; // Higher = faster

    private Quaternion targetRotation;

    void Start()
    {
        if (target != null)
            targetRotation = target.localRotation;
    }

    void Update()
    {
        if (target != null)
        {
            target.localRotation = Quaternion.Lerp(target.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
    }

    // BUTTON FUNCTIONS
    public void RotateUp()
    {
        targetRotation = Quaternion.Euler(rotationUp);
    }

    public void RotateDown()
    {
        targetRotation = Quaternion.Euler(rotationDown);
    }
}
