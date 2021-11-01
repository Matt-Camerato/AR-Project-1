using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public bool gameInitialized = false;

    [SerializeField] private int roundNum = 1;
    [SerializeField] private int turnNum = 1;

    private bool currentPlayerReady = false;

    private bool destinationDetermined = false;
    private bool attacksDetermined = false;


    private void Update()
    {
        if (gameInitialized)
        {
            if (!currentPlayerReady)
            {
                if (roundNum != 1 || turnNum != 1) //show ready up screen (dones't happen round 1 turn 1)
                {
                    
                }
            }
            else
            {
                if (!destinationDetermined)
                {
                    //move phase
                }
                else if(!attacksDetermined)
                {
                    //attack phase
                }
                else
                {
                    //turn is over (initialize scene for next player's turn)
                    //set new turn num (and round num if turn num greater than numPlayers)
                    //reset private bools

                }
            }
        }
    }

    public void ReadyButton() { currentPlayerReady = true; } //used for ready button on ready up screen
}
