using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedRecapInfo : MonoBehaviour
{
    //THIS ENTIRE SCRIPT IS JUST A HOTFIX DUE TO RECAP INFO NOT SAVING PROPERLY FOR SOME REASON
    //ATTACKS ONLY WORK ONCE AND THEN ARE OVERWRITTEN WHEN THEY SHOULDN"T BE, MOST LIKELY DUE TO RECAP INFO NOT ACTUALLY MAKING A NEW PLAYER COPY WHEN SAVING
    //UNFORTUNATELY, THE MOVE PHASE OF RECAP ANIMATION STILL USES OLD RECAP INFO AND SOMEHOW STILL WORKS, SO CAN'T TAKE IT OUT EITHER

    private GameplayManager gm;
    private GameObject[,] tiles;
    private Vector2Int[,] savedAttacks;

    private void Start()
    {
        gm = GetComponent<GameplayManager>();
        tiles = GetComponent<GameboardManager>().tiles;
    }

    public void SaveAttacks(Player[] _players)
    {
        savedAttacks = new Vector2Int[GameplayManager.numPlayers, gm.maxAttackNum]; //make savedAttacks 2D array where x = playerNum and y = attackNum

        Player[] players = _players;

        for(int p = 0; p < players.Length; p++)
        {
            for(int a = 0; a < gm.maxAttackNum; a++)
            {
                TileManager currentAttack = players[p].attacks[a];
                Vector2Int attackPos = new Vector2Int(currentAttack.x, currentAttack.y);

                savedAttacks[p, a] = attackPos; //save Vector2Int of position of each attack (a) for each player (p)
            }
        }
    }

    public TileManager[] GetCurrentAttacks(int p)
    {
        TileManager[] currentAttacks = new TileManager[gm.maxAttackNum];
        
        for(int a = 0; a < gm.maxAttackNum; a++) //loop through current player's saved attacks and set up currentAttacks tile manager array to return
        {
            Vector2Int currentAttackPos = savedAttacks[p, a];
            TileManager currentAttackTile = tiles[currentAttackPos.x, currentAttackPos.y].GetComponent<TileManager>();
            currentAttacks[a] = currentAttackTile;
        }

        return currentAttacks; //return current player's saved attacks
    }
}
