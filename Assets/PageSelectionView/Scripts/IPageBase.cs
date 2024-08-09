using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPageBase
{
    public void OnPageLoad();
    public void OnPageShow();
    public void OnPageRefresh();
    public void OnPageHide();
    public void OnRemove();
}
