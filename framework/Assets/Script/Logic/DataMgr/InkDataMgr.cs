using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

public class InkStoryData
{
    //对话拥有者
    public string m_playerName { private set; get; }
    
    //
}

public class InkDataMgr : Singleton<InkDataMgr> 
{
    //当前运行故事
    public Story m_curStory { private set; get; }
    
    //设置当前故事
    public void SetCurStory(string story)
    {
        m_curStory = new Story(story);
        
        
    }
    
    
    //解析故事内容
    public void PraseStory(string storyText)
    {
        
    }
    
}
