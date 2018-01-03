using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkConfigMgr : Singleton<TalkConfigMgr>
{
    //对话配置缓存
    private Dictionary<string, Dictionary<string, TalkConfig>> m_talkConfigDic =
        new Dictionary<string, Dictionary<string, TalkConfig>>();

    public TalkConfigMgr()
    {
        
    }

    //获取对话配置
    public TalkConfig GetTalkConfig(string chapterId, string playerId, string talkId)
    {
        //查找是否有缓存
        var key = chapterId + playerId;

        Dictionary<string, TalkConfig> cfgDic = null;
        if (!m_talkConfigDic.TryGetValue(key, out cfgDic))
        {
            cfgDic = LoadCfg(playerId + chapterId + "talk");
        }

        if (cfgDic == null)
        {
            return null;
        }

        if (cfgDic.ContainsKey(talkId))
        {
            return cfgDic[talkId];
        }

        return null;
    }

    //载入对话文件
    private Dictionary<string, TalkConfig> LoadCfg(string fileName)
    {
        //读配置
        var dataTable = DataManager.GetData(fileName);
        if (dataTable == null)
        {
            return null;
        }

        var cfgDic = new Dictionary<string, TalkConfig>();

        for (var i = 0; i < dataTable.TableIDs.Count; i++)
        {
            var tableId = dataTable.TableIDs[i];
            var talkCfg = new TalkConfig();
            talkCfg.LoadData(tableId, fileName);

            cfgDic.Add(tableId, talkCfg);
        }

        return cfgDic;
    }
}
