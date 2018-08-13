using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ApplicationStatusManager : Singleton<ApplicationStatusManager>
{
    /// <summary>
    /// 当前程序在哪个状态
    /// </summary>
    private IApplicationStatus m_currentAppStatus { set; get; }

    //可切换状态
    private static Dictionary<string, IApplicationStatus> s_status = new Dictionary<string, IApplicationStatus>();

    public ApplicationStatusManager()
    {
        ApplicationManager.m_onApplicationUpdate += AppUpdate;
        ApplicationManager.m_onApplicationOnGUI += AppOnGUI;
    }

    private void AppOnGUI()
    {
        if (m_currentAppStatus != null)
        {
            m_currentAppStatus.OnGUI();
        }
    }

    /// <summary>
    /// 应用程序每帧调用
    /// </summary>
    private void AppUpdate()
    {
        if (m_currentAppStatus != null)
        {
            m_currentAppStatus.OnUpdate();
        }
    }

    /// <summary>
    /// 切换游戏正常状态
    /// </summary>
    /// <param name="l_appStatus"></param>
    public void EnterStatus<T>() where T : IApplicationStatus
    {
        EnterStatus(typeof(T).Name);
    }

    public void EnterStatus(string statusName)
    {
        if (m_currentAppStatus != null)
        {
            m_currentAppStatus.CloseAllUI();
            m_currentAppStatus.OnExitStatus();
        }

        m_currentAppStatus = GetStatus(statusName);

        ApplicationManager.Instance.StartCoroutine(m_currentAppStatus.InChangeScene(() =>
        {
            m_currentAppStatus.OnEnterStatus();
        }));
    }

    public T GetStatus<T>() where T : IApplicationStatus
    {
        return (T) GetStatus(typeof(T).Name);
    }

    private IApplicationStatus GetStatus(string statusName)
    {
        if (s_status.ContainsKey(statusName))
        {
            return s_status[statusName];
        }
        else
        {
            IApplicationStatus statusTmp = (IApplicationStatus) Activator.CreateInstance(Type.GetType(statusName));
            s_status.Add(statusName, statusTmp);

            return statusTmp;
        }
    }

    /// <summary>
    /// 测试模式，流程入口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void EnterTestModel<T>() where T : IApplicationStatus
    {
        EnterTestModel(typeof(T).Name);
    }

    public void EnterTestModel(string statusName)
    {
        if (m_currentAppStatus != null)
        {
            m_currentAppStatus.CloseAllUI();
            m_currentAppStatus.OnExitStatus();
        }

        m_currentAppStatus = GetStatus(statusName);

        ApplicationManager.Instance.StartCoroutine(m_currentAppStatus.InChangeScene(() =>
        {
            m_currentAppStatus.EnterStatusTestData();
            m_currentAppStatus.OnEnterStatus();
        }));
    }
}
