using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region DATA
[System.Serializable]
public class PipeElement
{
    public Transform pipeParent;
    public List<Renderer> triggerRenderers = new();
    public List<float> triggerClipValues = new();
}
#endregion

public class Fluid_Model : MonoBehaviour
{
    public float pipeStringDuration = 1.5f;
    public List<PipeElement> pipeElements = new();

    // 🔒 LOCKS
    private HashSet<PipeElement> animatedPipes = new();
    private HashSet<Renderer> animatedPipeRenderers = new();
    private HashSet<Renderer> animatedTriggerRenderers = new();

    // 🔑 Renderer → Pipe lookup
    private Dictionary<Renderer, PipeElement> rendererToPipe =
        new Dictionary<Renderer, PipeElement>();

    // =================================================
    void Awake()
    {
        // Build lookup once
        foreach (PipeElement e in pipeElements)
        {
            if (!e.pipeParent) continue;

            Renderer[] rs =
                e.pipeParent.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in rs)
            {
                if (!rendererToPipe.ContainsKey(r))
                    rendererToPipe.Add(r, e);
            }
        }
    }

    // =================================================
    // 🔥 CALLED FROM Animate_Model
    public void StartFluid()
    {
        StopAllCoroutines();

        animatedPipes.Clear();
        animatedPipeRenderers.Clear();
        animatedTriggerRenderers.Clear();

        // Disable all at start
        foreach (PipeElement e in pipeElements)
        {
            if (e.pipeParent)
                e.pipeParent.gameObject.SetActive(false);

            foreach (Renderer r in e.triggerRenderers)
                if (r) r.gameObject.SetActive(false);
        }

        // Start first pipe
        if (pipeElements.Count > 0)
            StartPipe(pipeElements[0]);
    }

    // =================================================
    void StartPipe(PipeElement e)
    {
        if (e == null || animatedPipes.Contains(e))
            return;

        animatedPipes.Add(e);
        e.pipeParent.gameObject.SetActive(true);
        StartCoroutine(AnimatePipe(e));
    }

    // =================================================
    IEnumerator AnimatePipe(PipeElement element)
    {
        Renderer[] rs =
            element.pipeParent.GetComponentsInChildren<Renderer>(true);

        List<Material> mats = new();

        foreach (Renderer r in rs)
        {
            if (animatedPipeRenderers.Contains(r)) continue;

            Material m = new Material(r.material);
            r.material = m;

            // ✅ IMPORTANT FIX: NOT EXACTLY 1
            m.SetFloat("_Clip", 0.999f);

            mats.Add(m);
            animatedPipeRenderers.Add(r);
        }

        int triggerCount = Mathf.Min(
            element.triggerRenderers.Count,
            element.triggerClipValues.Count
        );

        bool[] fired = new bool[triggerCount];

        float clip = 0.999f;
        float prevClip = clip;

        while (clip > 0f)
        {
            prevClip = clip;

            // Smooth, visible animation from frame 1
            clip -= Time.deltaTime / pipeStringDuration;
            clip = Mathf.Clamp01(clip);

            foreach (Material m in mats)
                m.SetFloat("_Clip", clip);

            // 🔥 TRIGGERS
            for (int i = 0; i < triggerCount; i++)
            {
                if (fired[i]) continue;

                float triggerValue = element.triggerClipValues[i];

                if (prevClip > triggerValue && clip <= triggerValue)
                {
                    fired[i] = true;
                    Renderer tr = element.triggerRenderers[i];

                    if (tr)
                    {
                        tr.gameObject.SetActive(true);
                        StartCoroutine(AnimateTriggerRendererOnce(tr));

                        PipeElement next = FindPipeFromRenderer(tr);
                        if (next != null)
                            StartPipe(next);
                    }
                }
            }

            yield return null;
        }

        foreach (Material m in mats)
            m.SetFloat("_Clip", 0f);
    }

    // =================================================
    PipeElement FindPipeFromRenderer(Renderer r)
    {
        if (rendererToPipe.TryGetValue(r, out PipeElement direct))
            return direct;

        Transform t = r.transform.parent;
        while (t != null)
        {
            foreach (PipeElement p in pipeElements)
            {
                if (p.pipeParent == t)
                    return p;
            }
            t = t.parent;
        }
        return null;
    }

    // =================================================
    IEnumerator AnimateTriggerRendererOnce(Renderer r)
    {
        if (animatedTriggerRenderers.Contains(r))
            yield break;

        animatedTriggerRenderers.Add(r);

        Material m = new Material(r.material);
        r.material = m;

        float clip = 0.999f;
        while (clip > 0f)
        {
            clip -= Time.deltaTime / pipeStringDuration;
            clip = Mathf.Clamp01(clip);
            m.SetFloat("_Clip", clip);
            yield return null;
        }

        m.SetFloat("_Clip", 0f);
    }
}
