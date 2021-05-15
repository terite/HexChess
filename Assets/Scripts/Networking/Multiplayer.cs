using System;
using UnityEngine;

public class Multiplayer : MonoBehaviour
{
    [SerializeField] private GameObject whiteKeys;
    [SerializeField] private GameObject blackKeys;
    [SerializeField] private Timers timers;
    [SerializeField] private TurnChangePanel turnChangePanel;
    Networker networker;
    Board board;
    LastMoveTracker moveTracker;

    public GameParams gameParams { get; private set; }
    public Team localTeam => gameParams.localTeam;

    private void Awake()
    {
        networker = GameObject.FindObjectOfType<Networker>();
        board = GameObject.FindObjectOfType<Board>();
        moveTracker = GameObject.FindObjectOfType<LastMoveTracker>();
    }

    public void SetupGame(GameParams gameParams)
    {
        Debug.Log($"Setting up game for {gameParams.localTeam} team.");
        this.gameParams = gameParams;

        SmoothHalfOrbitalCamera cam = GameObject.FindObjectOfType<SmoothHalfOrbitalCamera>();
        cam?.SetDefaultTeam(gameParams.localTeam);

        whiteKeys.SetActive(gameParams.localTeam == Team.White);
        blackKeys.SetActive(gameParams.localTeam == Team.Black);

        if(gameParams.timerDuration <= 0)
        {
            timers.gameObject.SetActive(gameParams.showClock);
            timers.isClock = gameParams.showClock;
        }
        else
        {
            timers.gameObject.SetActive(true);
            timers.SetTimers(gameParams.timerDuration);
        }

        if(gameParams.localTeam == Team.White)
            turnChangePanel.Display(gameParams.localTeam);
    }

    public void ReceiveBoard(BoardState state)
    {
        if(board.GetCurrentTurn() == gameParams.localTeam)
            return;

        // Advance Turn is going to flip the current move to the next team
        // So we want to display the turn change panel if it's going to become our turn    
        if(state.currentMove != gameParams.localTeam)
            turnChangePanel.Display(gameParams.localTeam);

        board.SetBoardState(state, board.promotions);
        board.AdvanceTurn(state, false);

        moveTracker.UpdateText(BoardState.GetLastMove(board.turnHistory));
    }

    public void SendBoard(BoardState state)
    {
        state.executedAtTime = Time.timeSinceLevelLoad + board.timeOffset;
        networker.SendMessage(
            new Message(MessageType.BoardState, state.Serialize())
        );
    }

    public void Surrender(Team surrenderingTeam, float timestamp) => 
        board.EndGame(timestamp, GameEndType.Surrender, surrenderingTeam == Team.White ? Winner.Black : Winner.White);
    public void Surrender(Team surrenderingTeam) => 
        Surrender(surrenderingTeam, Time.timeSinceLevelLoad + board.timeOffset);

    public void Draw(float timestamp) => 
        board.EndGame(timestamp, GameEndType.Draw, Winner.Draw);
    public void ClaimDraw() =>
        networker.RespondToDrawOffer(MessageType.AcceptDraw);

    public void SendGameEnd(float timestamp, MessageType endType) => 
        networker.SendMessage(new Message(endType, BitConverter.GetBytes(timestamp)));
    public void ReceiveCheckmate(float timestamp) => board.EndGame(
        timestamp, 
        GameEndType.Checkmate, 
        gameParams.localTeam == Team.White ? Winner.White : Winner.Black
    );

    public void ReceiveStalemate(float timestamp) => 
        board.EndGame(timestamp, GameEndType.Stalemate, Winner.None);

    public void SendFlagfall(Flagfall flagfall) => 
        networker.SendMessage(new Message(MessageType.FlagFall, flagfall.Serialize()));
    public void ReceiveFlagfall(Flagfall flagfall) => board.EndGame(
        flagfall.timestamp, 
        GameEndType.Flagfall, 
        flagfall.flaggedTeam == Team.White ? Winner.Black : Winner.White
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
        if(board.activePieces.ContainsKey((promo.team, promo.from)))
        {
            IPiece piece = board.activePieces[(promo.team, promo.from)];
            if(piece is Pawn pawn)
                board.Promote(pawn, promo.to);
        }
    }
}