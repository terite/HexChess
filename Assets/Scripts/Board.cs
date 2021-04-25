using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System;

public class Board : SerializedMonoBehaviour
{
    [SerializeField] private PromotionDialogue promotionDialogue;
    [SerializeField] private LastMoveTracker moveTracker;
    [SerializeField] private TurnPanel turnPanel;
    [SerializeField] private Timers timers;
    [SerializeField] private SmoothHalfOrbitalCamera cam;
    public List<Jail> jails = new List<Jail>();
    [SerializeField] private GameObject hexPrefab;
    public Dictionary<(Team, Piece), GameObject> piecePrefabs = new Dictionary<(Team, Piece), GameObject>();
    public Game game;
    public List<BoardState> turnHistory = new List<BoardState>();
    [ReadOnly] public Dictionary<(Team, Piece), IPiece> activePieces = new Dictionary<(Team, Piece), IPiece>();
    public delegate void NewTurn(BoardState newState);
    [HideInInspector] public NewTurn newTurn;
    public delegate void GameOver(Game game);
    [HideInInspector] public GameOver gameOver;
    [SerializeField] public HexGrid hexGrid;
    [OdinSerialize] public List<List<Hex>> hexes = new List<List<Hex>>();
    List<Hex> highlightedHexes = new List<Hex>();
    [ReadOnly] public readonly string defaultBoardStateFileLoc = "DefaultBoardState";
    [ReadOnly] public List<Promotion> promotions = new List<Promotion>();
    public Color lastMoveHighlightColor;
    public float timeOffset {get; private set;} = 0f;

    // Used to write the default boardstate out to file
    [Button]
    public void WriteTurnHistoryToFile()
    {
        Game game = new Game(turnHistory, promotions);
        string json = game.Serialize();
        File.WriteAllText("Assets/Resources/" + defaultBoardStateFileLoc + ".json", json);
        Debug.Log($"Wrote to file: {defaultBoardStateFileLoc}");
    }
    // private void Awake() => SetBoardState(turnHistory[turnHistory.Count - 1]);

    private void Awake() => LoadGame(GetDefaultGame(defaultBoardStateFileLoc));
    private void Start() => newTurn.Invoke(turnHistory[turnHistory.Count - 1]);

    public void SetBoardState(BoardState newState, List<Promotion> promos = null)
    {
        BoardState defaultBoard = GetDefaultGame(defaultBoardStateFileLoc).turnHistory.FirstOrDefault();
        promotions = promos == null ? new List<Promotion>() : promos;
        foreach(KeyValuePair<(Team team, Piece piece), GameObject> prefabs in piecePrefabs)
        {
            IPiece piece;
            IPiece jailedPiece = jails[(int)prefabs.Key.team].GetPieceIfInJail(prefabs.Key.piece);

            if(activePieces.ContainsKey(prefabs.Key))
            {
                piece = activePieces[prefabs.Key];
                if(prefabs.Key.piece >= Piece.Pawn1 && !(piece is Pawn))
                {
                    IPiece old = activePieces[prefabs.Key];
                    activePieces.Remove(prefabs.Key);
                    Destroy(old.obj);

                    Index startLoc = defaultBoard.allPiecePositions[prefabs.Key];
                    Vector3 loc = hexes[startLoc.row][startLoc.col].transform.position + Vector3.up;
                    piece = Instantiate(prefabs.Value, loc, Quaternion.identity).GetComponent<IPiece>();
                    piece.Init(prefabs.Key.team, prefabs.Key.piece, defaultBoard.allPiecePositions[prefabs.Key]);
                    
                    activePieces.Add(prefabs.Key, piece);
                }
            }
            else if(jailedPiece != null)
                piece = jailedPiece;
            else
            {
                Index startLoc = defaultBoard.allPiecePositions[prefabs.Key];
                Vector3 loc = hexes[startLoc.row][startLoc.col].transform.position + Vector3.up;
                piece = Instantiate(prefabs.Value, loc, Quaternion.identity).GetComponent<IPiece>();
                piece.Init(prefabs.Key.team, prefabs.Key.piece, defaultBoard.allPiecePositions[prefabs.Key]);
                activePieces.Add(prefabs.Key, piece);
            }

            // It might need to be promoted. 
            // Do that before moving to avoid opening the promotiond dialogue when the pawn is moved to the promotion position
            piece = GetPromotedPieceIfNeeded(piece);
            
            // If the piece is on the board, place it at the correct location
            if(newState.allPiecePositions.ContainsKey(prefabs.Key))
            {
                Index loc = newState.allPiecePositions[prefabs.Key];                
                piece.MoveTo(hexes[loc.row][loc.col]);
                continue;
            }
            // Put the piece in the correct jail
            else
            {
                jails[(int)prefabs.Key.Item1].Enprison(piece);
                activePieces.Remove(prefabs.Key);
            }
        }

        newTurn?.Invoke(newState);
        if(newState.currentMove != Team.None && turnHistory.Count > 1)
            HighlightMove(BoardState.GetLastMove(turnHistory));
    }

