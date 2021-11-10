using UnityEngine;
using UnityEngine.UI;

public class FindMatchButton : TwigglyButton
{
    [SerializeField] private Networker networker;
    public string dns;
    private new void Awake()
    {
        base.Awake();
        base.onClick += Clicked;
    }
    public void Clicked() => networker.TryConnectClient(dns, networker.port, true);
}