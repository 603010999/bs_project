using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

//角色进度数据
public class PlayerStateData
{
    //玩家ID
    public string m_playerId = string.Empty;
    
    //当前故事ID
    public string m_curStoryId = string.Empty;
    
    //当前故事进度
    public string m_curStoryProgress = string.Empty;
    
    //玩家经历的故事列表
    public List<int> m_storyList = new List<int>();
    
    //参数值(好感度)
    public int m_paramValue;

    //按照配置，生成一个初始的玩家数据
    public PlayerStateData(string playerId)
    {
        
    }
}

//对话内容数据
public class TalkData
{
    //对话ID
    public int m_talkId;

    //对话内容
    public string m_talkText;
    
    //对话发生时间
    public string m_talkTime;

    public TalkData(int id, string text, string time = "")
    {
        m_talkId = id;
        m_talkText = text;

        m_talkTime = string.IsNullOrEmpty(time) ? DateTime.Now.ToString() : time;
    }
}

//游戏运行存储数据
public class StorySaveDataMgr : Singleton<StorySaveDataMgr>
{
    //角色当前故事，故事进度
    private Dictionary<string, PlayerStateData> m_playerStateData = new Dictionary<string, PlayerStateData>();

    //角色对话记录 key:playerID  value:对话数据(key:对话ID  value:对话列表)
    private Dictionary<string, Dictionary<string,List<TalkData>>> m_playerTalkContent = new Dictionary<string, Dictionary<string, List<TalkData>>>();

    //角色数据文件名
    private readonly string m_playerDataFileName = "player_data.txt";

    public StorySaveDataMgr()
    {
        LoadPlayerStateData();
    }
    
    #region handle data
    
    //获取玩家数据
    public PlayerStateData GetPlayerStateData(string playerId)
    {
        PlayerStateData data = null;
        if (!m_playerStateData.TryGetValue(playerId, out data))
        {
            //初始化一个默认的
            data = new PlayerStateData(playerId);

            m_playerStateData.Add(playerId, data);
        }

        return data;
    }
    
    //增加对话 角色，对话内容，所处的故事
    public void AddTalkData(string playerId,string storyId,string text)
    {
        //获取角色对话字典
        Dictionary<string, List<TalkData>> playerTalkDic = null;
        if (!m_playerTalkContent.TryGetValue(playerId, out playerTalkDic))
        {
            playerTalkDic = new Dictionary<string, List<TalkData>>();

            m_playerTalkContent.Add(playerId, playerTalkDic);
        }
        
        //获取聊天列表
        List<TalkData> list = null;
        if (!playerTalkDic.TryGetValue(storyId, out list))
        {
            list = new List<TalkData>();
            playerTalkDic.Add(storyId, list);
        }

        var talkId = GetTalkId(playerId);

        var data = new TalkData(talkId, text);

        list.Add(data);  
    }
    
    //获取聊天记录内容
    public List<TalkData> GetTalkData(string playerId,string storyId)
    {
        Dictionary<string, List<TalkData>> playerTalkDic = null;
        if (!m_playerTalkContent.TryGetValue(playerId, out playerTalkDic))
        {
            return null;
        }

        List<TalkData> list;
        if (!playerTalkDic.TryGetValue(storyId, out list))
        {
            return null;
        }
        
        return list;
    }
    
    #endregion
    
    //读取玩家数据
    private void LoadPlayerStateData()
    {
        var dic = ReadSaveData<PlayerStateData>(m_playerDataFileName);
        
        //没数据
        if (dic == null)
        {
            return;
        }

        m_playerStateData = dic;
    }
    
    //获取对话ID 就是当前数据数量
    private int GetTalkId(string playerId)
    {
        Dictionary<string, List<TalkData>> playerTalkDic = null;
        if (!m_playerTalkContent.TryGetValue(playerId, out playerTalkDic))
        {
            return 0;
        }

        var count = 0;

        var enu = playerTalkDic.GetEnumerator();
        while (enu.MoveNext())
        {
            count += enu.Current.Value.Count;
        }

        return count;
    }

    //加载对话文件
    private void LoadTalkFile(string playerId,string storyId)
    {
        Dictionary<string, List<TalkData>> playerTalkDic = null;
        if (!m_playerTalkContent.TryGetValue(playerId, out playerTalkDic))
        {
            playerTalkDic = new Dictionary<string, List<TalkData>>();
            m_playerTalkContent.Add(playerId, playerTalkDic);
        }


        var list = ReadListData<TalkData>(playerId + "_" + storyId + "_talk.txt");

        playerTalkDic[storyId] = list;
    }
    
    //检测和保存数据
    private void SavePlayerData()
    {
        //状态数据
        SavePlayerStateData();

        //对话内容
    }

    //保存玩家状态数据
    private void SavePlayerStateData()
    {
        SaveData(m_playerStateData,m_playerDataFileName);
    }
    
    //保存玩家对话内容
    private void SavePlayerTalkData()
    {
        
    }
    
    #region 工具函数

    //保存
    private void SaveData(object dicObj,string fileName)
    {
        //转码
        var saveBytes = Encoding.GetEncoding("UTF-8").GetBytes(JsonTool.Object2Json(dicObj));
        
        //生成保存路径
        var filePath = PathTool.GetAbsolutePath(ResLoadLocation.Persistent, fileName);
        
        try
        {
            FileTool.CreatFilePath(filePath);

            File.WriteAllBytes(filePath, saveBytes);
        }
        catch (Exception e)
        {
            Debug.LogError("File Create Fail! \n" + e.Message);
        }
    }

    //读取
    private Dictionary<string, T> ReadSaveData<T>(string fileName)
    {
        var filePath = PathTool.GetAbsolutePath(ResLoadLocation.Persistent, fileName);

        var text = ResourceIOTool.ReadStringByFile(filePath);

        return JsonTool.Json2Dictionary<T>(text);
    }
    
    //读取列表
    private List<T> ReadListData<T>(string fileName)
    {
        var filePath = PathTool.GetAbsolutePath(ResLoadLocation.Persistent, fileName);
        var text = ResourceIOTool.ReadStringByFile(filePath);

        return JsonTool.Json2List<T>(text);
    }

    #endregion
}
