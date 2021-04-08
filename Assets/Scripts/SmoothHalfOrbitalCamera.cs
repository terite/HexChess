using UnityEngine;
using UnityEngine.InputSystem;

public class SmoothHalfOrbitalCamera : MonoBehaviour
{
    SelectPiece selectPiece;
    public float cameraDistance = 18;
    public Vector3 origin;
    public float speed = 0.2f;

    public float transitionTime = 0.25f;

    public Vector2 defaultRotation = Vector2.right * 90;
    Vector3 temp_rotation;

    bool rotating;

    private void OnValidate()
    {
        defaultRotation.x = Mathf.Clamp(defaultRotation.x, 0, 90f);
        defaultRotation.y %= 360f;
        ResetRotation();
        LookTowardsOrigin();
    }

    private void Awake() => selectPiece = GameObject.FindObjectOfType<SelectPiece>();

    private void Start()
    {
        ResetRotation();
    }

    [ContextMenu("Reset Rotation")]
    public void ResetRotation()
    {
        temp_rotation = defaultRotation;
        LookTowardsOrigin();
    }

    public void ChangeDefaultRotation(Team team) 
    {
        defaultRotation = team == Team.White ? Vector2.right * 90 : new Vector2(90, 180);
        ResetRotation();
    } 

    public void RightClick(InputAction.CallbackContext context)
    {
        if(selectPiece.selectedPiece != null)
            return;

        if(context.started)
        {
            rotating = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if(context.canceled)
        {
            rotating = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            rotating = false;
            Cursor.lockState = CursorLockMode.None;
        }
        if(rotating)
        {
            Vector2 delta = Mouse.current.delta.ReadValue() * speed;
            temp_rotation += new Vector3(delta.y, delta.x);

            LookTowardsOrigin();
        }
    }

    public void LookTowardsOrigin()
    {
        temp_rotation.x = Mathf.Clamp(temp_rotation.x, 0, 89.999f);
        temp_rotation.y %= 360f;

        var rot = Quaternion.identity;
        rot *= Quaternion.Euler(temp_rotation * Vector2.one);
        transform.rotation = rot;

        transform.position = origin - transform.forward * cameraDistance;

        transform.LookAt(origin);
    }

}