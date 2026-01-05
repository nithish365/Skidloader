using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;

public class SequentialRotation : MonoBehaviour
{
    [Header("Common Rotation Target")]
    public Transform target;

    [Header("Rotation Steps")]
    public float[] rotationSteps;
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 5f;
    public float delayBetweenSteps = 0.5f;

    // ================= EMISSION =================
    [Header("Emission")]
    public Renderer[] emissionRenderers;
    public Color emissionColor = Color.yellow;
    public float emissionIntensity = 2f;
    public float emissionFadeInDuration = 0.6f;
    public float emissionHoldTime = 1.2f;
    public float emissionFadeOutDuration = 0.6f;

    // ================= CALLOUT =================
    [Header("Call Out")]
    public GameObject callOutPrefab;
    public Transform[] callOutSpawnPoints;
    public string[] callOutTexts;

    public float callOutFadeDuration = 0.6f;

    GameObject currentCallOutGO;

    void Start()
    {
        if (!target) target = transform;

        foreach (var r in emissionRenderers)
            DisableEmission(r);
    }

    // 🔑 Called from Animate_Model
    public IEnumerator PlaySequence()
    {
        int count = Mathf.Min(
            rotationSteps.Length,
            emissionRenderers.Length,
            callOutSpawnPoints.Length,
            callOutTexts.Length);

        for (int i = 0; i < count; i++)
        {
            yield return RotateStep(rotationSteps[i]);
            yield return EmissionWithCallOut(i);
            yield return new WaitForSeconds(delayBetweenSteps);
        }
    }

    // ================= ROTATION =================
    IEnumerator RotateStep(float targetAngle)
    {
        float currentAngle = Vector3.Dot(target.localEulerAngles, rotationAxis);

        while (Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle)) > 0.1f)
        {
            currentAngle = Mathf.LerpAngle(
                currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

            target.localRotation =
                Quaternion.AngleAxis(currentAngle, rotationAxis);

            yield return null;
        }
    }

    // ================= EMISSION + CALLOUT =================
    IEnumerator EmissionWithCallOut(int index)
    {
        Renderer emissionRenderer = emissionRenderers[index];
        Transform spawn = callOutSpawnPoints[index];

        // -------- Instantiate Callout --------
        currentCallOutGO = Instantiate(
            callOutPrefab,
            spawn.position,
            spawn.rotation,
            spawn);

        TMP_Text tmp =
            currentCallOutGO.GetComponentInChildren<TMP_Text>(true);

        if (tmp != null)
            tmp.text = callOutTexts[index];

        Renderer[] meshRenderers =
            currentCallOutGO.GetComponentsInChildren<Renderer>(true);

        TMP_Text[] tmpTexts =
            currentCallOutGO.GetComponentsInChildren<TMP_Text>(true);

        // 🔥 PREPARE TRANSPARENCY (CRITICAL)
        PrepareTransparent(meshRenderers);

        // Start invisible
        SetAlpha(meshRenderers, tmpTexts, 0f);

        Material emMat = emissionRenderer.material;
        emMat.EnableKeyword("_EMISSION");

        // ---------- FADE IN ----------
        float t = 0f;
        Color eStart = Color.black;
        Color eEnd = emissionColor * emissionIntensity;

        while (t < 1f)
        {
            t += Time.deltaTime / emissionFadeInDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            emMat.SetColor("_EmissionColor", Color.Lerp(eStart, eEnd, smooth));
            SetAlpha(meshRenderers, tmpTexts, smooth);

            yield return null;
        }

        // ---------- HOLD ----------
        yield return new WaitForSeconds(emissionHoldTime);

        // ---------- FADE OUT ----------
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / emissionFadeOutDuration;
            float smooth = Mathf.SmoothStep(1f, 0f, t);

            emMat.SetColor("_EmissionColor", Color.Lerp(eEnd, Color.black, 1f - smooth));
            SetAlpha(meshRenderers, tmpTexts, smooth);

            yield return null;
        }

        emMat.DisableKeyword("_EMISSION");

        Destroy(currentCallOutGO);
    }

    // ================= TRANSPARENCY HELPERS =================
    void PrepareTransparent(Renderer[] renderers)
    {
        foreach (Renderer r in renderers)
        {
            Material m = r.material;

            // URP / Standard safe transparency
            m.SetFloat("_Surface", 1);
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.renderQueue = (int)RenderQueue.Transparent;
        }
    }

    void SetAlpha(Renderer[] meshRenderers, TMP_Text[] texts, float alpha)
    {
        foreach (Renderer r in meshRenderers)
        {
            Color c = r.material.color;
            c.a = alpha;
            r.material.color = c;
        }

        foreach (TMP_Text t in texts)
        {
            Color c = t.color;
            c.a = alpha;
            t.color = c;
        }
    }

    // ================= EMISSION RESET =================
    void DisableEmission(Renderer r)
    {
        if (!r) return;
        r.material.SetColor("_EmissionColor", Color.black);
        r.material.DisableKeyword("_EMISSION");
    }
}
