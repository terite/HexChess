using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurrenderButton : MonoBehaviour
{
    Board board;
    [SerializeField] private Button button;
    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        button.onClick.AddListener(() => board.Surrender());
    }
}
