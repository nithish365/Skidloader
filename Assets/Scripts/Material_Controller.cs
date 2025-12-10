using UnityEngine;

public class Material_Controller : MonoBehaviour
{
    [Header("Target Material")]
    public Material targetMaterial;

    [Header("Colors (index-based)")]
    public Color[] colors;

    // XR Simple Interactable will call this and pass an index
    public void ChangeColorByIndex(int index)
    {
        if (index < 0 || index >= colors.Length)
        {
            Debug.LogError("Index out of range!");
            return;
        }

        targetMaterial.color = colors[index];
    }
}
