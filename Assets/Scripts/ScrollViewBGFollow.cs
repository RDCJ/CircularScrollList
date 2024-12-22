using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RSL
{
    /// <summary>
    /// 控制RawImage的UVRect, 使其跟随ScrollRect的滚动
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class ScrollViewBGFollow : MonoBehaviour
    {
        public RecyclingScrollList scroll_list;
        private RawImage bg;

        private void Awake()
        {
            bg = this.GetComponent<RawImage>();
            if (scroll_list == null) 
            {
                Debug.LogError("[ScrollViewBGFollow] this.scroll_list == null");
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            if (scroll_list.scrollType == RecyclingScrollList.ScrollType.Vertical)
            {
                scroll_list._ScrollRect.onValueChanged.AddListener((Vector2 value) => {
                    Rect rect = bg.uvRect;
                    rect.position = new Vector2(0, (scroll_list._ScrollRect.normalizedPosition.y - (scroll_list.Reverse ? 0 : 1)) * (scroll_list.ContentRtf.rect.height - scroll_list.ScrollRtf.rect.height) / scroll_list.ScrollRtf.rect.height);
                    bg.uvRect = rect;
                });
            }
            else
            {
                scroll_list._ScrollRect.onValueChanged.AddListener((Vector2 value) => {
                    Rect rect = bg.uvRect;
                    rect.position = new Vector2((scroll_list._ScrollRect.normalizedPosition.x - (scroll_list.Reverse ? 1 : 0)) * (scroll_list.ContentRtf.rect.width - scroll_list.ScrollRtf.rect.width) / scroll_list.ScrollRtf.rect.width, 0);
                    bg.uvRect = rect;
                });
            }
        }
    }
}

