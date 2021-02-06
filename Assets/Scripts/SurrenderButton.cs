using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurrenderButton : MonoBehaviour
{
    BoardManager boardManager;
    [SerializeField] private Button button;
    private void Awake() {
        boardManager = GameObject.FindObjectOfType<BoardManager>();
        button.onClick.AddListener(() => boardManager.Surrender());
    }
}
