using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class StoryLineEditor : EditorWindow
{
    //每个对话中对话内容分支的最大数量
    private const int m_maxTalkContentCnt = 4;

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

        #region chapter

        EditorGUILayout.BeginVertical(GUILayout.Width(100));

        ShowSelectChapterListGUI();

        EditorGUILayout.EndVertical();

        #endregion

        #region talk

        EditorGUILayout.BeginVertical(GUILayout.Width(520));

        ShowSelectTalkListGUI();

        AddTalkGUI();

        EditorTalkGUI();

        EditorGUILayout.EndVertical();

        #endregion

        EditorGUILayout.EndHorizontal();
    }

    private void OnEnable()
    {
        PreparePlayerData();
        PrepareChapterData();
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
                EditorGUILayout.LabelField("输入要添加的ID（角色id从101开始）=,,=");
            }
            else if (m_curPlayerDatas.ContainsKey(playerId))
            {
                EditorGUILayout.LabelField("重复啦=..=");
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

            //设置字段类型
            m_curPlayerDatas.SetFieldType("player_id", FieldType.String);
            m_curPlayerDatas.SetFieldType("player_name", FieldType.String);
            m_curPlayerDatas.SetFieldType("start_talk_id", FieldType.String);

            m_curPlayerDatas.SetData(playerContent);
            DataEditorWindow.SaveData(m_playerCfgName, m_curPlayerDatas);
        }
    }

    //载入角色数据
    private void LoadPlayerData(string Id)
    {
        if(playerId == Id)
        {
            return;
        }

        playerId = Id;
        playerContent = m_curPlayerDatas[playerId];

        playerContent.TryGetValue("player_name", out playerName);
        playerContent.TryGetValue("start_talk_id", out startTalkId);

        PrepareChapterData();
    }

    #endregion

    #region 章节部分

    //章节列表  章节编辑器中进行编辑
    private List<string> m_chapterList = new List<string>();

    //当前选中章节下标
    private int m_curChapterIndex = 0;

    //当前章节ID
    private string m_curChapterId;

    //章节数据
    private DataTable m_chapterData;

    //读取章节数据
    private void PrepareChapterData()
    {
        //查找所有角色对应的对话
        AssetDatabase.Refresh();

        //加载章节信息
        m_chapterList.Clear();
        m_chapterList.Add("none");

        m_chapterData = DataManager.GetData("chapter");
        foreach (var cha in m_chapterData.Keys)
        {
            m_chapterList.Add(cha);
        }
    }

    //显示章节选择GUI
    private void ShowSelectChapterListGUI()
    {
        var mask = m_chapterList.ToArray();

        if (m_chapterList.Count <= 1)
        {
            return;
        }

        m_curChapterIndex = EditorGUILayout.Popup("当前章节：", m_curChapterIndex, mask);

        var chapterId = string.Empty;
        if (mask.Length != 0 && m_curChapterIndex != 0)
        {
            chapterId = mask[m_curChapterIndex];
        }

        if (!string.IsNullOrEmpty(m_curChapterId) && m_chapterList.Contains(m_curChapterId))
        {
            var curName = string.Empty;
            m_chapterData[m_curChapterId].TryGetValue("chapter_name", out curName);
            EditorGUILayout.LabelField(curName);
        }

        if(chapterId == m_curChapterId)
        {
            return;
        }

        m_curChapterId = chapterId;

        if (string.IsNullOrEmpty(m_curChapterId))
        {
            return;
        }

        PrepareTalk();
    }

    #endregion

    #region 对话部分

    //当前角色对应的对话
    private List<string> m_talkList = new List<string>();

    //当前角色的全部对话信息
    private DataTable m_playerTalkDatas;

    //获取对话文件名 角色ID+对话ID+talk
    private string GetTalkConfigName()
    {
        return playerId + m_curChapterId + "talk";
    }

    //初始化对话，当人物信息确定后，开始
    private void PrepareTalk()
    {
        //查找所有角色对应的对话
        AssetDatabase.Refresh();

        m_talkList.Clear();

        m_talkList.Add("none");

        var dataName = GetTalkConfigName();

        //加载人物对话信息
        m_talkList.Clear();
        m_talkList.Add("none");
        if (!DataManager.IsDataFileExist(dataName))
        {
            m_playerTalkDatas = new DataTable();
            return;
        }

        m_playerTalkDatas = DataManager.GetData(dataName);
        
        foreach (var talk in m_playerTalkDatas.Keys)
        {
            m_talkList.Add(talk);
        }

        m_curTalkIndex = 0;
    }

    //创建对话ID
    private string m_inputTalkId = string.Empty;

    //对话ID
    private string m_talkId = string.Empty;

    //当前选中的对话下标
    private int m_curTalkIndex = 0;

    private SingleData m_talkContent;

    //对话选择列表
    private void ShowSelectTalkListGUI()
    {
        if (m_talkList.Count <= 1)
        {
            return;
        }

        if(string.IsNullOrEmpty(playerId))
        {
            return;
        }

        if(string.IsNullOrEmpty(m_curChapterId))
        {
            return;
        }

        var mask = m_talkList.ToArray();

        m_curTalkIndex = EditorGUILayout.Popup("当前对话：", m_curTalkIndex, mask);
        if(m_curTalkIndex ==  0)
        {
            m_talkId = string.Empty;
        }

        if (mask.Length != 0 && m_curTalkIndex != 0)
        {
            LoadTalkData(mask[m_curTalkIndex]);
        }
    }

    private bool newTalkIsFfold;

    //创建对话部分
    private void AddTalkGUI()
    {
        if(string.IsNullOrEmpty(playerId))
        {
            return;
        }

        if(string.IsNullOrEmpty(m_curChapterId))
        {
            return;
        }

        //当前没有选中对话时，才显示
        if (m_curTalkIndex != 0)
        {
            return;
        }

        if (m_playerTalkDatas == null)
        {
            return;
        }

        EditorGUILayout.Space();

        newTalkIsFfold = EditorGUILayout.Foldout(newTalkIsFfold, "创建对话");
        if (newTalkIsFfold)
        {
            EditorGUI.indentLevel++;

            var fieldData = new SingleField(FieldType.String, m_inputTalkId, null);
            m_inputTalkId = EditorUtilGUI.FieldGUI_Type(fieldData, "对话ID");

            if (string.IsNullOrEmpty(m_inputTalkId))
            {
                EditorGUILayout.LabelField("输入要添加的ID（对话id从1001开始）=,,=");
            }
            else if (m_playerTalkDatas.ContainsKey(m_inputTalkId))
            {
                EditorGUILayout.LabelField("重复啦=..=");
            }

            EditorGUILayout.Space();

            if (!m_playerTalkDatas.ContainsKey(m_inputTalkId) && !string.IsNullOrEmpty(m_inputTalkId))
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("创建"))
                {
                    CreateNewTalk(m_inputTalkId);
                    m_inputTalkId = string.Empty;
                }
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();
        }
    }

    //创建对话
    private void CreateNewTalk(string _talkId)
    {
        m_talkContent = new SingleData();
        m_talkContent.Add("talk_id", _talkId);
        if (m_playerTalkDatas.TableKeys.Count == 0)
        {
            m_playerTalkDatas.TableKeys.Add("talk_id");
        }
        m_playerTalkDatas.AddData(m_talkContent);
        DataEditorWindow.SaveData(GetTalkConfigName(), m_playerTalkDatas);
        PrepareTalk();
    }

    //当前对话玩家文本
    private string[] m_pTalkText = new string[m_maxTalkContentCnt];

    //当前对话npc文本
    private string[] m_nTalkText = new string[m_maxTalkContentCnt];

    //下一个对话ID
    private string[] m_nextTalkId = new string[m_maxTalkContentCnt];

    //对话编辑和跳转
    private void EditorTalkGUI()
    {
        if(string.IsNullOrEmpty(m_talkId))
        {
            return;
        }

        //todo 向上一个跳转  如果没上一个对话，则不显示跳转按钮
        //遍历所有对话，找到

        EditorGUILayout.Space();

        EditorTalkContentGUI();

        EditorGUILayout.Space();

        if (GUILayout.Button("保存对话"))
        {
            for(var i = 0; i < m_maxTalkContentCnt; ++i)
            {
                var pTalkKey = string.Format("p_talk_text_{0}", i + 1);
                m_talkContent[pTalkKey] = m_pTalkText[i];
                m_playerTalkDatas.SetFieldType(pTalkKey, FieldType.String);

                var nTalkKey = string.Format("n_talk_text_{0}", i + 1);
                m_talkContent[nTalkKey] = m_nTalkText[i];
                m_playerTalkDatas.SetFieldType(nTalkKey, FieldType.String);

                var nextTalkIdKey = string.Format("next_talk_id_{0}", i + 1);
                m_talkContent[nextTalkIdKey] = m_nextTalkId[i];
                m_playerTalkDatas.SetFieldType(nextTalkIdKey, FieldType.String);
            }

            m_playerTalkDatas.SetData(m_talkContent);
            DataEditorWindow.SaveData(GetTalkConfigName(), m_playerTalkDatas);
        }
    }

    //预设4个位置
    private bool[] m_talkFold = new bool[m_maxTalkContentCnt];

    //对话编辑部分
    private void EditorTalkContentGUI()
    {
        EditorGUI.indentLevel++;

        for (var j = 0; j < m_maxTalkContentCnt; ++j)
        {
            var foldName = string.Format("分支对话{0}", j + 1);
            m_talkFold[j] = EditorGUILayout.Foldout(m_talkFold[j], foldName);

            if (m_talkFold[j])
            {
                EditorGUILayout.Space();

                //当前玩家对话内容编辑
                var fieldData = new SingleField(FieldType.String, m_pTalkText[j], null);
                m_pTalkText[j] = EditorUtilGUI.FieldGUI_Type(fieldData, string.Format("玩家对话内容"));

                //当前npc回答对话内容编辑
                fieldData = new SingleField(FieldType.String, m_nTalkText[j], null);
                m_nTalkText[j] = EditorUtilGUI.FieldGUI_Type(fieldData, string.Format("NPC对话内容"));

                //下一个对话
                fieldData = new SingleField(FieldType.String, m_nextTalkId[j], null);
                m_nextTalkId[j] = EditorUtilGUI.FieldGUI_Type(fieldData, string.Format("下个对话ID"));

                EditorGUILayout.Space();

                //todo 向下一个对话跳转
                if (!string.IsNullOrEmpty(m_nextTalkId[j]))
                {
                    if (GUILayout.Button(string.Format("跳转到:{0}", m_nextTalkId[j])))
                    {
                        if (!m_playerTalkDatas.ContainsKey(m_nextTalkId[j]))
                        {
                            CreateNewTalk(m_nextTalkId[j]);
                            return;
                        }

                        for (var i = 0; i < m_talkList.Count; ++i)
                        {
                            if (m_nextTalkId[j] == m_talkList[i])
                            {
                                m_curTalkIndex = i;
                            }
                        }

                        LoadTalkData(m_nextTalkId[j]);
                    }
                }
            }
        }
    }

    //载入对话数据
    private void LoadTalkData(string _talkId)
    {
        //没有变化，不做赋值
        if (string.IsNullOrEmpty(_talkId) || m_talkId == _talkId)
        {
            return;
        }

        m_talkId = _talkId;

        for(var i = 0; i < m_talkFold.Length;++i)
        {
            m_talkFold[i] = false;
        }

        m_talkContent = m_playerTalkDatas[m_talkId];

        for (var i = 0; i < m_maxTalkContentCnt; ++i)
        {
            m_talkContent.TryGetValue(string.Format("p_talk_text_{0}]", i + 1), out m_pTalkText[i]);
            m_talkContent.TryGetValue(string.Format("n_talk_text_{0}", i + 1), out m_nTalkText[i]);
            m_talkContent.TryGetValue(string.Format("next_talk_id_{0}", i + 1), out m_nextTalkId[i]);
        }
    }

    #endregion
}
