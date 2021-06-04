using UnityEngine;

public class CopyIPButton : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button button;
    private void Awake() => button.onClick.AddListener(() => GUIUtility.systemCopyBuffer = Networker.GetPublicIPAddress());
}
