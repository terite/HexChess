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
    [SerializeField] private SelectPiece selectPiece;
    [SerializeField] private LastMoveTracker lastMoveTracker;
    [SerializeField] private TurnPanel turnPanel;
    MovePanel startPanel;
    MovePanel lastMovePanel;
    private List<MovePanel> panels = new List<MovePanel>();

    private float whiteTotal = 0;
    private float blackTotal = 0;

    public (int index, Team team) panelPointer {get; private set;} = (0, Team.None);
    public (int index, Team team) currentTurnPointer {get; private set;} = (0, Team.None);
    int? traverseDir = null;
    public float traverseDelay = 0.25f;
    private float traverseAtTime;
    VirtualCursor cursor;
    private void Awake() 
    {
        board.newTurn += NewTurn;
        cursor = GameObject.FindObjectOfType<VirtualCursor>();
    }
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

    private void Update()
    {
        if(traverseDir.HasValue && Time.timeSinceLevelLoad >= traverseAtTime)
        {
            traverseAtTime = Time.timeSinceLevelLoad + traverseDelay;
            HistoryStep(traverseDir.Value);
        }
    }

    private void OnDestroy() => board.newTurn -= NewTurn;

    public bool TryGetCurrentBoardState(out BoardState state)
    {
        if(panels.Count == 0)
        {
            state = (BoardState)default;
            return false;
        }

        state = panels[panelPointer.index].GetState(panelPointer.team);
        if(state.allPiecePositions == null)
            return false;
        return true;
    }

    private void NewTurn(BoardState newState)
    {
        // If the current move is black, we know white just made a move, let's add an entry to the list
        Move lastMove = BoardState.GetLastMove(board.turnHistory, board.promotions);
        UpdateMovePanels(newState, lastMove, Mathf.FloorToInt((float)board.turnHistory.Count / 2f) + board.turnHistory.Count % 2);
    }

    public void SetGame(Game game)
    {
        Clear();
        for(int i = game.turnHistory.Count <= 1 ? 0 : 1; i < game.turnHistory.Count; i++)
        {
            BoardState state = game.turnHistory[i];
            List<BoardState> subset = game.turnHistory.Take(i + 1).ToList();
            Move lastMove = BoardState.GetLastMove(subset, board.promotions);
            UpdateMovePanels(state, lastMove, Mathf.FloorToInt((float)subset.Count / 2f) + subset.Count % 2);
        }
    }

    public void UpdateMovePanels(BoardState newState, Move lastMove, int turnNumber)
    {
        if(newState.currentMove == Team.None)
            return;
        else if(newState.currentMove == Team.Black)
        {
            lastMovePanel?.ClearHighlight();

            foreach(MovePanel panel in panels)
            {
                if(panel.whiteState == newState && panel.whiteState.executedAtTime == newState.executedAtTime)
                    return;
            }
            
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
            lastMovePanel.SetIndex(panels.Count - 1);
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

    public void HistoryStep(int val)
    {
        if(panels.Count == 0)
            return;
        
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

        HistoryJump((newIndex, newTeam));
    }

    public void HistoryStep(CallbackContext context)
    {
        if(panels.Count == 0)
            return;

        // Will be 1, 0, or -1
        int val = (int)context.ReadValue<float>();
        if(context.started)
            traverseDir = val;
        else if(context.canceled)
        {
            traverseDir = null;
            traverseAtTime = 0;
        }
    }

    public void HistoryJump(CallbackContext context)
    {
        if(!context.performed)
            return;

        int val = (int)context.ReadValue<float>();

        HistoryJump(
            pointer: val == -1 ? (0, Team.White) : val == 1 ? currentTurnPointer : panelPointer,  
            scrollBarVal: val < 0 ? 0 : val > 0 ? 1 : scrollBar.value
        );
    }

    public void HistoryJump((int index, Team team) pointer, float? scrollBarVal = null)
    {
        if(panels.Count == 0)
            return;
        
        cursor?.SetCursor(CursorType.Default);
        if(selectPiece.selectedPiece != null)
        {
            selectPiece.PlayCancelNoise();
            selectPiece.DeselectPiece(selectPiece.selectedPiece.location, selectPiece.selectedPiece.captured);
        }
        
        if(scrollBarVal.HasValue)
            scrollBar.value = scrollBarVal.Value;
            
        (int index, Team team) previousPointer = panelPointer;
        panelPointer = pointer;
        MovePanel panel = panels[panelPointer.index];
        
        if(panel.TryGetState(panelPointer.team, out BoardState state))
        {
            selectPiece.ClearCheckOrMateHighlight();
            panels[previousPointer.index].ClearHighlight();
            panel.HighlightTeam(panelPointer.team);

            // If the preview is not disabled prior to setting the board state, we may end up with missing pointer references anytime a piece was promoted/demoted by traversing the history
            selectPiece.DisablePreview();
            Move move = panel.GetMove(panelPointer.team);
            board.SetBoardState(state, board.promotions, move.turn);
            board.HighlightMove(move);
            selectPiece.HighlightPotentialCheckOrMate(state);
            lastMoveTracker.UpdateText(move);

            // Determine if it's game over or not. If so, call turnPanel.GameOver
            if(board.game.endType == GameEndType.Pending || panelPointer != currentTurnPointer)
                turnPanel.NewTurn(state, move.lastTeam == Team.Black ? move.turn + 1 : move.turn);
            else
                turnPanel.SetGameEndText(board.game);
        }
    }

    public void JumpToFirst() => HistoryJump((0, Team.White), 0);
    public void JumpToPresent() => HistoryJump(currentTurnPointer, 1);
}