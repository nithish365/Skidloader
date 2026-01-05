using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Animate_Model : MonoBehaviour
{
    // ================= INITIAL ROTATION =================
    [Header("Initial Rotation")]
    public Transform rotateTarget;
    public Vector3 rotationAxis = Vector3.up;
    public int rotationCount = 1;
    public float delayBeforeRotate = 1f;
    public float rotationDurationPerLoop = 4f;

    // ================= AFTER SEQUENTIAL ROTATION =================
    [Header("Delays After Sequential")]
    public float delayAfterInitialRotation = 2f;   // before sequential
    public float delayAfterSequential = 2f;         // before second rotation
    public float delayAfterSecondRotation = 1.5f;   // before position move

    // ================= SECOND ROTATION =================
    [Header("Second Rotation (To Specific Orientation)")]
    public Vector3 secondRotationEuler;  // local euler angles
    public float secondRotationDuration = 2f;

    // ================= POSITION =================
    [Header("Move To Position")]
    public Vector3 targetPosition;
    public float moveDuration = 1.5f;

    // ================= TRANSPARENCY =================
    [Header("Transparency")]
    public Transform transparentParent;
    [Range(0f, 1f)] public float targetAlpha = 0f;
    public float fadeDuration = 1.2f;

    // ================= SEQUENTIAL =================
    [Header("Sequential Animation")]
    public SequentialRotation sequentialRotation;

    // ================= FLUID =================
    [Header("Fluid")]
    public float fluidStartDelay = 1f;

    // ================= INTERNAL =================
    readonly List<Material> materials = new();
    Fluid_Model fluidModel;

    // =================================================
    void Start()
    {
        CacheMaterialsOnce();
        fluidModel = FindObjectOfType<Fluid_Model>(true);
        DisableFluidRenderers();

        StartCoroutine(MainFlow());
    }

    // =================================================
    IEnumerator MainFlow()
    {
        // 1️⃣ INITIAL ROTATION
        yield return new WaitForSeconds(delayBeforeRotate);
        yield return RotateMultipleTimes();

        // 2️⃣ DELAY → SEQUENTIAL
        yield return new WaitForSeconds(delayAfterInitialRotation);

        // 3️⃣ SEQUENTIAL (WAIT UNTIL FINISHED)
        if (sequentialRotation != null)
            yield return StartCoroutine(sequentialRotation.PlaySequence());

        // 4️⃣ DELAY → SECOND ROTATION
        yield return new WaitForSeconds(delayAfterSequential);

        // 5️⃣ SECOND ROTATION
        yield return RotateToSpecificRotation();

        // 6️⃣ DELAY → POSITION
        yield return new WaitForSeconds(delayAfterSecondRotation);

        // 7️⃣ MOVE TO POSITION
        yield return MoveToTargetPosition();

        // 8️⃣ FADE ALPHA
        SetMaterialsTransparentOnce();
        yield return FadeBaseAlpha(1f, targetAlpha);

        // 9️⃣ FLUID
        if (fluidModel != null)
        {
            yield return new WaitForSeconds(fluidStartDelay);
            fluidModel.StartFluid();
        }
    }

    // ================= ROTATIONS =================
    IEnumerator RotateMultipleTimes()
    {
        Quaternion start = rotateTarget.localRotation;
        float totalAngle = 360f * rotationCount;
        float duration = rotationDurationPerLoop * rotationCount;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float angle = Mathf.Lerp(0, totalAngle, t / duration);
            rotateTarget.localRotation =
                start * Quaternion.AngleAxis(angle, rotationAxis);
            yield return null;
        }
    }

    IEnumerator RotateToSpecificRotation()
    {
        Quaternion start = rotateTarget.localRotation;
        Quaternion target = Quaternion.Euler(secondRotationEuler);

        float t = 0f;
        while (t < secondRotationDuration)
        {
            t += Time.deltaTime;
            rotateTarget.localRotation =
                Quaternion.Slerp(start, target, t / secondRotationDuration);
            yield return null;
        }

        rotateTarget.localRotation = target;
    }

    // ================= POSITION =================
    IEnumerator MoveToTargetPosition()
    {
        Vector3 start = rotateTarget.localPosition;
        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            rotateTarget.localPosition =
                Vector3.Lerp(start, targetPosition, t / moveDuration);
            yield return null;
        }
    }

    // ================= TRANSPARENCY =================
    void CacheMaterialsOnce()
    {
        materials.Clear();
        if (!transparentParent) return;

        foreach (MeshRenderer r in transparentParent.GetComponentsInChildren<MeshRenderer>(true))
            foreach (Material m in r.materials)
                if (!materials.Contains(m))
                    materials.Add(m);
    }

    void SetMaterialsTransparentOnce()
    {
        foreach (Material mat in materials)
        {
            mat.SetFloat("_Surface", 1);
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)RenderQueue.Transparent;
        }
    }

    IEnumerator FadeBaseAlpha(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);

            foreach (Material m in materials)
            {
                Color c = m.GetColor("_BaseColor");
                c.a = a;
                m.SetColor("_BaseColor", c);
            }
            yield return null;
        }
    }

    // ================= FLUID =================
    void DisableFluidRenderers()
    {
        if (!fluidModel) return;

        foreach (var e in fluidModel.pipeElements)
        {
            if (e.pipeParent)
                e.pipeParent.gameObject.SetActive(false);

            foreach (Renderer r in e.triggerRenderers)
                if (r) r.gameObject.SetActive(false);
        }
    }
}
