using System;
using System.Collections.Generic;
using UnityEngine;

namespace RSL
{
    public abstract class ElementDataBankBase<ElementDataType>: IElementDataBank
    {
        public ElementDataBankBase(bool overrideCalcElementPosition=false)
        {
            OverrideCalcElementPosition = overrideCalcElementPosition;
        }

        public bool OverrideCalcElementPosition {  get; set; }
        public Action DataUpdateEvent {  get; set; }
        public Action<int> DataDeleteEvent { get; set; }
        public Action<int> DataAddEvent { get; set; }
        public abstract int ElementCount { get; }
        public void ApplyElementData(RectTransform rtf, int element_idx) 
        {
            ElementDataType data = GetElementData(element_idx);
            if (data != null)
            {
                var elementUI = rtf.GetComponent<ListElementUIBase<ElementDataType>>();
                elementUI.OnApplyElementData(element_idx, data);
            }
            else
            {
                Debug.LogError($"[ElementDataBankBase:ApplyElementData] Can not get data for element: {element_idx}");
                return;
            }
        }
        public abstract ElementDataType GetElementData(int element_idx);

        public virtual void RemoveData(ElementDataType data, params object[] args) { }

        public virtual void AddData(ElementDataType data, params object[] args) { }
        /// <summary>
        /// 计算元素位置，可以在defaultPosition的基础上修改，或者重新计算
        /// </summary>
        /// <param name="element_idx"></param>
        /// <param name="defaultPosition"></param>
        /// <returns></returns>
        public virtual Vector3 CalcElementPosition(RecyclingScrollList.ScrollType scrollType, int element_idx, Vector3 defaultPosition) { return defaultPosition; }
    }
}

