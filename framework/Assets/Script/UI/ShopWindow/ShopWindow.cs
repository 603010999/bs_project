using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShopWindow : UIWindowBase 
{
    public ReusingScrollRect m_ShopItem;

    //UI的初始化请放在这里
    public override void OnOpen()
    {
        m_ShopItem.Init(UIEventKey, "ShopWindow_Item");

        m_ShopItem.SetData(GetShopData());
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

        AnimSystem.UguiMove(m_uiRoot, new Vector3(1000, 0, 0), Vector3.zero, time: 1, interp: InterpType.InOutBack);

        yield return new WaitForEndOfFrame();
    }

    //UI的退出动画
    public override IEnumerator ExitAnim(UIAnimCallBack animComplete, UICallBack callBack, params object[] objs)
    {
        AnimSystem.UguiAlpha(gameObject , null, 0, callBack:(object[] obj) =>
        {
            StartCoroutine(base.ExitAnim(animComplete, callBack, objs));
        });

        AnimSystem.UguiMove(m_uiRoot,Vector3.zero, new Vector3(1000, 0, 0),time:1,interp:InterpType.InOutBack);

        yield return new WaitForEndOfFrame();
    }

    List<Dictionary<string,object>> GetShopData()
    {
        var data = new List<Dictionary<string, object>>();

        DataTable itemData = DataManager.GetData("item");

        for (int i = 0; i < itemData.TableIDs.Count; i++)
        {
            Dictionary<string, object> tmp = new Dictionary<string, object>();

            tmp.Add("Name",DataGenerateManager<itemGenerate>.GetData(itemData.TableIDs[i]).m_ItemName);
            tmp.Add("Cost", DataGenerateManager<itemGenerate>.GetData(itemData.TableIDs[i]).m_key);

            data.Add(tmp);
        }

        return data;
    }

    public void OnClickClose()
    {
        UIManager.CloseUIWindow(this);
    }
}