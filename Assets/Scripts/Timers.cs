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
    }

    private void Start() {
        if(isClock)
        {
            UpdateClock(0, Team.White);
            UpdateClock(0, Team.Black);
        }
        else if(timerDruation > 0)
            SetDefaultTimers(timerDruation);
    }

    public void SetTimers(float duration)
    {
        isClock = false;
        timerDruation = duration;
        SetDefaultTimers(timerDruation);
    }

    private void SetDefaultTimers(float duration)
    {
        whiteTimer = duration;
        blackTimer = duration;
        UpdateTimerAndCheckForTimeout(duration, Team.White);
        UpdateTimerAndCheckForTimeout(duration, Team.Black);
    }

    private void NewTurn(BoardState newState)
    {
        currentTurn = newState.currentMove;
        RecalculateTimers();
    }

    private void RecalculateTimers()
    {
        // Recalculate clocks to ensure the times are synced properly, this may catch any differences caused by latency while in multiplayer
        if(isClock)
        {
            if(currentTurn == Team.Black)
            {
                // It's black's turn, so that means white just made a move, recalculate the clock
                float total = 0f;
                for(int i = 1; i < board.turnHistory.Count; i += 2)
                    total += board.turnHistory[i].executedAtTime - board.turnHistory[i - 1].executedAtTime;
                whiteTimer = total;
                UpdateClock(total, Team.White);
            }
            else if(currentTurn == Team.White)
            {
                // It's whites's turn, so that means black just made a move, recalculate the clock
                float total = 0f;
                for(int i = 2; i < board.turnHistory.Count; i += 2)
                    total += board.turnHistory[i].executedAtTime - board.turnHistory[i - 1].executedAtTime;
                blackTimer = total;
                UpdateClock(total, Team.Black);
            }
        }
        else if(timerDruation > 0)
        {
            if(currentTurn == Team.Black)
            {
                float total = timerDruation;
                for(int i = 1; i < board.turnHistory.Count; i += 2)
                {
                    float duration = board.turnHistory[i].executedAtTime - board.turnHistory[i - 1].executedAtTime;
                    total = total - duration > 0 ? total - duration : 0;
                }
                whiteTimer = total;
                UpdateTimerAndCheckForTimeout(total, Team.White);
            }
            else if(currentTurn == Team.White)
            {
                float total = timerDruation;
                for(int i = 2; i < board.turnHistory.Count; i += 2)
                {
                    float duration = board.turnHistory[i].executedAtTime - board.turnHistory[i - 1].executedAtTime;
                    total = total - duration > 0 ? total - duration : 0;
                }
                blackTimer = total;
                UpdateTimerAndCheckForTimeout(total, Team.Black);
            }
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
            
            UpdateTimerAndCheckForTimeout(GetTeamTime(currentTurn), currentTurn);
        }
    }

    string GetFormat(float seconds) => seconds < 60 
        ? @"%s\.f" 
        : seconds < 6000
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

    private void UpdateTimerAndCheckForTimeout(float seconds, Team team)
    {
        if(team == Team.None)
            return;

        string teamTime = TimeSpan.FromSeconds(seconds).ToString(GetFormat(seconds));
        string td = TimeSpan.FromSeconds(timerDruation).ToString(GetFormat(timerDruation));
        
        GetTeamText(team).text = $"{teamTime} / {td}";

        if(seconds == 0)
        {
            Multiplayer mp = GameObject.FindObjectOfType<Multiplayer>();
            if(mp != null)
                mp.SendFlagfall(team);
            board.Flagfall(team);
        }
    }
}