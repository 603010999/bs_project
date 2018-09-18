using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

/// <summary>
/// UI回调
/// </summary>
/// <param name="objs"></param>
public delegate void UICallBack(UIWindowBase UI, params object[] objs);
public delegate void UIAnimCallBack(UIWindowBase UIbase, UICallBack callBack, params object[] objs);

//UI类型
public enum UIType
{
    //游戏内部UI，最底层  一般是血条，飘字浮动UI
    GameUI,
    
    //固定UI，一般是常驻的UI，例如主界面
    Fixed,
    
    //普通UI，普通的逻辑UI
    Normal,
    
    //略高于普通UI，一般是显示金钱的数值之类
    TopBar,
    
    //最高层弹窗
    PopUp,
}

//UI事件
public enum UIEvent
{
    //开
    OnOpen,
    
    //关
    OnClose,
    
    //隐藏
    OnHide,
    
    //显示
    OnShow,

    //初始化
    OnInit,
    
    //销毁
    OnDestroy,

    //刷新
    OnRefresh,

    //开始进入动画
    OnStartEnterAnim,
    
    //完成进入动画
    OnCompleteEnterAnim,
    
    //开始退出动画
    OnStartExitAnim,
    
    //完成退出动画
    OnCompleteExitAnim,
}


[RequireComponent(typeof(UILayerManager))]
[RequireComponent(typeof(UIAnimManager))]
public class UIManager : MonoBehaviour
{
    //管理器实例
    public static GameObject m_instance;
    
    //UI层级管理器
    public static UILayerManager m_uiLayerManager; 
    
    //UI动画管理器
    public static UIAnimManager m_uiAnimManager;   
    
    //UICamera
    public static Camera m_uiCamera;              

    //打开的UI
    static public Dictionary<string, List<UIWindowBase>> m_curShowUIs = new Dictionary<string, List<UIWindowBase>>();
    
    //隐藏的UI
    static public Dictionary<string, List<UIWindowBase>> m_hideUIs = new Dictionary<string, List<UIWindowBase>>();

    #region 初始化

    public static void Init()
    {
        var instance = GameObject.Find("UIManager");

        if (instance == null)
        {
            instance = GameObjectManager.Instance.CreatePoolObject("UIManager");
        }

        m_instance = instance;

        m_uiLayerManager = instance.GetComponent<UILayerManager>();
        m_uiAnimManager  = instance.GetComponent<UIAnimManager>();
        m_uiCamera       = instance.GetComponentInChildren<Camera>();

        DontDestroyOnLoad(instance);
    }

#endregion

#region UI的打开与关闭方法

    /// <summary>
    /// 创建UI,存放在Hide列表中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CreateUIWindow<T>() where T : UIWindowBase
    {
        return (T)CreateUIWindow(typeof(T).Name);
    }
    
    /// <summary>
    /// 创建UI,存放在Hide列表中
    /// </summary>
    /// <param name="UIName"></param>
    /// <returns></returns>
    public static UIWindowBase CreateUIWindow(string UIName)
    {
        var uiTmp = GameObjectManager.Instance.CreatePoolObject(UIName, m_instance);
        var uiBase = uiTmp.GetComponent<UIWindowBase>();
        
        //派发OnInit事件
        UISystemEvent.Dispatch(uiBase, UIEvent.OnInit);
        
        try
        {
            uiBase.Init(GetUIID(UIName));
        }
        catch(Exception e)
        {
            Debug.LogError("OnInit Exception: " + e.ToString());
        }

        AddHideUI(uiBase);

        m_uiLayerManager.SetLayer(uiBase);      //设置层级

        return uiBase;
    }

    /// <summary>
    /// 打开UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T OpenUIWindow<T>() where T : UIWindowBase
    {
        return (T)OpenUIWindow(typeof(T).Name);
    }
    
    /// <summary>
    /// 打开UI
    /// </summary>
    /// <param name="UIName">UI名</param>
    /// <param name="callback">动画播放完毕回调</param>
    /// <param name="objs">回调传参</param>`
    /// <returns>返回打开的UI</returns>
    public static UIWindowBase OpenUIWindow(string UIName, UICallBack callback = null, params object[] objs)
    {
        //从隐藏UI中取出
        var uiBase = GetHideUI(UIName);

        if (uiBase == null)
        {
            uiBase = CreateUIWindow(UIName);
        }

        //从隐藏UI中删除
        RemoveHideUI(uiBase);
        
        //加入显示中UI
        AddUI(uiBase);

        //派发OnOpen事件
        UISystemEvent.Dispatch(uiBase, UIEvent.OnOpen);  
        try
        {
            uiBase.OnOpen();
        }
        catch (Exception e)
        {
            Debug.LogError(UIName + " OnOpen Exception: " + e.ToString());
        }

        //设置层级
        m_uiLayerManager.SetLayer(uiBase);
        
        //播放动画
        m_uiAnimManager.StartEnterAnim(uiBase, callback, objs); 
        
        return uiBase;
    }

