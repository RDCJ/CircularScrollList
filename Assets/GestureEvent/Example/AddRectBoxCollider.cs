using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AddRectBoxCollider : MonoBehaviour
{
    private void Awake()
    {
        var rtf = GetComponent<RectTransform>();
        var box_collider = this.AddComponent<BoxCollider2D>();
        box_collider.size = rtf.rect.size;
        box_collider.offset = -(rtf.pivot - new Vector2(0.5f, 0.5f)) * rtf.rect.size;
        Destroy(this);
    }
}
