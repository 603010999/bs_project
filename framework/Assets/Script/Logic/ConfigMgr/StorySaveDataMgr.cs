using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class PlayerSaveData
{
    //玩家ID
    public string m_playerId = string.Empty;
    
    //玩家经历的对话列表
    public List<int> m_talkList = new List<int>();
    
    //好感度
    public int m_favoValue;
}

//对话内容数据
public class TalkData
{
    //对话ID
    public int m_talkId;

    //对话内容
    public string m_talkText;
    
    //对话发生时间
    public int m_talkTime;
}

//游戏运行存储数据
public class StorySaveDataMgr : Singleton<StorySaveDataMgr> 
{
    //角色当前故事，故事进度
    private Dictionary<string,PlayerSaveData> m_playerSaveData = new Dictionary<string, PlayerSaveData>();
    
    //角色对话记录
    private Dictionary<string, List<int>> m_playerTalkContent = new Dictionary<string, List<int>>();
    
    //角色经历的对话ID
    
    //角色特殊状态值(好感度等)

    private readonly string m_playerDataFileName = "player_data.txt";
    
    #region 工具函数

    //保存json串
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

    private void ReadSaveData()
    {
        
    }
    
    #endregion
}
