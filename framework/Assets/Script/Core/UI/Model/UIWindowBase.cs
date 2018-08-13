using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIWindowBase : UIBase 
{
    public UIType m_UIType;

    public GameObject m_bgMask;
    public GameObject m_uiRoot;


    #region 重载方法

    public virtual void OnOpen()
    {
    }

    public virtual void OnClose()
    {

    }

    public virtual void OnHide()
    {

    }

    public virtual void OnShow()
    {

    }

    public virtual void OnRefresh()
    {
    }

    public virtual IEnumerator EnterAnim(UIAnimCallBack animComplete, UICallBack callBack,params object[] objs)
    {
        //默认无动画
        animComplete(this, callBack, objs);

        yield break;
    }

    public virtual void OnCompleteEnterAnim()
    {
    }

    public virtual IEnumerator ExitAnim(UIAnimCallBack animComplete, UICallBack callBack, params object[] objs)
    {
        //默认无动画
        animComplete(this, callBack, objs);

        yield break;
    }

    public virtual void OnCompleteExitAnim()
    {
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion 

    #region 继承方法


    //刷新是主动调用
    public void Refresh(params object[] args)
    {
        OnRefresh();
    }

    #endregion
}


