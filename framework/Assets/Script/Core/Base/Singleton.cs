using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : new()
{
    private static T sInstance;

    public static T Instance
    {
        get
        {
            if ((object)Singleton<T>.sInstance == null)
            {
                Singleton<T>.sInstance = new T();
            }
            return Singleton<T>.sInstance;
        }
    }
}
