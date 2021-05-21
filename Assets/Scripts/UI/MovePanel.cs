using System;
using Extensions;
using TMPro;
using UnityEngine;

public class MovePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnNumberText;
    [SerializeField] private TextMeshProUGUI whiteText;
    [SerializeField] private TextMeshProUGUI blackText;
    [SerializeField] private TextMeshProUGUI whiteTimestamp;
    [SerializeField] private TextMeshProUGUI blackTimestamp;
    [SerializeField] private TextMeshProUGUI whiteDeltaTime;
    [SerializeField] private TextMeshProUGUI blackDeltaTime;
    Board board;
    private void Awake() => board = GameObject.FindObjectOfType<Board>();

    public void SetMove(Move move)
    {
        TextMeshProUGUI toChange = move.lastTeam == Team.White ? whiteText : blackText;
        BoardState boardState = board.GetCurrentBoardState();
        string shortForm = move.GetNotation(board.promotions, boardState, NotationType.LongForm);
        
        toChange.text = shortForm;

        if(move.lastTeam == Team.White)
            blackText.text = "";
        
        TextMeshProUGUI deltaTimeToChange = move.lastTeam == Team.White ? whiteDeltaTime : blackDeltaTime;
        if(deltaTimeToChange != null)
            deltaTimeToChange.text = $"+{TimeSpan.FromSeconds(move.duration).ToString(move.duration.GetStringFromSeconds())}";
    }

    public void SetTimestamp(float seconds, Team team)
    {
        TextMeshProUGUI toChange = team == Team.White ? whiteTimestamp : blackTimestamp;
        toChange.text = TimeSpan.FromSeconds(seconds).ToString(seconds.GetStringFromSeconds());
    }
    public void ClearTimestamp(Team team)
    {
        TextMeshProUGUI toChange = team == Team.White ? whiteTimestamp : blackTimestamp;
        toChange.text = "";
    }
    public void ClearDeltaTime(Team team)
    {
        TextMeshProUGUI deltaTimeToChange = team == Team.White ? whiteDeltaTime : blackDeltaTime;
        if(deltaTimeToChange == null)
            return;
        deltaTimeToChange.text = "";
    }

    public void SetTurnNumber(int val) => turnNumberText.text = $"{val}";
}