    /// <summary>
    /// 关闭UI
    /// </summary>
    /// <param name="UI">目标UI</param>
    /// <param name="isPlayAnim">是否播放关闭动画</param>
    /// <param name="callback">动画播放完毕回调</param>
    /// <param name="objs">回调传参</param>
    public static void CloseUIWindow(UIWindowBase UI,bool isPlayAnim = true ,UICallBack callback = null, params object[] objs)
    {
        //从显示UI中移除UI
        RemoveUI(UI); 
        
        //层级管理
        m_uiLayerManager.RemoveUI(UI);

        //是否有退出动画
        if (isPlayAnim)
        {
            //动画播放完毕删除UI
            if (callback != null)
            {
                callback += CloseUIWindowCallBack;
            }
            else
            {
                callback = CloseUIWindowCallBack;
            }

            m_uiAnimManager.StartExitAnim(UI, callback, objs);
        }
        else
        {
            CloseUIWindowCallBack(UI, objs);
        }
    }
    
    /// <summary>
    /// 关闭UI回调，最终加入隐藏UI中
    /// </summary>
    /// <param name="UI"></param>
    /// <param name="objs"></param>
    private static void CloseUIWindowCallBack(UIWindowBase UI, params object[] objs)
    {
        //派发OnClose事件
        UISystemEvent.Dispatch(UI, UIEvent.OnClose);
        
        try
        {
            UI.OnClose();
        }
        catch (Exception e)
        {
            Debug.LogError(UI.UIName + " OnClose Exception: " + e.ToString());
        }

        AddHideUI(UI);
    }
    
    //关闭UI
    public static void CloseUIWindow(string UIname, bool isPlayAnim = true, UICallBack callback = null, params object[] objs)
    {
        var ui = GetUI(UIname);

        if (ui == null)
        {
            Debug.LogError("CloseUIWindow Error UI ->" + UIname + "<-  not Exist!");
        }
        else
        {
            CloseUIWindow(GetUI(UIname), isPlayAnim, callback, objs);
        }
    }

    //关闭UI窗口
    public static void CloseUIWindow<T>(bool isPlayAnim = true, UICallBack callback = null, params object[] objs) where T : UIWindowBase
    {
        CloseUIWindow(typeof(T).Name, isPlayAnim,callback, objs);
    }

    /// <summary>
    /// 显示某个UI
    /// </summary>
    /// <param name="UIname"></param>
    /// <returns></returns>
    public static UIWindowBase ShowUI(string UIname)
    {
        UIWindowBase ui = GetUI(UIname);
        return ShowUI(ui);
    }

    /// <summary>
    /// 显示某个UI
    /// </summary>
    /// <param name="ui"></param>
    /// <returns></returns>
    public static UIWindowBase ShowUI(UIWindowBase ui)
    {
        UISystemEvent.Dispatch(ui, UIEvent.OnShow);  //派发OnShow事件
        try
        {
            ui.Show();
            ui.OnShow();
        }
        catch (Exception e)
        {
            Debug.LogError(ui.UIName + " OnShow Exception: " + e.ToString());
        }

        return ui;
    }

    /// <summary>
    /// 隐藏某个UI
    /// </summary>
    /// <param name="UIname"></param>
    /// <returns></returns>
    public static UIWindowBase HideUI(string UIname)
    {
        var ui = GetUI(UIname);
        return HideUI(ui);
    }

    /// <summary>
    /// 隐藏某个UI
    /// </summary>
    /// <param name="ui"></param>
    /// <returns></returns>
    public static UIWindowBase HideUI(UIWindowBase ui)
    {
        UISystemEvent.Dispatch(ui, UIEvent.OnHide);  //派发OnHide事件

        try
        {
            ui.Hide();
            ui.OnHide();
        }
        catch (Exception e)
        {
            Debug.LogError(ui.UIName + " OnShow Exception: " + e.ToString());
        }

        return ui;
    }

