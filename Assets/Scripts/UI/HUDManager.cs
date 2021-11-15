using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("HUD References")]
    [SerializeField] private GameObject PassScreen;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text playerText;
    [SerializeField] private Transform heartIcons;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    
    public GameObject damagePanel;

    [HideInInspector] public GameplayManager gameplayManager;
    [HideInInspector] public bool currentPlayerReady;

    [HideInInspector] public Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>(); //setup animator reference

        currentPlayerReady = true; //set to true at start of game for round 1 turn 1 (since player 1 doesn't need to pass phone yet) 
    }

    private void OnEnable() //when game starts, HUD is turned on and set to default state based on initialized game settings
    {
        roundText.text = "Round 1"; //set default round text
        playerText.text = "Player 1"; //set default player text
        playerText.transform.parent.GetComponent<Image>().color = gameplayManager.currentPlayer.color; //set color to randomly chosen color for player 1

        //update heart icons to full health
        foreach (Transform child in heartIcons)
        {
            child.GetComponent<Image>().sprite = fullHeartSprite;
        }
    }

    public void BeginTurnButton()
    {
        PassScreen.SetActive(false);

        currentPlayerReady = true;

        EndStartupEvent();
    }

    public void EndStartupEvent()
    {
        if (currentPlayerReady)
        {
            if (gameplayManager.roundNum == 1)
            {
                anim.SetTrigger("Move"); //trigger move startup animation
            }
            else
            {
                anim.SetTrigger("Recap"); //if not round 1, trigger recap startup animation before move
            }
        }
    }

    public void UpdateHealthIcons(int health)
    {
        if (health == 3)
        {
            foreach (Transform child in heartIcons)
            {
                child.GetComponent<Image>().sprite = fullHeartSprite;
            }
        }
        else if (health == 2)
        {
            heartIcons.GetChild(0).GetComponent<Image>().sprite = fullHeartSprite;
            heartIcons.GetChild(1).GetComponent<Image>().sprite = fullHeartSprite;
            heartIcons.GetChild(2).GetComponent<Image>().sprite = emptyHeartSprite;
        }
        else if (health == 1)
        {
            heartIcons.GetChild(0).GetComponent<Image>().sprite = fullHeartSprite;
            heartIcons.GetChild(1).GetComponent<Image>().sprite = emptyHeartSprite;
            heartIcons.GetChild(2).GetComponent<Image>().sprite = emptyHeartSprite;
        }
        else
        {
            foreach (Transform child in heartIcons)
            {
                child.GetComponent<Image>().sprite = emptyHeartSprite;
            }
        }
    }

    public void OkayButton()
    {
        currentPlayerReady = false; //this skips rest of player's turn since they died in recap

        anim.SetTrigger("Okay");
    }

    public void MoveTutorialEvent()
    {
        if(gameplayManager.roundNum == 1) { anim.SetTrigger("MoveTutorial"); } //trigger tutorial text if round 1
    }

    public void ConfirmDestinationButton()
    {
        //turn off all indicators except the one for player's destination
        Vector2 destination = new Vector2(gameplayManager.currentPlayer.destination.x, gameplayManager.currentPlayer.destination.y);
        foreach (Vector2 move in gameplayManager.currentPossibleMoves)
        {
            if (move != destination)
            {
                gameplayManager.GetComponent<GameboardManager>().tiles[(int)move.x, (int)move.y].transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        gameplayManager.destinationDetermined = true;

        anim.SetTrigger("Attack"); //trigger attack startup animation (this includes removing move tutorial)
    }

    public void AttackTutorialEvent()
    {
        if (gameplayManager.roundNum == 1) { anim.SetTrigger("AttackTutorial"); } //trigger tutorial text if round 1
    }

    public void ConfirmAttacksButton()
    {
        gameplayManager.attacksDetermined = true;

        anim.SetTrigger("EndTurn"); //trigger end turn animation (event below will happen once screen fades to black
    }

    public void EndTurnEvent()
    {
        roundText.text = "Round " + gameplayManager.roundNum; //update round #
        playerText.text = "Player " + gameplayManager.currentPlayer.playerNum; //update player #
        playerText.transform.parent.GetComponent<Image>().color = gameplayManager.currentPlayer.color; //update player color

        //update heart icons (player health)
        int health = gameplayManager.currentPlayer.health;
        if(health == 3)
        {
            foreach(Transform child in heartIcons)
            {
                child.GetComponent<Image>().sprite = fullHeartSprite;
            }
        }
        else if(health == 2)
        {
            heartIcons.GetChild(0).GetComponent<Image>().sprite = fullHeartSprite;
            heartIcons.GetChild(1).GetComponent<Image>().sprite = fullHeartSprite;
            heartIcons.GetChild(2).GetComponent<Image>().sprite = emptyHeartSprite;
        }
        else if(health == 1)
        {
            heartIcons.GetChild(0).GetComponent<Image>().sprite = fullHeartSprite;
            heartIcons.GetChild(1).GetComponent<Image>().sprite = emptyHeartSprite;
            heartIcons.GetChild(2).GetComponent<Image>().sprite = emptyHeartSprite;
        }
        else
        {
            foreach (Transform child in heartIcons)
            {
                child.GetComponent<Image>().sprite = emptyHeartSprite;
            }
        }

        //loop through gameboard tiles and turn off all indicators before next player's turn
        var allTiles = gameplayManager.GetComponent<GameboardManager>().tiles;
        for (int x = 0; x < allTiles.GetLength(0); x++)
        {
            for (int y = 0; y < allTiles.GetLength(1); y++)
            {
                GameObject currentTileIndicator = allTiles[x, y].transform.GetChild(1).gameObject;
                if (currentTileIndicator.activeSelf) { currentTileIndicator.SetActive(false); } //if indicator is on, turn it off (indicator colors will be fixed during next player's turn)
            }
        }

        if(gameplayManager.roundNum > 1) { gameplayManager.ResetGameboard(); } //if its the end of the 2nd round or later, gameboard is reset so recap phase can be played for next player

        if (gameplayManager.currentPlayer.health != 0) { PassScreen.SetActive(true); } //turn on pass screen when screen fades to black so next player can ready up before playing (only if player isn't dead)
    }
}
