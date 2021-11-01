using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.AR;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("AR Setup References")]
    [SerializeField] private ARTapToPlace tapToPlace;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private GameObject gamePrefab;
    [SerializeField] private ARGestureInteractor gestureInteractor;

    [Header("UI References")]
    [SerializeField] private GameObject HUD_Canvas;
    [SerializeField] private TMP_Text numPlayersText;
    [SerializeField] private Button increasePlayersButton;
    [SerializeField] private Button decreasePlayersButton;
    [SerializeField] private Button confirmMoveButton;
    [SerializeField] private Button confirmAttacksButton;

    private GameplayManager gameplayManager;

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void ContinueButton() //after setting game marker, continue button lets player proceed to main menu
    {
        anim.SetTrigger("InitialSetupFinished"); //triggger UI animation

        planeManager.enabled = false; //disable plane manager
        foreach (var plane in planeManager.trackables) { plane.gameObject.SetActive(false); } //disable all current planes

        Vector3 gamePos = tapToPlace.spawnedGameMarker.transform.position; //save position of game marker
        Quaternion gameRot = tapToPlace.spawnedGameMarker.transform.rotation; //save rotation of game marker
        tapToPlace.enabled = false; //disable movement of game marker
        Destroy(tapToPlace.spawnedGameMarker); //delete game marker object

        GameObject gameBoard = Instantiate(gamePrefab, gamePos, gameRot); //instantiate game at markers position

        //set gameplay manager references
        gameplayManager = gameBoard.GetComponent<GameplayManager>();
        gameplayManager.UI_Canvas = gameObject;
        gameplayManager.HUD_Canvas = HUD_Canvas;
        gameplayManager.confirmMoveButton = confirmMoveButton;
        gameplayManager.confirmAttacksButton = confirmAttacksButton;
        gameplayManager.gestureInteractor = gestureInteractor;

        HUD_Canvas.GetComponent<HUDManager>().gameplayManager = gameplayManager; //set HUD reference for gameplay manager as well

        DebugLog.Log(tapToPlace.isActiveAndEnabled.ToString());
    }

    public void NewGame() //on main menu, takes player to game setup screen
    {
        anim.SetTrigger("NewGame"); //trigger UI animation
    }

    public void QuitGame() { Application.Quit(); } //on main menu, quits game

    public void UpdateNumPlayers(bool increased)
    {
        //update static int
        if (increased) { GameplayManager.numPlayers++; }
        else { GameplayManager.numPlayers--; }

        numPlayersText.text = GameplayManager.numPlayers.ToString(); //update counter text
        
        //update button interactability
        if(GameplayManager.numPlayers == 8)
        {
            increasePlayersButton.interactable = false;
        }
        else if(GameplayManager.numPlayers == 2)
        {
            decreasePlayersButton.interactable = false;
        }
        else
        {
            increasePlayersButton.interactable = true;
            decreasePlayersButton.interactable = true;
        }
    }

    public void StartGame() //start actual game loop
    {
        anim.SetTrigger("StartGame"); //trigger UI animation

        gameplayManager.InitializeGame(); //initialize game by creating players based on numPlayers and setting their colors
    }

    public void StartGameEvent() //animation event to trigger start of game
    {
        gameplayManager.startedGame = true; //once screen goes black, actually start game
        gameplayManager.SpawnPlayerPieces(); //spawn player pieces when player cant see (black screen)

        HUD_Canvas.SetActive(true); //turn on HUD_Canvas
        gameObject.SetActive(false); //turn off UI_Canvas
    }

    public void BackButton() //on setup screen, takes player back to main menu
    {
        anim.SetTrigger("BackToMainMenu"); //trigger UI animation
    }
}
