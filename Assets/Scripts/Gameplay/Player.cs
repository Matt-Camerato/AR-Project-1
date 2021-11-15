using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public int playerNum;
    public Color color;
    public Vector2Int position;
    public GameObject gamePiece;
    public int health = 3;

    public TileManager destination;
    public List<TileManager> attacks = new List<TileManager>();

    //variables used for attack recap phase
    public bool spawnedAttacks;
    public bool attacksDoneMoving;

    public List<Vector2> PossibleMoves()
    {
        List<Vector2> possibleMoves = new List<Vector2>();

        //move right
        Vector2 rightMove = position + Vector2.right;
        if (rightMove.x <= 4) { possibleMoves.Add(rightMove); }

        //move left
        Vector2 leftMove = position + Vector2.left;
        if (leftMove.x >= 0) { possibleMoves.Add(leftMove); }

        //move up
        Vector2 upMove = position + Vector2.down;
        if (upMove.y >= 0) { possibleMoves.Add(upMove); }

        //move down
        Vector2 downMove = position + Vector2.up;
        if (downMove.y <= 4) { possibleMoves.Add(downMove); }

        return possibleMoves;
    }
}
