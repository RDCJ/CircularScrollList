using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageSelectionViewMono : MonoBehaviour
{
    public RectTransform PageSwitchesRtf;
    public RectTransform PagesRtf;
    public GameObject SwitchPrefab;

    public int PageCount => PageToSwitchDic.Count;
    private ToggleGroup toggleGroup;
    private Dictionary<PageBaseMono, Toggle> PageToSwitchDic = new Dictionary<PageBaseMono, Toggle>();
    private PageBaseMono CurrentPage = null;

    private void Awake()
    {
        toggleGroup = PageSwitchesRtf.GetComponent<ToggleGroup>();
    }

    public void AddPage(PageBaseMono new_page)
    {
        if (new_page != null)
        {
            new_page.transform.SetParent(PagesRtf);
            Toggle new_toggle = Instantiate(SwitchPrefab, PageSwitchesRtf).GetComponent<Toggle>();
            new_toggle.group = toggleGroup;
            PageToSwitchDic.Add(new_page, new_toggle);
            new_toggle.onValueChanged.AddListener((value) =>
            {
                SwitchToPage(new_page);
            });
            new_page.OnPageLoad();
        }
    }

    public void SwitchToPage(PageBaseMono page)
    {
        if (!PageToSwitchDic.ContainsKey(page)) return;
        if (CurrentPage == page) return;
        if (CurrentPage != null) CurrentPage.OnPageHide();
        page.OnPageShow();
        CurrentPage = page;
    }

    public void SwitchToPage(int page_idx)
    {
        PageBaseMono page = PagesRtf.GetChild(page_idx).GetComponent<PageBaseMono>();
        SwitchToPage(page);
    }

    public void DeletePage(PageBaseMono page)
    {
        Toggle toggle = PageToSwitchDic[page];
        PageToSwitchDic.Remove(page);
        DestroyImmediate(toggle);
        page.OnRemove();
        DestroyImmediate(toggle);
    }

    public void DeletePage(int page_idx)
    {
        PageBaseMono page = PagesRtf.GetChild(page_idx).GetComponent<PageBaseMono>();
        DeletePage(page);
    }
}
