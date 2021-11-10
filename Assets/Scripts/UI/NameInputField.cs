using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class NameInputField : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TextMeshProUGUI nameErrorStatesText;
    VirtualCursor virtualCursor;
    private void Awake()
    {
        virtualCursor = GameObject.FindObjectOfType<VirtualCursor>();
        input.text = PlayerPrefs.GetString("PlayerName", "GUEST");
        
        // should the error state text be on?
        nameErrorStatesText.gameObject.SetActive(string.IsNullOrEmpty(input.text) || input.text.ToUpper() == "GUEST");

        input.onValueChanged.AddListener(newVal => {
            // should the error state text be on?
            nameErrorStatesText.gameObject.SetActive(string.IsNullOrEmpty(newVal) || newVal.ToUpper() == "GUEST");
            PlayerPrefs.SetString("PlayerName", newVal.ToUpper());
        });
    }

    public void OnPointerEnter(PointerEventData eventData) => virtualCursor?.SetCursor(CursorType.Pencil);

    public void OnPointerExit(PointerEventData eventData) => virtualCursor?.SetCursor(CursorType.Default);
}