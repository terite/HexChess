using System.Collections.Generic;
using UnityEngine;

public class Multiplayer : MonoBehaviour
{
    [SerializeField] private GameObject whiteKeys;
    [SerializeField] private GameObject blackKeys;
    Networker networker;
    Board board;
    LastMoveTracker moveTracker;

    public GameParams gameParams {get; private set;}
    public Team localTeam => gameParams.localTeam;

    Promotion? receivedPromotion;
    private void Awake() {
        networker = GameObject.FindObjectOfType<Networker>();
        board = GameObject.FindObjectOfType<Board>();
        moveTracker = GameObject.FindObjectOfType<LastMoveTracker>();
    }

    public void SetupGame(GameParams gameParams)
    {
        Debug.Log($"Setting up game for {gameParams.localTeam} team.");
        this.gameParams = gameParams;

        // rotate camera for black team player
        if(gameParams.localTeam == Team.Black)
        {
            // OrbitalCamera cam = GameObject.FindObjectOfType<OrbitalCamera>();
            Camera cam = Camera.main;
            cam.transform.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x, cam.transform.rotation.eulerAngles.y, 180);
        }

        whiteKeys.SetActive(gameParams.localTeam == Team.White);
        blackKeys.SetActive(gameParams.localTeam == Team.Black);
    }

    public void Surrender(Team surrenderingTeam) => board.Surrender(surrenderingTeam);
    public void UpdateBoard(BoardState state)
    {
        if(board.GetCurrentTurn() == gameParams.localTeam)
            return;
        
        board.SetBoardState(state, board.promotions);
        board.AdvanceTurn(state);

        moveTracker.UpdateText(BoardState.GetLastMove(board.turnHistory));
    }

    public void SendBoard(BoardState state) => networker.SendMessage(
        new Message(MessageType.BoardState, state.Serialize())
    );

    public void SendPromote(Promotion promo)
    {
        BoardState state = board.GetCurrentBoardState();
        state.currentMove = state.currentMove == Team.White ? Team.Black : Team.White;
        SendBoard(state);
        networker.SendMessage(new Message(MessageType.Promotion, promo.Serialize()));
    }
    public void ReceivePromotion(Promotion promo)
    {
        receivedPromotion = promo;
        if(board.activePieces.ContainsKey((promo.team, promo.from)))
        {
            IPiece piece = board.activePieces[(promo.team, promo.from)];
            if(piece is Pawn pawn)
                board.Promote(pawn, promo.to);
        }
    }
}