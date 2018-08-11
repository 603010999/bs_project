using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 数据列表渲染组件，Item缓存，支持无限循环列表，即用少量的Item实现大量的列表项显示
/// </summary>
public class DataGrid : MonoBehaviour
{
    [HideInInspector]
    //是否使用无限循环列表，对于列表项中OnDataSet方法执行消耗较大时不宜使用，因为OnDataSet方法会在滚动的时候频繁调用
    public bool useLoopItems = false;

    [HideInInspector]
    //列表项是否监听点击事件
    public bool useClickEvent = true;

    [HideInInspector]
    //创建时是否自动选中第一个对象
    public bool autoSelectFirst = true;

    public delegate void OnDataGridItemSelect(object renderData);
    //Item点击时的回调函数
    public OnDataGridItemSelect onItemSelected;

    /// <summary>
    /// 数据项
    /// </summary>
    public object[] Data
    {
        set
        {
            m_data = value;
            UpdateView();

            if (autoSelectFirst && m_data.Length > 0)
            {
                if (m_data[0] != m_selectedData)
                {
                    SelectItem(m_data[0]);
                }
                SetToggle(0, true);
            }
            else if (m_data.Length == 0)
            {
                SelectItem(null);
            }
        }
        get { return m_data; }
    }

    private RectTransform m_content;
    //private Vector2 m_lastContentPos;
    private ToggleGroup m_toggleGroup;

    //item对应的数据
    private object[] m_data;

    //item对象
    private GameObject m_goItemRender;

    //item类型
    private Type m_itemRenderType;

    //item实例列表
    private readonly List<ItemRender> m_items = new List<ItemRender>();

    //当前选中数据
    private object m_selectedData;

    private LayoutGroup m_LayoutGroup;
    private RectOffset m_oldPadding;

    //下面的属性会需要父对象上有ScrollRect组件

    //父对象上的，不一定存在
    private ScrollRect m_scrollRect;
    private RectTransform m_tranScrollRect;

    //每个Item的空间
    private int m_itemSpace;

    //可视区域内Item的数量（向上取整）
    private int m_viewItemCount;

    //是否是垂直滚动方式，否则是水平滚动
    private bool m_isVertical;

    //数据数组渲染的起始下标
    private int m_startIndex;

    //
    Vector2 Resolution = new Vector2(1242, 2208);

    //垂直方向时滑动位置
    public float verticalPos
    {
        get { return m_scrollRect.verticalNormalizedPosition; }
        set { m_scrollRect.verticalNormalizedPosition = value; }
    }

    //横向滑动位置
    public float horizonPos
    {
        get { return m_scrollRect.horizontalNormalizedPosition; }
        set { m_scrollRect.horizontalNormalizedPosition = value; }
    }

    //内容长度
    private float ContentSpace
    {
        get
        {
            return m_isVertical ? m_content.sizeDelta.y : m_content.sizeDelta.x;
        }
    }

    //可见区域长度
    private float ViewSpace
    {
        get
        {
            return m_isVertical ? (m_tranScrollRect.sizeDelta.y + Resolution.y) : (m_tranScrollRect.sizeDelta.x + Resolution.x);
        }
    }

    //约束常量（固定的行（列）数）
    private int ConstraintCount
    {
        get
        {
            return m_LayoutGroup == null ? 1 : ((m_LayoutGroup is GridLayoutGroup) ? (m_LayoutGroup as GridLayoutGroup).constraintCount : 1);
        }
    }

    //数据量个数
    private int DataCount
    {
        get
        {
            return m_data == null ? 0 : m_data.Length;
        }
    }

    //缓存数量
    private int CacheCount
    {
        get
        {
            return ConstraintCount + DataCount % ConstraintCount;
        }
    }

    //缓存单元的行（列）数
    private int CacheUnitCount
    {
        get
        {
            return m_LayoutGroup == null ? 1 : Mathf.CeilToInt((float)CacheCount / ConstraintCount);
        }
    }

    //数据单元的行（列）数
    private int DataUnitCount
    {
        get
        {
            return m_LayoutGroup == null ? DataCount : Mathf.CeilToInt((float)DataCount / ConstraintCount);
        }
    }

