using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameboardManager : MonoBehaviour
{
    [Header("Gameboard References")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject attackCubePrefab;

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

    //called by gameplay manager to spawn attacks at specific tiles
    public GameObject SpawnAttackCube(TileManager tile, Player attacker)
    {
        GameObject newCube = Instantiate(attackCubePrefab, Vector3.zero, transform.rotation, tile.transform);
        float yValue = -0.07f - (tile.currentCubeCount * 0.09f); //this equation determines what y level the cube should spawn at based on number of cubes under current tile
        newCube.transform.localPosition = new Vector3(0, yValue, 0);
        newCube.transform.GetChild(0).GetComponent<MeshRenderer>().materials[0].color = attacker.color; //set new cube border color to attacker's color

        return newCube; //return new cube so it can be added to list of spawned cubes (which can all be deleted before next player's recap)
    }
}
