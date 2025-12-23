using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class Animate_Model : MonoBehaviour
{// ---------------- ROTATION ----------------
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
    [Range(0f, 1f)] public float targetAlpha = 0f;
    public float fadeDuration = 1.2f;

    // ---------------- ANIMATION ----------------
    [Header("Animator")]
    public Animator animator;               // 👈 Assign Animator
    public string fadeEndTrigger = "FadeEnd"; // 👈 Trigger name

    // ---------------- INTERNAL ----------------
    private readonly List<Material> materials = new List<Material>();

    // =================================================

    void Start()
    {
        CacheMaterialsOnce();
        StartCoroutine(MainFlow());
    }

    // =================================================

    IEnumerator MainFlow()
    {
        yield return new WaitForSeconds(delayBeforeRotate);

        yield return RotateMultipleTimes();

        yield return new WaitForSeconds(delayBeforeMove);

        yield return MoveToTargetPosition();

        SetMaterialsTransparentOnce();
        yield return FadeBaseAlpha(1f, targetAlpha);

        TriggerAnimator();   // ✅ ANIMATION STARTS HERE
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
        MeshRenderer[] renderers =
            transparentParent.GetComponentsInChildren<MeshRenderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (!materials.Contains(mats[j]))
                    materials.Add(mats[j]);
            }
        }
    }

    // =================================================
    void SetMaterialsTransparentOnce()
    {
        for (int i = 0; i < materials.Count; i++)
        {
            Material mat = materials[i];
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

            for (int i = 0; i < materials.Count; i++)
            {
                Color c = materials[i].GetColor("_BaseColor");
                c.a = a;
                materials[i].SetColor("_BaseColor", c);
            }
            yield return null;
        }
    }

    // =================================================
    void TriggerAnimator()
    {
        if (animator == null) return;

        if (!string.IsNullOrEmpty(fadeEndTrigger))
            animator.SetTrigger(fadeEndTrigger);
    }
}