    public void LoadGame(Game game)
    {
        turnHistory = game.turnHistory;
        this.game = game;
        
        Move move = BoardState.GetLastMove(turnHistory);
        if(move.lastTeam != Team.None)
            moveTracker.UpdateText(move);

        foreach(Jail jail in jails)
            jail?.Clear();

        if(turnHistory.Count > 1)
            timeOffset = turnHistory[turnHistory.Count - 1].executedAtTime - Time.timeSinceLevelLoad;
        
        if(game.timerDuration <= 0)
        {
            timers.gameObject.SetActive(game.hasClock);
            timers.isClock = game.hasClock;
        }
        else
        {
            timers.gameObject.SetActive(true);
            timers.SetTimers(game.timerDuration);
        }
    
        SetBoardState(turnHistory[turnHistory.Count - 1], game.promotions);

        // game.endType may not exist in older game saves, this bit of code supports both new and old save styles
        if(game.endType != GameEndType.Pending)
            gameOver?.Invoke(game);
        else
        {
            if(game.winner == Winner.Pending)
                turnPanel.Reset();
            else
                gameOver?.Invoke(game);
        }
    }

    public Game GetDefaultGame(string loc) => 
        Game.Deserialize(((TextAsset)Resources.Load(loc, typeof(TextAsset))).text);

    public Team GetCurrentTurn()
    {
        if(promotionDialogue.gameObject.activeSelf)
            return Team.None;

        return turnHistory[turnHistory.Count - 1].currentMove;
    }

    public BoardState GetCurrentBoardState() => turnHistory[turnHistory.Count - 1];

    public void AdvanceTurn(BoardState newState, bool updateTime = true)
    {
        List<IPiece> checkingPieces = GetCheckingPieces(newState, newState.currentMove);
        Multiplayer multiplayer = GameObject.FindObjectOfType<Multiplayer>();
        float timestamp = Time.timeSinceLevelLoad + timeOffset;
        
        if(updateTime)
            newState.executedAtTime = timestamp;

        newState.check = Team.None;
        newState.checkmate = Team.None;

        Team otherTeam = newState.currentMove == Team.White ? Team.Black : Team.White;
        
        if(checkingPieces.Count > 0)
        {
            List<(Hex, MoveType)> validMoves = new List<(Hex, MoveType)>();
            // Check for mate
            foreach(KeyValuePair<(Team, Piece), IPiece> kvp in activePieces)
            {
                (Team team, Piece piece) = kvp.Key;
                if(team == newState.currentMove)
                    continue;
                List<(Hex, MoveType)> vm = GetAllValidMovesForPiece(kvp.Value, newState);
                // Debug.Log($"{team}, {piece} has {vm.Count} valid moves.");
                validMoves.AddRange(vm);
            }
            if(validMoves.Count == 0)
                newState.checkmate = otherTeam;
            else
                newState.check = otherTeam;
        }

        // Handle potential checkmate
        if(newState.checkmate != Team.None)
        {
            if(multiplayer)
            {
                if(multiplayer.gameParams.localTeam == newState.checkmate)
                    multiplayer.SendGameEnd(timestamp, MessageType.Checkmate);
                else
                    return;
            }
            
            EndGame(
                timestamp, 
                endType: GameEndType.Checkmate, 
                winner: newState.checkmate == Team.White ? Winner.Black : Winner.White
            );
            return;
        }

        // Check for stalemate
        IEnumerable<KeyValuePair<(Team, Piece), IPiece>> otherTeamPieces = activePieces.Where(piece => piece.Key.Item1 == otherTeam);
        List<(Hex, MoveType)> validMovesForStalemateCheck = new List<(Hex, MoveType)>();
        foreach(KeyValuePair<(Team, Piece), IPiece> otherTeamPiece in otherTeamPieces)
        {
            List<(Hex, MoveType)> validMove = GetAllValidMovesForPiece(otherTeamPiece.Value, newState);
            validMovesForStalemateCheck.AddRange(validMove);
        }

        // Handle potential stalemate
        if(validMovesForStalemateCheck.Count() == 0)
        {
            if(multiplayer)
            {
                if(multiplayer.gameParams.localTeam == otherTeam)
                    multiplayer.SendGameEnd(timestamp, MessageType.Stalemate);
                else
                    return;
            }

            newState.currentMove = otherTeam;
            turnHistory.Add(newState);
            newTurn.Invoke(newState);
            HighlightMove(BoardState.GetLastMove(turnHistory));

            EndGame(timestamp, GameEndType.Stalemate, Winner.None);
            return;
        }

        newState.currentMove = otherTeam;
        turnHistory.Add(newState);
        newTurn.Invoke(newState);
        HighlightMove(BoardState.GetLastMove(turnHistory));
        
        if(multiplayer == null)
        {
            FlipCameraToggle flipCameraToggle = GameObject.FindObjectOfType<FlipCameraToggle>();
            if(flipCameraToggle != null && flipCameraToggle.toggle.isOn)
                cam.SetToTeam(newState.currentMove);
        }
    }

