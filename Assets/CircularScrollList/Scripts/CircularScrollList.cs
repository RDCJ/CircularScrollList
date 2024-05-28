using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace SCL
{
    [RequireComponent(typeof(ScrollRect))]
    public class CircularScrollList : MonoBehaviour
    {
        public enum ScrollType
        {
            Horizontal,
            Vertical
        }

        /// <summary>
        /// 滚动方向
        /// </summary>
        public ScrollType scrollType;

        #region grid format
        public int Column;
        public int Row;
        public Vector2 CellSize;
        public Vector2 Space;
        #endregion
        /// <summary>
        /// 元素总数
        /// </summary>
        [SerializeField]
        private int element_count;
        /// <summary>
        /// 元素预制体
        /// </summary>
        public GameObject element_prefab;

        #region component
        [HideInInspector]
        public ScrollRect scrollRect;
        private RectTransform scroll_rtf;
        private RectTransform viewport_rtf;
        private RectTransform content_rft;
        public IElementDataBank dataBank;
        #endregion

        /// <summary>
        /// 对象池
        /// </summary>
        private Stack<GameObject> element_pool;
        /// <summary>
        /// 对象池中的GameObject放置于此
        /// </summary>
        public RectTransform element_pool_rtf;
        
        

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
        /// <summary>
        /// 第一个元素的index
        /// </summary>
        private int head_idx;
        /// <summary>
        /// 最后一个元素的index
        /// </summary>
        private int tail_idx;
        /// <summary>
        /// 元素总数
        /// </summary>
        public int ElementCount => element_count;
        /// <summary>
        /// 同屏展示的元素数量
        /// </summary>
        private int ElementShowCount
        {
            get
            {
                if (scrollType== ScrollType.Vertical)
                {
                    int row = (int)(scroll_rtf.sizeDelta.y / (CellSize.y + Space.y)) + 2;
                    return Mathf.Min(ElementCount, Column * row);
                }
                else
                {
                    int column = (int)(scroll_rtf.sizeDelta.x / (CellSize.x + Space.x)) + 2;
                    return Mathf.Min(ElementCount, column * Row);
                }
                
            }
        }

        public void RefreshGrid()
        {
            ReturnAllElement();
            UpdateContentSize();
            for (int i = 0; i < ElementShowCount; i++)
            {
                GetNewElement(i);
            }
            head_idx = 0;
            tail_idx = ElementShowCount - 1;
        }

        public void SetElementCount(int value)
        {
            if (value < 0)
                Debug.LogError("[CircularScrollList:SetElementCount] value < 0");
            else if (value == element_count)
                Debug.Log("[CircularScrollList:SetElementCount] value == this.element_count");
            else
            {
                element_count = value;
                RefreshGrid();
            }
        }

        private void Awake()
        {
            scrollRect = this.GetComponent<ScrollRect>();
            scroll_rtf = scrollRect.GetComponent<RectTransform>();
            viewport_rtf = scroll_rtf.Find("Viewport").GetComponent<RectTransform>();
            content_rft = viewport_rtf.Find("Content").GetComponent<RectTransform>();
            element_pool = new Stack<GameObject>();
        }

        private void Start()
        {
            element_pool_rtf.gameObject.SetActive(false);
            RefreshGrid();
        }

        private void Update()
        {
            scrollRect.horizontal = scrollType == ScrollType.Horizontal;
            scrollRect.vertical = scrollType == ScrollType.Vertical;
            UpdateContentSize();
            while (CheckDeleteHead()) ;
            while (CheckDeleteTail()) ;
            while (CheckCreateHead()) ;
            while (CheckCreateTail()) ;
        }

/*        private bool IsOutOfBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, rtf);
            if (bound.min.y > 0) return true;
            if (bound.center.y + bound.size.y * 0.5 < -scroll_rtf.sizeDelta.y) return true;
            return false;
        }*/

        private bool IsOutOfTopBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, rtf);
            if (bound.min.y - 0.5f * scroll_rtf.sizeDelta.y > 0) return true;
            return false;
        }

        private bool IsOutOfBottomBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, rtf);
            if (bound.max.y + 0.5f * scroll_rtf.sizeDelta.y < 0) return true;
            return false;
        }

        private bool IsOutOfLeftBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, rtf);
            if (bound.max.x + 0.5f * scroll_rtf.sizeDelta.x< 0) return true;
            return false;
        }

        private bool IsOutOfRightBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport_rtf, rtf);
            if (bound.min.x  > 0.5f * scroll_rtf.sizeDelta.x) return true;
            return false;
        }

        public void UpdateContentSize()
        {
            if (scrollType == ScrollType.Vertical)
            {
                Vector2 content_size = content_rft.sizeDelta;
                content_size.x = CellSize.x * Column + Space.x * (Column - 1);
                int row = element_count / Column + (element_count % Column == 0 ? 0 : 1);
                content_size.y = CellSize.y * row + Space.y * (row - 1);
                content_rft.sizeDelta = content_size;
            }
            else
            {
                Vector2 content_size = content_rft.sizeDelta;
                content_size.y = CellSize.y * Row + Space.y * (Row - 1);
                int column = element_count / Row + (element_count % Row == 0 ? 0 : 1);
                content_size.x = CellSize.x * column + Space.x * (column - 1);
                content_rft.sizeDelta = content_size;
            }
        }

        public bool CheckDeleteHead()
        {
            if (head_idx > tail_idx) return false;
            RectTransform rtf_tmp = FirstElementRtf;
            if (rtf_tmp != null)
            {
                if (scrollType == ScrollType.Vertical ? IsOutOfTopBound(rtf_tmp) : IsOutOfLeftBound(rtf_tmp))
                {
                    //Debug.Log($"Delete at head");
                    ReturnElement(rtf_tmp);
                    head_idx++;
                    return true;
                }
            }
            return false;
        }

        public bool CheckDeleteTail()
        {
            if (head_idx > tail_idx) return false;
            RectTransform rtf_tmp = LastElementRtf;
            if (rtf_tmp != null)
            {
                if (scrollType == ScrollType.Vertical ? IsOutOfBottomBound(rtf_tmp) : IsOutOfRightBound(rtf_tmp))
                {
                    //Debug.Log($"Delete at tail");
                    ReturnElement(rtf_tmp);
                    tail_idx--;
                    return true;
                }
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
                    if (scrollType == ScrollType.Vertical? first_bounds.max.y - 0.5f * scroll_rtf.sizeDelta.y < -Space.y : first_bounds.min.x + 0.5f * scroll_rtf.sizeDelta.x > Space.x)
                    {
                        //Debug.Log("Create new at head");
                        int k = scrollType == ScrollType.Vertical ? Column : Row;
                        for (int i=0; i<k; i++)
                        {
                            head_idx--;
                            RectTransform new_element = GetNewElement(head_idx);
                            new_element.SetAsFirstSibling();
                            if (head_idx <= 0) break;
                        }
                        return true;
                    }
                }
                else if (scrollType == ScrollType.Vertical ? content_rft.anchoredPosition.y < content_rft.sizeDelta.y : content_rft.localPosition.x + 0.5f *scroll_rtf.sizeDelta.x > - content_rft.sizeDelta.x)
                {
                    //Debug.Log("Create new at head");
                    int k = (scrollType == ScrollType.Vertical) ? (element_count % Column == 0? Column : element_count % Column) : (element_count % Row == 0 ? Row : element_count % Row);
                    for (int i=0; i<k; i++)
                    {
                        head_idx--;
                        RectTransform new_element = GetNewElement(head_idx);
                        new_element.SetAsFirstSibling();
                        if (head_idx <= 0) break;
                    }
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
                    if (scrollType == ScrollType.Vertical ? 0.5f * scroll_rtf.sizeDelta.y + last_bounds.min.y > Space.y : 0.5f * scroll_rtf.sizeDelta.x - last_bounds.max.x > Space.x)
                    {
                        //Debug.Log("Create new at tail");
                        int k = (scrollType == ScrollType.Vertical) ? Mathf.Min(Column, ElementCount - 1  - tail_idx) : Mathf.Min(Row, ElementCount - 1 - tail_idx);
                        for (int i=0; i<k; i++)
                        {
                            tail_idx++;
                            RectTransform new_element = GetNewElement(tail_idx);
                            new_element.SetAsLastSibling();
                            if (tail_idx >= element_count - 1) break;
                        }
                        return true;
                    }
                }
                else if (scrollType == ScrollType.Vertical ? content_rft.anchoredPosition.y > -scroll_rtf.sizeDelta.y : content_rft.localPosition.x < 0.5f * scroll_rtf.sizeDelta.x)
                {
                    //Debug.Log("Create new at tail");
                    int k = (scrollType == ScrollType.Vertical) ? Mathf.Min(Column, ElementCount - 1 - tail_idx) : Mathf.Min(Row, ElementCount - 1 - tail_idx);
                    for (int i = 0; i < k; i++)
                    {
                        tail_idx++;
                        RectTransform new_element = GetNewElement(tail_idx);
                        new_element.SetAsLastSibling();
                        if (tail_idx >= element_count - 1) break;
                    }
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
            new_element.sizeDelta = CellSize;
            new_element.anchoredPosition = CalcElementPosition(element_idx);
            if (dataBank != null)
            {
                dataBank.ApplyElementData(new_element, element_idx);
            }
            return new_element;
        }

        public Vector3 CalcElementPosition(int element_idx)
        {
            if (scrollType == ScrollType.Vertical)
            {
                int row_idx = element_idx / Column;
                int column_idx = element_idx % Column;
                return new Vector3(
                    0.5f * CellSize.x + column_idx * (CellSize.x + Space.x),
                    -0.5f * CellSize.y - row_idx * (CellSize.y + Space.y),
                    0);
            }
            else
            {
                int row_idx = element_idx % Row;
                int column_idx = element_idx / Row;
                return new Vector3(
                    0.5f * CellSize.x + column_idx * (CellSize.x + Space.x),
                    -0.5f * CellSize.y - row_idx * (CellSize.y + Space.y),
                    0);
            }
        }
    }
}