using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopWindowItem : ReusingScrollItemBase
{
    public Text m_nameText;

    public Text m_costText;
    
    public override void SetContent(int index, Dictionary<string, object> data)
    {
        m_nameText.text = (string) data["Name"];
        m_costText.text = "$ " + (string) data["Cost"];
    }
}