    public List<(Hex, MoveType)> GetAllValidMovesForPiece(IPiece piece, BoardState boardState, bool includeBlocking = false)
    {
        // Eliminate invalid moves
        // Simulate moves, eliminating any that leave the current player in check
        List<(Hex, MoveType)> possibleMoves = piece.GetAllPossibleMoves(this, boardState, includeBlocking);
        // Debug.Log($"{piece.team} {piece.type} has {possibleMoves.Count} possible moves.");
        for(int i = possibleMoves.Count - 1; i >= 0; i--)
        {
            (Hex possibleHex, MoveType possibleMoveType) = possibleMoves[i];
            if (possibleHex == null)
            {
                possibleMoves.RemoveAt(i);
                continue;
            }

            BoardState newState = default;
            if(possibleMoveType == MoveType.Move || possibleMoveType == MoveType.Attack)
                newState = MovePiece(piece, possibleHex, boardState, true, includeBlocking);
            else if(possibleMoveType == MoveType.Defend)
                newState = Swap(piece, activePieces[boardState.allPiecePositions[possibleHex.index]], boardState, true);
            else if(possibleMoveType == MoveType.EnPassant)
            {
                int teamOffset = boardState.currentMove == Team.White ? -2 : 2;
                Index enemyLoc = new Index(possibleHex.index.row + teamOffset, possibleHex.index.col);
                (Team enemyTeam, Piece enemyPiece) = boardState.allPiecePositions[enemyLoc];
                newState = EnPassant((Pawn)piece, enemyTeam, enemyPiece, possibleHex, boardState, true);
            }

            Team otherTeam = piece.team == Team.White ? Team.Black : Team.White;
            // If any piece is checking, the move is invalid, remove it from the list of possible moves
            List<IPiece> checkingPieces = GetCheckingPieces(newState, otherTeam);
            if(checkingPieces.Count > 0)
                possibleMoves.RemoveAt(i);
        }
        return possibleMoves;
    }

    public IEnumerable<IPiece> GetThreateningPieces(Hex hoveredHex)
    {
        BoardState currentState = GetCurrentBoardState();
        List<IPiece> threateningPieces = new List<IPiece>();
        foreach(KeyValuePair<(Team, Piece), IPiece> piece in activePieces)
        {
            List<(Hex, MoveType)> possibleMoves = GetAllValidMovesForPiece(piece.Value, currentState, false);
            if(possibleMoves.Where(move => move.Item1 == hoveredHex && move.Item2 == MoveType.Attack).Count() == 1)
                threateningPieces.Add(piece.Value);
        }
        return threateningPieces;
    }

