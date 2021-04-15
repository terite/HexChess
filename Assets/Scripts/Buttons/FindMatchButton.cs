using UnityEngine;
using UnityEngine.UI;

public class FindMatchButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Networker networker;
    public string dns;
    private void Awake() => 
        button.onClick.AddListener(() => networker.TryConnectClient(dns, networker.port));
}