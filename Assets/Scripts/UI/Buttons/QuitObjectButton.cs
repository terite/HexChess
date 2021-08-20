using UnityEngine;

public class QuitObjectButton : MonoBehaviour, IObjectButton
{
    public void Click()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}