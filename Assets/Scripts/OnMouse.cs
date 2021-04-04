using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class OnMouse : MonoBehaviour
{
    [ReadOnly, ShowInInspector] public GameObject pickedUp {get; private set;}
    Camera cam;
    public float distance;
    public Color currentColor {get; private set;}
    
    private void Awake() => cam = Camera.main;

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        transform.position = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));
    }

    public void PickUp(GameObject toPickup) 
    {
        pickedUp = Instantiate(toPickup, transform.position, Quaternion.identity, transform);
        IPiece piece = pickedUp.GetComponent<IPiece>();
        piece?.DestroyScript();
    } 

    public void PutDown() => Destroy(pickedUp);

    public void SetColor(Color color)
    {
        if(pickedUp == null)
            return;
        currentColor = color;
        MeshRenderer renderer = pickedUp.GetComponentInChildren<MeshRenderer>();
        renderer.material.SetColor("_BaseColor", color);
    }
}