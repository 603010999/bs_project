using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConfigMgr : Singleton<PlayerConfigMgr>
{
    //角色配置
    private Dictionary<string, PlayerConfig> m_playerCfgDic = new Dictionary<string, PlayerConfig>();

    //角色列表
    private List<string> m_playerList = new List<string>();

    public PlayerConfigMgr()
    {
        LoadPlayerData();
    }

    //获取玩家列表
    public List<string> GetPlayerList()
    {
        return m_playerList;
    }

    //获取单独数据
    public PlayerConfig GetPlayerCfg(string playerId)
    {
        PlayerConfig cfg = null;
        if(m_playerCfgDic.TryGetValue(playerId,out cfg))
        {
            return cfg;
        }

        return null;
    }

    //读取
    private void LoadPlayerData()
    {
        m_playerList.Clear();

        //读配置
        var dataTable = DataManager.GetData("player");
        if (dataTable == null)
        {
            return;
        }

        for (var i = 0; i < dataTable.TableIDs.Count; ++i)
        {
            var player = new PlayerConfig();

            var playerId = dataTable.TableIDs[i];

            player.LoadData(playerId);

            m_playerCfgDic.Add(playerId, player);

            m_playerList.Add(playerId);
        }
    }
}
