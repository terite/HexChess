using Extensions;
using UnityEngine;
using UnityEngine.UI;

public class HandicapOverlayToggle : MonoBehaviour
{
    public Toggle toggle;
    [SerializeField] private Image image;
    Networker networker;
    public Color uncheckedColor;
    public Color readyColor;

    private void Awake() {
        toggle.isOn = PlayerPrefs.GetInt("HandicapOverlay", 1).IntToBool();
        image.color = toggle.isOn ? readyColor : uncheckedColor;

        networker = GameObject.FindObjectOfType<Networker>();
        
        toggle.onValueChanged.AddListener(newVal => {
            PlayerPrefs.SetInt("HandicapOverlay", newVal.BoolToInt());

            MessageType handicapOverlayType = newVal ? MessageType.HandicapOverlayOn : MessageType.HandicapOverlayOff;
            networker?.SendMessage(new Message(handicapOverlayType));
            image.color = newVal ? readyColor : uncheckedColor;
        });
    }
}