using UnityEngine;

namespace RSL
{
    public abstract class ListElementUIBase<ElementDataType> : MonoBehaviour
    {
        public virtual void OnApplyElementData(int element_idx, ElementDataType data) { }
    }
}