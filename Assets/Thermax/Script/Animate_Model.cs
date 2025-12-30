using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Animate_Model : MonoBehaviour
{
    // ---------------- ROTATION ----------------
    [Header("Rotation")]
    public Transform rotateTarget;
    public Vector3 rotationAxis = Vector3.up;
    public int rotationCount = 1;
    public float delayBeforeRotate = 3f;
    public float rotationDurationPerLoop = 4f;

    // ---------------- POSITION ----------------
    [Header("Move To Position")]
    public Vector3 targetPosition;
    public float delayBeforeMove = 1f;
    public float moveDuration = 1.5f;

    // ---------------- TRANSPARENCY ----------------
    [Header("Transparency")]
    public Transform transparentParent;
    [Range(0f, 1f)]
    public float targetAlpha = 0f;
    public float fadeDuration = 1.2f;

    // ---------------- FLUID ----------------
    [Header("Fluid")]
    public float fluidStartDelay = 1.5f;

    // ---------------- INTERNAL ----------------
    private readonly List<Material> materials = new();
    private Fluid_Model fluidModel;

    // =================================================
    void Start()
    {
        CacheMaterialsOnce();

        // 🔑 Auto-find Fluid_Model (even if inactive)
        fluidModel = FindObjectOfType<Fluid_Model>(true);

        // 🔒 IMPORTANT: Disable ALL fluid renderers at start
        DisableFluidRenderers();

        StartCoroutine(MainFlow());
    }

    // =================================================
    void DisableFluidRenderers()
    {
        if (fluidModel == null) return;

        foreach (var e in fluidModel.pipeElements)
        {
            if (e.pipeParent)
                e.pipeParent.gameObject.SetActive(false);

            foreach (Renderer r in e.triggerRenderers)
                if (r) r.gameObject.SetActive(false);
        }
    }

    // =================================================
    IEnumerator MainFlow()
    {
        // ---- ROTATE ----
        yield return new WaitForSeconds(delayBeforeRotate);
        yield return RotateMultipleTimes();

        // ---- MOVE ----
        yield return new WaitForSeconds(delayBeforeMove);
        yield return MoveToTargetPosition();

        // ---- FADE ----
        SetMaterialsTransparentOnce();
        yield return FadeBaseAlpha(1f, targetAlpha);

        // ---- FLUID START ----
        if (fluidModel != null)
        {
            yield return new WaitForSeconds(fluidStartDelay);
            fluidModel.StartFluid();
        }
        else
        {
            Debug.LogWarning("Fluid_Model not found in scene.");
        }
    }

    // =================================================
    IEnumerator RotateMultipleTimes()
    {
        Quaternion startRotation = rotateTarget.localRotation;
        float totalAngle = 360f * Mathf.Max(1, rotationCount);
        float duration = rotationDurationPerLoop * Mathf.Max(1, rotationCount);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float angle = Mathf.Lerp(0f, totalAngle, t / duration);
            rotateTarget.localRotation =
                startRotation * Quaternion.AngleAxis(angle, rotationAxis.normalized);
            yield return null;
        }
    }

    // =================================================
    IEnumerator MoveToTargetPosition()
    {
        Vector3 startPos = rotateTarget.localPosition;
        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            rotateTarget.localPosition =
                Vector3.Lerp(startPos, targetPosition, t / moveDuration);
            yield return null;
        }

        rotateTarget.localPosition = targetPosition;
    }

    // =================================================
    void CacheMaterialsOnce()
    {
        materials.Clear();
        if (!transparentParent) return;

        MeshRenderer[] renderers =
            transparentParent.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer r in renderers)
            foreach (Material m in r.materials)
                if (!materials.Contains(m))
                    materials.Add(m);
    }

    // =================================================
    void SetMaterialsTransparentOnce()
    {
        foreach (Material mat in materials)
        {
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)RenderQueue.Transparent;
        }
    }

    // =================================================
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
}
