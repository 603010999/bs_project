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

//对话文件数据  文件名称 角色ID字符+故事ID字符
public class TalkFileData
{
    //文件所属角色
    public string m_playerId;
    
    //文件所属故事
    public string m_storyId;
    
    //文件编号
    public int m_fileNo = 0;
    
    //文件中对话列表
    public List<int> m_talkList = new List<int>();
    
    //起始ID
    public int m_startId = 0;
    
    //是否加载
    public bool m_isLoad = false;

    //初始化
    public TalkFileData(string playerId, string storyId, int startId, int fileNo)
    {
        m_playerId = playerId;
        m_storyId = storyId;
        m_startId = startId;
        m_fileNo = fileNo;
    }

    //能否添加 每个文件限定放100句话
    public bool CanAdd()
    {
        return m_talkList.Count <= 100;
    }

    //添加聊天信息
    public void AddTalkId(int talkId)
    {
        if (!m_talkList.Contains(talkId))
        {
            m_talkList.Add(talkId);
        }
    }

    //保存文件名
    public string GetFileName()
    {
        return m_playerId + "_" + m_storyId + "_" + m_fileNo + ".txt";
    }
}


//游戏运行存储数据
public class StorySaveDataMgr : Singleton<StorySaveDataMgr>
{
    //角色当前故事，故事进度
    private Dictionary<string, PlayerStateData> m_playerStateData = new Dictionary<string, PlayerStateData>();

    //角色对话记录 key:playerID  value:对话数据
    private Dictionary<string, List<TalkData>> m_playerTalkContent = new Dictionary<string, List<TalkData>>();

    //角色对话文件数据 启动时直接全部读取 key:playerId value:
    private Dictionary<string, Dictionary<string, TalkFileData>> m_playerTalkFileData =
        new Dictionary<string, Dictionary<string, TalkFileData>>();

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
    public void AddTalkData(string playerId,string text,string storyId)
    {
        //添加到总的聊天列表
        List<TalkData> list = null;
        if (!m_playerTalkContent.TryGetValue(playerId, out list))
        {
            list = new List<TalkData>();
        }

        var talkId = list.Count;

        var data = new TalkData(talkId, text);

        list.Add(data);
        
        //文件保存内容增加
        Dictionary<string, TalkFileData> fileDic = null;
        if (!m_playerTalkFileData.TryGetValue(playerId, out fileDic))
        {
            fileDic = new Dictionary<string, TalkFileData>();
        }

        TalkFileData fileData = null;
        if (!fileDic.TryGetValue(storyId, out fileData))
        {
            fileData = new TalkFileData(playerId, storyId, talkId, fileDic.Count);
        }

        fileData.AddTalkId(talkId);      
    }
    
    //获取聊天记录内容  minID是当前最老的消息的ID，如果minID>0时，需要读取一个更早期的聊天记录文件
    public List<TalkData> GetTalkData(string playerId,bool needLoad)
    {
        List<TalkData> list = null;
        if (!m_playerTalkContent.TryGetValue(playerId, out list))
        {
            LoadTalkFile(playerId);
        }

        //默认状态下，读取默认的即可
        if (!needLoad)
        {
            return list;
        }

        //加载一个文件
        LoadTalkFile(playerId);
        
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

    //加载对话文件
    private void LoadTalkFile(string playerId)
    {
        List<TalkData> list = null;
        if (!m_playerTalkContent.TryGetValue(playerId, out list))
        {
            list = new List<TalkData>();
            m_playerTalkContent.Add(playerId, list);
        }

        //没有数据
        Dictionary<string, TalkFileData> fileDataDic;
        if (!m_playerTalkFileData.TryGetValue(playerId, out fileDataDic))
        {
            return;
        }

        var enu = fileDataDic.GetEnumerator();
        while (enu.MoveNext())
        {
            if (enu.Current.Value.m_isLoad)
            {
                continue;
            }

            enu.Current.Value.m_isLoad = true;

            var talkDic = ReadSaveData<TalkData>(enu.Current.Value.GetFileName());
            if (talkDic == null)
            {
                continue;
            }
            
            var talkEnu = talkDic.GetEnumerator();
            while (talkEnu.MoveNext())
            {
                list.Add(talkEnu.Current.Value);
            }
        }
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

    #endregion
}
