using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SurrenderButton : MonoBehaviour
{
    Board board;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button resetButtonPrefab;
    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        button.onClick.AddListener(() => board.Surrender());
        board.gameOver += gameOver;
    }

    private void gameOver(Game game)
    {
        board.gameOver -= gameOver;
        button.onClick.RemoveAllListeners();
        Button resetButton = Instantiate(resetButtonPrefab, transform.parent);
        resetButton.transform.SetSiblingIndex(transform.GetSiblingIndex());
        Destroy(gameObject);
        // text.text = "Reset";
        // button.onClick.RemoveAllListeners();
        // button.onClick.AddListener(() => board.Reset());
    }

    // public void Reset()
    // {
    //     text.text = "Surrender";
    //     button.onClick.RemoveAllListeners();
    //     button.onClick.AddListener(() => board.Surrender());
    // }
}