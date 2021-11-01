using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPieceManager : MonoBehaviour
{
    private bool startupRiseDone = false;

    private void Update()
    {
        if (!startupRiseDone)
        {
            transform.localPosition += Vector3.up * Time.deltaTime * 0.05f;

            if (transform.localPosition.y >= 0.15f)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, 0.15f, transform.localPosition.z);

                startupRiseDone = true;
            }
        }
    }

    public void MovePlayerPiece(Vector2 destination)
    {
        
    }
}
