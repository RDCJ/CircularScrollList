using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace SCL
{
    public class CircluarScrollList : MonoBehaviour
    {
        public GameObject element_prefab;
        public ScrollRect scrollRect;
        public RectTransform viewport_rtf;
        public RectTransform content_rft;
        public GridLayoutGroup content_grid;

        private RectTransform scroll_rtf;

        private RectTransform FirstElementRtf
        {
            get
            {
                if (content_rft.childCount == 0) return null;
                return content_rft.GetChild(0) as RectTransform;
            }
        }

        private RectTransform LastElementRtf
        {
            get
            {
                if (content_rft.childCount == 0) return null;
                return content_rft.GetChild(content_rft.childCount - 1) as RectTransform;
            }
        }

        private int element_count = 0;
        public int ElementCount => element_count;

        public void SetElementCount(int value)
        {
            if (value < 0)
                Debug.LogError("[CircluarScrollList:SetElementCount] value < 0");
            else if (value == element_count)
                Debug.Log("[CircluarScrollList:SetElementCount] value == this.element_count");
            else
            {
                Utils.ClearChild(content_rft);
                for (int i=0; i<value; i++)
                {
                    Instantiate(element_prefab, content_rft);
                }
                element_count = value;
            }
        }

        private void Awake()
        {
            scroll_rtf = scrollRect.GetComponent<RectTransform>();    
        }

        private void Start()
        {
            SetElementCount(5);
        }

        private void Update()
        {
            UpdateContentSize();

            /*            for (int i=0; i<content_rft.childCount; i++)
                        {
                            RectTransform element_rtf = content_rft.GetChild(i) as RectTransform;
                            Text txt = element_rtf.Find("Text").GetComponent<Text>();

                            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, element_rtf);
                            txt.text = bound.center.y.ToString() + " " + IsOutOfBound(element_rtf).ToString();
                        }*/
/*            RectTransform rtf_tmp = FirstElementRtf;
            if (IsOutOfBound(rtf_tmp))
            {
                Destroy(rtf_tmp.gameObject);
            }
            rtf_tmp = LastElementRtf;
            if (IsOutOfBound(rtf_tmp))
            {
                Destroy(rtf_tmp.gameObject);
            }*/
        }

        private bool IsOutOfBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, rtf);
            if (bound.min.y > 0) return true;
            if (bound.center.y + bound.size.y * 0.5 < -scroll_rtf.sizeDelta.y) return true;
            return false;
        }

        public void UpdateContentSize()
        {
            Vector2 content_size = content_rft.sizeDelta;
            content_size.y = content_grid.cellSize.y * element_count + content_grid.spacing.y * (element_count - 1);
            content_rft.sizeDelta = content_size;
        }
    }
}


