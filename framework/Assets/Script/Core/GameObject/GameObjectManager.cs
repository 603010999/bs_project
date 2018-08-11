using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//对象池 用于缓存动态创建的对象  大多用于由预设实例话的对象
public class GameObjectManager :BehaviourSingleton<GameObjectManager>
{
    //屏幕外座标
    private Vector3 m_outOfRange = new Vector3(9000, 9000, 9000);

    //池挂点
    private Transform m_poolParent;
    public Transform PoolParent
    {
        get
        {
            if (m_poolParent == null)
            {
                var instancePool = new GameObject("ObjectPool");
                m_poolParent = instancePool.transform;

                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(m_poolParent);
                }
            }

            return m_poolParent;
        }
    }

    //对象池字典
    private Dictionary<string, List<GameObject>> m_objectPool = new Dictionary<string, List<GameObject>>();

    /// <summary>
    /// 加载一个对象并把它实例化
    /// </summary>
    /// <param name="gameObjectName">对象名</param>
    /// <param name="parent">对象的父节点,可空</param>
    /// <returns></returns>
    public GameObject CreatePoolObject(string gameObjectName, GameObject parent = null)
    {
        var go = ResourceManager.Load<GameObject>(gameObjectName);

        if (go == null)
        {
            throw new Exception("CreatPoolObject error dont find : ->" + gameObjectName + "<-");
        }

        var instanceTmp = Instantiate(go);
        instanceTmp.name = go.name;


        if (parent != null)
        {
            instanceTmp.transform.SetParent(parent.transform);
        }

        instanceTmp.SetActive(true);

        return instanceTmp;
    }

    //对象是否在池中
    private bool IsExist(string objectName)
    {
        if (objectName == null)
        {
            Debug.LogError("IsExist_New error : objectName is null!");
            return false;
        }

        if (m_objectPool.ContainsKey(objectName) && m_objectPool[objectName].Count > 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 从对象池取出一个对象，如果没有，则直接创建它
    /// </summary>
    /// <param name="name">对象名</param>
    /// <param name="parent">要创建到的父节点</param>
    /// <returns>返回这个对象</returns>
    public GameObject GetPoolObject(string name, GameObject parent = null)
    {
        GameObject go;
        if (IsExist(name))
        {
            return CreatePoolObject(name, parent);
        }
        
        go = m_objectPool[name][0];
        m_objectPool[name].RemoveAt(0);

        if (parent == null)
        {
            go.transform.SetParent(null);
        }
        else
        {
            go.transform.SetParent(parent.transform);
        }
        
        return go;
    }

    /// <summary>
    /// 将一个对象放入对象池
    /// </summary>
    /// <param name="obj">目标对象</param>
    public void DestroyPoolObject(GameObject obj)
    {
        var key = obj.name.Replace("(Clone)", "");

        if (m_objectPool.ContainsKey(key) == false)
        {
            m_objectPool.Add(key, new List<GameObject>());
        }

        if (m_objectPool[key].Contains(obj))
        {
            throw new Exception("DestroyPoolObject:-> Repeat Destroy GameObject !" + obj);
        }

        m_objectPool[key].Add(obj);

        obj.transform.position = m_outOfRange;

        obj.name = key;
        obj.transform.SetParent(PoolParent);
    }

    /// <summary>
    /// 增加延迟调用
    /// </summary>
    /// <param name="go"></param>
    /// <param name="time"></param>
    public void DestroyPoolObject(GameObject go, float time)
    {
        Timer.DelayCallBack(time, (object[] obj) =>
        {
            DestroyPoolObject(go);
        });
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void CleanPool()
    {
        foreach (var name in m_objectPool.Keys)
        {
            if (m_objectPool.ContainsKey(name))
            {
                var objList = m_objectPool[name];

                for (var i = 0; i < objList.Count; i++)
                {
                    Destroy(objList[i].gameObject);
                }

                objList.Clear();
            }
        }

        m_objectPool.Clear();
    }

    /// <summary>
    /// 清除掉某一个对象的所有对象池缓存
    /// </summary>
    public void CleanPoolByName(string name)
    {
        if (m_objectPool.ContainsKey(name))
        {
            List<GameObject> objList = m_objectPool[name];

            for (int i = 0; i < objList.Count; i++)
            {
                Destroy(objList[i].gameObject);
            }

            objList.Clear();
            m_objectPool.Remove(name);
        }
    }

    #region 异步方法

    public void CreatePoolObjectAsync(string name, CallBack<GameObject> callback, GameObject parent = null)
    {
        ResourceManager.LoadAsync(name, (status,res) =>
        {
            try
            {
                callback(CreatePoolObject(name, parent));
            }
            catch(Exception e)
            {
                Debug.LogError("CreatePoolObjectAsync Exception: " + e.ToString());
            }
        });
    }

    #endregion
}
