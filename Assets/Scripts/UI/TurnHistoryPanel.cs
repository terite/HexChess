using System.Collections.Generic;
using UnityEngine;

public class TurnHistoryPanel : MonoBehaviour
{
    [SerializeField] private MovePanel movePanelPrefab;
    [SerializeField] private RectTransform collectionContainer;
    [SerializeField] private Board board;
    MovePanel lastMovePanel;
    private List<MovePanel> panels = new List<MovePanel>();

    private void Awake() => board.newTurn += NewTurn;
    private void Start() => Instantiate(movePanelPrefab, collectionContainer);
    private void OnDestroy() => board.newTurn -= NewTurn;

    private void NewTurn(BoardState newState)
    {
        // If the current move is black, we know white just made a move, let's add an entry to the list
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

            lastMovePanel.SetTurnNumber(Mathf.FloorToInt((float)board.turnHistory.Count / 2f) + board.turnHistory.Count % 2);
            lastMovePanel.SetMove(BoardState.GetLastMove(board.turnHistory));
        }
        else
            lastMovePanel?.SetMove(BoardState.GetLastMove(board.turnHistory));
    }

    public void Clear()
    {
        for(int i = panels.Count - 1; i >= 0; i--)
            Destroy(panels[i].gameObject);
        
        panels.Clear();
    }
}