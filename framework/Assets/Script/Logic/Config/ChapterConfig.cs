using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterConfig : DataGenerateBase
{
    public string m_key;

    public override void LoadData(string key)
    {
        DataTable table = DataManager.GetData("chapter");

        if (!table.ContainsKey(key))
        {
            throw new Exception("chapterGenerate LoadData Exception Not Fond key ->" + key + "<-");
        }

        SingleData data = table[key];

        m_key = key;
    }
}
