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
    public const string c_directoryName = "Data";
    public const string c_expandName = "txt";

    /// <summary>
    /// 数据缓存
    /// </summary>
    static Dictionary<string, DataTable> s_dataCache = new Dictionary<string, DataTable>();

    public static bool GetIsExistData(string DataName)
    {
        return ResourcesConfigManager.GetIsExitRes(DataName);
    }

    //文件是否存在
    public static bool IsDataFileExist(string dataName)
    {
        var path = PathTool.GetRelativelyPath(c_directoryName, dataName, c_expandName);

        var fullPath = PathTool.GetAbsolutePath(ResLoadLocation.Resource, path);

        return File.Exists(fullPath);
    }

    //获取某个配置的数据  参数为配置名称
    public static DataTable GetData(string DataName)
    {
        try
        {
            //编辑器下不处理缓存
            if (s_dataCache.ContainsKey(DataName))
            {
                return s_dataCache[DataName];
            }

            DataTable data = null;
            string dataJson = "";

#if UNITY_EDITOR

            if (Application.isPlaying)
            {
                dataJson = ResourceManager.ReadTextFile(DataName);
            }
            else
            {
                var path = PathTool.GetRelativelyPath(c_directoryName, DataName, c_expandName);
                dataJson = ResourceIOTool.ReadStringByResource(path);
            }
#else
            dataJson = ResourceManager.ReadTextFile(DataName);
#endif

            if (dataJson == "")
            {
                throw new Exception("Dont Find ->" + DataName + "<-");
            }

            data = DataTable.Analysis(dataJson);
            data.m_tableName = DataName;

            s_dataCache.Add(DataName, data);
            return data;
        }
        
        catch (Exception e)
        {
            throw new Exception("GetData Exception ->" + DataName + "<- : " + e.ToString());
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
