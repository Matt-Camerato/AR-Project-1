using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecapInfo
{
    public List<Player> players = new List<Player>();

    public void SavePlayerInfo(Player player)
    {
        Player p = player;
        players.Add(p);
    }
}
