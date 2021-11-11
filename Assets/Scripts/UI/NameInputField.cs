using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class NameInputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TextMeshProUGUI nameErrorStatesText;
    private void Awake()
    {
        input.text = PlayerPrefs.GetString("PlayerName", "GUEST");
        
        // should the error state text be on?
        nameErrorStatesText.gameObject.SetActive(string.IsNullOrEmpty(input.text) || input.text.ToUpper() == "GUEST");

        input.onValueChanged.AddListener(newVal => {
            // should the error state text be on?
            nameErrorStatesText.gameObject.SetActive(string.IsNullOrEmpty(newVal) || newVal.ToUpper() == "GUEST");
            PlayerPrefs.SetString("PlayerName", newVal.ToUpper());
        });
    }
}