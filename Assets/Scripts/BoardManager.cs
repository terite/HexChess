using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardManager : SerializedMonoBehaviour
{
    public List<BoardState> turnHistory = new List<BoardState>();
    public Dictionary<(Team, PieceType), GameObject> piecePrefabs = new Dictionary<(Team, PieceType), GameObject>();
    public Dictionary<(Team, PieceType), IPiece> activePieces = new Dictionary<(Team, PieceType), IPiece>();
    [SerializeField] private HexSpawner boardSpawner;
    [SerializeField] private PromotionDialogue promotionDialogue;
    public List<Pawn> enPassantables = new List<Pawn>();

    public delegate void NewTurn(BoardState newState);
    public NewTurn newTurn;

    public List<Jail> jails = new List<Jail>();

    private void Awake() => SetBoardState(turnHistory[turnHistory.Count - 1]);
    private void Start() => newTurn.Invoke(turnHistory[turnHistory.Count - 1]);

    public void SetBoardState(BoardState newState)
    {
        foreach(KeyValuePair<(Team, PieceType), Index> pieceAtLocation in newState.biDirPiecePositions)
        {
            Index index = pieceAtLocation.Value;
            Vector3 piecePosition = boardSpawner.hexes[index.row][index.col].transform.position + Vector3.up;

            // If the piece already exists, move it
            if(activePieces.ContainsKey(pieceAtLocation.Key))
            {
                IPiece piece = activePieces[pieceAtLocation.Key];
                piece.MoveTo(boardSpawner.hexes[index.row][index.col]);
                continue;
            }

            // Spawn a new piece at the proper location
            IPiece newPiece = Instantiate(piecePrefabs[pieceAtLocation.Key], piecePosition, Quaternion.identity).GetComponent<IPiece>();
            (Team team, PieceType type) = pieceAtLocation.Key;
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

    public void SubmitMove(IPiece piece, Hex targetLocation)
    {
        // Copy the existing board state
        BoardState currentState = GetCurrentBoardState();
        BidirectionalDictionary<(Team, PieceType), Index> allPositions = new BidirectionalDictionary<(Team, PieceType), Index>(currentState.biDirPiecePositions);
        
        // If the hex being moved into contains an enemy piece, capture it
        if(currentState.biDirPiecePositions.Contains(targetLocation.hexIndex))
        {
            (Team occupyingTeam, PieceType occupyingType) = currentState.biDirPiecePositions[targetLocation.hexIndex];
            if(occupyingTeam != piece.team)
            {
                IPiece occupyingPiece = activePieces[(occupyingTeam, occupyingType)];

                // Capture the enemy piece
                jails[(int)occupyingTeam].Enprison(occupyingPiece);
                // Remove captured piece from boardstate and active pieces dictionary
                allPositions.Remove((occupyingTeam, occupyingType));
                activePieces.Remove((occupyingTeam, occupyingType));
            }
        }

        // Move piece
        piece.MoveTo(targetLocation);

        // Update boardstate
        allPositions.Remove((piece.team, piece.type));
        allPositions.Add((piece.team, piece.type), targetLocation.hexIndex);
        currentState.biDirPiecePositions = allPositions;
        
        AdvanceTurn(currentState);
    }

    public void QueryPromote(Pawn pawn) => promotionDialogue.Display((pieceType) => Promote(pawn, pieceType));

    public void Promote(Pawn pawn, PieceType type)
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

    public void Swap(IPiece p1, IPiece p2)
    {
        Index p1StartLoc = p1.location;
        p1.MoveTo(boardSpawner.GetHexIfInBounds(p2.location));
        p2.MoveTo(boardSpawner.GetHexIfInBounds(p1StartLoc));

        BoardState currentState = GetCurrentBoardState();
        BidirectionalDictionary<(Team, PieceType), Index> allPositions = new BidirectionalDictionary<(Team, PieceType), Index>(currentState.biDirPiecePositions);
        allPositions.Remove((p1.team, p1.type));
        allPositions.Remove((p2.team, p2.type));
        allPositions.Add((p1.team, p1.type), p1.location);
        allPositions.Add((p2.team, p2.type), p2.location);
        
        currentState.biDirPiecePositions = allPositions;
        AdvanceTurn(currentState);
    }

    private void AdvanceTurn(BoardState newState)
    {
        ClearPassantables();
        newState.currentMove = newState.currentMove == Team.White ? Team.Black : Team.White;
        newTurn.Invoke(newState);
        turnHistory.Add(newState);
    }

    public void EnPassant(Pawn pawn, Team enemyTeam, PieceType enemyType, Hex targetHex)
    {
        BoardState currentState = GetCurrentBoardState();
        IPiece enemyPiece = activePieces[(enemyTeam, enemyType)];
        BidirectionalDictionary<(Team, PieceType), Index> allPositions = new BidirectionalDictionary<(Team, PieceType), Index>(currentState.biDirPiecePositions);
        
        // Capture enemy
        allPositions.Remove((enemyTeam, enemyType));
        jails[(int)enemyTeam].Enprison(enemyPiece);
        
        // Move pawn
        pawn.MoveTo(targetHex);

        // Update board state
        allPositions.Remove((pawn.team, pawn.type));
        allPositions.Add((pawn.team, pawn.type), targetHex.hexIndex);
        currentState.biDirPiecePositions = allPositions;
        AdvanceTurn(currentState);
    }

    public void EnPassantable(Pawn pawn) => enPassantables.Add(pawn);
    public void ClearPassantables()
    {
        for(int i = enPassantables.Count - 1; i >= 0; i--)
        {
            Pawn pawn = enPassantables[i];
            if(pawn.turnsPassed >= 1)
            {
                pawn.passantable = false;
                pawn.turnsPassed = 0;
                enPassantables.RemoveAt(i);
            }
            else
                pawn.turnsPassed++;
        }
    }

    public void Surrender()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}