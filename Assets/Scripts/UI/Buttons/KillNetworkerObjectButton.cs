using UnityEngine;

public class KillNetworkerObjectButton : MonoBehaviour, IObjectButton
{
    [SerializeField] private Networker networker;
    public void Click()
    {
        if(networker != null)
            Destroy(networker.gameObject);
    }
}