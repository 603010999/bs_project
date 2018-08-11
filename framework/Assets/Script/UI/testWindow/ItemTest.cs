using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ItemTest :ReusingScrollItemBase
{
    public Text m_nameText;
    
    protected override void OnInit()
    {
        base.OnInit();
    }

    public override void SetContent(int index, Dictionary<string, object> data)
    {
        m_nameText.text = index.ToString() + RandomService.Range(0, 100);
    }

    public void OnClick(InputUIOnClickEvent e)
    {
        Debug.Log("item onclick " + m_index);
    }
}
