using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class InkTalkWindowItem : ReusingScrollItemBase
{
    public Text m_name;

    public Text m_content;

    public override void SetContent(int index, Dictionary<string, object> data)
    {
        var playerName = (string) data["Name"];

        m_name.text = playerName;
        m_content.text = (string) data["text"];

        if (playerName == "我")
        {
            m_name.alignment = TextAnchor.UpperRight;
            m_content.alignment = TextAnchor.UpperRight;
        }
        else
        {
            m_name.alignment = TextAnchor.UpperLeft;
            m_content.alignment = TextAnchor.UpperRight;
        }
    }   
}
