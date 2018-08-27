using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Object = UnityEngine.Object;

public enum ResLoadLocation
{
    //从游戏资源中读取
    Resource,
    
    //对应streamingAssetsPath，使用ab时，ab数据会解压到这里，只有读权限
    Streaming,
    
    //persistentDataPath，可读写，一些游戏时生存的资源，或者热更新资源，从这里读写
    Persistent,
    
    //对应temporaryCachePath，临时存取位置，可读写
    Catch,
}

public static class ResourceManager
{
    /// <summary>
    /// 游戏内资源读取类型
    /// //默认从resourcePath中读取
    /// </summary>
    public static ResLoadLocation m_gameLoadType = ResLoadLocation.Resource; 

    //UI预设存储位置
    private static readonly string m_uiPrefabPath = "UI/{0}/{1}";
    
    //读取一个文本
    public static string ReadTextFile(string textName)
    {
        var text = new TextAsset();

        if (text == null)
        {
            throw new Exception("ReadTextFile not find " + textName);
        }
        else
        {
            return text.text;
        }
    }
    
    //加载一个UI预设
    public static T LoadUiPrefab<T>() where T :UIWindowBase
    {
        var filePath = string.Format(m_uiPrefabPath, typeof(T), typeof(T));

        return Resources.Load<T>(filePath);
    }

    //按照名字加载UI预设
    public static GameObject LoadUiPrefab(string uiName,bool isItem = false)
    {
        var fileName = isItem ? uiName + "Item" : uiName;
        var filePath = string.Format(m_uiPrefabPath, uiName, fileName);
        
        return Resources.Load<GameObject>(filePath);
    }
    
    //加载声音文件
    public static T LoadAudioPrefab<T>(string fileName) where T:Object
    {
        return null;
    }
    

    public static void LoadAsync(string name)
    {
        var filePath = string.Empty;
        ResourceIOTool.ResourceLoadAsync(filePath);
    }
}



