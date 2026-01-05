using System.Collections;
using UnityEngine;

public class CalloutAnimator : MonoBehaviour
{
    public Renderer[] parts;

    void OnEnable()
    {
        for (int i = 0; i < parts.Length; i++)
            parts[i].enabled = false;
    }
}