    /// <summary>
    /// 隐藏除了参数UI外的其他所有显示中的UI
    /// </summary>
    /// <param name="UIName"></param>
    public static void HideOtherUI(string UIName)
    {
        var keys = new List<string>(m_curShowUIs.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            var list = m_curShowUIs[keys[i]];
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].UIName != UIName)
                {
                    HideUI(list[j]);
                }
            }
        }
    }

    public static void ShowOtherUI(string UIName)
    {
        var keys = new List<string>(m_curShowUIs.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            List<UIWindowBase> list = m_curShowUIs[keys[i]];
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].UIName != UIName)
                {
                    ShowUI(list[j]);
                }
            }
        }
    }

    /// <summary>
    /// 移除全部UI
    /// </summary>
    public static void CloseAllUI(bool isPlayerAnim = false)
    {
        List<string> keys = new List<string>(m_curShowUIs.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            List<UIWindowBase> list = m_curShowUIs[keys[i]];
            for(int j = 0;j<list.Count;j++)
            {
                CloseUIWindow(list[i], isPlayerAnim);
            }
        }
    }

#endregion

#region UI的打开与关闭 异步方法

    public static void OpenUIAsync<T>( UICallBack callback, params object[] objs) where T : UIWindowBase
    {
        string UIName = typeof(T).Name;
        OpenUIAsync(UIName,callback,objs);
    }

    public static void OpenUIAsync(string UIName , UICallBack callback, params object[] objs)
    {
     /*   ResourceManager.LoadAsync(UIName, (loadState,resObject) =>
         {
             if(loadState.isDone)
             {
                 OpenUIWindow(UIName, callback, objs);
             }
         });
      */   
    }

#endregion

#region UI内存管理

    public static void DestroyUI(UIWindowBase UI)
    {
        Debug.Log("UIManager DestroyUI " + UI.name);

        if (GetIsExitsHide(UI))
        {
            RemoveHideUI(UI);
        }
        else if(GetIsExits(UI))
        {
            RemoveUI(UI);   
        }

        UISystemEvent.Dispatch(UI, UIEvent.OnDestroy);  //派发OnDestroy事件
        try
        {
            UI.DestroyUI();
        }
        catch(Exception e)
        {
            Debug.LogError("OnDestroy :" + e.ToString());
        }
        Destroy(UI.gameObject);
    }

    public static void DestroyAllUI()
    {
        DestroyAllActiveUI();
        DestroyAllHideUI();
    }

#endregion

#region 打开UI列表的管理

    /// <summary>
    /// 删除所有打开的UI
    /// </summary>
    public static void DestroyAllActiveUI()
    {
        foreach (List<UIWindowBase> uis in m_curShowUIs.Values)
        {
            for (int i = 0; i < uis.Count; i++)
            {
                UISystemEvent.Dispatch(uis[i], UIEvent.OnDestroy);  //派发OnDestroy事件
                try
                {
                    uis[i].DestroyUI();
                }
                catch (Exception e)
                {
                    Debug.LogError("OnDestroy :" + e.ToString());
                }
                Destroy(uis[i].gameObject);
            }
        }

        m_curShowUIs.Clear();
    }
    
    /// <summary>
    /// 获取UI实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetUI<T>() where T : UIWindowBase
    {
        return (T)GetUI(typeof(T).Name);
    }
    
    /// <summary>
    /// 获取UI实例
    /// </summary>
    /// <param name="UIname"></param>
    /// <returns></returns>
    public static UIWindowBase GetUI(string UIname)
    {
        if (!m_curShowUIs.ContainsKey(UIname))
        {
            //Debug.Log("!ContainsKey " + l_UIname);
            return null;
        }
        else
        {
            if (m_curShowUIs[UIname].Count == 0)
            {
                //Debug.Log("s_UIs[UIname].Count == 0");
                return null;
            }
            else
            {
                //默认返回最后创建的那一个
                return m_curShowUIs[UIname][m_curShowUIs[UIname].Count - 1];
            }
        }
    }

    /// <summary>
    /// 是否存在UI
    /// </summary>
    /// <param name="UI"></param>
    /// <returns></returns>
    private static bool GetIsExits(UIWindowBase UI)
    {
        if (!m_curShowUIs.ContainsKey(UI.name))
        {
            return false;
        }
        else
        {
            return m_curShowUIs[UI.name].Contains(UI);
        }
    }

    /// <summary>
    /// 添加UI
    /// </summary>
    /// <param name="UI"></param>
    private static void AddUI(UIWindowBase UI)
    {
        if (!m_curShowUIs.ContainsKey(UI.name))
        {
            m_curShowUIs.Add(UI.name, new List<UIWindowBase>());
        }

        m_curShowUIs[UI.name].Add(UI);

        UI.Show();
    }

    /// <summary>
    /// 移除UI
    /// </summary>
    /// <param name="UI"></param>
    /// <exception cref="Exception"></exception>
    private static void RemoveUI(UIWindowBase UI)
    {
        if (UI == null)
        {
            throw new Exception("UIManager: RemoveUI error l_UI is null: !");
        }

        if (!m_curShowUIs.ContainsKey(UI.name))
        {
            throw new Exception("UIManager: RemoveUI error dont find UI name: ->" + UI.name + "<-  " + UI);
        }

        if (!m_curShowUIs[UI.name].Contains(UI))
        {
            throw new Exception("UIManager: RemoveUI error dont find UI: ->" + UI.name + "<-  " + UI);
        }
        else
        {
            m_curShowUIs[UI.name].Remove(UI);
        }
    }

    /// <summary>
    /// 获取UI ID
    /// </summary>
    /// <param name="UIname"></param>
    /// <returns></returns>
    static int GetUIID(string UIname)
    {
        if (!m_curShowUIs.ContainsKey(UIname))
        {
            return 0;
        }
        else
        {
            int id = m_curShowUIs[UIname].Count;

            for (int i = 0; i < m_curShowUIs[UIname].Count; i++)
			{
			    if(m_curShowUIs[UIname][i].UIID == id)
                {
                    id++;
                    i = 0;
                }
			}

            return id;
        }
    }

