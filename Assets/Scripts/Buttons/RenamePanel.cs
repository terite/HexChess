using UnityEngine;

public class RenamePanel : MonoBehaviour
{
    public void Rename(string newName)
    {
        Networker networker = GameObject.FindObjectOfType<Networker>();
        networker?.UpdateName(newName);
    }
}
