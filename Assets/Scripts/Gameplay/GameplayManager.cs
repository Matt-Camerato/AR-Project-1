using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class GameplayManager : MonoBehaviour
{
    [Header("Game Info")]
    public int roundNum = 1;
    [SerializeField] private int turnNum = 1;
    public int maxAttackNum;

    static public int numPlayers = 4;

    [Header("Gameplay Settings")]
    [SerializeField] private GameObject playerPiecePrefab;
    [SerializeField] private GameObject damageParticlesPrefab;
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

    [HideInInspector] public Player[] players;

    [HideInInspector] public Player currentPlayer;

    private bool tapGestureEnabled = false;

    //Recap variables
    private RecapInfo currentRecapInfo;
    private bool roundRecapFinished = true; //set to true for first round
    private float recapDelay = 2;
    private float damageDelay = 5;

    private bool recapMoveInitialized = false;
    private List<Player> doneMoving = new List<Player>();
    private bool recapMoveDone = false;

    private bool recapAttacksInitialized = false;
    private List<Player> doneAttacking = new List<Player>();
    private List<GameObject> spawnedCubes = new List<GameObject>();
    private bool recapAttacksDone = false;

    private bool recapDamageInitialized = false;
    private bool recapDamageDisplayed = false;
    private List<Player> highestPlayer = new List<Player>();
    private int highestHeight;
    private bool playerJustDied = false;
    private bool lastRound = false; //only true when last player has died, meaning there is no reason for a move + attack phase for anyone
    
    //Move Phase variables
    private bool destinationInitialized = false;
    [HideInInspector] public List<Vector2> currentPossibleMoves;
    private bool destinationSelected = false;
    [HideInInspector] public bool destinationDetermined = false;

    //Attack Phase variables
    private bool attacksInitialized = false;
    [HideInInspector] public bool attacksDetermined = false;

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
            if (HUD_Canvas.GetComponent<HUDManager>().currentPlayerReady) //show ready up screen (doesn't happen on first turn of game)
            {
                if (!roundRecapFinished) //once player is ready, play round recap (1st round there isn't one)
                {
                    //---RECAP PHASE---

                    if (!recapMoveDone)
                    {
                        if(recapDelay > 0)
                        {
                            recapDelay -= Time.deltaTime;
                        }
                        else
                        {
                            HandleMoveRecap(); //this will start happening 3 seconds after recap phase begins
                        }
                    }
                    else if (!recapAttacksDone)
                    {
                        if (recapDelay > 0)
                        {
                            recapDelay -= Time.deltaTime;
                        }
                        else
                        {
                            HandleAttacksRecap(); //this will start happening 1 second after move recap ends
                        }
                    }
                    else
                    {
                        if (recapDelay > 0)
                        {
                            recapDelay -= Time.deltaTime;
                        }
                        else
                        {
                            HandleDamageRecap(); //this will start happening 3 seconds after attacks recap ends
                        }   
                    }
                }
                else
                {
                    if (!destinationDetermined)
                    {
                        //---MOVE PHASE---

                        //initialize possible moves for current player with tile indicators
                        if (!destinationInitialized)
                        {
                            //before start of move phase, if player moved to their desition in recap, update position and destination variables
                            if(currentPlayer.destination != null)
                            {
                                currentPlayer.position = new Vector2Int(currentPlayer.destination.x, currentPlayer.destination.y);
                                currentPlayer.destination = null;
                            }

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
                            currentPlayer.attacks.Clear(); //before start of attack phase, clear attacks list

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
                            confirmAttacksButton.interactable = false;

                            attacksInitialized = true;
                        }
                    }
                    else //turn is over (initialize scene for next player's turn)
                    {
                        tapGestureEnabled = false; //turn off gestures after player has confirmed attacks

                        UpdateTurnNum(); //set new turn num (and round num if end of round)

                        currentPlayer = players[turnNum - 1]; //update current player with new turn #

                        ResetRecapBools(); //reset bools used during recap

                        destinationInitialized = false;
                        destinationSelected = false;
                        destinationDetermined = false;

                        attacksInitialized = false;
                        attacksDetermined = false;

                        HUD_Canvas.GetComponent<HUDManager>().currentPlayerReady = false; //this will show ready up screen for next player
                    }
                }
            }
            else if(currentPlayer.health == 0 || (currentRecapInfo != null && currentRecapInfo.players[currentPlayer.playerNum - 1].health == 0 && playerJustDied) || lastRound) //if player is dead (or just died in recap), or if its the last round, skip the entire turn process and go to next player
            {
                UpdateTurnNum(); //set new turn num (and round num if end of round)

                currentPlayer = players[turnNum - 1]; //update current player with new turn #

                ResetRecapBools(); //reset bools used during recap

                playerJustDied = false; //make sure this is reset in case it is the reason this turn was skipped
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
            Vector2Int playerCoords = PosFromPlayerNum(i);
            players[i].position = playerCoords;

            Transform parentTile = GetComponent<GameboardManager>().tiles[(int)playerCoords.x, (int)playerCoords.y].transform;
            GameObject spawnedPiece = Instantiate(playerPiecePrefab, Vector3.zero, Quaternion.identity, parentTile);
            spawnedPiece.transform.localPosition = new Vector3(0, -0.02f, 0);
            spawnedPiece.GetComponent<MeshRenderer>().material.color = players[i].color; //set game piece color
            spawnedPiece.transform.GetChild(0).GetComponent<ParticleSystem>().startColor = players[i].color; //set game piece particles color
            players[i].gamePiece = spawnedPiece;


            //OLD CODE - Parents all players to playerParent object
            /*
            Vector3 playerPos = GetComponent<GameboardManager>().tiles[(int)playerCoords.x, (int)playerCoords.y].transform.position;
            playerPos.y = 0; //pieces rise up from tiles when game is initialized

            players[i].gamePiece = Instantiate(playerPiecePrefab, playerPos, Quaternion.identity, transform.GetChild(2));
            players[i].gamePiece.transform.localPosition = new Vector3(players[i].gamePiece.transform.localPosition.x, 0, players[i].gamePiece.transform.localPosition.z);
            players[i].gamePiece.GetComponent<MeshRenderer>().material.color = players[i].color; //set game piece color using playerColors[randomColorIndex];
            players[i].gamePiece.transform.GetChild(0).GetComponent<ParticleSystem>().startColor = players[i].color; //set game piece particles color
            */
        }
    }
    
    private Vector2Int PosFromPlayerNum(int playerNum)
    {
        switch (playerNum)
        {
            case 0:
                return new Vector2Int(0, 0);
            case 1:
                return new Vector2Int(4, 4);
            case 2:
                return new Vector2Int(4, 0);
            case 3:
                return new Vector2Int(0, 4);
            case 4:
                return new Vector2Int(0, 2);
            case 5:
                return new Vector2Int(4, 2);
            case 6:
                return new Vector2Int(2, 0);
            case 7:
                return new Vector2Int(2, 4);
            default:
                Debug.Log("Not a possible player num");
                return Vector2Int.zero;
        }
    }

    private void HandleMoveRecap()
    {
        if (!recapMoveInitialized) //initialize each player with their start y value and reset doneMoving bool
        {
            List<TileManager> destinations = new List<TileManager>(); //set up list to check if players have same destination
            List<TileManager> duplicateDestinations = new List<TileManager>(); //set up list to keep track of duplicates

            foreach (Player p in currentRecapInfo.players)
            {
                if (players[p.playerNum - 1].health > 0 && p.gamePiece != null) //make sure (ACTUAL) player isn't dead
                {
                    p.gamePiece.GetComponent<PlayerPieceManager>().doneMoving = false;
                }

                if (destinations.Contains(p.destination)) { duplicateDestinations.Add(p.destination); } //if duplicate destination, add it to the list of duplicates
                else { destinations.Add(p.destination); } //if not duplicate destination, add it to the list of destinations
            }

            foreach(TileManager tile in duplicateDestinations) { FixDestinations(tile); } //adjust destinations based on duplicates

            doneMoving.Clear(); //reset list for how many players have finished moving

            //if 2 (or more) players try to move to a tile, the player(s) who doesn't get to move remains on their tile
            //if someone else was planning on moving to this tile they remained on, they will also have to remain on their tile (hence why this process repeats until no duplicate destinations exist)
            if (duplicateDestinations.Count == 0) { recapMoveInitialized = true; } //entire initialization process repeats until game is sure that no player is moving to already occupied tile
        }
        else
        {
            //handle moving of player pieces
            foreach (Player p in currentRecapInfo.players)
            {
                if (players[p.playerNum - 1].health > 0 && p.gamePiece != null && !p.gamePiece.GetComponent<PlayerPieceManager>().doneMoving) //make sure (ACTUAL) player isn't dead and not done moving
                {
                    p.gamePiece.GetComponent<PlayerPieceManager>().MovePlayerPiece(p.destination);
                }
                else
                {
                    if (!doneMoving.Contains(p)) { doneMoving.Add(p); } //add player to done moving list if not already on it and done moving (or dead)
                }
            }

            if (doneMoving.Count == numPlayers) 
            {
                recapMoveDone = true; //once all players are on list, allow attack recap to begin
                recapDelay = 1; //set 1 second of delay before attack recap starts
            }
        }
    }

    private void FixDestinations(TileManager tile)
    {
        List<Player> sameDestination = new List<Player>(); //setup list for players with same destination
        int priorityPlayerNum = 0; //setup int for priority player that will get to keep destination (no players have 0 as playernum, so this is default value)

        //first, loop through all players and add anyone with this destination to list
        foreach (Player p in currentRecapInfo.players)
        {
            if(p.destination == tile) { sameDestination.Add(p); }
        }

        //next, loop through this list of players to see if anyone has this destination set as their position already (meaning they already went through this process and had to stay on their original tile)
        Vector2Int tilePos = new Vector2Int(tile.x, tile.y); //Vector2Int position of tile (used for test below)
        foreach(Player p in sameDestination)
        {
            if(p.position == tilePos) { priorityPlayerNum = p.playerNum; }
        }
        
        //lastly, if priority player wasn't set by previous check, determine this randomly
        if(priorityPlayerNum == 0)
        {
            priorityPlayerNum = sameDestination[Random.Range(0, sameDestination.Count)].playerNum;
        }

        Player priorityPlayer = currentRecapInfo.players[priorityPlayerNum - 1]; //setup variable for priority player

        //now that priority player has been determined, set destinations of all other players with same destination to their position (meaning they won't move anywhere)
        foreach(Player p in sameDestination)
        {
            if(p != priorityPlayer)
            {
                TileManager positionTile = GetComponent<GameboardManager>().tiles[p.position.x, p.position.y].GetComponent<TileManager>();
                p.destination = positionTile;
                players[p.playerNum - 1].destination = positionTile; //update actual player as well so correct possible moves are shown on their turn
            }
        }
    }

    //used below in HandleAttacksRecap();
    private Player currentAttacker; 
    private TileManager[] currentAttacks;

    private void HandleAttacksRecap()
    {
        if (!recapAttacksInitialized)
        {
            if (doneAttacking.Count > 0) { doneAttacking.Clear(); } //clear list of players done attacking

            foreach(Player p in currentRecapInfo.players)
            {
                if(players[p.playerNum - 1].health == 0) { doneAttacking.Add(p); } //add (ACTUALLY) dead players to done attacking list so they may be skipped

                //also reset attack recap variables for each player
                p.spawnedAttacks = false;
                p.attacksDoneMoving = false;
            }

            currentAttacker = currentRecapInfo.players[0]; //set first player as current attacker

            recapAttacksInitialized = true;
        }
        else
        {
            if (doneAttacking.Contains(currentAttacker)) //if current attacker is done attacking, move to next player or end attack recap
            {
                if(currentAttacker.playerNum == numPlayers)
                {
                    recapAttacksDone = true; //move on to damage recap
                    recapDelay = 2; //set 2 seconds of delay before damage recap begins
                }
                else
                {
                    //If more players left, advance to next player's attacks

                    currentAttacker = currentRecapInfo.players[currentAttacker.playerNum]; //sets the next player as current attacker (this works because playerNum starts at 1 and index number starts at 0)
                } 
            }
            else //if not done attacking, handle displaying of attacks
            {
                if (!currentAttacker.spawnedAttacks) //spawn attack cubes if not done already
                {
                    currentAttacks = GetComponent<SavedRecapInfo>().GetCurrentAttacks(currentAttacker.playerNum - 1);

                    for(int i = 0; i < currentAttacks.Length; i++)
                    {
                        TileManager currentAttackTile = currentAttacks[i];
                        GameObject spawnedCube = GetComponent<GameboardManager>().SpawnAttackCube(currentAttackTile, currentAttacker); //spawn attack cubes based one player and their selected attacks
                        spawnedCubes.Add(spawnedCube); //add cube to list so they can be deleted at end of current players turn (if not end of round)
                        currentAttackTile.currentCubeCount += 1; //increase cube count after spawning new cube
                    }

                    currentAttacker.spawnedAttacks = true; //set bool to true after player has successfully spawned all attacks
                }
                else if(!currentAttacker.attacksDoneMoving) //once attacks are spawned, wait until they rise before moving to next attacker (or ending attack recap)
                {
                    foreach (TileManager tile in currentAttacks)
                    {
                        tile.transform.localPosition += Vector3.up * Time.deltaTime * 0.07f;
                        if(tile.transform.localPosition.y >= 0.035f + (0.09f * tile.currentCubeCount))
                        {
                            currentAttacker.attacksDoneMoving = true;
                        }
                    }
                }
                else
                {
                    //before moving to next player, make sure cubes are in right position
                    foreach(TileManager tile in currentAttacks)
                    {
                        tile.transform.localPosition = new Vector3(tile.transform.localPosition.x, 0.035f + (0.09f * tile.currentCubeCount), tile.transform.localPosition.z);
                    }

                    doneAttacking.Add(currentAttacker); //add current attacker to list of players done attacking
                }
            }
        }
    }

    private void HandleDamageRecap()
    {
        //determine which player is highest and damage them
        if (!recapDamageInitialized)
        {
            //initialize damage phase on first player's turn so information can be used for any other players

            //make sure highest player list and highest height is refreshed before calculation begins
            highestPlayer = new List<Player>();
            highestHeight = 0;

            //calculation to determine current highest player
            for (int i = 0; i < currentRecapInfo.players.Count; i++)
            {
                if (currentRecapInfo.players[i].health > 0) //make sure player isn't dead before calculating height (this would've happened last round)
                {
                    int x = currentRecapInfo.players[i].destination.x;
                    int y = currentRecapInfo.players[i].destination.y;
                    int height = GetComponent<GameboardManager>().tiles[x, y].GetComponent<TileManager>().currentCubeCount;

                    if (highestPlayer.Count == 0) //if no highest player yet, add this player
                    {
                        highestPlayer.Add(currentRecapInfo.players[i]);
                        highestHeight = height;
                    }
                    else if (highestHeight < height) //if higher than previous highest player (or players), clear list and add player
                    {
                        highestPlayer.Clear();
                        highestPlayer.Add(currentRecapInfo.players[i]);
                        highestHeight = height;
                    }
                    else if (highestHeight == height) //if the same height as previous highest player (or players), just add player to list
                    {
                        highestPlayer.Add(currentRecapInfo.players[i]);
                    }
                }
                //if none of these, player is lower than previous highest player and nothing happens
            }

            //based on calculation, determine if multiple players will be damaged, or no one at all

            //in this version, all highest players will get damaged unless every (alive) player is tied (it only takes 1 lower person for all other players to be damaged)
            int numPlayersAlive = 0;
            foreach (Player p in currentRecapInfo.players) { if (p.health > 0) { numPlayersAlive++; } }
            int maxPlayersDamaged = numPlayersAlive - 1;

            if (highestPlayer.Count <= maxPlayersDamaged)
            {
                foreach (Player p in highestPlayer)
                {
                    p.health--; //update each highest player's health

                    if (p.health == 0) { numPlayersAlive--; } //this will be used to determine if this is the last round or not
                }
            }
            else
            {
                highestPlayer.Clear(); //if no one gets damaged (all players are equal height in this case), remove them from list so damage isn't displayed
            }

            if (numPlayersAlive == 1) { lastRound = true; } //if only 1 person alive after damage is dealt, this is the last round

            recapDamageInitialized = true;

            //POSSIBLE FEATURE: whoever has a cube in damaged player's tower gets a life back, or instead whoever placed a tile there that round (possibly just last person to place tile so everyone doesn't heal)

        }
        else //once damage has been initialized and dealt (on first recap of round), display damage animations accordingly during all players' recaps, then continue with turn accordingly
        {
            if (!recapDamageDisplayed) //displaying damage is handled in a single frame, so this is only called once
            {
                foreach (Player p in highestPlayer)
                {
                    Instantiate(damageParticlesPrefab, p.gamePiece.transform.position, Quaternion.identity); //display damage particles whether player died or not

                    if (currentPlayer == players[p.playerNum - 1]) //if player taking turn is one damaged, update heart icons accordingly
                    {
                        HUD_Canvas.GetComponent<HUDManager>().UpdateHealthIcons(p.health);
                        HUD_Canvas.GetComponent<HUDManager>().damagePanel.SetActive(true); //also turn on damage panel for effect (it automatically turns itself off after fade)
                    }

                    if (p.health == 0) //if player has died, deal with in game animation played to everyone, as well as display HUD animation if on dead player's turn
                    {
                        p.gamePiece.SetActive(false); //turn off playing piece (will be deleted at end of round, but must be turned on again for other player's recaps)

                        if (currentPlayer == players[p.playerNum - 1]) //if player taking turn is one who died, display you lose animation
                        {
                            playerJustDied = true;
                            HUD_Canvas.GetComponent<HUDManager>().anim.SetTrigger("YouLose");
                        }
                    }
                }

                damageDelay = 2; //set up 2 seconds of delay after damage is shown and before the next phase (or turn) starts

                recapDamageDisplayed = true;
            }
            else if (!playerJustDied) //if player just died, they will skip their turn after hitting the okay button on "you lose" screen
            {
                if(damageDelay > 0)
                {
                    damageDelay -= Time.deltaTime; //wait for 5 seconds before ending recap phase
                }
                else
                {
                    recapDelay = 2; //setup recap delay to be used for next player's recap phase

                    if (!lastRound) //end recap phase if not last round, else skip player's turn since move + attack phases are useless during last round
                    {
                        roundRecapFinished = true;
                        HUD_Canvas.GetComponent<HUDManager>().anim.SetTrigger("Move");
                    }
                    else
                    {
                        HUD_Canvas.GetComponent<HUDManager>().currentPlayerReady = false; //skip rest of player's turn if last round
                    }
                }  
            }
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

    private void UpdateTurnNum()
    {
        if (turnNum == numPlayers)
        {
            roundNum++;
            turnNum = 1;

            if (roundNum > 2) //this only needs to be done at the end of the second round (start of 3rd round) after recaps have been played
            {
                foreach (Player p in currentRecapInfo.players)
                {
                    players[p.playerNum - 1].health = p.health; //update all player's ACTUAL health at end of round
                    if (p.health == 0)
                    {
                        Destroy(players[p.playerNum - 1].gamePiece); //if player died during this round, delete their playing piece before start of next round
                    }
                }
            }

            //FOR SOME REASON, SAVING RECAP INFO FOR ATTACKS ISN'T WORKING THROUGH CURRENTRECAPINFO, SO HOTFIXING IT WITH SAVEDRECAPINFO COMPONENT
            GetComponent<SavedRecapInfo>().SaveAttacks(players); //this saves all chosen attacks at end of round in another script that will be used for next rounds' recaps
            
            //If end of round, set up recap info so that next round players can overwrite their destination and attacks without losing last rounds info
            currentRecapInfo = new RecapInfo(); //set up new info script
            for (int i = 0; i < players.Length; i++)
            {
                currentRecapInfo.SavePlayerInfo(players[i]); //save end of round state for each player before they begin selecting new destinations and attacks
            }

            currentRecapInfo.SaveTileInfo(GetComponent<GameboardManager>().tiles); //save gameboard state so that it can be be reset for recap phase
            spawnedCubes.Clear(); //clear list of spawned cubes so that they aren't deleted before next person's turn (making them permanent)

            recapDamageInitialized = false; //reset damage recap initialization at end of round so it can be done again on first alive player's turn next round
        }
        else { turnNum++; }
    }

    private void ResetRecapBools()
    {
        if (roundNum > 1) { roundRecapFinished = false; } //turn on recap for each player's turn after round 1
        recapMoveInitialized = false;
        recapMoveDone = false;
        recapAttacksInitialized = false;
        recapAttacksDone = false;
        recapDamageDisplayed = false;
    }

    public void ResetGameboard() //this is only called before an alive player's turn begins. During end of turn fade, game skips through dead players to first available alive player
    {
        foreach(Player p in players)
        {
            if(p.gamePiece != null && !p.gamePiece.activeSelf) //first turn on any playing pieces that may have been turned off in last players recap (dead players won't have a playing piece)
            { 
                p.gamePiece.SetActive(true); 
            }

            //next reset all player pieces to original positions (based on currentRecapInfo.players[?].position)
            TileManager pos = GetComponent<GameboardManager>().tiles[currentRecapInfo.players[p.playerNum - 1].position.x, currentRecapInfo.players[p.playerNum - 1].position.y].GetComponent<TileManager>();
            p.gamePiece.GetComponent<PlayerPieceManager>().ResetPosition(pos);
        }

        foreach(GameObject cube in spawnedCubes) { Destroy(cube); } //delete all previously spawned cubes (list will be empty if start of new round)

        //loop through all tiles and reset them based on saved tile managers (in currentRecapInfo)
        var allTiles = GetComponent<GameboardManager>().tiles;
        for (int x = 0; x < allTiles.GetLength(0); x++)
        {
            for (int y = 0; y < allTiles.GetLength(1); y++)
            {
                int savedCubeCount = currentRecapInfo.cubeCounts[x, y];
                allTiles[x, y].GetComponent<TileManager>().currentCubeCount = savedCubeCount; //first reset current cube count to saved cube count
                allTiles[x, y].transform.localPosition = new Vector3(allTiles[x, y].transform.localPosition.x, 0.035f + (0.09f * savedCubeCount), allTiles[x, y].transform.localPosition.z); //then reset position of tile based on saved cube count
            }
        }
    }
}
