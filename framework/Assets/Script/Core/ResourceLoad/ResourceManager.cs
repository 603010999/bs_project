using UnityEngine;
using System.Collections;
using System.Text;
using System;

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

    //读取一个文本
    public static string ReadTextFile(string textName)
    {
        var text = Load<TextAsset>(textName);

        if (text == null)
        {
            throw new Exception("ReadTextFile not find " + textName);
        }
        else
        {
            return text.text;
        }
    }

    public static object Load(string name)
    {
        var packData  = ResourcesConfigManager.Instance.GetBundleConfig(name);

        if(packData == null)
        {
            throw new Exception("Load Exception not find " + name);
        }

        if (m_gameLoadType == ResLoadLocation.Resource)
        {
            return Resources.Load(packData.path);
        }
        else
        {
            return AssetsBundleManager.Load(name);
        }
    }

    //按照某个类型加载数据
    public static T Load<T>(string name) where T: UnityEngine.Object
    {
        var packData = ResourcesConfigManager.Instance.GetBundleConfig(name);

        if (packData == null)
        {
            throw new Exception("Load Exception not find " + name);
        }

        if (m_gameLoadType == ResLoadLocation.Resource)
        {
            return Resources.Load<T>(packData.path);
        }
        else
        {
            return AssetsBundleManager.Load<T>(name);
        }
    }

    public static void LoadAsync(string name,LoadCallBack callBack)
    {
        var packData  = ResourcesConfigManager.Instance.GetBundleConfig(name);

        if (packData == null)
        {
            return ;
        }

        if (m_gameLoadType == ResLoadLocation.Resource)
        {
            ResourceIOTool.ResourceLoadAsync(packData.path, callBack);
        }
        else
        {
            AssetsBundleManager.LoadAsync(name,callBack);
        }
    }

    public static void UnLoad(string name)
    {
        if (m_gameLoadType == ResLoadLocation.Resource)
        {

        }
        else
        {
            AssetsBundleManager.UnLoadBundle(name);
        }
    }
}



