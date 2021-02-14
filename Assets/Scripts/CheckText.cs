using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CheckText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    Board board;

    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        board.newTurn += NewTurn;
        gameObject.SetActive(false);
    }

    private void NewTurn(BoardState newState)
    {
        if(newState.check == Team.None && newState.checkmate == Team.None)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        if(newState.check > 0)
        {
            text.color = Color.yellow;
            text.text = "Check";
        }
        else if(newState.checkmate > 0)
        {
            text.color = Color.red;
            text.text = "Checkmate";
        }
        else
            text.text = "Something went wrong";
    }
}
