using UnityEngine;

[System.Serializable]
public class ObjMaskedGroup
{
    public string name;            // string
    public GameObject[] ObjMasked; // Mesh_a, Mesh_b, Mesh_c
}

[System.Serializable]
public class ObjectMainGroup
{
    public ObjMaskedGroup[] ObjMaskedStrings; // inner array
}

public class MaskObject : MonoBehaviour
{
    [Header("Main Array")]
    public ObjectMainGroup[] ObjectMainArray;

    void Start()
    {
        for (int i = 0; i < ObjectMainArray.Length; i++)
        {
            for (int j = 0; j < ObjectMainArray[i].ObjMaskedStrings.Length; j++)
            {
                GameObject[] objs = ObjectMainArray[i].ObjMaskedStrings[j].ObjMasked;

                for (int k = 0; k < objs.Length; k++)
                {
                    if (objs[k] == null) continue;

                    objs[k].GetComponent<MeshRenderer>()
                           .material.renderQueue = 3002;
                }
            }
        }

        Destroy(this);
    }
}
