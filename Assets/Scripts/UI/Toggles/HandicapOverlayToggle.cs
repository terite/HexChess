using Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandicapOverlayToggle : MonoBehaviour
{
    public Toggle toggle;
    Networker networker;
    public Color uncheckedColor;
    public Color readyColor;

    private void Awake()
    {
        toggle.isOn = PlayerPrefs.GetInt("HandicapOverlay", 1).IntToBool();
        UpdateColors(toggle.isOn);

        networker = GameObject.FindObjectOfType<Networker>();

        toggle.onValueChanged.AddListener(newVal =>
        {
            PlayerPrefs.SetInt("HandicapOverlay", newVal.BoolToInt());

            MessageType handicapOverlayType = newVal ? MessageType.HandicapOverlayOn : MessageType.HandicapOverlayOff;
            networker?.SendMessage(new Message(handicapOverlayType));
            UpdateColors(newVal);
            EventSystem.current.Deselect();
        });
    }

    private void UpdateColors(bool isOn)
    {
        ColorBlock block = toggle.colors;
        block.normalColor = isOn ? readyColor : uncheckedColor;
        block.disabledColor = block.normalColor;
        block.selectedColor = block.normalColor;
        toggle.colors = block;
    }
}