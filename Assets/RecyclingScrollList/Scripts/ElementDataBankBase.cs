using System;
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
        public Action<int> OnElementCountChanged {  get; set; }
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
        /// <summary>
        /// ����Ԫ��λ�ã�������defaultPosition�Ļ������޸ģ��������¼���
        /// </summary>
        /// <param name="element_idx"></param>
        /// <param name="defaultPosition"></param>
        /// <returns></returns>
        public virtual Vector3 CalcElementPosition(RecyclingScrollList.ScrollType scrollType, int element_idx, Vector3 defaultPosition) { return defaultPosition; }
    }
}
