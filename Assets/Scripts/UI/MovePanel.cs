using System;
using Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnNumberText;
    [SerializeField] private TextMeshProUGUI whiteText;
    [SerializeField] private TextMeshProUGUI blackText;
    [SerializeField] private TextMeshProUGUI whiteTimestamp;
    [SerializeField] private TextMeshProUGUI blackTimestamp;
    [SerializeField] private TextMeshProUGUI whiteDeltaTime;
    [SerializeField] private TextMeshProUGUI blackDeltaTime;
    [SerializeField] private Image background;
    [SerializeField] private Image whiteBG;
    [SerializeField] private Image blackBG;
    public Color darkColor;
    public Color lightColor;
    Board board;
    public BoardState whiteState {get; private set;}
    public BoardState blackState {get; private set;}
    [ShowInInspector] public Move whiteMove {get; private set;}
    [ShowInInspector] public Move blackMove {get; private set;}

    private void Awake() => board = GameObject.FindObjectOfType<Board>();

    public void SetMove(BoardState state, Move move)
    {
        if(move.lastTeam == Team.White)
        {
            whiteState = state;
            whiteMove = move;
        }
        else if(move.lastTeam == Team.Black)
        {
            blackState = state;
            blackMove = move;
        }

        TextMeshProUGUI toChange = move.lastTeam == Team.White ? whiteText : blackText;
        string shortForm = move.GetNotation(board.promotions, state, NotationType.LongForm);
        
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
    public void SetDark() => background.color = darkColor;
    public void SetLight() => background.color = lightColor;
    public void FlipColor() => background.color = background.color == darkColor ? lightColor : darkColor;
    public void HighlightTeam(Team team)
    {
        Image toChange = team == Team.White ? whiteBG : blackBG;
        Image other = team == Team.White ? blackBG : whiteBG;
        toChange.enabled = true;
        other.enabled = false;
    }

    public void ClearHighlight()
    {
        whiteBG.enabled = false;
        blackBG.enabled = false;
    }
    public BoardState GetState(Team team) => team == Team.White ? whiteState : blackState;
    public Move GetMove(Team team) => team == Team.White ? whiteMove : blackMove;
}