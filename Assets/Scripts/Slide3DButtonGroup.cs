using System.Collections;
using UnityEngine;

public class Slide3DButtonGroup : MonoBehaviour
{
    [Header("Panels")]
    public Transform[] panels;

    [Header("Shared Local Positions")]
    public Vector3 hiddenLocalPos;
    public Vector3 shownLocalPos;

    public float slideSpeed = 600f;

    [Header("Show Delay")]
    public float showDelay = 0.25f;   // delay BEFORE new panel comes

    int currentIndex = 0;

    void Start()
    {
        // Only first panel visible
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].localPosition =
                (i == 0) ? shownLocalPos : hiddenLocalPos;
        }
    }

    public void ShowPanel(int showIndex)
    {
        if (showIndex == currentIndex)
            return;

        StopAllCoroutines();

        // 1️⃣ Hide current panel immediately
        StartCoroutine(Slide(panels[currentIndex], hiddenLocalPos));

        // 2️⃣ After delay, show new panel
        StartCoroutine(ShowAfterDelay(showIndex));
    }

    IEnumerator ShowAfterDelay(int showIndex)
    {
        yield return new WaitForSeconds(showDelay);
        StartCoroutine(Slide(panels[showIndex], shownLocalPos));
        currentIndex = showIndex;
    }

    IEnumerator Slide(Transform panel, Vector3 target)
    {
        while (Vector3.Distance(panel.localPosition, target) > 0.01f)
        {
            panel.localPosition = Vector3.MoveTowards(
                panel.localPosition,
                target,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        panel.localPosition = target;
    }
}
