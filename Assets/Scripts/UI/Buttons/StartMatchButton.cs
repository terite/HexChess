using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMatchButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Lobby lobby;
    private void Awake()
    {
        HideButton();

        button.onClick.AddListener(() =>
        {
            Networker networker = GameObject.FindObjectOfType<Networker>();
            if(networker != null && networker.clientIsReady && networker.isHost)
                networker.HostMatch();
            // If networker is null, we should load into an AI match with whatever AI settings the player set
            else if(networker == null)
                lobby.LoadAIGame();
        });
    }

    public void HideButton()
    {
        group.alpha = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public void ShowEnabledButton()
    {
        group.alpha = 1;
        group.interactable = true;
        group.blocksRaycasts = true;
    }
    public void ShowDisabledButton()
    {
        group.alpha = 1;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}