    public BoardState MovePiece(IPiece piece, Hex targetLocation, BoardState boardState, bool isQuery = false, bool includeBlocking = false)
    {
        // Copy the existing board state
        BoardState currentState = boardState;
        BidirectionalDictionary<(Team, Piece), Index> allPiecePositions = new BidirectionalDictionary<(Team, Piece), Index>(boardState.allPiecePositions);
        
        // If the hex being moved into contains an enemy piece, capture it
        Piece? takenPieceAtLocation = null;
        Piece? defendedPieceAtLocation = null;
        if(currentState.allPiecePositions.Contains(targetLocation.index))
        {
            (Team occupyingTeam, Piece occupyingType) = currentState.allPiecePositions[targetLocation.index];
            if(occupyingTeam != piece.team || includeBlocking)
            {
                takenPieceAtLocation = occupyingType;
                IPiece occupyingPiece = activePieces[(occupyingTeam, occupyingType)];

                // Capture the enemy piece
                if(!isQuery)
                {
                    jails[(int)occupyingTeam].Enprison(occupyingPiece);
                    activePieces.Remove((occupyingTeam, occupyingType));
                }
                allPiecePositions.Remove((occupyingTeam, occupyingType));
            }
            else
                defendedPieceAtLocation = occupyingType;
        }

        // Move piece
        if(!isQuery)
        {
            moveTracker.UpdateText(new Move(
                piece.team, 
                piece.piece, 
                piece.location, 
                targetLocation.index, 
                takenPieceAtLocation, 
                defendedPieceAtLocation
            ));
            piece.MoveTo(targetLocation);
        }

        // Update boardstate
        if(allPiecePositions.ContainsKey((piece.team, piece.piece)))
            allPiecePositions.Remove((piece.team, piece.piece));
        allPiecePositions.Add((piece.team, piece.piece), targetLocation.index);
        currentState.allPiecePositions = allPiecePositions;
        
        return currentState;
    }

    public void QueryPromote(Pawn pawn) 
    {
        // We don't want to display the query promote screen if we're not the team making the promote
        // That information will arrive to us across the network
        Multiplayer multiplayer = GameObject.FindObjectOfType<Multiplayer>();
        if(multiplayer != null && multiplayer.localTeam != GetCurrentTurn())
            return;
        promotionDialogue.Display(pieceType => {
            Promote(pawn, pieceType);

            Multiplayer multiplayer = GameObject.FindObjectOfType<Multiplayer>();
            multiplayer?.SendPromote(new Promotion(pawn.team, pawn.piece, pieceType));
        });
    } 

    public IPiece Promote(Pawn pawn, Piece type)
    {
        // Replace the pawn with the chosen piece type
        // Worth noting: Even though the new IPiece is of a different type than Pawn, 
        // we still use the PieceType.Pawn# (read from the pawn) to store it's position in the game state to maintain it's unique key
        IPiece newPiece = Instantiate(piecePrefabs[(pawn.team, type)], pawn.transform.position, Quaternion.identity).GetComponent<IPiece>();
        newPiece.Init(pawn.team, pawn.piece, pawn.location);
        Promotion newPromo = new Promotion(pawn.team, pawn.piece, type);
        promotions.Add(newPromo);
        activePieces[(pawn.team, pawn.piece)] = newPiece;
        Destroy(pawn.gameObject);
        return newPiece;
    }

    private IPiece GetPromotedPieceIfNeeded(IPiece piece)
    {
        if(piece is Pawn pawn)
        {
            Piece p = pawn.piece;
            foreach(Promotion promo in promotions)
                if(promo.team == pawn.team && promo.from == p)
                    p = promo.to;
            if(p != pawn.piece)
                piece = Promote(pawn, p);
        }

        return piece;
    }

    public BoardState Swap(IPiece p1, IPiece p2, BoardState boardState, bool isQuery = false)
    {
        if(p1 == p2)
            return boardState;

        Index p1StartLoc = p1.location;
        Index p2StartLoc = p2.location;
        BoardState currentState = boardState;
        
        if(!isQuery)
        {
            moveTracker.UpdateText(new Move(
                p1.team, 
                p1.piece, 
                p1StartLoc, 
                p2StartLoc, 
                null, 
                p2.piece
            ));
            p1.MoveTo(GetHexIfInBounds(p2.location));
            p2.MoveTo(GetHexIfInBounds(p1StartLoc));
        }

        BidirectionalDictionary<(Team, Piece), Index> allPiecePositions = new BidirectionalDictionary<(Team, Piece), Index>(currentState.allPiecePositions);
        allPiecePositions.Remove((p1.team, p1.piece));
        allPiecePositions.Remove((p2.team, p2.piece));
        allPiecePositions.Add((p1.team, p1.piece), p2StartLoc);
        allPiecePositions.Add((p2.team, p2.piece), p1StartLoc);
        
        currentState.allPiecePositions = allPiecePositions;

        return currentState;
    }

