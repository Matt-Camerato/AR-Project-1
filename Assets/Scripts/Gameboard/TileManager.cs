using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [Header("Tile Info")]
    public int currentCubeCount = 0;
    public int x, y;
    public Player playerOnTile = null;

    private float speed;
    private bool hasRisen = false;

    //startup cube animation based on position in grid
    private void Start()
    {
        int xFromMid = Mathf.Abs(x - 2); //2, 1, 0, 1, 2
        int yFromMid = Mathf.Abs(y - 2); //2, 1, 0, 1, 2

        //GRID DIAGRAM (center will go up first, then effect ripples
        //2,2 1,2 0,2 1,2 2,2 (4, 3, 2, 3, 4)
        //2,1 1,1 0,1 1,1 2,1 (3, 2, 1, 2, 3)
        //2,0 1,0 0,0 1,0 2,0 (2, 1, 0, 1, 2)
        //2,1 1,1 0,1 1,1 2,1 (3, 2, 1, 2, 3)
        //2,2 1,2 0,2 1,2 2,2 (4, 3, 2, 3, 4)

        speed = 5 - (xFromMid + yFromMid); //center tile is speed 5, corner tile is speed 1
        speed /= 50; //tiles from corner to center are now 0.02, 0.04, 0.06, 0.08, and 0.1
        speed += (0.1f - speed) / 2; //lastly, speeds become 0.06, 0.07, 0.08, 0.09, and 0.1
    }

    //tile rising animation
    private void Update()
    {
        if (!hasRisen)
        {
            transform.localPosition += Vector3.up * Time.deltaTime * speed * 0.7f; //make tile rise

            if (transform.localPosition.y >= 0.05f) 
            {
                transform.localPosition = new Vector3(transform.localPosition.x, 0.05f, transform.localPosition.z); //make sure all tiles are at same height in finished position

                hasRisen = true;
            }
        }
    }
}
