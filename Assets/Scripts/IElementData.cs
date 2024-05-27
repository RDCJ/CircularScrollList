using UnityEngine;

public class IElementData 
{
}

public class IElementDataBank: MonoBehaviour
{
    public virtual void ApplyElementData(RectTransform rtf, int element_idx) { }
    public virtual IElementData GetElementData(int element_idx) { return null; }
}
