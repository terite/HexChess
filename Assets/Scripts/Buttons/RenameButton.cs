using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RenameButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_InputField inputField;

    private void Awake()
    {
        button.onClick.AddListener(() => {
            if(string.IsNullOrEmpty(inputField.text))
                return;
                
            Networker networker = GameObject.FindObjectOfType<Networker>();
            networker?.UpdateName(inputField.text);
        });
    }
}