using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollRectInput : ScrollRect
{
    //事件key
    public string m_uiEventKey;
    
    //事件注册
    private InputEventRegisterInfo<InputUIOnScrollEvent> m_register;

    public virtual void Init(string uiEventKey)
    {
        m_uiEventKey = uiEventKey;
        m_register = InputUIEventProxy.GetOnScrollListener(m_uiEventKey, name, OnSetContentAnchoredPosition);
    }

    public virtual void Dispose()
    {
        m_register.RemoveListener(true);
    }

    protected override void SetContentAnchoredPosition(Vector2 position)
    {
        InputUIEventProxy.DispatchScrollEvent(m_uiEventKey, name,"", position);
    }

    protected virtual void OnSetContentAnchoredPosition(InputUIOnScrollEvent e)
    {
        base.SetContentAnchoredPosition(e.m_pos);
    }

}
