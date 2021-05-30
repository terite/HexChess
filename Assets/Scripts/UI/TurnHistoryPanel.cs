using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class TurnHistoryPanel : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollBar;
    [SerializeField] private MovePanel movePanelPrefab;
    [SerializeField] private MovePanel startPanelPrefab;
    [SerializeField] private RectTransform collectionContainer;
    [SerializeField] private Board board;
    MovePanel startPanel;
    MovePanel lastMovePanel;
    private List<MovePanel> panels = new List<MovePanel>();

    private float whiteTotal = 0;
    private float blackTotal = 0;

    public (int index, Team team) panelPointer {get; private set;} = (0, Team.None);
    public (int index, Team team) currentTurnPointer {get; private set;} = (0, Team.None);


    private void Awake() => board.newTurn += NewTurn;
    private void Start() 
    {
        startPanel = Instantiate(startPanelPrefab, collectionContainer);
        startPanel.SetDark();
        startPanel.ClearHighlight();
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
            lastMovePanel?.ClearHighlight();

            lastMovePanel = Instantiate(movePanelPrefab, collectionContainer);
            lastMovePanel.SetLight();
            foreach(MovePanel panel in panels)
                panel.FlipColor();
            
            panels.Add(lastMovePanel);
            
            int i = 1;
            for(int j = panels.Count - 1; j >= 0; j--)
            {
                panels[j].transform.SetSiblingIndex(i);
                i++;
            }

            lastMovePanel.SetTurnNumber(turnNumber);
            lastMovePanel.SetMove(newState, lastMove);
            lastMovePanel.SetTimestamp(newState.executedAtTime, Team.White);
            lastMovePanel.ClearTimestamp(Team.Black);
            lastMovePanel.ClearDeltaTime(Team.Black);

            whiteTotal += lastMove.duration;
            startPanel.SetTimestamp(whiteTotal, Team.White);

            lastMovePanel.HighlightTeam(Team.White);
            panelPointer = (panels.Count - 1, Team.White);
            currentTurnPointer = panelPointer;
        }
        else
        {
            lastMovePanel?.SetMove(newState, lastMove);
            lastMovePanel?.SetTimestamp(newState.executedAtTime, Team.Black);

            blackTotal += lastMove.duration;
            startPanel?.SetTimestamp(blackTotal, Team.Black);

            lastMovePanel?.HighlightTeam(Team.Black);
            panelPointer = (panels.Count - 1, Team.Black);
            currentTurnPointer = panelPointer;
        }
    }

    public void Clear()
    {
        for(int i = panels.Count - 1; i >= 0; i--)
            Destroy(panels[i].gameObject);
        
        panels.Clear();
    }

    public void HistoryStep(CallbackContext context)
    {
        if(!context.performed)
            return;
        
        if(panels.Count == 0)
            return;

        // Will be 1, 0, or -1
        int val = (int)context.ReadValue<float>();
        
        // Prevent trying to move past the final move
        if(panelPointer.index == panels.Count - 1 && val > 0 && panelPointer.team == Team.Black)
            return;
        // Prevent trying to move past the first move
        else if(panelPointer.index == 0 && val < 0 && panelPointer.team == Team.White)
            return;
        // Prevent moving to the current pending move
        else if(val > 0 && panelPointer.team == Team.White && panelPointer.index == currentTurnPointer.index && currentTurnPointer.team == Team.White)
            return;

        (int index, Team team) previousPointer = panelPointer;
        // Calculate the new index based on the button pressed and the previous index
        int newIndex = (previousPointer.team == Team.White && val == -1) || (previousPointer.team == Team.Black && val == 1)
                ? Mathf.Clamp(previousPointer.index + val, 0, panels.Count - 1)
                : previousPointer.index;

        Team newTeam = previousPointer.team == Team.White ? Team.Black : Team.White;
        panelPointer = (newIndex, newTeam);
        
        panels[previousPointer.index].ClearHighlight();
        MovePanel panel = panels[panelPointer.index];
        panel.HighlightTeam(panelPointer.team);
        board.SetBoardState(panel.GetState(panelPointer.team));
        board.HighlightMove(panel.GetMove(panelPointer.team));
    }

    public void HistoryJump(CallbackContext context)
    {
        if(!context.performed)
            return;
        
        if(panels.Count == 0)
            return;

        int val = (int)context.ReadValue<float>();

        (int index, Team team) previousPointer = panelPointer;
        
        panelPointer = val == -1 ? (0, Team.White) : val == 1 ? currentTurnPointer : panelPointer;
        scrollBar.value = val < 0 ? 0 : val > 0 ? 1 : scrollBar.value;

        panels[previousPointer.index].ClearHighlight();
        MovePanel panel = panels[panelPointer.index];
        panel.HighlightTeam(panelPointer.team);
        board.SetBoardState(panel.GetState(panelPointer.team));
        board.HighlightMove(panel.GetMove(panelPointer.team));
    }

    public void JumpToPresent()
    {
        if(panels.Count == 0)
            return;

        (int index, Team team) previousPointer = panelPointer;
        panelPointer = currentTurnPointer;
        scrollBar.value = 1;
        panels[previousPointer.index].ClearHighlight();
        MovePanel panel = panels[panelPointer.index];
        panel.HighlightTeam(panelPointer.team);
        board.SetBoardState(panel.GetState(panelPointer.team));
        board.HighlightMove(panel.GetMove(panelPointer.team));
    }
}