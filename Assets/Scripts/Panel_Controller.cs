using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MainGroup
{
    public GameObject mainPanelRoot;
    public List<GameObject> animatedPanels;
    public List<GameObject> mainButtons;
    public List<ChildEntry> childEntries;
}

[System.Serializable]
public class ChildEntry
{
    public int childID;
    public GameObject childEntryPanel;
    public List<SubPanelList> subLists;
}

[System.Serializable]
public class SubPanelList
{
    public List<GameObject> subPanels;
}

public class Panel_Controller : MonoBehaviour
{
    public List<MainGroup> mainGroups;

    int currentMainGroup = 0, currentChildID = -1;
    bool clickLocked = false;

    [SerializeField] float scaleSpeed = 6f;
    [SerializeField] float appearDelay = 0.15f;   // NEW DELAY TIME

    Dictionary<GameObject, Coroutine> running = new Dictionary<GameObject, Coroutine>();


    //===========================================================
    public void SelectMainButton(int buttonIndex)
    {
        if (!IsReady()) return;

        var g = mainGroups[currentMainGroup];

        if (currentChildID != -1)
        {
            CloseChild(g);
            return;
        }

        ScaleList(g.animatedPanels, true);
        SetActiveList(g.mainButtons, true);
        HideAllChildren(g);

        var target = g.childEntries.Find(x => x.childID == buttonIndex);
        if (target != null) ShowChild(target);

        currentChildID = -1;
    }


    //===========================================================
    public void SelectChildEntry(int id)
    {
        if (!IsReady()) return;

        var g = mainGroups[currentMainGroup];

        if (currentChildID == id)
        {
            CloseChild(g);
            return;
        }

        ScaleList(g.animatedPanels, false);
        HideAllChildren(g);

        var found = g.childEntries.Find(x => x.childID == id);
        if (found != null) ShowChild(found);

        currentChildID = id;
    }


    //===========================================================
    void CloseChild(MainGroup g)
    {
        HideAllChildren(g);
        ScaleList(g.animatedPanels, true);
        SetActiveList(g.mainButtons, true);
        currentChildID = -1;
    }

    void HideAllChildren(MainGroup g)
    {
        foreach (var c in g.childEntries) HideChild(c);
    }


    //===========================================================
    void ShowChild(ChildEntry c)
    {
        if (c == null) return;

        StartScale(c.childEntryPanel, true);
        foreach (var s in c.subLists)
            ScaleList(s.subPanels, true);
    }

    void HideChild(ChildEntry c)
    {
        StartScale(c.childEntryPanel, false);
        foreach (var s in c.subLists)
            ScaleList(s.subPanels, false);
    }


    //===========================================================
    void ScaleList(List<GameObject> list, bool show)
    {
        foreach (var o in list)
            StartScale(o, show);
    }

    void SetActiveList(List<GameObject> list, bool state)
    {
        foreach (var o in list)
            if (o) o.SetActive(state);
    }


    //===========================================================
    void StartScale(GameObject obj, bool show)
    {
        if (!obj) return;

        if (running.TryGetValue(obj, out Coroutine c))
            StopCoroutine(c);

        running[obj] = StartCoroutine(Anim(obj, show));
    }


    //===========================================================
    IEnumerator Anim(GameObject obj, bool show)
    {
        // --- NEW DELAY BEFORE APPEARING ---
        if (show)
        {
            yield return new WaitForSeconds(appearDelay);
            obj.SetActive(true);
        }

        Vector3 start = obj.transform.localScale;
        Vector3 end = show ? Vector3.one : Vector3.zero;
        Vector3 fixedPos = obj.transform.localPosition;

        float dist = Vector3.Distance(start, end);
        float dur = Mathf.Max(0.05f, dist / scaleSpeed);

        float t = 0;

        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(0f, 1f, t / dur);

            obj.transform.localScale = Vector3.Lerp(start, end, a);
            obj.transform.localPosition = fixedPos;

            yield return null;
        }

        obj.transform.localScale = end;
        obj.transform.localPosition = fixedPos;

        if (!show)
            obj.SetActive(false);

        running.Remove(obj);
    }


    //===========================================================
    bool IsReady()
    {
        if (clickLocked) return false;
        StartCoroutine(Cooldown());
        return true;
    }

    IEnumerator Cooldown()
    {
        clickLocked = true;
        yield return new WaitForSeconds(0.15f);
        clickLocked = false;
    }
}
