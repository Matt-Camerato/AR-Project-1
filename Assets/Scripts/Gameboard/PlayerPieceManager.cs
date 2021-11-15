using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPieceManager : MonoBehaviour
{
    private bool startupRiseDone = false;

    [HideInInspector] public bool doneMoving = false;
    
    private bool doneLowering = false;
    private bool doneRising = false;

    private void Update()
    {
        if (!startupRiseDone)
        {
            transform.localPosition += Vector3.up * Time.deltaTime * 0.1f;

            if (transform.localPosition.y >= 0.1f)
            {
                transform.localPosition = new Vector3(0, 0.1f, 0);

                startupRiseDone = true;
            }
        }
    }

    public void MovePlayerPiece(TileManager destination)
    {
        if (!doneLowering)
        {
            transform.localPosition += Vector3.down * Time.deltaTime * 0.1f;

            if (transform.localPosition.y <= -0.02f)
            {
                doneLowering = true;

                transform.parent = destination.transform;
                transform.localPosition = new Vector3(0, -0.02f, 0);
            }
        }
        else if(!doneRising)
        {
            transform.localPosition += Vector3.up * Time.deltaTime * 0.1f;

            if(transform.localPosition.y >= 0.1f)
            {
                transform.localPosition = new Vector3(0, 0.1f, 0);

                doneRising = true;
            }
        }
        else
        {
            doneMoving = true; //this functions stops being called once this is true

            //on last call of this function, reset private bools for next time piece moves
            doneLowering = false;
            doneRising = false;
        }
    }

    public void ResetPosition(TileManager pos)
    {
        //reset pieces parent and local position
        transform.parent = pos.transform;
        transform.localPosition = new Vector3(0, 0.1f, 0);
    }
}
