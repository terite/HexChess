using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public Sprite on;
    public Sprite off;
    public Sprite hover;
    public delegate void OnValueChanged(bool isOn);
    public OnValueChanged onValueChanged;
    public bool isOn {get; private set;} = false;
    private bool hovered = false;
    private void Start() => image.sprite = isOn ? on : off;
    public void OnPointerClick(PointerEventData eventData)
    {
        if(isOn)
            return;
        
        Toggle(true);
    }

    public void Toggle(bool val)
    {
        if(isOn == val)
            return;
        isOn = val;
        image.sprite = isOn ? on : off;
        onValueChanged?.Invoke(isOn);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        image.sprite = hover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        image.sprite = isOn ? on : off;
    }
}
