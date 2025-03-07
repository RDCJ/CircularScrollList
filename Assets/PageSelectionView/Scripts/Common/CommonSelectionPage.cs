using UnityEngine;

public class CommonSelectionPage : ISelectionPage
{
    public CommonSelectionPage(GameObject pageGameObject)
    {
        PageGameObject = pageGameObject;
    }

    public GameObject PageGameObject { get; set; }

    public virtual void OnPageHide()
    {
        PageGameObject.SetActive(false);
    }

    public virtual void OnPageRefresh()
    {

    }

    public virtual void OnPageShow()
    {
        PageGameObject.SetActive(true);
    }
}
