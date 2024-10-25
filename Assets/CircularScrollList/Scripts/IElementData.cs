using System;
using UnityEngine;

namespace SCL
{
    public class IElementData
    {
    }

    public class IElementDataBank : MonoBehaviour
    {
        public bool OverrideCalcElementPosition;
        public Action<int> OnElementCountChanged = null;
        public virtual int ElementCount => 0;
        public virtual void ApplyElementData(RectTransform rtf, int element_idx) { }
        public virtual IElementData GetElementData(int element_idx) { return null; }
        /// <summary>
        /// 计算元素位置，可以在defaultPosition的基础上修改，或者重新计算
        /// </summary>
        /// <param name="element_idx"></param>
        /// <param name="defaultPosition"></param>
        /// <returns></returns>
        public virtual Vector3 CalcElementPosition(CircularScrollList.ScrollType scrollType, int element_idx, Vector3 defaultPosition) { return defaultPosition; }
    }
}

