using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class IPTextBox : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    public string IP {get; private set;}

    private void Awake() {
        ipInputField.onValueChanged.AddListener(newIP => {
            if(!newIP.Contains("*"))
                IP = newIP;
        });
        ipInputField.onEndEdit.AddListener(newIP => {
            IP = newIP;
            ipInputField.text = Regex.Replace(IP, "[a-f0-9]", "*");
        });
    }
    
    public void SetIP(string newIP)
    {
        IP = newIP;
        ipInputField.text = Regex.Replace(IP, "[a-f0-9]", "*");
    }
}