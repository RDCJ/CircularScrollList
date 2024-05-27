using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace SCL
{
    public class CircluarScrollList : MonoBehaviour
    {
        public Vector2 CellSize;
        public Vector2 Space;
        public GameObject element_prefab;
        public ScrollRect scrollRect;
        public RectTransform viewport_rtf;
        public RectTransform content_rft;
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

        private int head_idx;
        private int tail_idx;
        private int element_count = 0;
        public int ElementCount => element_count;
        private int ElementShowCount
        {
            get
            {
                return Mathf.Min(ElementCount, (int)(scroll_rtf.sizeDelta.y / (CellSize.y + Space.y) + 1));
            }
        }

        public RectTransform element_pool_rtf;
        private Stack<GameObject> element_pool;

        public IElementDataBank dataBank;

        public void SetElementCount(int value)
        {
            if (value < 0)
                Debug.LogError("[CircluarScrollList:SetElementCount] value < 0");
            else if (value == element_count)
                Debug.Log("[CircluarScrollList:SetElementCount] value == this.element_count");
            else
            {
                ReturnAllElement();
                element_count = value;
                for (int i=0; i< ElementShowCount; i++)
                {
                    GetNewElement(i);
                }
                head_idx = 0;
                tail_idx = ElementShowCount - 1;
            }
            UpdateGrid();
        }

        private void Awake()
        {
            scroll_rtf = scrollRect.GetComponent<RectTransform>();
            element_pool = new Stack<GameObject>();
        }

        private void Start()
        {
            element_pool_rtf.gameObject.SetActive(false);
            SetElementCount(5);
            scrollRect.onValueChanged.AddListener((Vector2 value) => {
                CheckDeleteHead();
                CheckDeleteTail();
                CheckCreateHead();
                CheckCreateTail();
            });
        }

        private void Update()
        {
            UpdateContentSize();

            for (int i = 0; i < content_rft.childCount; i++)
            {
                RectTransform element_rtf = content_rft.GetChild(i) as RectTransform;
                Text txt = element_rtf.Find("Text").GetComponent<Text>();

                Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, element_rtf);
                txt.text = bound.center.y.ToString() + " " + IsOutOfBound(element_rtf).ToString();
            }
        }

        private bool IsOutOfBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, rtf);
            if (bound.min.y > 0) return true;
            if (bound.center.y + bound.size.y * 0.5 < -scroll_rtf.sizeDelta.y) return true;
            return false;
        }

        private bool IsOutOfTopBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, rtf);
            if (bound.min.y > 0) return true;
            return false;
        }

        private bool IsOutOfBottomBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, rtf);
            if (bound.center.y + bound.size.y * 0.5 < -scroll_rtf.sizeDelta.y) return true;
            return false;
        }

        public void UpdateContentSize()
        {
            Vector2 content_size = content_rft.sizeDelta;
            content_size.y = CellSize.y * element_count + Space.y * (element_count - 1);
            content_rft.sizeDelta = content_size;
        }

        public void UpdateGrid()
        {
            Vector3 position = new Vector3(CellSize.x / 2, -CellSize.y/2, 0);
            for (int i=0; i<content_rft.childCount; i++)
            {
                var rtf_tmp = content_rft.GetChild(i) as RectTransform;
                rtf_tmp.sizeDelta = CellSize;
                rtf_tmp.localPosition = position;
                position.y -= Space.y + CellSize.y;
            }
        }

        public bool CheckDeleteHead()
        {
            if (head_idx > tail_idx) return false;
            RectTransform rtf_tmp = FirstElementRtf;
            if (rtf_tmp != null && IsOutOfTopBound(rtf_tmp))
            {
                Debug.Log($"Delete at head");
                ReturnElement(rtf_tmp);
                head_idx++;
                return true;
            }
            return false;
        }

        public bool CheckDeleteTail()
        {
            if (head_idx > tail_idx) return false;
            RectTransform rtf_tmp = LastElementRtf;
            if (rtf_tmp != null && IsOutOfBottomBound(rtf_tmp))
            {
                Debug.Log($"Delete at tail");
                ReturnElement(rtf_tmp);
                tail_idx--;
                return true;
            }
            return false;
        }

        public bool CheckCreateHead()
        {
            if (tail_idx - head_idx + 1 >= ElementShowCount) return false; 
            if (head_idx > 0)
            {
                RectTransform first_element = FirstElementRtf;
                if (first_element != null)
                {
                    Bounds first_bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, first_element);
                    if (first_bounds.max.y < -Space.y)
                    {
                        head_idx--;
                        Debug.Log("Create new at head");
                        RectTransform new_element = GetNewElement(head_idx);
                        new_element.SetAsFirstSibling();
                        new_element.localPosition = first_element.localPosition + new Vector3(0, CellSize.y + Space.y, 0);
                        new_element.sizeDelta = CellSize;
                        return true;
                    }
                }
                else if (content_rft.localPosition.y < content_rft.sizeDelta.y)
                {
                    head_idx--;
                    Debug.Log("Create new at head");
                    RectTransform new_element = GetNewElement(head_idx);
                    new_element.SetAsFirstSibling();
                    new_element.localPosition = new Vector3(0.5f * CellSize.x, -content_rft.sizeDelta.y + 0.5f * CellSize.y, 0);
                    new_element.sizeDelta = CellSize;
                    return true;
                }
            }
            return false;
        }

        public bool CheckCreateTail()
        {
            if (tail_idx - head_idx + 1 >= ElementShowCount) return false;
            if (tail_idx < element_count - 1)
            {
                RectTransform last_element = LastElementRtf;
                if (last_element != null)
                {
                    Bounds last_bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, last_element);
                    if (scroll_rtf.sizeDelta.y + last_bounds.min.y > Space.y)
                    {
                        tail_idx++;
                        Debug.Log("Create new at tail");
                        RectTransform new_element = GetNewElement(tail_idx);
                        new_element.SetAsLastSibling();
                        new_element.localPosition = last_element.localPosition - new Vector3(0, CellSize.y + Space.y, 0);
                        new_element.sizeDelta = CellSize;
                        return true;
                    }
                }
                else if (content_rft.localPosition.y > -scroll_rtf.sizeDelta.y)
                {
                    tail_idx++;
                    Debug.Log("Create new at tail");
                    RectTransform new_element = GetNewElement(tail_idx);
                    new_element.SetAsLastSibling();
                    new_element.localPosition = new Vector3(0.5f * CellSize.x, -0.5f * CellSize.y, 0);
                    new_element.sizeDelta = CellSize;
                    return true;
                }
            }
            return false;
        }

        private void ReturnElement(Transform element)
        {
            element_pool.Push(element.gameObject);
            element.SetParent(element_pool_rtf);
        }

        private void ReturnAllElement()
        {
            Transform[] children = new Transform[content_rft.childCount];
            for (int i = 0; i < content_rft.childCount; i++)
                children[i] = content_rft.GetChild(i);
            foreach (var child in children)
            {
                ReturnElement(child);
            }
        }

        public RectTransform GetNewElement(int element_idx)
        {
            RectTransform new_element;
            if (element_pool.Count > 0)
            {
                new_element = element_pool.Pop().GetComponent<RectTransform>();
                new_element.SetParent(content_rft);
            }
            else
            {
                new_element = Instantiate(element_prefab, content_rft).GetComponent<RectTransform>();
            }
            new_element.name = element_idx.ToString();
            if (dataBank != null)
            {
                dataBank.ApplyElementData(new_element, element_idx);
            }
            return new_element;
        }
    }
}