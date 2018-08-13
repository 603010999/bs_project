using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//资源配置管理
//所有资源，按照名称一一区分，项目里不允许重名文件  
public class ResourcesConfigManager :Singleton<ResourcesConfigManager>
{
    //资源清单，用于记录所有Resources下资源的情况
    public const string m_manifestFileName = "ResourcesManifest";
    
    //依赖key
    public const string m_relyBundleKey    = "relyBundles";
    
    //
    public const string m_bundlesKey       = "AssetsBundles";

    //依赖bundle的配置
    public Dictionary<string, ResourcesConfig> m_relyBundleConfigs;
    
    //依赖配置
    public Dictionary<string, ResourcesConfig> m_bundleConfigs ;

    //初始化
    public ResourcesConfigManager()
    {
        Initialize();
    }

    //初始化，用于热更新之后，再次调用
    public void Initialize()
    {
        var cfg = GetResourcesConfig();

        m_relyBundleConfigs = cfg.m_relyCfgDic;
        m_bundleConfigs = cfg.m_bundleCfgDic;
    }

    public bool GetIsExitRes(string resName)
    {
        if (m_bundleConfigs == null)
        {
            throw new Exception("RecourcesConfigManager GetBundleConfig : bundleConfigs is null  do you Initialize?");
        }

        return m_bundleConfigs.ContainsKey(resName);
    }

    public ResourcesConfig GetBundleConfig(string bundleName)
    {
        if (m_bundleConfigs == null)
        {
            throw new Exception("RecourcesConfigManager GetBundleConfig : bundleConfigs is null  do you Initialize?");
        }

        if (m_bundleConfigs.ContainsKey(bundleName))
        {
            return m_bundleConfigs[bundleName];
        }
        else
        {
            throw new Exception("RecourcesConfigManager GetBundleConfig : Dont find ->" + bundleName + "<- please check BundleConfig!");
        }
    }

    public ResourcesConfig GetRelyBundleConfig(string bundleName)
    {
        if (m_relyBundleConfigs == null)
        {
            throw new Exception("ResourcesConfigManager GetRelyBundleConfig Exception: relyBundleConfigs is null do you Initialize?");
        }

        if (m_relyBundleConfigs.ContainsKey(bundleName))
        {
            return m_relyBundleConfigs[bundleName];
        }
        else
        {
            throw new Exception("ResourcesConfigManager GetRelyBundleConfig Exception: Dont find ->" + bundleName + "<- please check BundleConfig!");
        }
    }

    //资源路径数据不依赖任何其他数据
    public ResourcesConfigData GetResourcesConfig()
    {
        var dataJson = string.Empty;

        dataJson = ReadResourceConfigContent();

        if (string.IsNullOrEmpty(dataJson))
        {
            throw new Exception("ResourcesConfig not find " + m_manifestFileName);
        }
        else
        {
            return AnalysisResourcesConfig2Struct(dataJson);
        }
    }

    //读取配置内容
    public string ReadResourceConfigContent()
    {
        var dataJson = string.Empty;

        if (ResourceManager.m_gameLoadType == ResLoadLocation.Resource)
        {
            dataJson = ResourceIOTool.ReadStringByResource(
                m_manifestFileName + "." + ConfigManager.c_expandName);
        }
        else
        {
            var type = ResLoadLocation.Streaming;

            if (RecordManager.Instance.GetData(HotUpdateManager.c_HotUpdateRecordName).GetRecord(HotUpdateManager.c_useHotUpdateRecordKey, false))
            {
                type = ResLoadLocation.Persistent;

                dataJson = ResourceIOTool.ReadStringByFile(
                PathTool.GetAbsolutePath(
                     type,
                     m_manifestFileName + "." + ConfigManager.c_expandName));
            }
            else
            {
                var ab = AssetBundle.LoadFromFile(PathTool.GetAbsolutePath(
                     type,
                     m_manifestFileName + "." +  AssetsBundleManager.c_AssetsBundlesExpandName));

                TextAsset text = (TextAsset)ab.mainAsset;
                dataJson = text.text;

                ab.Unload(true);
            }
        }

        return dataJson;
    }

    //转化读取的字符串为数据
    public ResourcesConfigData AnalysisResourcesConfig2Struct(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new Exception("ResourcesConfigcontent is null ! ");
        }

        var result = new ResourcesConfigData();

        var data = (Dictionary<string, object>)FrameWork.Json.Deserialize(content);

        var gameRelyBundles = (Dictionary<string, object>)data[m_relyBundleKey];
        var gameAssetsBundles = (Dictionary<string, object>)data[m_bundlesKey];

        result.m_relyCfgDic = new Dictionary<string, ResourcesConfig>();
        result.m_bundleCfgDic = new Dictionary<string, ResourcesConfig>();
        foreach (var item in gameRelyBundles.Values)
        {
            var tmp = (Dictionary<string, object>)item;

            var config = new ResourcesConfig();
            config.name = (string)tmp["name"];
            config.path = (string)tmp["path"];
            config.relyPackages = (tmp["relyPackages"].ToString()).Split('|');
            config.md5 = (string)tmp["md5"];

            result.m_relyCfgDic.Add(config.name,config);
        }

        foreach (var item in gameAssetsBundles.Values)
        {
            var tmp = (Dictionary<string, object>)item;

            var config = new ResourcesConfig();
            config.name = (string)tmp["name"];
            config.path = (string)tmp["path"];
            config.relyPackages = ((string)tmp["relyPackages"]).Split('|');
            config.md5 = (string)tmp["md5"];

            result.m_bundleCfgDic.Add(config.name,config);
        }

        return result;
    }
}

public class ResourcesConfig
{
    //名称
    public string name;     
    
    //加载相对路径
    public string path;     
    
    //依赖包
    public string[] relyPackages;    
    
    //md5
    public string md5;                
}

public class ResourcesConfigData
{
    public Dictionary<string, ResourcesConfig> m_relyCfgDic;
    public Dictionary<string, ResourcesConfig> m_bundleCfgDic;
}