using Extensions;
using UnityEngine;
using UnityEngine.UI;

public class PreviewMovesToggle : MonoBehaviour
{
    public Toggle toggle;
    [SerializeField] private Image image;
    Networker networker;
    public Color uncheckedColor;
    public Color readyColor;

    private void Awake() {
        toggle.isOn = PlayerPrefs.GetInt("PreviewMoves", 1).IntToBool();
        image.color = toggle.isOn ? readyColor : uncheckedColor;

        networker = GameObject.FindObjectOfType<Networker>();
        
        toggle.onValueChanged.AddListener(newVal => {
            PlayerPrefs.SetInt("PreviewMoves", newVal.BoolToInt());

            MessageType previewMovesType = newVal ? MessageType.PreviewMovesOn : MessageType.PreviewMovesOff;
            networker?.SendMessage(new Message(previewMovesType));
            image.color = newVal ? readyColor : uncheckedColor;
        });
    }
}