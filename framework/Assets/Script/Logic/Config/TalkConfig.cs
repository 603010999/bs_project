using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkConfig : DataGenerateBase
{
    //对话ID
    public string m_key;

    public string[] m_p_talk_text = new string[4];
    public string[] m_n_talk_text = new string[4];
    public string[] m_next_talk_id = new string[4];

    public void LoadData(string key,string fileName)
    {
        var table = DataManager.GetData(fileName);

        if (!table.ContainsKey(key))
        {
            throw new Exception(fileName + "LoadData Exception Not Fond key ->" + key + "<-");
        }

        SingleData data = table[key];

        m_key = key;

        for (var i = 0; i < 4; ++i)
        {
            m_p_talk_text[i] = data.GetString(string.Format("p_talk_text_{0}", i + 1));
            m_n_talk_text[i] = data.GetString(string.Format("n_talk_text_{0}", i + 1));
            m_next_talk_id[i] = data.GetString(string.Format("next_talk_id_{0}", i + 1));
        }
    }
}
