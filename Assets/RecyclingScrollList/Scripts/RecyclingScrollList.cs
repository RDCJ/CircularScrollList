using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RSL
{
    [RequireComponent(typeof(ScrollRect))]
    public class RecyclingScrollList : MonoBehaviour
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
        /// <summary>
        /// 排列是否反向
        /// </summary>
        public bool Reverse;
        /// <summary>
        /// element在Hierachy中是否按照idx反向排列
        /// </summary>
        public bool SiblingOrderReverse;

        #region grid format
        public int Column;
        public int Row;
        public bool UsePrefabSize;
        /// <summary>
        /// 根据滚动方向和行列大小，自动计算cellSize
        /// </summary>
        public bool AutoFitCellSize;
        public Vector2 cellSize;
        public Vector2 CellSize
        {
            get
            {
                if (UsePrefabSize)
                    return ElementPrefabRtf.rect.size;
                else
                    return cellSize;
            }
        }
        public Vector2 Space;
        private void AutoCalculateCellSize()
        {
            if (scrollType == ScrollType.Vertical)
            {
                cellSize.x = (ScrollRtf.rect.width - (Column - 1) * Space.x) / Column;
            }
            else
            {
                cellSize.y = (ScrollRtf.rect.height - (Row - 1) * Space.y) / Row;
            }
        }
        #endregion
        /// <summary>
        /// 元素预制体
        /// </summary>
        public GameObject ElementPrefab;
        private RectTransform element_prefab_rtf;
        private RectTransform ElementPrefabRtf
        {
            get
            {
                if (element_prefab_rtf == null || element_prefab_rtf.gameObject != ElementPrefab)
                    element_prefab_rtf = ElementPrefab.GetComponent<RectTransform>();
                return element_prefab_rtf;
            }
        }

        #region component
        [HideInInspector]
        private ScrollRect scrollRect;
        public ScrollRect _ScrollRect
        {
            get
            {
                if (scrollRect == null)
                    scrollRect = this.GetComponent<ScrollRect>();
                return scrollRect;
            }
        }
        private RectTransform scroll_rtf;
        public RectTransform ScrollRtf
        {
            get
            {
                if (scroll_rtf == null)
                    scroll_rtf = _ScrollRect.GetComponent<RectTransform>();
                return scroll_rtf;
            }
        }
        private RectTransform viewport_rtf;
        public RectTransform ViewportRtf
        {
            get
            {
                if (viewport_rtf == null)
                    viewport_rtf = ScrollRtf.Find("Viewport").GetComponent<RectTransform>();
                return viewport_rtf;
            }
        }
        private RectTransform content_rtf;
        public RectTransform ContentRtf
        {
            get
            {
                if (content_rtf == null)
                    content_rtf = ViewportRtf.Find("Content").GetComponent<RectTransform>();
                return content_rtf;
            }
        }
        public IElementDataBank dataBank;
        #endregion

        #region 对象池
        private Stack<GameObject> element_pool;
        /// <summary>
        /// 对象池
        /// </summary>
        private Stack<GameObject> ElementPool
        {
            get
            {
                if (element_pool == null)
                    element_pool = new Stack<GameObject>();
                return element_pool;
            }
        }
        private Transform ElementPool_rtf;
        /// <summary>
        /// 对象池中的GameObject放置于此
        /// </summary>
        private Transform ElementPoolRtf
        {
            get
            {
                if (ElementPool_rtf == null)
                {
                    ElementPool_rtf = ViewportRtf.Find("ElementPool");
                    if (ElementPool_rtf == null)
                    {
                        ElementPool_rtf = new GameObject("ElementPool").transform;
                        ElementPool_rtf.parent = ViewportRtf;
                    }
                }
                return ElementPool_rtf;
            }
            
        }
        #endregion
        private RectTransform FirstElementRtf
        {
            get
            {
                if (ContentRtf.childCount == 0) return null;
                return (SiblingOrderReverse ? ContentRtf.GetChild(ContentRtf.childCount - 1) : ContentRtf.GetChild(0) ) as RectTransform;
            }
        }

        private RectTransform LastElementRtf
        {
            get
            {
                if (ContentRtf.childCount == 0) return null;
                return (SiblingOrderReverse ? ContentRtf.GetChild(0) : ContentRtf.GetChild(ContentRtf.childCount - 1)) as RectTransform;
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
        public int ElementCount => dataBank == null ? 0 : dataBank.ElementCount;
        /// <summary>
        /// 同屏展示的元素数量
        /// </summary>
        private int ElementShowCount
        {
            get
            {
                if (scrollType== ScrollType.Vertical)
                {
                    int row = (int)(ScrollRtf.rect.height / (CellSize.y + Space.y)) + 2;
                    return Mathf.Min(ElementCount, Column * row);
                }
                else
                {
                    int column = (int)(ScrollRtf.rect.width / (CellSize.x + Space.x)) + 2;
                    return Mathf.Min(ElementCount, column * Row);
                }
                
            }
        }

        #region Const
        private static Vector2 ElementAnchor = new Vector2(0, 1);
        private static Vector2 ContentAnchorVertical = new Vector2(0.5f, 1);
        private static Vector2 ContentAnchorHorizontal = new Vector2(0, 0.5f);

        private static Vector2 ContentAnchorVerticalReverse = new Vector2(0.5f, 0);
        private static Vector2 ContentAnchorHorizontalReverse = new Vector2(1, 0.5f);
        #endregion
        /// <summary>
        /// 启用曲线
        /// </summary>
        public bool enable_curve;
        /// <summary>
        /// 采样曲线：根据element的位置动态更新postion
        /// </summary>
        public AnimationCurve position_offset_curve;
        /// <summary>
        /// 采样曲线：根据element的位置动态更新scale
        /// </summary>
        public AnimationCurve scale_curve;

        public void RefreshGrid()
        {
            if (!gameObject.activeInHierarchy) return;
            ReturnAllElement();
            UpdateContentSize();
            for (int i = 0; i < ElementShowCount; i++)
            {
                var new_element = GetNewElement(i);
                if (SiblingOrderReverse)
                    new_element.SetAsFirstSibling();
                else
                    new_element.SetAsLastSibling();
            }
            head_idx = 0;
            tail_idx = ElementShowCount - 1;
        }

        private void Awake()
        {
            element_pool = new Stack<GameObject>();
            
        }

        private void Start()
        {
            ElementPoolRtf.gameObject.SetActive(false);
            if (scrollType == ScrollType.Vertical)
                ContentRtf.anchoredPosition = new Vector2(0, ContentRtf.anchoredPosition.y);
            else
                ContentRtf.anchoredPosition = new Vector2(ContentRtf.anchoredPosition.x, 0);
            if (dataBank != null)
            {
                RefreshGrid();
            }
        }

        private void Update()
        {
            _ScrollRect.horizontal = scrollType == ScrollType.Horizontal;
            _ScrollRect.vertical = scrollType == ScrollType.Vertical;
            UpdateContentSize();
            if (ElementCount > 0)
            {
                while (CheckDeleteHead()) ;
                while (CheckDeleteTail()) ;
                while (CheckCreateHead()) ;
                while (CheckCreateTail()) ;
            }

            if (enable_curve)
                UpdateWithCurve();
        }

        private void OnEnable()
        {
            if (dataBank != null)
            {
                RefreshGrid();
            }
        }

        public void Init(IElementDataBank dataBank, GameObject elementPrefab=null)
        {
            this.dataBank = dataBank;
            if (dataBank != null)
            {
                dataBank.OnElementCountChanged += (x) => RefreshGrid();
            }
            if (elementPrefab != null) this.ElementPrefab = elementPrefab;
            RefreshGrid();
        }

        private bool IsOutOfTopBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(ViewportRtf, rtf);
            if (bound.min.y - 0.5f * ScrollRtf.rect.height > 0) return true;
            return false;
        }

        private bool IsOutOfBottomBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(ViewportRtf, rtf);
            if (bound.max.y + 0.5f * ScrollRtf.rect.height < 0) return true;
            return false;
        }

        private bool IsOutOfLeftBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(ViewportRtf, rtf);
            if (bound.max.x + 0.5f * ScrollRtf.rect.width< 0) return true;
            return false;
        }

        private bool IsOutOfRightBound(RectTransform rtf)
        {
            Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(ViewportRtf, rtf);
            if (bound.min.x  > 0.5f * ScrollRtf.rect.width) return true;
            return false;
        }

        private void UpdateContentSize()
        {
            if (scrollType == ScrollType.Vertical)
            {
                ContentRtf.anchorMin = Reverse ? ContentAnchorVerticalReverse : ContentAnchorVertical;
                ContentRtf.anchorMax = Reverse ? ContentAnchorVerticalReverse : ContentAnchorVertical;
                ContentRtf.pivot = Reverse ? ContentAnchorVerticalReverse : ContentAnchorVertical;
                Vector2 content_size = ContentRtf.sizeDelta;
                content_size.x = CellSize.x * Column + Space.x * (Column - 1);
                int row = ElementCount / Column + (ElementCount % Column == 0 ? 0 : 1);
                content_size.y = CellSize.y * row + Space.y * (row - 1);
                ContentRtf.sizeDelta = content_size;
            }
            else
            {
                ContentRtf.anchorMin = Reverse ? ContentAnchorHorizontalReverse : ContentAnchorHorizontal;
                ContentRtf.anchorMax = Reverse ? ContentAnchorHorizontalReverse : ContentAnchorHorizontal;
                ContentRtf.pivot = Reverse ? ContentAnchorHorizontalReverse : ContentAnchorHorizontal;
                Vector2 content_size = ContentRtf.sizeDelta;
                content_size.y = CellSize.y * Row + Space.y * (Row - 1);
                int column = ElementCount / Row + (ElementCount % Row == 0 ? 0 : 1);
                content_size.x = CellSize.x * column + Space.x * (column - 1);
                ContentRtf.sizeDelta = content_size;
            }
        }

        private bool CheckDeleteHead()
        {
            if (head_idx > tail_idx) return false;
            RectTransform rtf_tmp = FirstElementRtf;
            if (rtf_tmp != null)
            {
                bool is_out;
                if (scrollType == ScrollType.Vertical)
                {
                    is_out = Reverse ? IsOutOfBottomBound(rtf_tmp) : IsOutOfTopBound(rtf_tmp);
                }
                else
                {
                    is_out = Reverse ? IsOutOfRightBound(rtf_tmp) : IsOutOfLeftBound(rtf_tmp);
                }
                if (is_out)
                {
                    //Debug.Log($"Delete at head");
                    ReturnElement(rtf_tmp);
                    head_idx++;
                    return true;
                }
            }
            return false; 
        }

        private bool CheckDeleteTail()
        {
            if (head_idx > tail_idx) return false;
            RectTransform rtf_tmp = LastElementRtf;
            if (rtf_tmp != null)
            {
                bool is_out;
                if (scrollType == ScrollType.Vertical)
                {
                    is_out = Reverse ? IsOutOfTopBound(rtf_tmp)  : IsOutOfBottomBound(rtf_tmp);
                }
                else
                {
                    is_out = Reverse ? IsOutOfLeftBound(rtf_tmp) : IsOutOfRightBound(rtf_tmp);
                }
                if (is_out)
                {
                    //Debug.Log($"Delete at tail");
                    ReturnElement(rtf_tmp);
                    tail_idx--;
                    return true;
                }
            }
            return false;
        }

        private void CreateAtHead(int num)
        {
            for (int i = 0; i < num; i++)
            {
                head_idx--;
                RectTransform new_element = GetNewElement(head_idx);
                if (SiblingOrderReverse)
                    new_element.SetAsLastSibling();
                else
                    new_element.SetAsFirstSibling();
                if (head_idx <= 0) break;
            }
        }

        private bool CheckCreateHead()
        {
            if (tail_idx - head_idx + 1 >= ElementShowCount) return false; 
            if (head_idx > 0)
            {
                RectTransform first_element = FirstElementRtf;
                if (first_element != null)
                {
                    Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(ViewportRtf, first_element);
                    bool need_create;
                    if (scrollType == ScrollType.Vertical)
                        need_create = Reverse ? 0.5f * ScrollRtf.rect.height + bounds.min.y > Space.y : bounds.max.y - 0.5f * ScrollRtf.rect.height < -Space.y;
                    else
                        need_create = Reverse ? 0.5f * ScrollRtf.rect.width - bounds.max.x > Space.x : bounds.min.x + 0.5f * ScrollRtf.rect.width > Space.x;
                    if (need_create)
                    {
                        //Debug.Log("Create new at head");
                        int k = scrollType == ScrollType.Vertical ? Column : Row;
                        CreateAtHead(k);
                        return true;
                    }
                }
                else
                {
                    bool need_create;
                    if (scrollType == ScrollType.Vertical)
                        need_create = Reverse ? ContentRtf.anchoredPosition.y > -ContentRtf.rect.height : ContentRtf.anchoredPosition.y < ContentRtf.rect.height;
                    else
                        need_create = Reverse ? ContentRtf.anchoredPosition.x < ContentRtf.rect.width : ContentRtf.anchoredPosition.x  > -ContentRtf.rect.width;
                    if (need_create)
                    {
                        //Debug.Log("Create new at head");
                        int k = (scrollType == ScrollType.Vertical) ? (ElementCount % Column == 0 ? Column : ElementCount % Column) : (ElementCount % Row == 0 ? Row : ElementCount % Row);
                        CreateAtHead(k);
                        return true;
                    }
                }  
            }
            return false;
        }

        private void CreateAtTail(int num)
        {
            for (int i = 0; i < num; i++)
            {
                tail_idx++;
                RectTransform new_element = GetNewElement(tail_idx);
                if (SiblingOrderReverse)
                    new_element.SetAsFirstSibling();
                else
                    new_element.SetAsLastSibling();
                if (tail_idx >= ElementCount - 1) break;
            }
        }

        private bool CheckCreateTail()
        {
            if (tail_idx - head_idx + 1 >= ElementShowCount) return false;
            if (tail_idx < ElementCount - 1)
            {
                RectTransform last_element = LastElementRtf;
                if (last_element != null)
                {
                    Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(ViewportRtf, last_element);

                    bool need_create;
                    if (scrollType == ScrollType.Vertical)
                        need_create = Reverse ? bounds.max.y - 0.5f * ScrollRtf.rect.height < -Space.y : 0.5f * ScrollRtf.rect.height + bounds.min.y > Space.y;
                    else
                        need_create = Reverse ? bounds.min.x + 0.5f * ScrollRtf.rect.width > Space.x : 0.5f * ScrollRtf.rect.width - bounds.max.x > Space.x;

                    if (need_create)
                    {
                        //Debug.Log("Create new at tail");
                        int k = (scrollType == ScrollType.Vertical) ? Mathf.Min(Column, ElementCount - 1  - tail_idx) : Mathf.Min(Row, ElementCount - 1 - tail_idx);
                        CreateAtTail(k);
                        return true;
                    }
                }
                else
                {
                    bool need_create;
                    if (scrollType == ScrollType.Vertical)
                        need_create = Reverse ? ContentRtf.anchoredPosition.y < ScrollRtf.rect.height : ContentRtf.anchoredPosition.y > -ScrollRtf.rect.height;
                    else
                        need_create = Reverse ? ContentRtf.anchoredPosition.x > -ScrollRtf.rect.width : ContentRtf.anchoredPosition.x < ScrollRtf.rect.width;

                    if (need_create)
                    {
                        //Debug.Log("Create new at tail");
                        int k = (scrollType == ScrollType.Vertical) ? Mathf.Min(Column, ElementCount - 1 - tail_idx) : Mathf.Min(Row, ElementCount - 1 - tail_idx);
                        CreateAtTail(k);
                    }
                }
            }
            return false;
        }

        private void UpdateWithCurve()
        {
            Vector3 v = new Vector3(0, 0, 0);
            for (int i = 0; i < ContentRtf.childCount; i++)
            {
                RectTransform rtf_tmp = ContentRtf.GetChild(i).GetComponent<RectTransform>();
                int element_idx = int.Parse(rtf_tmp.name);
                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(ViewportRtf, rtf_tmp);
                float percent;
                if (scrollType == ScrollType.Vertical)
                {
                    percent = (0.5f * ScrollRtf.rect.height - bounds.center.y) / ScrollRtf.rect.height;
                    v.x = position_offset_curve.Evaluate(percent) * ScrollRtf.rect.width;
                    v.y = 0;
                }
                else
                {
                    percent = (bounds.center.x + 0.5f * ScrollRtf.rect.width) / ScrollRtf.rect.width;
                    v.x = 0;
                    v.y = position_offset_curve.Evaluate(percent) * ScrollRtf.rect.width;
                }
               
                rtf_tmp.anchoredPosition = CalcElementPosition(element_idx) + v;

                float scale = scale_curve.Evaluate(percent);
                rtf_tmp.localScale = new Vector3(scale, scale, scale);
            }
        } 

        private void ReturnElement(Transform element)
        {
            ElementPool.Push(element.gameObject);
            element.SetParent(ElementPoolRtf);
        }

        private void ReturnAllElement()
        {
            Transform[] children = new Transform[ContentRtf.childCount];
            for (int i = 0; i < ContentRtf.childCount; i++)
                children[i] = ContentRtf.GetChild(i);
            foreach (var child in children)
            {
                ReturnElement(child);
            }
        }

        private RectTransform GetNewElement(int element_idx)
        {
            RectTransform new_element;
            if (ElementPool.Count > 0)
            {
                new_element = ElementPool.Pop().GetComponent<RectTransform>();
                new_element.SetParent(ContentRtf);
            }
            else
            {
                new_element = Instantiate(ElementPrefab, ContentRtf).GetComponent<RectTransform>();
                // 固定anchor
                new_element.anchorMin = ElementAnchor;
                new_element.anchorMax = ElementAnchor;
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

        /// <summary>
        /// 用dataBank中的数据刷新element，调用前应更新dataBank中的数据
        /// </summary>
        /// <param name="element_idx"></param>
        public void RefreshElement(int element_idx)
        {
            if (element_idx >= head_idx && element_idx <= tail_idx)
            {
                RectTransform rtf_tmp = ContentRtf.Find(element_idx.ToString()) as RectTransform;
                dataBank.ApplyElementData(rtf_tmp, element_idx);
            }
        }

        /// <summary>
        /// 用dataBank中的数据刷新所有element，调用前应更新dataBank中的数据
        /// </summary>
        public void RefreshAllElement()
        {
            if (dataBank != null)
            {
                for (int i = 0; i < ContentRtf.childCount; i++)
                {
                    RectTransform rtf_tmp = ContentRtf.GetChild(i) as RectTransform;
                    dataBank.ApplyElementData(rtf_tmp, int.Parse(rtf_tmp.name));
                }
            }
        }

        /// <summary>
        /// 计算element在content中的位置
        /// </summary>
        /// <param name="element_idx"></param>
        /// <returns></returns>
        private Vector3 CalcElementPosition(int element_idx)
        {
            Vector3 defaultPosition = CalcDefalutElementPosition(element_idx);
            if (dataBank != null && dataBank.OverrideCalcElementPosition)
            {
                return dataBank.CalcElementPosition(scrollType, element_idx, defaultPosition);
            }
            else
            {
                return defaultPosition;
            }
        }

        private Vector3 CalcDefalutElementPosition(int element_idx)
        {
            Vector3 pivot_offset = (ElementPrefabRtf.pivot - new Vector2(0.5f, 0.5f)) * CellSize;
            if (scrollType == ScrollType.Vertical)
            {
                int row_idx = Reverse ? ElementCount / Column + (ElementCount % Column == 0 ? 0 : 1) - element_idx / Column - 1 : element_idx / Column;
                int column_idx = element_idx % Column;
                return new Vector3(
                    0.5f * CellSize.x + column_idx * (CellSize.x + Space.x),
                    -0.5f * CellSize.y - row_idx * (CellSize.y + Space.y),
                    0) + pivot_offset;
            }
            else
            {
                int row_idx = element_idx % Row;
                int column_idx = Reverse ? ElementCount / Row + (ElementCount % Row == 0 ? 0 : 1) - element_idx / Row  - 1 : element_idx / Row;
                return new Vector3(
                    0.5f * CellSize.x + column_idx * (CellSize.x + Space.x),
                    -0.5f * CellSize.y - row_idx * (CellSize.y + Space.y),
                    0) + pivot_offset;
            }
        }

        /// <summary>
        /// 滚动到指定element的位置
        /// </summary>
        /// <param name="element_idx"></param>
        /// <param name="viewport_position">滚动结束时element在viewport中的位置，0~1</param>
        public void ScrollToElement(int element_idx, float viewport_position)
        {
            viewport_position = Mathf.Clamp01(viewport_position);
            if (scrollType == ScrollType.Vertical)
            {
                viewport_position = (Reverse ? 1 : 0) - viewport_position;
                int row_idx = element_idx / Column;
                Vector2 p = ContentRtf.anchoredPosition;
                p.y = row_idx * (CellSize.y + Space.y) * (Reverse ? -1 : 1) + (ScrollRtf.rect.height - CellSize.y) * viewport_position;
                ContentRtf.anchoredPosition = p;
            }
            else
            {
               viewport_position = viewport_position - (Reverse ? 1 : 0);
                int column_idx = element_idx / Row;
                Vector3 p = ContentRtf.anchoredPosition;
                p.x = column_idx * (CellSize.x + Space.x) * (Reverse ? 1 : -1) + (ScrollRtf.rect.width - CellSize.x) * viewport_position;
                ContentRtf.anchoredPosition = p;
            }
        }

        public void ClearGrid()
        {
            Utils.ClearChild(ContentRtf);
            Utils.ClearChild(ElementPoolRtf);
            while (ElementPool.Count > 0)
            {
                element_pool.Pop();
            }
        }

        private void OnValidate()
        {
            if ( AutoFitCellSize && UsePrefabSize)
            {
                AutoFitCellSize = false;
                UsePrefabSize = false;
            }
            Column = Mathf.Max(Column, 1);
            Row = Mathf.Max(Row, 1);
            if (AutoFitCellSize)
            {
                AutoCalculateCellSize();
            }
        }
    }
}