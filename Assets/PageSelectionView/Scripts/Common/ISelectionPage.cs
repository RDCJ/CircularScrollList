using UnityEngine;

public interface ISelectionPage
{
    public GameObject PageGameObject { get; set; }
    public void OnPageShow();
    public void OnPageRefresh();
    public void OnPageHide();
}
