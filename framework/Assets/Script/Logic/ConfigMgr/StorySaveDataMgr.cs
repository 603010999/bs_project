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

//对话文件数据  文件名称 角色ID字符+估计ID字符
public class TalkFileData
{
    //文件所属故事
    public int m_storyId;
    
    //文件中对话列表
    public List<int> m_talkList = new List<int>();
}


//游戏运行存储数据
public class StorySaveDataMgr : Singleton<StorySaveDataMgr>
{
    //角色当前故事，故事进度
    private Dictionary<string, PlayerStateData> m_playerSaveData = new Dictionary<string, PlayerStateData>();

    //角色对话记录
    private Dictionary<string, List<TalkData>> m_playerTalkContent = new Dictionary<string, List<TalkData>>();

    //角色对话文件数据 启动时直接全部读取
    private Dictionary<string, List<TalkFileData>> m_playerTalkFileData = new Dictionary<string, List<TalkFileData>>();

    //角色数据文件名
    private readonly string m_playerDataFileName = "player_data.txt";

    //获取玩家数据
    public PlayerStateData GetPlayerStateData(string playerId)
    {
        PlayerStateData data = null;
        if (!m_playerSaveData.TryGetValue(playerId, out data))
        {
            //初始化一个默认的
            data = new PlayerStateData(playerId);
        }

        return data;
    }

    //增加对话
    public void AddTalkData(string playerId,string text,string storyId)
    {
        List<TalkData> list = null;
        if (!m_playerTalkContent.TryGetValue(playerId, out list))
        {
            list = new List<TalkData>();
        }

        var talkId = list.Count;

        var data = new TalkData(talkId, text);

        list.Add(data);
        
        //文件保存内容增加
        List<TalkFileData> fileList = null;
        if (m_playerTalkFileData.TryGetValue(playerId, out fileList))
        {
            
        }
    }


//检测和保存数据
    private void SavePlayerData()
    {
        //
        
        //对话内容
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
    private void ReadSaveData()
    {
        
    }
    
    #endregion
}
