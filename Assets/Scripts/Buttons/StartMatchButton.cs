using UnityEngine;
using UnityEngine.UI;

public class StartMatchButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup group;
    private void Awake()
    {
        HideButton();

        button.onClick.AddListener(() =>
        {
            Networker networker = GameObject.FindObjectOfType<Networker>();
            if(networker != null && networker.clientIsReady && networker.isHost)
                networker.HostMatch();
        });
    }

    public void HideButton()
    {
        group.alpha = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public void ShowButton()
    {
        group.alpha = 1;
        group.interactable = true;
        group.blocksRaycasts = true;
    }
}