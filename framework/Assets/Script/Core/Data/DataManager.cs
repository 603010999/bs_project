using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System;

/*
 * 数据管理器，只读，可热更新，可使用默认值
 * 通过ResourceManager加载
 * */
public class DataManager
{
    public const string m_directoryName = "Data";
    public const string m_expandName = "txt";

    /// <summary>
    /// 数据缓存
    /// </summary>
    static Dictionary<string, DataTable> s_dataCache = new Dictionary<string, DataTable>();

    //文件是否存在
    public static bool IsDataFileExist(string dataName)
    {
        var path = PathTool.GetRelativelyPath(m_directoryName, dataName, m_expandName);

        var fullPath = PathTool.GetAbsolutePath(ResLoadLocation.Resource, path);

        return File.Exists(fullPath);
    }

    //获取某个配置的数据  参数为配置名称
    public static DataTable GetData(string dataName)
    {
        try
        {
            //编辑器下不处理缓存
            if (s_dataCache.ContainsKey(dataName))
            {
                return s_dataCache[dataName];
            }

            DataTable data = null;
            string dataJson = "";

#if UNITY_EDITOR

            if (Application.isPlaying)
            {
                dataJson = ResourceManager.ReadTextFile(dataName);
            }
            else
            {
                var path = PathTool.GetRelativelyPath(m_directoryName, dataName, m_expandName);
                dataJson = ResourceIOTool.ReadStringByResource(path);
            }
#else
            dataJson = ResourceManager.ReadTextFile(DataName);
#endif

            if (dataJson == "")
            {
                throw new Exception("Dont Find ->" + dataName + "<-");
            }

            data = DataTable.Analysis(dataJson);
            data.m_tableName = dataName;

            s_dataCache.Add(dataName, data);
            return data;
        }
        
        catch (Exception e)
        {
            throw new Exception("GetData Exception ->" + dataName + "<- : " + e.ToString());
        }
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public static void CleanCache()
    {
        s_dataCache.Clear();
    }
}
