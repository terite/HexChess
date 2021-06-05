using Unity.MLAgents;
using UnityEngine;

public class HexamasterAcademy : MonoBehaviour
{
    private void Awake() => Academy.Instance.OnEnvironmentReset += EnvironmentReset;

    private void EnvironmentReset()
    {
        // Reset scene
    }
}