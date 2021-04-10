using UnityEngine;
using UnityEngine.InputSystem;

public class SmoothHalfOrbitalCamera : MonoBehaviour
{
    readonly float hardScrollModifier = 0.01f;
    [SerializeField] [HideInInspector] SelectPiece selectPiece;
    public Team team = Team.White;
    public float defaultScroll = 18;
    public float minScroll = 18;
    public float maxScroll = 21;
    public float scrollModifier = 0.5f;
    public Vector3 origin;
    public float speed = 0.2f;

    public Vector2 defaultRotation = Vector2.right * 90;
    public float cameraResetTime = 0.5f;
    // Kind of like rubberbanding so that smaller rotations don't take the same amount of time as big ones
    public float minimumRotationMagnitude = 100f;

    Vector3 temp_rotation;
    bool rotating;

    float scroll;
    float adjustedResetTime;
    float nomalizedElaspedTime;
    float released_scroll;
    Vector2 release_rotation;

    public bool IsSandboxMode { get; private set; }

    private void OnValidate()
    {
        selectPiece = FindObjectOfType<SelectPiece>();
        defaultRotation.x = Mathf.Clamp(defaultRotation.x, 0, 90f);
        defaultRotation.y %= 360f;
        ResetRotation();
        LookTowardsOrigin();
        SetDefaultTeam(team);
    }

    private void Start()
    {
        IsSandboxMode = !FindObjectOfType<Multiplayer>();
        scroll = defaultScroll;
        ResetRotation();
    }

    [ContextMenu("Reset Rotation")]
    public void ResetRotation()
    {
        temp_rotation = defaultRotation;
        StopRotating(); 
        LookTowardsOrigin();
    }

    public void SetDefaultTeam(Team team)
    {
        this.team = team;
        defaultRotation = team == Team.White ? Vector2.right * 90 : new Vector2(90, 180);
    }

    public void ToggleTeam()
    {
        if(rotating)
            return;

        team = team switch
        {
            Team.None => Team.White,
            Team.White => Team.Black,
            Team.Black => Team.White,
            _ => throw new System.NotSupportedException($"Team {team} not supported"),
        };
        SetDefaultTeam(team);
        StopRotating();
    }

    public void OnSpacebar(InputAction.CallbackContext context)
    {
        if(context.started && IsSandboxMode)
            ToggleTeam();
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
        nomalizedElaspedTime = 0;
        float delta = (defaultRotation - release_rotation).magnitude / minimumRotationMagnitude;
        if(delta >= 1)
            adjustedResetTime = cameraResetTime;
        else
            adjustedResetTime = cameraResetTime * delta;
        released_scroll = scroll;
    }

    void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
            StopRotating();
        else if(rotating)
        {
            scroll -= Mouse.current.scroll.ReadValue().y * scrollModifier * hardScrollModifier;
            scroll = Mathf.Clamp(scroll, minScroll, maxScroll);

            Vector2 delta = Mouse.current.delta.ReadValue() * speed;
            temp_rotation += new Vector3(delta.y, delta.x);

            if(!IsSandboxMode)
                switch(team)
                {
                    case Team.White:
                        temp_rotation.y = Mathf.Clamp(temp_rotation.y, -90, 90);
                        break;
                    case Team.Black:
                        temp_rotation.y = Mathf.Clamp(temp_rotation.y, 90, 270);
                        break;
                }

            LookTowardsOrigin();
        }
        else
        {
            if(nomalizedElaspedTime < 1)
            {
                temp_rotation = Vector3.Slerp(release_rotation, defaultRotation, nomalizedElaspedTime);
                scroll = Mathf.Clamp(Mathf.Lerp(released_scroll, defaultScroll, nomalizedElaspedTime), minScroll, maxScroll);

                nomalizedElaspedTime += Time.deltaTime / adjustedResetTime;
            }
            else
            {
                temp_rotation = defaultRotation;
                scroll = defaultScroll;
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

        transform.position = origin - transform.forward * scroll;

        transform.LookAt(origin);
    }

}