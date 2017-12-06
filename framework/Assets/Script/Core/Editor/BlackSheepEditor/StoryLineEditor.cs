using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StoryLineEditor : EditorWindow
{
    #region open

    [MenuItem("Tools/剧情编辑器", priority = 100)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(StoryLineEditor));
    }

    private void OnGUI()
    {
        titleContent.text = "剧情编辑器";

        EditorGUILayout.BeginHorizontal();

        #region player

        EditorGUILayout.BeginVertical(GUILayout.Width(80));

        ShowSelectPlayerList();

        AddPlayerGUI();

        EditorPlayerDataGUI();

        EditorGUILayout.EndVertical();

        #endregion 

        #region talk

        EditorGUILayout.BeginVertical(GUILayout.Width(120));



        EditorGUILayout.EndVertical();

        #endregion

        #region message

        EditorGUILayout.BeginVertical(GUILayout.Width(120));


        EditorGUILayout.EndVertical();

        #endregion

        EditorGUILayout.EndHorizontal();
    }

    private void OnEnable()
    {
        PreparePlayerData();
    }

    #endregion


    #region 角色部分

    //角色数据文件名称
    private readonly string m_playerCfgName = "player";

    //角色列表
    private List<string> m_playerNameList = new List<string>();

    //当前下标
    private int m_curPlayerIndex = 0;

    //当前所有角色数据
    private DataTable m_curPlayerDatas;

    //载入数据
    private void PreparePlayerData()
    {
        AssetDatabase.Refresh();

        m_playerNameList.Clear();

        m_playerNameList.Add("none");

        m_curPlayerDatas = DataManager.GetData(m_playerCfgName);

        foreach(var player in m_curPlayerDatas.Keys)
        {
            m_playerNameList.Add(player);
        }
    }

    //显示角色选择列表
    private void ShowSelectPlayerList()
    {
        var mask = m_playerNameList.ToArray();

        if (m_playerNameList.Count <= 1)
        {
            return;
        }

        m_curPlayerIndex = EditorGUILayout.Popup("当前角色：", m_curPlayerIndex, mask);
        if (mask.Length != 0 && m_curPlayerIndex != 0)
        {
            LoadPlayerData(mask[m_curPlayerIndex]);
        }
    }

    private bool newPlayerisFold = true;

    private SingleData playerContent;

    private string playerId = string.Empty;

    private void AddPlayerGUI()
    {
        //当前没有选中角色时时，才显示
        if(m_curPlayerIndex != 0)
        {
            return;
        }

        newPlayerisFold = EditorGUILayout.Foldout(newPlayerisFold, "创建角色");
        if (newPlayerisFold)
        {
            EditorGUI.indentLevel++;

            var fieldData = new SingleField(FieldType.String, playerId, null);
            playerId = EditorUtilGUI.FieldGUI_Type(fieldData, "角色ID");

            if (string.IsNullOrEmpty(playerId))
            {
                EditorGUILayout.LabelField("输入要添加的ID（角色id从1001开始）=,,=");
            }
            else if (m_curPlayerDatas.ContainsKey(playerId))
            {
                EditorGUILayout.LabelField("重复啦=..=", EditorGUIStyleData.WarnMessageLabel);
            }

            EditorGUILayout.Space();

            if (!m_curPlayerDatas.ContainsKey(playerId) && !string.IsNullOrEmpty(playerId))
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("创建"))
                {
                    playerContent = new SingleData();
                    playerContent.Add("player_id", playerId);
                    m_curPlayerDatas.AddData(playerContent);
                    DataEditorWindow.SaveData(m_playerCfgName, m_curPlayerDatas);
                    PreparePlayerData();
                }
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();
        }
    }

    //角色名称
    private string playerName;

    //开始块ID
    private string startTalkId;

    //编辑角色数据
    private void EditorPlayerDataGUI()
    {
        if (m_curPlayerIndex == 0)
        {
            return;
        }

        var fieldData = new SingleField(FieldType.String, playerId, null);
        playerId = EditorUtilGUI.FieldGUI_Type(fieldData, "角色ID");

        fieldData = new SingleField(FieldType.String, playerName, null);
        playerName = EditorUtilGUI.FieldGUI_Type(fieldData, "角色名");

        fieldData = new SingleField(FieldType.String, startTalkId, null);
        startTalkId = EditorUtilGUI.FieldGUI_Type(fieldData, "初始对话ID");

        if (GUILayout.Button("保存"))
        {
            playerContent["player_id"] = playerId;
            playerContent["player_name"] = playerName;
            playerContent["start_talk_id"] = startTalkId;
            m_curPlayerDatas.SetData(playerContent);
            DataEditorWindow.SaveData(m_playerCfgName, m_curPlayerDatas);
        }
    }

    //载入角色数据
    private void LoadPlayerData(string Id)
    {
        playerId = Id;
        playerContent = m_curPlayerDatas[playerId];
        playerName = playerContent["player_name"];
        startTalkId = playerContent["start_talk_id"];
    }

    #endregion

    #region 对话部分

    //当前角色对应的对话
    private List<string> m_talkList = new List<string>();

    //当前选中的对话下标
    private int m_curTalkIndex = 0;

    //获取对话文件名
    private string GetTalkConfigName()
    {
        return string.Empty;
    }

    //初始化对话，当人物信息确定后，开始
    private void PrepareTalk()
    {
        
    }

    #endregion


    #region 语句部分

    #endregion
}
