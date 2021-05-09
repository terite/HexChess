using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class Timers : MonoBehaviour
{
    Board board;

    [SerializeField] private TextMeshProUGUI whiteTimerText;
    float whiteTimer;
    [SerializeField] private TextMeshProUGUI blackTimerText;
    float blackTimer;

    public bool isClock = false;
    public float timerDruation {get; private set;}    

    Team currentTurn;

    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        board.newTurn += NewTurn;
        
        if(isClock)
        {
            UpdateClock(0, Team.White);
            UpdateClock(0, Team.Black);
        }
        else if(timerDruation > 0)
            SetDefaultTimers(timerDruation);
    }

    public void SetClock()
    {
        if(currentTurn == Team.None)
        {
            if(board == null)
                board = GameObject.FindObjectOfType<Board>();
            currentTurn = board.GetCurrentTurn();
        }
        isClock = true;
        timerDruation = 0;
        RecalculateTimers();
    }

    public void SetTimers(float duration)
    {
        if(currentTurn == Team.None)
        {
            if(board == null)
                board = GameObject.FindObjectOfType<Board>();
            currentTurn = board.GetCurrentTurn();
        }
        isClock = false;
        timerDruation = duration;
        SetDefaultTimers(timerDruation);
    }

    private void SetDefaultTimers(float duration)
    {
        whiteTimer = duration;
        blackTimer = duration;
        UpdateTimerAndCheckForFlagfall(duration, Team.White);
        UpdateTimerAndCheckForFlagfall(duration, Team.Black);
    }

    private void NewTurn(BoardState newState)
    {
        currentTurn = newState.currentMove;
        RecalculateTimers();
    }

    public void RecalculateTimers()
    {
        // Recalculate clocks to ensure the times are synced properly, this may catch any differences caused by latency while in multiplayer
        if(isClock)
        {
            float whiteTotal = 0f;
            float blackTotal = 0f;

            for(int i = 1; i < board.turnHistory.Count; i++)
            {
                BoardState nowState = board.turnHistory[i];
                BoardState lastState = board.turnHistory[i - 1];
                float duration = nowState.executedAtTime - lastState.executedAtTime;

                if(nowState.currentMove == Team.None)
                {
                    if(lastState.currentMove == Team.White)
                        whiteTotal += duration;
                    else if(lastState.currentMove == Team.Black)
                        blackTotal += duration;
                }
                else if(i % 2 == 0)
                    blackTotal += duration;
                else
                    whiteTotal += duration;
            }

            whiteTimer = whiteTotal;
            blackTimer = blackTotal;
            UpdateClock(whiteTotal, Team.White);
            UpdateClock(blackTotal, Team.Black);

        }
        else if(timerDruation > 0)
        {
            float whiteTotal = timerDruation;
            float blackTotal = timerDruation;

            for(int i = 1; i < board.turnHistory.Count; i++)
            {
                BoardState nowState = board.turnHistory[i];
                BoardState lastState = board.turnHistory[i - 1];
                float duration = nowState.executedAtTime - lastState.executedAtTime;
                if(nowState.currentMove == Team.None)
                {
                    if(lastState.currentMove == Team.White)
                        whiteTotal = whiteTotal - duration > 0 ? whiteTotal - duration : 0;
                    else if(lastState.currentMove == Team.Black)
                        blackTotal = blackTotal - duration > 0 ? blackTotal - duration : 0;
                }
                else if(i % 2 == 0)
                    blackTotal = blackTotal - duration > 0 ? blackTotal - duration : 0;
                else
                    whiteTotal = whiteTotal - duration > 0 ? whiteTotal - duration : 0;
            }
            
            whiteTimer = whiteTotal;
            blackTimer = blackTotal;
            UpdateTimerAndCheckForFlagfall(whiteTotal, Team.White);
            UpdateTimerAndCheckForFlagfall(blackTotal, Team.Black);
        }
    }

    private void Update()
    {
        if(currentTurn == Team.None)
            return;

        if(isClock)
        {
            if(currentTurn == Team.White)
                whiteTimer += Time.deltaTime;
            else
                blackTimer += Time.deltaTime;
            
            UpdateClock(GetTeamTime(currentTurn), currentTurn);
        }
        else if(timerDruation > 0)
        {
            if(currentTurn == Team.White)
                whiteTimer = whiteTimer - Time.deltaTime > 0 ? whiteTimer - Time.deltaTime : 0;
            else
                blackTimer = blackTimer - Time.deltaTime > 0 ? blackTimer - Time.deltaTime : 0;
            
            UpdateTimerAndCheckForFlagfall(GetTeamTime(currentTurn), currentTurn);
        }
    }

    string GetFormat(float seconds) => seconds < 60 
        ? @"%s\.f" 
        : seconds < 3600
            ? @"%m\:%s\.f"
            : @"%h\:%m\:%s\.f";
        
    TextMeshProUGUI GetTeamText(Team team) => team == Team.White ? whiteTimerText : blackTimerText;
    float GetTeamTime(Team team) => team == Team.White ? whiteTimer : blackTimer;

    public void UpdateClock(float seconds, Team team)
    {
        if(team == Team.None)
            return;

        GetTeamText(team).text = TimeSpan.FromSeconds(seconds).ToString(GetFormat(seconds));
    }

    private void UpdateTimerAndCheckForFlagfall(float seconds, Team team)
    {
        if(team == Team.None)
            return;

        string teamTime = TimeSpan.FromSeconds(seconds).ToString(GetFormat(seconds));
        string td = TimeSpan.FromSeconds(timerDruation).ToString(GetFormat(timerDruation));
        
        GetTeamText(team).text = $"{teamTime} / {td}";

        if(seconds == 0)
        {
            Multiplayer mp = GameObject.FindObjectOfType<Multiplayer>();
            float timestamp = Time.timeSinceLevelLoad + board.timeOffset;
            Flagfall flagfall = new Flagfall(team, timestamp);
            if(mp != null)
            {
                if(mp.localTeam == team)
                {
                    mp.SendFlagfall(flagfall);
                    board.EndGame(flagfall.timestamp, GameEndType.Flagfall, flagfall.flaggedTeam == Team.White ? Winner.Black : Winner.White);
                }
            }
            else
                board.EndGame(flagfall.timestamp, GameEndType.Flagfall, flagfall.flaggedTeam == Team.White ? Winner.Black : Winner.White);
        }
    }
}
