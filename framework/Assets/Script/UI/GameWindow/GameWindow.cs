using UnityEngine;
using System.Collections;

public class GameWindow : UIWindowBase
{
    public GameObject m_goTextTitle;

    public GameObject m_goReturnBtn;
    
    
    //UI的初始化请放在这里
    public override void OnOpen()
    {
        
    }

    //请在这里写UI的更新逻辑，当该UI监听的事件触发时，该函数会被调用
    public override void OnRefresh()
    {

    }

    //UI的进入动画
    public override IEnumerator EnterAnim(UIAnimCallBack l_animComplete, UICallBack l_callBack, params object[] objs)
    {
        yield return new WaitForSeconds(0.2f);

        AnimSystem.UguiAlpha(gameObject, 0, 1, callBack:(object[] obj)=>
        {
            StartCoroutine(base.EnterAnim(l_animComplete, l_callBack, objs));
        });

        AnimSystem.UguiMove(m_goTextTitle, new Vector3(0, 50, 0), new Vector3(0, -50, 0));
        AnimSystem.UguiMove(m_goReturnBtn, new Vector3(0, -70, 0), new Vector3(0, 70, 0));

        yield return new WaitForEndOfFrame();
    }

    //UI的退出动画
    public override IEnumerator ExitAnim(UIAnimCallBack l_animComplete, UICallBack l_callBack, params object[] objs)
    {
        AnimSystem.UguiAlpha(gameObject , null, 0, callBack:(object[] obj) =>
        {
            StartCoroutine(base.ExitAnim(l_animComplete, l_callBack, objs));
        });

        AnimSystem.UguiMove(m_goTextTitle, new Vector3(0, -50, 0), new Vector3(0, 50, 0));
        AnimSystem.UguiMove(m_goReturnBtn, new Vector3(0, 70, 0), new Vector3(0, -70, 0));

        yield return new WaitForEndOfFrame();
    }

    public void OnClickReturnMainMenu()
    {
        ApplicationStatusManager.EnterStatus<DemoStatus>();
    }
}