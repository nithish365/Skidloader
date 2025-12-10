using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_Appear : MonoBehaviour
{
    public List<GameObject> panels = new List<GameObject>();
    public float fadeSpeed = 3f;
    public float delayTime = 0.5f;   // <-- your delay time here

    private bool isFading = false;
    private GameObject currentPanel;
    private bool fadeIn = false;
    private float alpha = 0f;
    private Renderer[] renderers;

    // ENABLE with delay + smooth fade
    public void EnablePanel(int index)
    {
        StartCoroutine(EnableWithDelay(index));
    }

    // DISABLE with delay + smooth fade
    public void DisablePanel(int index)
    {
        StartCoroutine(DisableWithDelay(index));
    }

    IEnumerator EnableWithDelay(int index)
    {
        yield return new WaitForSeconds(delayTime);   // <-- DELAY

        if (index < 0 || index >= panels.Count) yield break;

        currentPanel = panels[index];
        currentPanel.SetActive(true);

        renderers = currentPanel.GetComponentsInChildren<Renderer>();
        alpha = 0f;
        fadeIn = true;
        isFading = true;

        SetAlpha(0f);
    }

    IEnumerator DisableWithDelay(int index)
    {
        yield return new WaitForSeconds(delayTime);   // <-- DELAY

        if (index < 0 || index >= panels.Count) yield break;

        currentPanel = panels[index];
        renderers = currentPanel.GetComponentsInChildren<Renderer>();
        alpha = 1f;
        fadeIn = false;
        isFading = true;

        SetAlpha(1f);
    }

    void Update()
    {
        if (!isFading) return;

        if (fadeIn)
            alpha = Mathf.Lerp(alpha, 1f, Time.deltaTime * fadeSpeed);
        else
            alpha = Mathf.Lerp(alpha, 0f, Time.deltaTime * fadeSpeed);

        SetAlpha(alpha);

        if (fadeIn && alpha > 0.98f)
        {
            alpha = 1f;
            SetAlpha(alpha);
            isFading = false;
        }

        if (!fadeIn && alpha < 0.02f)
        {
            alpha = 0f;
            SetAlpha(alpha);
            currentPanel.SetActive(false);
            isFading = false;
        }
    }

    void SetAlpha(float a)
    {
        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_Color"))
            {
                Color c = r.material.color;
                c.a = a;
                r.material.color = c;

                r.material.SetFloat("_Surface", 1);
                r.material.renderQueue = 3000;
            }
        }
    }
}
