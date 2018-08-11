using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum AppMode
{
    Developing,
    QA,
    Release
}

public delegate void ApplicationBoolCallback(bool status);
public delegate void ApplicationVoidCallback();

public class ApplicationManager : MonoBehaviour 
{
    //管理器实例
    private static ApplicationManager instance;
    public static ApplicationManager Instance
    {
        get { return ApplicationManager.instance; }
        set { ApplicationManager.instance = value; }
    }

    public AppMode m_AppMode = AppMode.Developing;    
    public static AppMode AppMode
    {
        get { return instance.m_AppMode; }
    }

    //是否是ab模式
    public bool m_useAssetsBundle = false;
    public bool UseAssetsBundle
    {
        get
        {
            return m_useAssetsBundle;
        }
    }

    //快速启动
    public bool m_quickLunch = true;

    //当前状态名称
    [HideInInspector]
    public string m_Status { set; get; }

    [HideInInspector]
    public List<string> m_globalLogic;

    public void Awake()
    {
        m_Status = "DemoStatus";
        instance = this;
        AppLaunch();
    }

    /// <summary>
    /// 程序启动
    /// </summary>
    private void AppLaunch()
    {
        //处理常驻
        DontDestroyOnLoad(gameObject);
        
        //设置资源加载类型
        SetResourceLoadType();
        
        //资源路径管理器启动  载入配置
        ResourcesConfigManager.Initialize(); 

        //内存管理初始化
        MemoryManager.Init();           
        
        //计时器启动
        Timer.Init();           
        
        //输入管理器启动
        InputManager.Init();                 

#if !UNITY_WEBGL
        //UIManager启动
        UIManager.Init();                    
#else
        //异步加载UIManager
        UIManager.InitAsync();               
#endif

        //游戏流程状态机初始化
        ApplicationStatusManager.Init();     
        
        //初始化全局逻辑
        GlobalLogicManager.Init();           

        if (AppMode != AppMode.Release)
        {
            GUIConsole.Init(); //运行时Console

            DevelopReplayManager.OnLunchCallBack += () =>
            {
                //全局逻辑
                InitGlobalLogic();          
                
                //可以从此处进入测试流程
                ApplicationStatusManager.EnterTestModel(m_Status);
            };

            //开发者复盘管理器 
            DevelopReplayManager.Init(m_quickLunch);                              
        }
        else
        {
            Log.Init(false); //关闭 Debug

            //全局逻辑
            InitGlobalLogic();
            
            //游戏流程状态机，开始第一个状态
            ApplicationStatusManager.EnterStatus(m_Status);
        }
    }

    #region 程序生命周期事件派发
 
    //退出
    public static ApplicationVoidCallback m_onApplicationQuit = null;
    
    //暂停  （手机上失去焦点）
    public static ApplicationBoolCallback m_onApplicationPause = null;
    
    //恢复焦点
    public static ApplicationBoolCallback m_onApplicationFocus = null;
    
    //update 每帧调用一次
    public static ApplicationVoidCallback m_onApplicationUpdate = null;
    
    //fixed update 按照时间间隔调用，和帧数无关 可以设置调用时间step  Edit->ProjectSetting->time  找到Fixedtimestep
    public static ApplicationVoidCallback m_onApplicationFixedUpdate = null;
    
    //GUI渲染后调用
    public static ApplicationVoidCallback m_onApplicationOnGUI = null;
    
    //Gizmos（编辑器辅助线）渲染后调用
    public static ApplicationVoidCallback m_onApplicationOnDrawGizmos = null;
    
    //所有update执行完之后开始执行
    public static ApplicationVoidCallback m_onApplicationLateUpdate = null;

    void OnApplicationQuit()
    {
        if (m_onApplicationQuit != null)
        {
            try
            {
                m_onApplicationQuit();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }

    /*
     * 强制暂停时，先 OnApplicationPause，后 OnApplicationFocus
     * 重新“启动”游戏时，先OnApplicationFocus，后 OnApplicationPause
     */
    void OnApplicationPause(bool pauseStatus)
    {
        if (m_onApplicationPause != null)
        {
            try
            {
                m_onApplicationPause(pauseStatus);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }

    void OnApplicationFocus(bool focusStatus)
    {
        if (m_onApplicationFocus != null)
        {
            try
            {
                m_onApplicationFocus(focusStatus);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }

    void Update()
    {
        if (m_onApplicationUpdate != null)
            m_onApplicationUpdate();
    }

    private void LateUpdate()
    {
        if(m_onApplicationLateUpdate != null)
        {
            m_onApplicationLateUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (m_onApplicationFixedUpdate != null)
            m_onApplicationFixedUpdate();
    }

    void OnGUI()
    {
        if (m_onApplicationOnGUI != null)
            m_onApplicationOnGUI();
    }

    private void OnDrawGizmos()
    {
        if (m_onApplicationOnDrawGizmos != null)
            m_onApplicationOnDrawGizmos();
    }

    #endregion

    #region 程序启动细节
    /// <summary>
    /// 设置资源加载方式
    /// </summary>
    void SetResourceLoadType()
    {
        if (UseAssetsBundle)
        {
            ResourceManager.m_gameLoadType = ResLoadLocation.Streaming;
        }
        else
        {
            ResourceManager.m_gameLoadType = ResLoadLocation.Resource;
        }
    }

    /// <summary>
    /// 初始化全局逻辑
    /// </summary>
    void InitGlobalLogic()
    {
        for (int i = 0; i < m_globalLogic.Count; i++)
        {
            GlobalLogicManager.InitLogic(m_globalLogic[i]);
        }
    }
#endregion
}
