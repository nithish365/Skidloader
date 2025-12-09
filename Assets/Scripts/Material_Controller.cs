using UnityEngine;

public class Material_Controller : MonoBehaviour
{

    [Header("Color Objects (each object works like a button)")]
    public GameObject[] colorButton;   // Previously buttons

    [Header("Target Material (shared across multiple objects)")]
    public Material targetMaterial;

    [Header("Colors (match each object's order)")]
    public Color[] colors;

    void Start()
    {
        if (targetMaterial == null || colorButton.Length == 0 || colors.Length == 0)
        {
            Debug.LogError("Please assign the Material, Objects, and Colors!");
            return;
        }

        if (colorButton.Length != colors.Length)
        {
            Debug.LogWarning("Number of objects and colors do not match!");
        }
    }

    // This will be called when object is clicked/touched/triggered
    public void ChangeColorByObject(GameObject clickedObject)
    {
        for (int i = 0; i < colorButton.Length; i++)
        {
            if (clickedObject == colorButton[i])
            {
                targetMaterial.color = colors[i];
                break;
            }
        }
    }
}
