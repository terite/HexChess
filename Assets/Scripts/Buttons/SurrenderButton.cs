using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SurrenderButton : MonoBehaviour
{
    Board board;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        board.gameOver += gameOver;
        button.onClick.AddListener(() => board.Surrender());
    }

    private void gameOver(Game game)
    {
        text.text = "Reset";
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => board.Reset());
    }

    public void Reset()
    {
        text.text = "Surrender";
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => board.Surrender());
    }
}