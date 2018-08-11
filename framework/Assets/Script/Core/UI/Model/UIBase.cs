using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class UIBase : MonoBehaviour
{
    //画布
    public Canvas m_canvas;

    //销毁UI
    public void DestroyUI()
    {
        OnUIDestroy();
    }
    
    #region 重载方法
    //当UI第一次打开时调用OnInit方法，调用时机在OnOpen之前
    protected virtual void OnInit()
    {
        
    }

    //销毁UI吼调用
    protected virtual void OnUIDestroy()
    {

    }

    #endregion
    
    private int m_UIID = -1;

    public int UIID
    {
        get { return m_UIID; }
        //set { m_UIID = value; }
    }

    public string UIEventKey
    {
        get { return UIName + m_UIID; }
        //set { m_UIID = value; }
    }

    string m_UIName = null;
    public string UIName
    {
        get
        {
            if (m_UIName == null)
            {
                m_UIName = name;
            }

            return m_UIName;
        }
        set
        {
            m_UIName = value;
        }
    }

    public void Init(int id)
    {
        m_UIID = id;
        m_canvas = GetComponent<Canvas>();
        m_UIName = null;
        OnInit();
    }

    #region 获取对象

    private RectTransform m_rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (m_rectTransform == null)
            {
                m_rectTransform = GetComponent<RectTransform>();
            }

            return m_rectTransform;
        }
        set { m_rectTransform = value; }
    }

    #endregion
}
