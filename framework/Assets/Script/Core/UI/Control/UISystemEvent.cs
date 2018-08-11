using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UISystemEvent
{
    //事件派发记录  key：uiname value<(int)UIEvent,callback>
    public static Dictionary<string, Dictionary<int, UICallBack>> m_uiEventsDic =
        new Dictionary<string, Dictionary<int, UICallBack>>();

    /// <summary>
    /// 注册单个UI派发的事件
    /// </summary>
    /// <param name="Event">事件类型</param>
    /// <param name="callback">回调函数</param>
    public static void RegisterEvent(string uiName, UIEvent uiEvent, UICallBack callBack)
    {
        Dictionary<int, UICallBack> dic = null;

        if (!m_uiEventsDic.TryGetValue(uiName, out dic))
        {
            dic = new Dictionary<int, UICallBack>();
            m_uiEventsDic.Add(uiName, dic);
        }
        
        //这里需要注意，防止内存泄漏，注册数量必须可控制
        
        //同一个界面，只留一个事件回调   如果有多个相同界面需求，增加UIID
        dic[(int) uiEvent] = callBack;
    }

    /// <summary>
    /// 派发事件
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="uiEvent"></param>
    /// <param name="param"></param>
    public static void Dispatch(UIWindowBase ui, UIEvent uiEvent,params object[] param)
    {
        if (ui == null)
        {
            Debug.LogError("Dispatch ui is null!");
            return;
        }

        if (!m_uiEventsDic.ContainsKey(ui.name))
        {
            return;
        }

        var eventId = (int) uiEvent;
        
        if (!m_uiEventsDic[ui.name].ContainsKey(eventId))
        {
            return;
        }
        
        try
        {
            if (m_uiEventsDic[ui.name][eventId] != null)
            {
                m_uiEventsDic[ui.name][eventId](ui, param);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("UISystemEvent Dispatch error:" + e.ToString());
        }
    }
}
