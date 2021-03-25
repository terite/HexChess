using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectButton : MonoBehaviour
{
    [SerializeField] private Networker networker;
    [SerializeField] private Button button;
    [SerializeField] private TMP_InputField ipInput;
    private void Awake() => button.onClick.AddListener(() => networker.TryConnectClient(ipInput.text, 8080));
}