#endregion

#region 隐藏UI列表的管理

    /// <summary>
    /// 删除所有隐藏的UI
    /// </summary>
    public static void DestroyAllHideUI()
    {
        foreach (List<UIWindowBase> uis in m_hideUIs.Values)
        {
            for (int i = 0; i < uis.Count; i++)
            {
                UISystemEvent.Dispatch(uis[i], UIEvent.OnDestroy);  //派发OnDestroy事件
                try
                {
                    uis[i].DestroyUI();
                }
                catch (Exception e)
                {
                    Debug.LogError("OnDestroy :" + e.ToString());
                }
                Destroy(uis[i].gameObject);
            }
        }

        m_hideUIs.Clear();
    }

    /// <summary>
    /// 获取一个隐藏的UI,如果有多个同名UI，则返回最后创建的那一个
    /// </summary>
    /// <param name="UIname">UI名</param>
    /// <returns></returns>
    private static UIWindowBase GetHideUI(string UIname)
    {
        if (!m_hideUIs.ContainsKey(UIname))
        {
            return null;
        }
        else
        {
            if (m_hideUIs[UIname].Count == 0)
            {
                return null;
            }
            else
            {
                UIWindowBase ui = m_hideUIs[UIname][m_hideUIs[UIname].Count - 1];
                //默认返回最后创建的那一个
                return ui;
            }
        }
    }

    /// <summary>
    /// 某个UI是否在隐藏中
    /// </summary>
    /// <param name="UI"></param>
    /// <returns></returns>
    private static bool GetIsExitsHide(UIWindowBase UI)
    {
        if (!m_hideUIs.ContainsKey(UI.name))
        {
            return false;
        }
        else
        {
            return m_hideUIs[UI.name].Contains(UI);
        }
    }

    /// <summary>
    /// 加入隐藏列表，并且隐藏
    /// </summary>
    /// <param name="UI"></param>
    private static void AddHideUI(UIWindowBase UI)
    {
        if (!m_hideUIs.ContainsKey(UI.name))
        {
            m_hideUIs.Add(UI.name, new List<UIWindowBase>());
        }

        m_hideUIs[UI.name].Add(UI);

        UI.Hide();
    }

    /// <summary>
    /// 从隐藏列表中去掉
    /// </summary>
    /// <param name="UI"></param>
    /// <exception cref="Exception"></exception>
    private static void RemoveHideUI(UIWindowBase UI)
    {
        if (UI == null)
        {
            throw new Exception("UIManager: RemoveUI error l_UI is null: !");
        }

        if (!m_hideUIs.ContainsKey(UI.name))
        {
            throw new Exception("UIManager: RemoveUI error dont find: " + UI.name + "  " + UI);
        }

        if (!m_hideUIs[UI.name].Contains(UI))
        {
            throw new Exception("UIManager: RemoveUI error dont find: " + UI.name + "  " + UI);
        }
        else
        {
            m_hideUIs[UI.name].Remove(UI);
        }
    }

#endregion
}
