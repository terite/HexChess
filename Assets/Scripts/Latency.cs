using UnityEngine;
using TMPro;

public class Latency : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI latencyText;

    public void UpdateLatency(float latency) => latencyText.text = string.Format("{0:0.00}", latency) + " ms";
}