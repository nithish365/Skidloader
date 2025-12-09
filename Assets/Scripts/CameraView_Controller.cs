using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class PosRot
{
    public string Name;
    public Vector3 Rotation;   // rotation in Euler angles
    public Vector3 Position;   // LOCAL position
}

public class CameraView_Controller : MonoBehaviour
{
    public List<PosRot> m_posRot = new List<PosRot>();

    public Transform targetObject;   // Skid loader

    public float smoothSpeed = 2f;

    private Quaternion targetRotation;
    private Vector3 targetLocalPosition;

    private bool isTransitioning = false;

    void Start()
    {
       
        targetRotation = Quaternion.Euler(m_posRot[0].Rotation);
        targetLocalPosition = m_posRot[0].Position;

        isTransitioning = true;
    }

    void Update()
    {
        if (!isTransitioning) return;

        targetObject.localRotation = Quaternion.Slerp( targetObject.localRotation,targetRotation, Time.deltaTime * smoothSpeed);
        targetObject.localPosition = Vector3.Lerp( targetObject.localPosition, targetLocalPosition, Time.deltaTime * smoothSpeed);

        
        bool rotDone = Quaternion.Angle(targetObject.localRotation, targetRotation) < 0.2f;
        bool posDone = Vector3.Distance(targetObject.localPosition, targetLocalPosition) < 0.02f;

        if (rotDone && posDone)
            isTransitioning = false;
    }

    // VIEW BUTTON FUNCTION
    public void SetView(int viewIndex)
    {
      
        targetRotation = Quaternion.Euler(m_posRot[viewIndex].Rotation);
        targetLocalPosition = m_posRot[viewIndex].Position;

        isTransitioning = true;
    }
}
