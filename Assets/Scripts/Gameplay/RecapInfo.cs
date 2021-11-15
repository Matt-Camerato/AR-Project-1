using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecapInfo
{
    public List<Player> players = new List<Player>();
    public int[,] cubeCounts = new int[5, 5];

    public void SavePlayerInfo(Player player)
    {
        //make copy of current player's info and save it to player list
        //(don't know if all of this is necessary to get a copy but will keep it like this for now)

        Player p = new Player();
        p.attacks = player.attacks;
        p.playerNum = player.playerNum;
        p.color = player.color;
        p.position = player.position;
        p.gamePiece = player.gamePiece;
        p.health = player.health;
        p.destination = player.destination;
        

        players.Add(p);
    }

    public void SaveTileInfo(GameObject[,] currentTiles)
    {
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                cubeCounts[x, y] = currentTiles[x, y].GetComponent<TileManager>().currentCubeCount;
            }
        }
    }
}
