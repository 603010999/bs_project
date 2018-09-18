using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

//读取Ink文件  只读
public class InkConfigMgr : Singleton<InkConfigMgr>
{
    //ink文件 key:player id value<storyID,text>
    private Dictionary<string, Dictionary<string, string>> m_inkDic =
        new Dictionary<string, Dictionary<string, string>>();
    
    //获取ink文本
    public string GetInkText(string playerId,string storyId)
    {
        Dictionary<string, string> dic = null;
        if (!m_inkDic.TryGetValue(playerId, out dic))
        {
            dic = new Dictionary<string, string>();
            m_inkDic.Add(playerId, dic);
        }

        var text = string.Empty;
        if (!dic.TryGetValue(storyId, out text))
        {
            var asset = Resources.Load("InkConfig", typeof(TextAsset)) as TextAsset;
            if (asset == null)
            {
                return string.Empty;
            }

            dic[storyId] = asset.text;
            
            text = asset.text;           
        }

        return text;
    }
}