    //变量初始化
    void Awake()
    {
        //添加组件
        gameObject.AddComponent<CanvasRenderer>();

        //
        m_toggleGroup = GetComponent<ToggleGroup>();

        m_LayoutGroup = GetComponentInChildren<LayoutGroup>();

        if (m_LayoutGroup == null)
        {
            return;
        }

        m_content = m_LayoutGroup.gameObject.GetComponent<RectTransform>();

        m_oldPadding = m_LayoutGroup.padding;

        m_scrollRect = transform.GetComponentInParent<ScrollRect>();
        if (m_scrollRect == null)
        {
            Debug.LogError("scrollRect is null or verticalLayoutGroup is null");
            return;
        }

        m_scrollRect.gameObject.layer = LayerMask.NameToLayer("UI");

        m_scrollRect.decelerationRate = 0.2f;

        m_tranScrollRect = m_scrollRect.GetComponent<RectTransform>();
        if (m_tranScrollRect == null)
        {
            return;
        }

        m_isVertical = m_scrollRect.vertical;
        var layoutgroup = m_LayoutGroup as GridLayoutGroup;
        if (layoutgroup == null)
        {
            return;
        }

        m_itemSpace = (int)(m_isVertical
            ? (layoutgroup.cellSize.y + layoutgroup.spacing.y)
            : (layoutgroup.cellSize.x + layoutgroup.spacing.x));

        m_viewItemCount = Mathf.CeilToInt(ViewSpace / m_itemSpace);
    }

    void Start()
    {
        if (m_scrollRect == null)
        {
            return;
        }

        if (useLoopItems)
        {
            m_scrollRect.onValueChanged.AddListener(OnScroll);
        }

        if (m_toggleGroup != null)
        {
            m_toggleGroup.allowSwitchOff = useLoopItems;
        }
    }

    void Destroy()
    {
        onItemSelected = null;
        m_items.Clear();
    }

    /// <summary>
    /// 设置渲染项
    /// </summary>
    /// <param name="goItemRender"></param>
    /// <param name="itemRenderType"></param>
    public void SetItemRender(GameObject goItemRender, Type itemRenderType)
    {
        m_goItemRender = goItemRender;
        m_itemRenderType = itemRenderType.IsSubclassOf(typeof(ItemRender)) ? itemRenderType : null;
        var layoutEle = goItemRender.GetComponent<LayoutElement>();
        if (layoutEle == null)
        {
            return;
        }

        var layoutGroup = m_LayoutGroup as HorizontalOrVerticalLayoutGroup;
        if (layoutGroup == null)
        {
            return;
        }

        if (m_tranScrollRect == null)
        {
            m_scrollRect = transform.GetComponentInParent<ScrollRect>();
            m_tranScrollRect = m_scrollRect.GetComponent<RectTransform>();
        }
        m_itemSpace = (int)(layoutEle.preferredHeight + (int)layoutGroup.spacing);
        m_viewItemCount = Mathf.CeilToInt(ViewSpace / m_itemSpace);
    }

    //外部获取实例接口
    public List<ItemRender> ItemRenders
    {
        get { return m_items; }
    }

    public void Remove(object item)
    {
        if (item == null || Data == null)
        {
            return;
        }
        var newList = new List<object>(Data);
        if (newList.Contains(item))
        {
            newList.Remove(item);
        }
        Data = newList.ToArray();
    }

    /// <summary>
    /// 当前选择的数据项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T SelectedData<T>()
    {
        return (T)m_selectedData;
    }

    /// <summary>
    /// 重置滚动位置，
    /// </summary>
    /// <param name="top">true则跳转到顶部，false则跳转到底部</param>
    public void ResetScrollPosition(bool top = true)
    {
        if (m_data == null)
        {
            return;
        }

        var index = top ? 0 : m_data.Length;

        ResetScrollPosition(index);
    }

    /// <summary>
    /// 重置滚动位置，如果同时还要赋值新的Data，请在赋值之前调用本方法
    /// </summary>
    public void ResetScrollPosition(int index)
    {
        if (m_data == null)
            return;
        var unitIndex = Mathf.Clamp(index / ConstraintCount, 0, DataUnitCount - m_viewItemCount > 0 ? DataUnitCount - m_viewItemCount : 0);
        var value = (unitIndex * m_itemSpace) / (Mathf.Max(ViewSpace, ContentSpace - ViewSpace));
        value = Mathf.Clamp01(value);

        //特殊处理无法使指定条目置顶的情况——拉到最后
        if (unitIndex != index / ConstraintCount)
            value = 1;

        if (m_scrollRect != null)
        {
            if (m_isVertical)
                m_scrollRect.verticalNormalizedPosition = 1 - value;
            else
                m_scrollRect.horizontalNormalizedPosition = value;
        }

        m_startIndex = unitIndex * ConstraintCount;
        UpdateView();
    }

    /// <summary>
    /// 更新视图
    /// </summary>
    public void UpdateView()
    {
        if (useLoopItems)
        {
            if (m_data != null)
            {
                m_startIndex =
                    Mathf.Max(0,
                        Mathf.Min(m_startIndex / ConstraintCount, DataUnitCount - m_viewItemCount - CacheUnitCount)) *
                    ConstraintCount;
            }

            var frontSpace = m_startIndex / ConstraintCount * m_itemSpace;
            var behindSpace = Mathf.Max(0,
                m_itemSpace * (DataUnitCount - CacheUnitCount) - frontSpace - (m_itemSpace * m_viewItemCount));
            if (m_isVertical)
            {
                m_LayoutGroup.padding = new RectOffset(m_oldPadding.left, m_oldPadding.right, frontSpace,
                    behindSpace);
            }
            else
            {
                m_LayoutGroup.padding = new RectOffset(frontSpace, behindSpace, m_oldPadding.top,
                    m_oldPadding.bottom);
            }
        }
        else
        {
            m_startIndex = 0;
        }

        if (m_goItemRender == null || m_itemRenderType == null || m_data == null || m_content == null)
        {
            return;
        }

        UpdateItems();
    }

