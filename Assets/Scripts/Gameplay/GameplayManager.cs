using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class GameplayManager : MonoBehaviour
{
    [Header("Game Info")]
    public int roundNum = 1;
    [SerializeField] private int turnNum = 1;
    [SerializeField] private int maxAttackNum;

    static public int numPlayers = 4;

    [Header("Gameplay Settings")]
    [SerializeField] private GameObject playerPiecePrefab;
    [SerializeField] private List<Color> playerColors;

    [Header("Tile Indicator Colors")]
    [SerializeField] private Color tileIndicatorColor;
    [SerializeField] private Color tileSelectedColor;

    //UI References
    [HideInInspector] public GameObject UI_Canvas;
    [HideInInspector] public GameObject HUD_Canvas;
    [HideInInspector] public Button confirmMoveButton;
    [HideInInspector] public Button confirmAttacksButton;

    [HideInInspector] public ARGestureInteractor gestureInteractor;

    private List<Color> selectedColors;

    [HideInInspector] public bool startedGame = false;
    private bool gameInitialized = false;

    //in-game variables
    [HideInInspector] public Player currentPlayer;

    private bool tapGestureEnabled = false;
    
    private bool roundRecapFinished = true; //set to true for first round
    
    private bool destinationInitialized = false;
    [HideInInspector] public List<Vector2> currentPossibleMoves;
    private bool destinationSelected = false;
    [HideInInspector] public bool destinationDetermined = false;

    private bool attacksInitialized = false;
    [HideInInspector] public bool attacksDetermined = false;


    private Player[] players;
    //private bool movedPosition = false;
    //private int currentAttackNum = 0;

    private void Start()
    {
        HUD_Canvas.GetComponent<HUDManager>().gameplayManager = this;
        gestureInteractor.tapGestureRecognizer.onGestureStarted += OnTileTapped;
    }

    private void OnEnable()
    {
        gestureInteractor.tapGestureRecognizer.onGestureStarted += OnTileTapped;
    }

    private void OnDisable()
    {
        gestureInteractor.tapGestureRecognizer.onGestureStarted -= OnTileTapped;
    }

    private void Update()
    {
        if (startedGame && gameInitialized)
        {
            if (HUD_Canvas.GetComponent<HUDManager>().currentPlayerReady) //show ready up screen (doesn't happen round 1 turn 1)
            {
                if (!roundRecapFinished) //once player is ready, play round recap (1st round there isn't one)
                {
                    //---RECAP PHASE---
                        
                    //NEED TO DO THIS STILL!!!!!! (FOR EVERY ROUND BUT 1)

                    //Handle playing of recap HERE <--------

                    //first, show every players movement by pieces lowering into tiles, teleporting to new ones, and then rising (if two players have same destination, whoever has less life gets priority)

                    //once pieces all moved, go through attacks player by player
                    //using attack locations of player, spawn cubes in those positions and raise them

                    //lastly, determine who is highest player once all attacks are done and cause them to lose gggga life
                    //POSSIBLE FEATURE: whoever has a cube in damaged player's tower gets a life back, or instead whoever placed a tile there that round (possibly just last person to place tile so everyone doesn't heal)
                }
                else
                {
                    if (!destinationDetermined)
                    {
                        //---MOVE PHASE---

                        //initialize possible moves for current player with tile indicators
                        if (!destinationInitialized)
                        {
                            currentPossibleMoves = currentPlayer.PossibleMoves();

                            //turn on tile indicator for each possible move
                            foreach (Vector2 move in currentPossibleMoves)
                            {
                                //fix color of each tile indicator before turning it on
                                GameObject tileIndicator = GetComponent<GameboardManager>().tiles[(int)move.x, (int)move.y].transform.GetChild(1).gameObject;
                                Color indicatorColorAlphaFix = new Color(tileIndicatorColor.r, tileIndicatorColor.g, tileIndicatorColor.b, 0.5f);
                                tileIndicator.GetComponent<MeshRenderer>().material.color = indicatorColorAlphaFix;
                                tileIndicator.transform.GetChild(0).GetComponent<ParticleSystem>().startColor = tileIndicatorColor;

                                tileIndicator.SetActive(true);
                            }

                            tapGestureEnabled = true; //enable tap gestures so player can choose destination (and later attack locations)

                            destinationInitialized = true;
                        }
                    }
                    else if (!attacksDetermined)
                    {
                        //---ATTACK PHASE---

                        //initialize attacks by setting all indicators (except players destination) to current player's color
                        if (!attacksInitialized)
                        {
                            var allTiles = GetComponent<GameboardManager>().tiles;
                            for(int x = 0; x < allTiles.GetLength(0); x++)
                            {
                                for(int y = 0; y < allTiles.GetLength(1); y++)
                                {
                                    if (allTiles[x,y].GetComponent<TileManager>() != currentPlayer.destination)
                                    {
                                        GameObject tileIndicator = allTiles[x, y].transform.GetChild(1).gameObject;
                                        Color playerColorAlphaFix = new Color(currentPlayer.color.r, currentPlayer.color.g, currentPlayer.color.b, 0.5f);
                                        tileIndicator.GetComponent<MeshRenderer>().material.color = playerColorAlphaFix;
                                        tileIndicator.transform.GetChild(0).GetComponent<ParticleSystem>().startColor = currentPlayer.color;
                                    }
                                }
                            }

                            confirmAttacksButton.gameObject.SetActive(true);

                            attacksInitialized = true;
                        }
                    }
                    else
                    {
                        //turn is over (initialize scene for next player's turn)

                        tapGestureEnabled = false; //turn off gestures after player has confirmed attacks

                        //set new turn num (and round num if end of round)
                        if (turnNum == numPlayers)
                        {
                            roundNum++;
                            turnNum = 1;
                        }
                        else { turnNum++; }

                        //save current player destination and attacks for use in recap (if not done earlier) <-----STILL NEED TO DO THIS

                        currentPlayer = players[turnNum - 1]; //update current player with new turn #

                        //------> NEED TO ADD CHECK IF PLAYER HAS DIED YET TO SEE IF THEIR TURN WILL BE SKIPPED <------

                        //reset bools
                        destinationInitialized = false;
                        destinationSelected = false;
                        destinationDetermined = false;

                        attacksInitialized = false;
                        attacksDetermined = false;

                        HUD_Canvas.GetComponent<HUDManager>().currentPlayerReady = false; //this will show ready up screen for next player
                    }
                }
            }
        }
    }

    private void OnTileTapped(TapGesture obj)
    {

        if (tapGestureEnabled) //first check if player can tap (only can do this during move and attack phases)
        {
            Vector2 tapPos = obj.startPosition;
            if (!tapPos.IsPointOverUIObject()) //make sure UI isn't in way
            {
                var ray = Camera.main.ScreenPointToRay(tapPos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.collider.CompareTag("Tile"))
                    {
                        //---PLAYER HIT A TILE---

                        TileManager tile = hit.collider.transform.parent.GetComponent<TileManager>();

                        if (!destinationDetermined)
                        {
                            HandleMovePhase(tile); //---MOVE PHASE---
                        }
                        else
                        {
                            HandleAttackPhase(tile); //---ATTACK PHASE---
                        }
                    }
                }
            }
        }
    }

    public void InitializeGame()
    {
        players = new Player[numPlayers];
        selectedColors = new List<Color>();

        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new Player();

            //set player #
            players[i].playerNum = i + 1;

            //determine player color
            int randomColorIndex = Random.Range(0, playerColors.Count);
            while (selectedColors.Contains(playerColors[randomColorIndex]))
            {
                randomColorIndex = Random.Range(0, playerColors.Count);
            }
            players[i].color = playerColors[randomColorIndex];
            selectedColors.Add(playerColors[randomColorIndex]);
        }
        
        currentPlayer = players[0]; //set current player to player 1

        gameInitialized = true;
    }

    public void SpawnPlayerPieces()
    {
        for (int i = 0; i < players.Length; i++)
        {
            //set position based on playerNum and numPlayers + instantiate player piece
            Vector2 playerCoords = PosFromPlayerNum(i);
            players[i].position = playerCoords;
            Vector3 playerPos = GetComponent<GameboardManager>().tiles[(int)playerCoords.x, (int)playerCoords.y].transform.position;
            playerPos.y = 0; //pieces rise up from tiles when game is initialized

            players[i].gamePiece = Instantiate(playerPiecePrefab, playerPos, Quaternion.identity, transform.GetChild(2));
            players[i].gamePiece.transform.localPosition = new Vector3(players[i].gamePiece.transform.localPosition.x, 0, players[i].gamePiece.transform.localPosition.z);
            players[i].gamePiece.GetComponent<MeshRenderer>().material.color = players[i].color; //set game piece color using playerColors[randomColorIndex];
            players[i].gamePiece.transform.GetChild(0).GetComponent<ParticleSystem>().startColor = players[i].color; //set game piece particles color
        }
    }
    
    private Vector2 PosFromPlayerNum(int playerNum)
    {
        switch (playerNum)
        {
            case 0:
                return new Vector2(0, 0);
            case 1:
                return new Vector2(4, 4);
            case 2:
                return new Vector2(4, 0);
            case 3:
                return new Vector2(0, 4);
            case 4:
                return new Vector2(0, 2);
            case 5:
                return new Vector2(4, 2);
            case 6:
                return new Vector2(2, 0);
            case 7:
                return new Vector2(2, 4);
            default:
                Debug.Log("Not a possible player num");
                return Vector2.zero;
        }
    }

    private void HandleMovePhase(TileManager tile)
    {
        if (currentPossibleMoves.Contains(new Vector2(tile.x, tile.y))) //make sure tile selected is a possible move
        {
            if (destinationSelected)
            {
                //if player had already selected a destinaiton, change old destination tile back to default state (fix color of indicator and particles)
                GameObject tileIndicator = currentPlayer.destination.transform.GetChild(1).gameObject;
                Color indicatorColorAlphaFix = new Color(tileIndicatorColor.r, tileIndicatorColor.g, tileIndicatorColor.b, 0.5f);
                tileIndicator.GetComponent<MeshRenderer>().material.color = indicatorColorAlphaFix;

                ParticleSystem psIndicator = tileIndicator.transform.GetChild(0).GetComponent<ParticleSystem>();
                ParticleSystem.Particle[] psIndicatorParticles = new ParticleSystem.Particle[psIndicator.particleCount];
                psIndicator.GetParticles(psIndicatorParticles);
                for (int i = 0; i < psIndicatorParticles.Length; i++)
                {
                    Color32 indicatorColorFix = tileIndicatorColor;
                    psIndicatorParticles[i].color = indicatorColorFix;
                }
                tileIndicator.transform.GetChild(0).GetComponent<ParticleSystem>().startColor = tileIndicatorColor;
            }
            else
            {
                //if no previous destination tile exists, turn on confirm move button (this is first time player has tapped on tile in move phase)
                confirmMoveButton.gameObject.SetActive(true);
            }

            //change colors of indicator on newly selected destination
            Transform currentTileIndicator = tile.transform.GetChild(1);
            Color selectedColorAlphaFix = new Color(tileSelectedColor.r, tileSelectedColor.g, tileSelectedColor.b, 0.5f);
            currentTileIndicator.GetComponent<MeshRenderer>().material.color = selectedColorAlphaFix;

            ParticleSystem psSelected = currentTileIndicator.GetChild(0).GetComponent<ParticleSystem>();
            ParticleSystem.Particle[] psSelectedParticles = new ParticleSystem.Particle[psSelected.particleCount];
            psSelected.GetParticles(psSelectedParticles);
            for (int i = 0; i < psSelectedParticles.Length; i++)
            {
                Color32 selectedColorFix = tileSelectedColor;
                psSelectedParticles[i].color = selectedColorFix;
            }
            currentTileIndicator.GetChild(0).GetComponent<ParticleSystem>().startColor = tileSelectedColor;

            currentPlayer.destination = tile; //set player's new destination tile

            destinationSelected = true;
        }
    }

    private void HandleAttackPhase(TileManager tile)
    {
        if(tile != currentPlayer.destination) //player can attack any space but destination
        {
            if (currentPlayer.attacks.Contains(tile)) //if player selects already chosen tile, deselct it as an attack
            {
                currentPlayer.attacks.Remove(tile);

                tile.transform.GetChild(1).gameObject.SetActive(false);

                confirmAttacksButton.interactable = false;
            }
            else //if not already chosen tile, select it as an attack
            {
                if (currentPlayer.attacks.Count < maxAttackNum) //player can only select new attack if they dont have max attacks selected
                {
                    currentPlayer.attacks.Add(tile);

                    tile.transform.GetChild(1).gameObject.SetActive(true);

                    if(currentPlayer.attacks.Count == maxAttackNum)
                    {
                        confirmAttacksButton.interactable = true;
                    }
                }
            }
        }
    }
}
