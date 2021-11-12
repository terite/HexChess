using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Graphic targetGraphic;
    public Color onColor;
    public Color offColor;
    public Color hoverColor;
    public Color pressedColor;
    public delegate void OnValueChanged(bool isOn);
    public OnValueChanged onValueChanged;
    public bool isOn {get; private set;} = false;
    private bool hovered = false;
    private void Start() => targetGraphic.color = isOn ? onColor : offColor;
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
        targetGraphic.color = isOn ? onColor : offColor;
        onValueChanged?.Invoke(isOn);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        targetGraphic.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        targetGraphic.color = isOn ? onColor : offColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetGraphic.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(hovered)
            targetGraphic.color = hoverColor;
        else
            targetGraphic.color = isOn ? onColor : offColor;
    }
}
