using UnityEngine;
using UnityEngine.UI;

public class HostButton : TwigglyButton
{
    [SerializeField] private Networker networker;
    private new void Awake()
    {
        base.Awake();
        base.onClick += Clicked;
    }
    public void Clicked() => networker.Host();
}