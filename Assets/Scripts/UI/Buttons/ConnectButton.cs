using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectButton : MonoBehaviour
{
    [SerializeField] private Networker networker;
    [SerializeField] private Button button;
    [SerializeField] private TMP_InputField ipInput;
    private void Awake() 
    {

        button.onClick.AddListener(() => {
            if(!networker.attemptingConnection)
                networker.TryConnectClient(ipInput.text, networker.port);
        });
    } 
}