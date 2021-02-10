using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromotionDialogue : MonoBehaviour
{
    [SerializeField] private Button rookButton;
    [SerializeField] private Button knightButton;
    [SerializeField] private Button bishopButton;
    [SerializeField] private Button squireButton;
    [SerializeField] private Button queenButton;
    IEnumerable<(Button, PieceType)> GetButtons()
    {
        yield return (rookButton, PieceType.QueensRook);
        yield return (knightButton, PieceType.QueensKnight);
        yield return (bishopButton, PieceType.QueensBishop);
        yield return (squireButton, PieceType.BlackSquire);
        yield return (queenButton, PieceType.Queen);
    }

    private void Awake() => gameObject.SetActive(false);

    public void Display(Action<PieceType> callback)
    {
        gameObject.SetActive(true);
        
        foreach((Button button, PieceType type) in GetButtons())
        {
            button.onClick.RemoveAllListeners();
            
            button.onClick.AddListener(() => {
                callback.Invoke(type);
                gameObject.SetActive(false);
            });
        }
    }
}   