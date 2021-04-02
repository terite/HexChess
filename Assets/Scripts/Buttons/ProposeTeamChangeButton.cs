using UnityEngine;
using UnityEngine.UI;

public class ProposeTeamChangeButton : MonoBehaviour
{
    [SerializeField] private Button button;
    Networker networker;
    private void Awake() {
        networker = GameObject.FindObjectOfType<Networker>();
        button.onClick.AddListener(() => {
            if(!networker.isHost)
            {
                ReadyButton ready = GameObject.FindObjectOfType<ReadyButton>();
                if(ready != null && ready.toggle.isOn)
                    ready.toggle.isOn = false;
            }
            
            networker?.ProposeTeamChange();
        });
    }
}