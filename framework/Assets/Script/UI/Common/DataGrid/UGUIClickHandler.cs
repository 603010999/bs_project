
using UnityEngine;
using UnityEngine.EventSystems;

public class UGUIClickHandler : MonoBehaviour, IPointerClickHandler
{
    public delegate void PointerEvetCallBackFunc(GameObject target, PointerEventData eventData);

    public event PointerEvetCallBackFunc onPointerClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Input.touchCount > 1)
        {
            return;
        }

        if (onPointerClick != null)
        {
            onPointerClick(gameObject, eventData);
        }
        else
        {
            Debug.LogError("系统暂未开放");
        }
    }

    public void RemoveAllHandler(bool isDestroy = true)
    {
        onPointerClick = null;
        if (isDestroy) DestroyImmediate(this);
    }

    public static UGUIClickHandler Get(GameObject go)
    {
        var listener = go.GetComponent<UGUIClickHandler>();
        if (listener == null)
        {
            listener = go.AddComponent<UGUIClickHandler>();
        }

        return listener;
    }

    public static UGUIClickHandler Get(Transform tran)
    {
        return Get(tran.gameObject);
    }
}
