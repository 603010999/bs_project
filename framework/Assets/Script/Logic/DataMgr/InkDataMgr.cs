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
    
    //当前内容 固定2个，0:名字 1:文本内容
    private List<string> m_curContent = new List<string>();
    

    //设置当前故事
    public void SetCurStory(string playerId, string storyId)
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

    //继续进行
    public string ContinueStory()
    {
        if (m_curStory == null)
        {
            return string.Empty;
        }

        //todo 记录数据

        return m_curStory.Continue();
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

        var list = GetChoiceList();
        if (list == null)
        {
            return;
        }

        m_curStory.ChooseChoiceIndex(index);

        //记录内容  做选择的一定是主角


        //todo 发送界面刷新消息
    }

    public List<string> ParseContent(string content)
    {
        m_curContent.Clear();
        
        if (string.IsNullOrEmpty(content))
        {
            return m_curContent;
        }
        
        var index = content.IndexOf(":");

        if (index == -1)
        {
            //中文也匹配下
            index = content.IndexOf("：");
        }

        if (index == -1)
        {
            var loginfo = string.Format("not find : in content: {0} ", content);
            Debug.Log(loginfo);
            return m_curContent;
        }

        m_curContent.Add(content.Substring(0, index));
        m_curContent.Add(content.Substring(index));

        return m_curContent;
    }
}
