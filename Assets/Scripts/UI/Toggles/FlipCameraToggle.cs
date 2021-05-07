using UnityEngine;
using UnityEngine.UI;

public class FlipCameraToggle : MonoBehaviour
{
    public Toggle toggle;
    [SerializeField] private Image image;
    public Color uncheckedColor;
    public Color readyColor;

    private void Awake()
    {
        toggle.isOn = true;
        image.color = readyColor;

        toggle.onValueChanged.AddListener(newVal => image.color = newVal ? readyColor : uncheckedColor);
    }
}
