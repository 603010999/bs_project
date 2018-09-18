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
    private Story m_curStory;
    
    //设置当前故事
    public void SetCurStory(string playerId,string storyId)
    {        
        var text = InkConfigMgr.Instance.GetInkText(playerId, storyId);

        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        
        
        m_curStory = new Story(text);  
    }

    //能否继续
    public bool CanContinue()
    {
        if (m_curStory == null)
        {
            return false;
        }

        return m_curStory.canContinue;
    }
 
    //获取选择列表
    public List<Choice> GetChoiceList()
    {
        if (m_curStory == null)
        {
            return null;
        }

        if (m_curStory.currentChoices.Count == 0)
        {
            return null;
        }
        
        return m_curStory.currentChoices;
    }    
    
    //选择选项
    public void Choose(int index)
    {
        if (m_curStory == null)
        {
            return;
        }
        
        m_curStory.ChooseChoiceIndex(index);
        
        //todo 发送界面刷新消息
    }
}
