using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButton : MonoBehaviour
{
    [SerializeField] public Toggle toggle;
    [SerializeField] private Image image;
    [SerializeField] private AudioUI audioUI;
    [SerializeField] private TextMeshProUGUI readyButtonContextText;
    Networker networker;

    public Color uncheckedColor;
    public Color readyColor;

    private void Awake() {
        networker = GameObject.FindObjectOfType<Networker>();

        toggle.onValueChanged.AddListener(newVal => {
            MessageType readyMessageType = newVal ? MessageType.Ready : MessageType.Unready;
            networker?.SendMessage(new Message(readyMessageType));
            image.color = newVal ? readyColor : uncheckedColor;
            readyButtonContextText.text = newVal ? "Waiting for opponent to start match" : "";
        });
    }

    public void Hide()
    {
        if(toggle.isOn)
            toggle.isOn = false;
        audioUI.canPlay = false;
        toggle.interactable = false;
    }

    public void Show()
    {
        audioUI.canPlay = true;
        toggle.interactable = true;
    }
}