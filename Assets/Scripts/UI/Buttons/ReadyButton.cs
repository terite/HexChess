using UnityEngine;
using UnityEngine.UI;

public class ReadyButton : MonoBehaviour
{
    [SerializeField] public Toggle toggle;
    [SerializeField] private Image image;
    Networker networker;

    public Color uncheckedColor;
    public Color readyColor;

    private void Awake() {
        networker = GameObject.FindObjectOfType<Networker>();
        gameObject.SetActive(!networker.isHost);

        toggle.onValueChanged.AddListener(newVal => {
            MessageType readyMessageType = newVal ? MessageType.Ready : MessageType.Unready;
            networker?.SendMessage(new Message(readyMessageType));
            image.color = newVal ? readyColor : uncheckedColor;
        });
    }
}