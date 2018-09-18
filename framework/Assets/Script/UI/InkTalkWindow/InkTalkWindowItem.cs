using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class InkTalkWindowItem : ReusingScrollItemBase
{
    //对方内容
    public Text m_leftContent;

    //我方内容
    public Text m_rightContent;

    //对方底图
    public Image m_leftBg;
    
    //我方底图
    public Image m_rightBg;
    
    public override void SetContent(int index, Dictionary<string, object> data)
    {
        var playerName = (string) data["Name"];

        
        m_leftContent.text = (string) data["text"];
        m_rightContent.text = (string) data["text"];

        var isSelf = playerName == "我";

        m_leftBg.gameObject.SetActive(!isSelf);
        m_rightBg.gameObject.SetActive(isSelf);
    }   
}
