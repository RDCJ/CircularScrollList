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
        /// ����Ԫ��λ�ã�������defaultPosition�Ļ������޸ģ��������¼���
        /// </summary>
        /// <param name="element_idx"></param>
        /// <param name="defaultPosition"></param>
        /// <returns></returns>
        public virtual Vector3 CalcElementPosition(CircularScrollList.ScrollType scrollType, int element_idx, Vector3 defaultPosition) { return defaultPosition; }
    }
}

