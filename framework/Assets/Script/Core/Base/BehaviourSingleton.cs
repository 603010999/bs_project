using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	//是否实例过
	private static bool m_haveInstanced = false;

	//实例对象
	private static T m_instance;

	public static T Instance
	{
		get
		{
			if (m_instance == null && !m_haveInstanced)
			{
				if (!Application.isPlaying)
				{
					return default (T);
				}
				
				var rootGo = BehhaviourRoot.GetRootGo();
				
				if (rootGo == null)
				{
					return null;
				}

				var name = "_" + typeof(T).ToString();
				var transform = rootGo.Find(name);
				
				if (transform == null)
				{
					var gameObject = new GameObject(name);
					gameObject.transform.parent = rootGo;
					m_instance = gameObject.AddComponent<T>();
					DontDestroyOnLoad(gameObject);
				}
				else
				{
					m_instance = transform.gameObject.GetComponent<T>();
				}
				
				m_haveInstanced = true;
			}
			return m_instance;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return m_instance != null;
		}
	}
}

internal class BehhaviourRoot
{
	private static Transform m_goRootTrans;

	public static Transform GetRootGo()
	{
		if (m_goRootTrans == null)
		{
			var gameObject = new GameObject("__instset_root");
			Object.DontDestroyOnLoad(gameObject);
			m_goRootTrans = gameObject.transform;
		}
		return m_goRootTrans;
	}
}
