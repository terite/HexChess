using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnHistoryPanel : MonoBehaviour
{
    [SerializeField] private MovePanel movePanelPrefab;
    [SerializeField] private MovePanel startPanelPrefab;
    [SerializeField] private RectTransform collectionContainer;
    [SerializeField] private Board board;
    MovePanel startPanel;
    MovePanel lastMovePanel;
    private List<MovePanel> panels = new List<MovePanel>();

    private float whiteTotal = 0;
    private float blackTotal = 0;

    private void Awake() => board.newTurn += NewTurn;
    private void Start() 
    {
        startPanel = Instantiate(startPanelPrefab, collectionContainer);
        foreach(Team team in EnumArray<Team>.Values)
        {
            if(team == Team.None)
                continue;

            startPanel.SetTimestamp(0, team);
        }
    }

    private void OnDestroy() => board.newTurn -= NewTurn;

    private void NewTurn(BoardState newState)
    {
        // If the current move is black, we know white just made a move, let's add an entry to the list
        Move lastMove = BoardState.GetLastMove(board.turnHistory);
        UpdateMovePanels(newState, lastMove, Mathf.FloorToInt((float)board.turnHistory.Count / 2f) + board.turnHistory.Count % 2);
    }

    public void SetGame(Game game)
    {
        Clear();
        for(int i = 0; i < game.turnHistory.Count; i++)
        {
            BoardState state = game.turnHistory[i];
            List<BoardState> subset = game.turnHistory.Take(i + 1).ToList();
            Move lastMove = BoardState.GetLastMove(subset);
            UpdateMovePanels(state, lastMove, Mathf.FloorToInt((float)subset.Count / 2f) + subset.Count % 2);
        }
    }

    public void UpdateMovePanels(BoardState newState, Move lastMove, int turnNumber)
    {
        if(newState.currentMove == Team.Black)
        {
            lastMovePanel = Instantiate(movePanelPrefab, collectionContainer);
            
            panels.Add(lastMovePanel);
            
            int i = 1;
            for(int j = panels.Count - 1; j >= 0; j--)
            {
                panels[j].transform.SetSiblingIndex(i);
                i++;
            }

            lastMovePanel.SetTurnNumber(turnNumber);
            lastMovePanel.SetMove(lastMove);
            lastMovePanel.SetTimestamp(newState.executedAtTime, Team.White);
            lastMovePanel.ClearTimestamp(Team.Black);
            lastMovePanel.ClearDeltaTime(Team.Black);

            whiteTotal += lastMove.duration;
            startPanel.SetTimestamp(whiteTotal, Team.White);
        }
        else
        {
            lastMovePanel?.SetMove(lastMove);
            lastMovePanel?.SetTimestamp(newState.executedAtTime, Team.Black);

            blackTotal += lastMove.duration;
            startPanel?.SetTimestamp(blackTotal, Team.Black);
        }
    }

    public void Clear()
    {
        for(int i = panels.Count - 1; i >= 0; i--)
            Destroy(panels[i].gameObject);
        
        panels.Clear();
    }
}