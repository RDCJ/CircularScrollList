using RSL;
using System;
using UnityEngine;

public interface IElementDataBank
{
    public int ElementCount { get; }
    public bool OverrideCalcElementPosition { get; set; }
    public Action DataUpdateEvent { get; set; }

    public Action<int> DataDeleteEvent { get; set; }
    public Action<int> DataAddEvent { get; set; }
    public void ApplyElementData(RectTransform rtf, int element_idx);
    public Vector3 CalcElementPosition(RecyclingScrollList.ScrollType scrollType, int element_idx, Vector3 defaultPosition);
}
