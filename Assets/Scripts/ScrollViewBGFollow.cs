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
                scroll_list.scrollRect.onValueChanged.AddListener((Vector2 value) => {
                    Rect rect = bg.uvRect;
                    rect.position = new Vector2(0, (scroll_list.scrollRect.normalizedPosition.y - (scroll_list.Reverse ? 0 : 1)) * (scroll_list.contentRtf.rect.height - scroll_list.scrollRtf.rect.height) / scroll_list.scrollRtf.rect.height);
                    bg.uvRect = rect;
                });
            }
            else
            {
                scroll_list.scrollRect.onValueChanged.AddListener((Vector2 value) => {
                    Rect rect = bg.uvRect;
                    rect.position = new Vector2((scroll_list.scrollRect.normalizedPosition.x - (scroll_list.Reverse ? 1 : 0)) * (scroll_list.contentRtf.rect.width - scroll_list.scrollRtf.rect.width) / scroll_list.scrollRtf.rect.width, 0);
                    bg.uvRect = rect;
                });
            }
        }
    }
}

