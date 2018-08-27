using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// 资源读取器，负责从不同路径读取资源
/// </summary>
public class ResourceIOTool :MonoBehaviour
{
    static ResourceIOTool instance;
    public static ResourceIOTool GetInstance()
    {
        if (instance == null)
        {
            GameObject resourceIOTool = new GameObject();
            resourceIOTool.name = "ResourceIO";
            DontDestroyOnLoad(resourceIOTool);

            instance = resourceIOTool.AddComponent<ResourceIOTool>();
        }

        return instance;
    }

    #region 读操作
    public static string ReadStringByFile(string path)
    {
        StringBuilder line = new StringBuilder();
        try
        {
            if (!File.Exists(path))
            {
                Debug.Log("path dont exists ! : " + path);
                return "";
            }

            StreamReader sr = File.OpenText(path);
            line.Append(sr.ReadToEnd());

            sr.Close();
            sr.Dispose();
        }
        catch (Exception e)
        {
            Debug.Log("Load text fail ! message:" + e.Message);
        }

        return line.ToString();
    }

    public static string ReadStringByBundle(string path)
    {
        AssetBundle ab = AssetBundle.LoadFromFile(path);

        TextAsset ta = (TextAsset)ab.mainAsset;

        string content = ta.ToString();
        ab.Unload(true);

        return content;
    }

    public static string ReadStringByResource(string path)
    {
        path = FileTool.RemoveExpandName(path);
        TextAsset text = (TextAsset)Resources.Load(path);

        if(text == null)
        {
            return "";
        }
        else
        {
            return text.text;
        }
    }

    public static void ResourceLoadAsync(string path)
    {
        
    }


    #endregion

    #region 写操作
    
    public static void WriteStringByFile(string path, string content)
    {
        byte[] dataByte = Encoding.GetEncoding("UTF-8").GetBytes(content);

        CreateFile(path, dataByte);
    }

    public static void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else
        {
            Debug.Log("File:[" + path + "] dont exists");
        }
    }


    public static void CreateFile(string path, byte[] byt)
    {
        try
        {
            FileTool.CreatFilePath(path);
            File.WriteAllBytes(path, byt);
        }
        catch (Exception e)
        {
            Debug.LogError("File Create Fail! \n" + e.Message);
        }
    }

    #endregion

}