    public BoardState EnPassant(Pawn pawn, Team enemyTeam, Piece enemyPiece, Hex targetHex, BoardState boardState, bool isQuery = false)
    {
        BoardState currentState = boardState;
        IPiece enemyIPiece = activePieces[(enemyTeam, enemyPiece)];
        BidirectionalDictionary<(Team, Piece), Index> allPiecePositions = new BidirectionalDictionary<(Team, Piece), Index>(currentState.allPiecePositions);
        
        allPiecePositions.Remove((enemyTeam, enemyPiece));
        
        if(!isQuery)
        {
            // Capture enemy
            jails[(int)enemyTeam].Enprison(enemyIPiece);
            // Move pawn
            moveTracker.UpdateText(new Move(
                pawn.team, 
                pawn.piece, 
                pawn.location, 
                targetHex.index, 
                enemyPiece,
                null
            ));
            pawn.MoveTo(targetHex);
        }
        
        // Update board state
        allPiecePositions.Remove((pawn.team, pawn.piece));
        allPiecePositions.Add((pawn.team, pawn.piece), targetHex.index);
        
        currentState.allPiecePositions = allPiecePositions;
        return currentState;
    }

    public void Enprison(IPiece toPrison)
    {
        jails[(int)toPrison.team].Enprison(toPrison);
        BoardState currentState = GetCurrentBoardState();
        BidirectionalDictionary<(Team, Piece), Index> allPiecePositions = new BidirectionalDictionary<(Team, Piece), Index>(currentState.allPiecePositions);
        allPiecePositions.Remove((toPrison.team, toPrison.piece));
        currentState.allPiecePositions = allPiecePositions;
        activePieces.Remove((toPrison.team, toPrison.piece));

        AdvanceTurn(currentState);
    }

    public List<IPiece> GetCheckingPieces(BoardState boardState, Team checkForTeam)
    {
        List<IPiece> checkingPieces = new List<IPiece>();

        foreach(KeyValuePair<(Team, Piece), IPiece> kvp in activePieces)
        {
            (Team team, Piece piece) = kvp.Key;
            // If the IPiece doesn't exist in the boardstate, it might be a simulated boardstate, skip that piece
            if(team != checkForTeam || !boardState.allPiecePositions.ContainsKey((team, piece)))
                continue;

            List<(Hex, MoveType)> moves = kvp.Value.GetAllPossibleMoves(this, boardState);
            foreach((Hex hex, MoveType moveType) in moves)
            {
                if(moveType != MoveType.Attack)
                    continue;
                
                if(boardState.allPiecePositions.ContainsKey(hex.index))
                {
                    (Team occupyingTeam, Piece occupyingPiece) = boardState.allPiecePositions[hex.index];
                    // Check
                    if(occupyingTeam != checkForTeam && occupyingPiece == Piece.King)
                        checkingPieces.Add(kvp.Value);
                }
            }
        }
        return checkingPieces;
    }

    public void EndGame(float timestamp, GameEndType endType = GameEndType.Pending, Winner winner = Winner.Pending)
    {
        BoardState currentState = GetCurrentBoardState();
        if(currentState.currentMove == Team.None)
            return;

        currentState.currentMove = Team.None;
        currentState.executedAtTime = timestamp;
        turnHistory.Add(currentState);
        newTurn.Invoke(currentState);

        game = new Game(
            turnHistory, 
            promotions, 
            winner, 
            endType, 
            timers.timerDruation, 
            timers.isClock
        );

        gameOver.Invoke(game);
    }

    public void HighlightMove(Move move)
    {
        foreach(Hex hex in highlightedHexes)
            hex.Unhighlight();
        highlightedHexes.Clear();
        
        Hex fromHex = GetHexIfInBounds(move.from);
        Hex toHex = GetHexIfInBounds(move.to);

        fromHex.Highlight(lastMoveHighlightColor);
        toHex.Highlight(move.capturedPiece.HasValue 
            ? Color.red 
            : move.defendedPiece.HasValue 
                ? Color.green 
                : lastMoveHighlightColor
        );

        highlightedHexes.Add(fromHex);
        highlightedHexes.Add(toHex);
    }

