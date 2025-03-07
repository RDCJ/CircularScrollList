using UnityEngine;

public class PageBaseMono : MonoBehaviour, IPageBase
{
    public virtual void OnPageHide()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnPageLoad()
    {

    }

    public virtual void OnPageRefresh()
    {

    }

    public virtual void OnPageShow()
    {
        gameObject.SetActive(true);
        OnPageRefresh();
    }

    public virtual void OnRemove()
    {

    }
}
