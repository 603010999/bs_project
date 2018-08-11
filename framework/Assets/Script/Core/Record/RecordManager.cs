using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class RecordManager:Singleton<RecordManager>
{
    //文件夹名
    public const string m_directoryName = "Record";
    
    //后缀名
    public const string m_expandName    = "json";

    /// <summary>
    /// 记录缓存
    /// </summary>
    static Dictionary<string, RecordTable> m_recordCache = new Dictionary<string, RecordTable>();

    //获取数据
    public RecordTable GetData(string recordName)
    {
        if (m_recordCache.ContainsKey(recordName))
        {
            return m_recordCache[recordName];
        }

        RecordTable record = null;

        var dataJson = string.Empty;

        var path = PathTool.GetRelativelyPath(m_directoryName, recordName, m_expandName);

        var fullPath = PathTool.GetAbsolutePath(ResLoadLocation.Persistent, path);
        if (File.Exists(fullPath))
        {
            //记录永远从沙盒路径读取
            dataJson = ResourceIOTool.ReadStringByFile(fullPath);
        }

        if (string.IsNullOrEmpty(dataJson))
        {
            record = new RecordTable();
        }
        else
        {
            record = RecordTable.Analysis(dataJson);
        }

        m_recordCache.Add(recordName, record);

        return record;
    }

    public void SaveData(string RecordName, RecordTable data)
    {
#if !UNITY_WEBGL

        ResourceIOTool.WriteStringByFile(
            PathTool.GetAbsolutePath(ResLoadLocation.Persistent,
                PathTool.GetRelativelyPath(m_directoryName,
                                                    RecordName,
                                                    m_expandName)),
                RecordTable.Serialize(data));

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.AssetDatabase.Refresh();
        }
        #endif
#endif
    }

    public void CleanRecord(string recordName)
    {
        var table = GetData(recordName);
        table.Clear();
        SaveData(recordName, table);
    }

    public static void CleanAllRecord()
    {
        FileTool.DeleteDirectory(Application.persistentDataPath + "/" + RecordManager.m_directoryName);
        CleanCache();
    }

    public static void CleanCache()
    {
        m_recordCache.Clear();
    }

#region 保存封装

    public void SaveRecord(string RecordName, string key, string value)
    {
        var table = GetData(RecordName);
        table.SetRecord(key,value);
        SaveData(RecordName, table);
    }

    public void SaveRecord(string RecordName, string key, int value)
    {
        var table = GetData(RecordName);
        table.SetRecord(key, value);
        SaveData(RecordName, table);
    }

    public void SaveRecord(string RecordName, string key, bool value)
    {
        var table = GetData(RecordName);
        table.SetRecord(key, value);
        SaveData(RecordName, table);
    }

    public void SaveRecord(string RecordName, string key, float value)
    {
        var table = GetData(RecordName);
        table.SetRecord(key, value);
        SaveData(RecordName, table);
    }

    public void SaveRecord(string RecordName, string key, Vector2 value)
    {
        var table = GetData(RecordName);
        table.SetRecord(key, value);
        SaveData(RecordName, table);
    }

    public void SaveRecord(string RecordName, string key, Vector3 value)
    {
        var table = GetData(RecordName);
        table.SetRecord(key, value);
        SaveData(RecordName, table);
    }

    public void SaveRecord(string RecordName, string key, Color value)
    {
        var table = GetData(RecordName);
        table.SetRecord(key, value);
        SaveData(RecordName, table);
    }


    #endregion

    #region 取值封装

    public int GetIntRecord(string RecordName, string key,int defaultValue)
    {
        var table = GetData(RecordName);

        return table.GetRecord(key, defaultValue);
    }

    public string GetStringRecord(string RecordName, string key, string defaultValue)
    {
        var table = GetData(RecordName);

        return table.GetRecord(key, defaultValue);
    }

    public bool GetBoolRecord(string RecordName, string key, bool defaultValue)
    {
        var table = GetData(RecordName);

        return table.GetRecord(key, defaultValue);
    }

    public float GetFloatRecord(string RecordName, string key, float defaultValue)
    {
        var table = GetData(RecordName);

        return table.GetRecord(key, defaultValue);
    }

    public Vector2 GetVector2Record(string RecordName, string key, Vector2 defaultValue)
    {
        var table = GetData(RecordName);

        return table.GetRecord(key, defaultValue);
    }

    public Vector3 GetVector3Record(string RecordName, string key, Vector3 defaultValue)
    {
        var table = GetData(RecordName);

        return table.GetRecord(key, defaultValue);
    }

    public Color GetColorRecord(string RecordName, string key, Color defaultValue)
    {
        var table = GetData(RecordName);

        return table.GetRecord(key, defaultValue);
    }

    #endregion

}
