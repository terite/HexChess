using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectButton : TwigglyButton
{
    [SerializeField] private Networker networker;
    [SerializeField] private IPTextBox ipInput;
    private new void Awake()
    {
        base.Awake();
        base.onClick += Clicked;
    } 
    public void Clicked()
    {
        if(!networker.attemptingConnection)
            networker.TryConnectClient(ipInput.IP, networker.port);
    }
}