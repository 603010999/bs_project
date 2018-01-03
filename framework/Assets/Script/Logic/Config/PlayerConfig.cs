using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConfig : DataGenerateBase
{
    //主键
    public string m_key;

    //角色名
    public string m_player_name;

    //起始对话ID
    public string m_start_talk_id;

    public override void LoadData(string key)
    {
        var table = DataManager.GetData("player");

        if (!table.ContainsKey(key))
        {
            throw new Exception("playerGenerate LoadData Exception Not Fond key ->" + key + "<-");
        }

        var data = table[key];

        m_key = key;
        m_player_name = data.GetString("player_name");
        m_start_talk_id = data.GetString("start_talk_id");
    }
}
