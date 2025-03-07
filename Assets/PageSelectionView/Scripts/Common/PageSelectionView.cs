using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// π‹¿Ì«–“≥
/// </summary>
public class PageSelectionView
{
    private ToggleGroup toggleGroup;
    private Dictionary<ISelectionPage, Toggle> pageDic;
    public ISelectionPage CurrentPage { get; private set; }

    public PageSelectionView(Transform tf)
    {
        toggleGroup = tf.GetComponent<ToggleGroup>();
        pageDic = new Dictionary<ISelectionPage, Toggle>();
        CurrentPage = null;
    }

    public void AddPage(ISelectionPage page, Toggle ctrlToggle)
    {
        if (page != null && ctrlToggle != null)
        {
            ctrlToggle.group = toggleGroup;
            pageDic.Add(page, ctrlToggle);
            page.PageGameObject.SetActive(false);
            ctrlToggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                {
                    _SwitchToPage(page);
                }
            });
        }
    }

    private void _SwitchToPage(ISelectionPage page)
    {
        if (CurrentPage == page) return;
        if (CurrentPage != null) CurrentPage.OnPageHide();
        page.OnPageShow();
        CurrentPage = page;
    }

    public void SwitchToPage(ISelectionPage page)
    {
        if (pageDic.TryGetValue(page, out Toggle toggle))
        {
            toggle.isOn = true;
            _SwitchToPage(page);
            CurrentPage.OnPageRefresh();
        }
    }
}