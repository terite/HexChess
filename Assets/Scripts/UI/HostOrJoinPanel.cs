using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class HostOrJoinPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] public IPTextBox ipInput;
    [SerializeField] private Button closeButton;
    [SerializeField] private GroupFader fader;
    public bool visible => fader.visible;
    [SerializeField] private GameObject state1Container;
    [SerializeField] private GameObject state2Container;

    [SerializeField] private HostButton hostButton;
    [SerializeField] private JoinButton joinButton;

    private void Awake() {
        closeButton.onClick.AddListener(() => Hide());
    }

    private void ReturnToDefault()
    {
        titleText.text = "SELECT";
        state2Container.SetActive(false);
        state1Container.SetActive(true);
        hostButton.SetNorm();
        joinButton.SetNorm();
    }

    public void Show()
    {
        ReturnToDefault();
        fader.FadeIn();
    }

    public void Hide()
    {
        fader.FadeOut();
        ipInput.SetIP("");

        // Reset all twigglybuttons to norm
        var allTwigglyButtons = Resources.FindObjectsOfTypeAll<TwigglyButton>().ToList();
        foreach(var tb in allTwigglyButtons)
            tb.SetNorm();
    }

    public void EscapedInput(CallbackContext context)
    {
        if(!context.performed || !fader.visible)
            return;
        
        Hide();
    }

}