using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameboardManager : MonoBehaviour
{
    [Header("Gameboard References")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject playerCubePrefab;

    [HideInInspector] public GameObject[,] tiles;

    private Animator anim;


    private void Start()
    {
        anim = GetComponent<Animator>();
        
        tiles = new GameObject[5, 5]; //gameboard is 5x5 grid only (FOR NOW)

        for (int x = 0; x < 5; x++)
        {
            for(int y = 0; y < 5; y++)
            {
                GameObject currentTile = Instantiate(tilePrefab, Vector3.zero, transform.rotation, transform.GetChild(1));
                currentTile.transform.localPosition = new Vector3((x * 0.16f) - 0.32f, -0.05f, (y * 0.16f) - 0.32f);
                currentTile.name = "Tile (" + x + "," + y + ")";
                tiles[x, y] = currentTile;

                TileManager currentTileManager = currentTile.GetComponent<TileManager>();
                currentTileManager.x = x;
                currentTileManager.y = y;
            }
        }
    }

    //called by players to attack specific tiles
    public static void SpawnPlayerCube(int x, int y, Player attacker)
    {

    }
}