    public void Reset() 
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SceneTransition sceneTransition = GameObject.FindObjectOfType<SceneTransition>();
        if(sceneTransition != null)
            sceneTransition.Transition(sceneName);
        else
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void MaybeNewHex()
    {
        #if UNITY_EDITOR
        Hex[] selectedHexes = Selection.GetFiltered<Hex>(SelectionMode.Unfiltered);
        Debug.Log(selectedHexes.Length);
        #endif
    }

    [Button("Spawn Hexes")]
    private void SpawnHexes()
    {
        if(hexes.Count > 0)
            ClearHexes();
        
        for(int row = 0; row < hexGrid.rows; row++) 
        {
            hexes.Add(new List<Hex>());
            for(int col = 0; col < hexGrid.cols; col++)
            {
                if(hexGrid.cols % 2 != 0 && col == hexGrid.cols - 1 && row % 2 == 0)
                    continue;

                GameObject newGo = Instantiate(
                    original: hexPrefab,
                    position: new Vector3(
                        x: hexGrid.radius * 3 * col + Get_X_Offset(row),
                        y: UnityEngine.Random.Range(hexGrid.minHeight, hexGrid.maxHeight),
                        z: row * hexGrid.Apothem
                    ),
                    rotation: Quaternion.identity,
                    parent: transform
                );

                Hex newHex = newGo.GetComponent<Hex>();

                newHex.transform.localScale = new Vector3(
                    x: newHex.transform.localScale.x * hexGrid.radius,
                    y: newHex.transform.localScale.y * hexGrid.height,
                    z: newHex.transform.localScale.z * hexGrid.radius
                );

                newHex.AssignIndex(new Index(row, col), this);

                hexes[row].Add(newHex);
                newHex.SetColor(GetColor(row));
            }
        }
    }

    public Color GetColor(int row) => row % 2 == 0  
        ? hexGrid.colors[(Mathf.FloorToInt(row/2) + 1) % 3]
        : hexGrid.colors[Mathf.FloorToInt(row/2) % 3];

    private float Get_X_Offset(int row) => row % 2 == 0 ? hexGrid.radius * 1.5f : 0f;

    [Button("Clear Hexes")]
    private void ClearHexes()
    {
        for(int row = 0; row < hexes.Count; row++)
        {
            for(int col = 0; col < hexes[row].Count; col++)
            {
#if UNITY_EDITOR
                DestroyImmediate(hexes[row][col].gameObject);
#elif !UNITY_EDITOR
                Destroy(hexes[row][col].gameObject);
#endif                
            }
        }
        hexes = new List<List<Hex>>();
    }

    public Hex GetNeighborAt(Index source, HexNeighborDirection direction)
    {
        (int row, int col) offsets = GetOffsetInDirection(source.row % 2 == 0, direction);
        return GetHexIfInBounds(source.row + offsets.row, source.col + offsets.col);
    }

    public Hex GetHexIfInBounds(int row, int col)
    {
        if(hexGrid.cols % 2 != 0 && col == hexGrid.cols - 1 && row % 2 == 0)
            return null;
        return hexGrid.IsInBounds(row, col) ? hexes[row][col] : null;
    }
    public Hex GetHexIfInBounds(Index index) => GetHexIfInBounds(index.row, index.col);

    private (int row, int col) GetOffsetInDirection(bool isEven, HexNeighborDirection direction) => direction switch {
        HexNeighborDirection.Up => (2, 0),
        HexNeighborDirection.UpRight => isEven ? (1, 1) : (1, 0),
        HexNeighborDirection.DownRight => isEven ? (-1, 1) : (-1, 0),
        HexNeighborDirection.Down => (-2, 0),
        HexNeighborDirection.DownLeft => isEven ? (-1, 0) : (-1, -1),
        HexNeighborDirection.UpLeft => isEven ? (1, 0) : (1, -1),
        _ => (0, 0)
    };
}

public enum HexNeighborDirection{Up, UpRight, DownRight, Down, DownLeft, UpLeft};