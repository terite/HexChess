using UnityEngine;
using UnityEngine.UI;

public class HostButton : MonoBehaviour
{
    [SerializeField] private Networker networker;
    [SerializeField] private Button button;
    private void Awake() => button.onClick.AddListener(() => networker.Host());
}