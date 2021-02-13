using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Board : SerializedMonoBehaviour
{
    [SerializeField] private PromotionDialogue promotionDialogue;
    public List<Jail> jails = new List<Jail>();
    [SerializeField] private GameObject hexPrefab;
    public Dictionary<(Team, Piece), GameObject> piecePrefabs = new Dictionary<(Team, Piece), GameObject>();
    public List<BoardState> turnHistory = new List<BoardState>();
    [ReadOnly] public Dictionary<(Team, Piece), IPiece> activePieces = new Dictionary<(Team, Piece), IPiece>();
    public delegate void NewTurn(BoardState newState);
    [HideInInspector] public NewTurn newTurn;
    [SerializeField] public HexGrid hexGrid;
    [OdinSerialize] public List<List<Hex>> hexes = new List<List<Hex>>();

    private void Awake() => SetBoardState(turnHistory[turnHistory.Count - 1]);
    private void Start() => newTurn.Invoke(turnHistory[turnHistory.Count - 1]);

    public void SetBoardState(BoardState newState)
    {
        foreach(KeyValuePair<(Team, Piece), Index> pieceAtLocation in newState.biDirPiecePositions)
        {
            Index index = pieceAtLocation.Value;
            Vector3 piecePosition = hexes[index.row][index.col].transform.position + Vector3.up;

            // If the piece already exists, move it
            if(activePieces.ContainsKey(pieceAtLocation.Key))
            {
                IPiece piece = activePieces[pieceAtLocation.Key];
                piece.MoveTo(hexes[index.row][index.col]);
                continue;
            }

            // Spawn a new piece at the proper location
            IPiece newPiece = Instantiate(piecePrefabs[pieceAtLocation.Key], piecePosition, Quaternion.identity).GetComponent<IPiece>();
            (Team team, Piece type) = pieceAtLocation.Key;
            newPiece.Init(team, type, index);
            activePieces.Add(
                pieceAtLocation.Key, 
                newPiece
            );
        }
    }

    public Team GetCurrentTurn()
    {
        if(promotionDialogue.gameObject.activeSelf)
            return Team.None;

        return turnHistory[turnHistory.Count - 1].currentMove;
    }

    public BoardState GetCurrentBoardState() => turnHistory[turnHistory.Count - 1];

    public BoardState SubmitMove(IPiece piece, Hex targetLocation, BoardState boardState, bool isQuery = false)
    {
        // Copy the existing board state
        BoardState currentState = boardState;
        BidirectionalDictionary<(Team, Piece), Index> allPositions = new BidirectionalDictionary<(Team, Piece), Index>(boardState.biDirPiecePositions);
        
        // If the hex being moved into contains an enemy piece, capture it
        if(currentState.biDirPiecePositions.Contains(targetLocation.index))
        {
            (Team occupyingTeam, Piece occupyingType) = currentState.biDirPiecePositions[targetLocation.index];
            if(occupyingTeam != piece.team)
            {
                IPiece occupyingPiece = activePieces[(occupyingTeam, occupyingType)];

                // Capture the enemy piece
                if(!isQuery)
                {
                    jails[(int)occupyingTeam].Enprison(occupyingPiece);
                    activePieces.Remove((occupyingTeam, occupyingType));
                }
                allPositions.Remove((occupyingTeam, occupyingType));
            }
        }

        // Move piece
        if(!isQuery)
            piece.MoveTo(targetLocation);

        // Update boardstate
        allPositions.Remove((piece.team, piece.type));
        allPositions.Add((piece.team, piece.type), targetLocation.index);
        currentState.biDirPiecePositions = allPositions;
        
        return currentState;
    }

    public void QueryPromote(Pawn pawn) => promotionDialogue.Display((pieceType) => Promote(pawn, pieceType));

    public void Promote(Pawn pawn, Piece type)
    {
        // Replace the pawn with the chosen piece type
        // Worth noting: Even though the new IPiece is of a different type than Pawn, 
        // we still use the PieceType.Pawn# (read from the pawn) to store it's position in the game state to maintain it's unique key
        // This may need changed when doing networking/saving/loading, or some singal will have to be sent about what the pawn is promoted to
        IPiece newPiece = Instantiate(piecePrefabs[(pawn.team, type)], pawn.transform.position, Quaternion.identity).GetComponent<IPiece>();
        newPiece.Init(pawn.team, pawn.type, pawn.location);
        activePieces[(pawn.team, pawn.type)] = newPiece;
        Destroy(pawn.gameObject);
    }

    public BoardState Swap(IPiece p1, IPiece p2, BoardState boardState, bool isQuery = false)
    {
        Index p1StartLoc = p1.location;
        Index p2StartLoc = p2.location;
        
        if(!isQuery)
        {
            p1.MoveTo(GetHexIfInBounds(p2.location));
            p2.MoveTo(GetHexIfInBounds(p1StartLoc));
        }

        BoardState currentState = boardState;
        BidirectionalDictionary<(Team, Piece), Index> allPositions = new BidirectionalDictionary<(Team, Piece), Index>(currentState.biDirPiecePositions);
        allPositions.Remove((p1.team, p1.type));
        allPositions.Remove((p2.team, p2.type));
        allPositions.Add((p1.team, p1.type), p2StartLoc);
        allPositions.Add((p2.team, p2.type), p1StartLoc);
        
        currentState.biDirPiecePositions = allPositions;

        return currentState;
    }

    public BoardState EnPassant(Pawn pawn, Team enemyTeam, Piece enemyType, Hex targetHex, BoardState boardState, bool isQuery = false)
    {
        BoardState currentState = boardState;
        IPiece enemyPiece = activePieces[(enemyTeam, enemyType)];
        BidirectionalDictionary<(Team, Piece), Index> allPositions = new BidirectionalDictionary<(Team, Piece), Index>(currentState.biDirPiecePositions);
        
        allPositions.Remove((enemyTeam, enemyType));
        
        if(!isQuery)
        {
            // Capture enemy
            jails[(int)enemyTeam].Enprison(enemyPiece);
            // Move pawn
            pawn.MoveTo(targetHex);
        }
        
        // Update board state
        allPositions.Remove((pawn.team, pawn.type));
        allPositions.Add((pawn.team, pawn.type), targetHex.index);
        currentState.biDirPiecePositions = allPositions;
        
        return currentState;
    }

    public void AdvanceTurn(BoardState newState)
    {
        // ClearPassantables();
        List<IPiece> checkingPieces = GetCheckingPieces(newState, newState.currentMove);
        if(checkingPieces.Count > 0)
        {
            Debug.Log("Check");

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
                Debug.Log("Mate");
        }
        newState.currentMove = newState.currentMove == Team.White ? Team.Black : Team.White;
        newTurn.Invoke(newState);
        turnHistory.Add(newState);
    }

    public List<IPiece> GetCheckingPieces(BoardState boardState, Team checkForTeam)
    {
        List<IPiece> checkingPieces = new List<IPiece>();

        foreach(KeyValuePair<(Team, Piece), IPiece> kvp in activePieces)
        {
            (Team team, Piece piece) = kvp.Key;
            // If the IPiece doesn't exist in the boardstate, it might be a simulated boardstate, skip that piece
            if(team != checkForTeam || !boardState.biDirPiecePositions.ContainsKey((team, piece)))
                continue;

            List<(Hex, MoveType)> moves = kvp.Value.GetAllPossibleMoves(this, boardState);
            foreach((Hex hex, MoveType moveType) in moves)
            {
                if(moveType != MoveType.Attack)
                    continue;
                
                if(boardState.biDirPiecePositions.ContainsKey(hex.index))
                {
                    (Team occupyingTeam, Piece occupyingPiece) = boardState.biDirPiecePositions[hex.index];
                    // Check
                    if(occupyingTeam != checkForTeam && occupyingPiece == Piece.King)
                        checkingPieces.Add(kvp.Value);
                }
            }
        }
        return checkingPieces;
    }

    public List<(Hex, MoveType)> GetAllValidMovesForPiece(IPiece piece, BoardState boardState)
    {
        // Eliminate invalid moves
        // Simulate moves, eliminating any that leave the current player in check
        List<(Hex, MoveType)> possibleMoves = piece.GetAllPossibleMoves(this, boardState);
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
                newState = SubmitMove(piece, possibleHex, boardState, true);
            else if(possibleMoveType == MoveType.Defend)
                newState = Swap(piece, activePieces[boardState.biDirPiecePositions[possibleHex.index]], boardState, true);
            else if(possibleMoveType == MoveType.EnPassant)
            {
                int teamOffset = boardState.currentMove == Team.White ? -2 : 2;
                Index enemyLoc = new Index(possibleHex.index.row + teamOffset, possibleHex.index.col);
                (Team enemyTeam, Piece enemyType) = boardState.biDirPiecePositions[enemyLoc];
                newState = EnPassant((Pawn)piece, enemyTeam, enemyType, possibleHex, boardState, true);
            }

            Team otherTeam = piece.team == Team.White ? Team.Black : Team.White;
            // If any piece is checking, the move is invalid, remove it from the list of possible moves
            List<IPiece> checkingPieces = GetCheckingPieces(newState, otherTeam);
            if(checkingPieces.Count > 0)
                possibleMoves.RemoveAt(i);
        }
        return possibleMoves;
    }

    public void Surrender() => SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);

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