    private void UpdateItems()
    {
        var itemLength = useLoopItems ? m_viewItemCount * ConstraintCount + CacheCount : m_data.Length;
        itemLength = Mathf.Min(itemLength, m_data.Length);

        for (var i = itemLength; i < m_items.Count; i++)
        {
            Destroy(m_items[i].gameObject);
            m_items[i] = null;
        }

        for (int i = m_items.Count - 1; i >= 0; i--)
        {
            if (m_items[i] == null)
                m_items.RemoveAt(i);
        }

        for (int i = 0; i < itemLength; i++)
        {
            var index = m_startIndex + i;
            if (index >= m_data.Length || index < 0)
                continue;
            if (i < m_items.Count)
            {
                m_items[i].SetData(m_data[index]);

                if (useClickEvent || autoSelectFirst)
                    SetToggle(i, m_selectedData == m_data[index]);
            }
            else
            {
                var go = Instantiate(m_goItemRender) as GameObject;
                go.name = m_goItemRender.name;
                go.transform.SetParent(m_content, false);
                go.SetActive(true);
                var script = go.AddComponent(m_itemRenderType) as ItemRender;
                if (!go.activeInHierarchy)
                    script.Awake();
                script.SetData(m_data[index]);
                script.m_owner = this;
                if (useClickEvent)
                    UGUIClickHandler.Get(go).onPointerClick += OnItemClick;
                if (m_toggleGroup != null)
                {
                    var toggle = go.GetComponent<Toggle>();
                    if (toggle != null)
                    {
                        toggle.group = m_toggleGroup;

                        //使用循环模式的话不能用渐变效果，否则视觉上会出现破绽
                        if (useLoopItems)
                            toggle.toggleTransition = Toggle.ToggleTransition.None;
                    }
                }
                m_items.Add(script);
            }
        }
    }

    /// <summary>
    /// 选择指定项
    /// </summary>
    /// <param name="index"></param>
    public void Select(int index)
    {
        if (index >= m_data.Length)
            return;

        if (m_data[index] != m_selectedData)
            SelectItem(m_data[index]);

        UpdateView();
    }

    /// <summary>
    /// 开启或关闭某一项的响应
    /// </summary>
    /// <param name="index"></param>
    public void Enable(int index, bool isEnable = true)
    {
        if (index >= m_items.Count || index < 0)
        {
            return;
        }

        var toggle = m_items[index].GetComponent<Toggle>();
        if (toggle == null)
        {
            return;
        }

        toggle.isOn = isEnable;
        toggle.enabled = isEnable;
        if (isEnable)
        {
            UGUIClickHandler.Get(toggle.gameObject).onPointerClick += OnItemClick;
        }
        else
        {
            UGUIClickHandler.Get(toggle.gameObject).RemoveAllHandler();
        }
    }

    /// <summary>
    /// 选择指定项
    /// </summary>
    /// <param name="renderData"></param>
    public void Select(object renderData)
    {
        if (renderData == null)
        {
            SelectItem(null);
            UpdateView();
            return;
        }

        for (int i = 0; i < m_data.Length; i++)
        {
            if (m_data[i] == renderData)
            {
                SelectItem(m_data[i]);
                UpdateView();
                break;
            }
        }
    }

    //滑动之后
    private void OnScroll(Vector2 data)
    {
        var value = (ContentSpace - ViewSpace) * (m_isVertical ? data.y : 1 - data.x);
        var start = ContentSpace - value - ViewSpace;
        var startIndex = Mathf.FloorToInt(start / m_itemSpace) * ConstraintCount;
        startIndex = Mathf.Max(0, startIndex);

        if (startIndex != m_startIndex)
        {
            m_startIndex = startIndex;
            UpdateView();
        }
    }

    //选择列表item
    private void SelectItem(object renderData)
    {
        m_selectedData = renderData;
        if (onItemSelected != null)
            onItemSelected(m_selectedData);
    }

    //点击item
    private void OnItemClick(GameObject target, BaseEventData baseEventData)
    {
        var renderData = target.GetComponent<ItemRender>().m_renderData;
        if (useLoopItems && renderData == m_selectedData)
        {
            var toggle = target.GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = true;
            }
        }
        SelectItem(renderData);
    }

    //设置选中效果
    private void SetToggle(int index, bool value)
    {
        if (index < m_items.Count)
        {
            var toggle = m_items[index].GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = value;
            }
        }
    }
}