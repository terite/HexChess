using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    Board board;
    [SerializeField] private Button button;

    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        button.onClick.AddListener(() => board.Reset());
    }
}
