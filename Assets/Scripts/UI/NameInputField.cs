using UnityEngine;
using TMPro;

public class NameInputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TextMeshProUGUI nameErrorStatesText;
    Networker networker;
    private void Awake()
    {
        networker = GameObject.FindObjectOfType<Networker>();
        input.text = PlayerPrefs.GetString("PlayerName", "GUEST");
        
        // should the error state text be on?
        nameErrorStatesText.gameObject.SetActive(string.IsNullOrEmpty(input.text) || input.text.ToUpper() == "GUEST");

        input.onValueChanged.AddListener(newVal => {
            // should the error state text be on?
            string newName = newVal.ToUpper();
            nameErrorStatesText.gameObject.SetActive(string.IsNullOrEmpty(newVal) || newName == "GUEST");
            PlayerPrefs.SetString("PlayerName", newName);
            networker.UpdateName(newName);
        });
    }
}