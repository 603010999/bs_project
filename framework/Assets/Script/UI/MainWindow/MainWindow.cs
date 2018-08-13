using UnityEngine;
using System.Collections;

public class MainWindow : UIWindowBase 
{

    //UI的初始化请放在这里
    public override void OnOpen()
    {
        
    }

    //请在这里写UI的更新逻辑，当该UI监听的事件触发时，该函数会被调用
    public override void OnRefresh()
    {

    }

    //UI的进入动画
    public override IEnumerator EnterAnim(UIAnimCallBack animComplete, UICallBack callBack, params object[] objs)
    {
        AnimSystem.UguiAlpha(gameObject, 0, 1, callBack:(object[] obj)=>
        {
            StartCoroutine(base.EnterAnim(animComplete, callBack, objs));
        });

        yield return new WaitForEndOfFrame();
    }

    //UI的退出动画
    public override IEnumerator ExitAnim(UIAnimCallBack animComplete, UICallBack callBack, params object[] objs)
    {
        AnimSystem.UguiAlpha(gameObject , null, 0, callBack:(object[] obj) =>
        {
            StartCoroutine(base.ExitAnim(animComplete, callBack, objs));
        });

        yield return new WaitForEndOfFrame();
    }

    public void OnClickGameStart()
    {
        ApplicationStatusManager.Instance.EnterStatus<GameStatus>();
    }

    public void OnClickShop()
    {
        UIManager.OpenUIWindow<ShopWindow>();
    }

    public void OnClickSetting()
    {
        
    }
}