using UnityEngine;
using UnityEngine.InputSystem;

public class SmoothHalfOrbitalCamera : MonoBehaviour
{
    // This is how I do it, though if you rather look for it at start than have Unity find and assign it in the editor, delete this line
    // and uncomment awake.
    [SerializeField] [HideInInspector] SelectPiece selectPiece;
    public Team team = Team.White;
    public float cameraDistance = 18;
    public Vector3 origin;
    public float speed = 0.2f;
    
    public Vector2 defaultRotation = Vector2.right * 90;
    public float cameraResetTime = 0.5f;
    // Kind of like rubberbanding so that smaller rotations don't take the same amount of time as big ones
    public float minimumRotationMagnitude = 100f;

    Vector3 temp_rotation;
    bool rotating;

    float adjustedResetTime;
    float nomalizedElaspedTime;
    Vector2 release_rotation;

    private void OnValidate()
    {
        selectPiece = FindObjectOfType<SelectPiece>();
        defaultRotation.x = Mathf.Clamp(defaultRotation.x, 0, 90f);
        defaultRotation.y %= 360f;
        ResetRotation();
        LookTowardsOrigin();
        ChangeDefaultRotation(team);
    }

    //private void Awake() => selectPiece = FindObjectOfType<SelectPiece>();

    private void Start()
    {
        ResetRotation();
    }

    [ContextMenu("Reset Rotation")]
    public void ResetRotation()
    {
        temp_rotation = defaultRotation;
        StopRotating();
        LookTowardsOrigin();
    }

    public void ChangeDefaultRotation(Team team)
    {
        defaultRotation = team == Team.White ? Vector2.right * 90 : new Vector2(90, 180);
        ResetRotation();
    }

    public void ToggleTeam()
    {
        team = team switch
        {
            Team.None => Team.White,
            Team.White => Team.Black,
            Team.Black => Team.White,
            _ => throw new System.NotSupportedException($"Team {team} not supported"),
        };
        ChangeDefaultRotation(team);
    }

    public void RightClick(InputAction.CallbackContext context)
    {
        if(selectPiece.selectedPiece != null)
            return;

        if(context.started)
            StartRotating();
        else if(context.canceled)
            StopRotating();
    }

    void StartRotating()
    {
        nomalizedElaspedTime = 0;
        rotating = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void StopRotating()
    {
        rotating = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        release_rotation = temp_rotation;
        float delta = (defaultRotation - release_rotation).magnitude / minimumRotationMagnitude;
        if(delta >= 1)
            adjustedResetTime = cameraResetTime;
        else
            adjustedResetTime = cameraResetTime * delta;
    }

    void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
            StopRotating();
        else if(rotating)
        {
            Vector2 delta = Mouse.current.delta.ReadValue() * speed;
            temp_rotation += new Vector3(delta.y, delta.x);

            LookTowardsOrigin();
        }
        else
        {
            if(nomalizedElaspedTime < 1)
            {
                temp_rotation = Vector3.Slerp(release_rotation, defaultRotation, nomalizedElaspedTime);
                nomalizedElaspedTime += Time.deltaTime / adjustedResetTime;
            }

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