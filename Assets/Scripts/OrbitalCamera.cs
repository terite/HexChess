using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class OrbitalCamera : MonoBehaviour
{
    public bool orbiting {get; private set;} = false;
    public float distance;
    Board board;
    SelectPiece selectPiece;
    Transform centerTarget;

    Vector3 defaultPosition;
    Quaternion defaultRotation;

    public float orbitSpeed = 5f;

    private void Awake() {
        board = GameObject.FindObjectOfType<Board>();
        selectPiece = GameObject.FindObjectOfType<SelectPiece>();
    }

    private void Start() {
        // centerTarget = board.GetHexIfInBounds(9, 2).transform;
        // transform.position = centerTarget.position + (Vector3.up * distance);
        // transform.LookAt(centerTarget);
        // defaultRotation = transform.rotation;
        // defaultPosition = transform.position;
    }
    
    public void UpdateDefaultRotation(Quaternion newDefault)
    {
        defaultRotation = newDefault;
        transform.rotation = newDefault;
    }

    private void LateUpdate()
    {
        if(!orbiting)
            return;

        if(selectPiece.selectedPiece != null)
            return;

        // Temporary garbage code

        // Vector2 angleChange = Mouse.current.delta.ReadValue() * orbitSpeed;

        // Vector3 beforePos = transform.position;
        // Quaternion beforeRot = transform.rotation;

        // transform.RotateAround(centerTarget.position, Vector3.right, angleChange.y * Time.deltaTime);
        // if(Vector3.Dot(Vector3.up, (transform.position - centerTarget.position).normalized) < 0)
        // {
        //     transform.position = beforePos;
        //     transform.rotation = beforeRot;
        // }
        // else
        // {
        //     beforePos = transform.position;
        //     beforeRot = transform.rotation;
        // }

        // transform.RotateAround(centerTarget.position, Vector3.forward, angleChange.x * Time.deltaTime);
        // if(Vector3.Dot(Vector3.up, (transform.position - centerTarget.position).normalized) < 0)
        // {
        //     transform.position = beforePos;
        //     transform.rotation = beforeRot;
        // }
        // else
        // {
        //     beforePos = transform.position;
        //     beforeRot = transform.rotation;
        // }

        // transform.rotation = Quaternion.LookRotation(-(transform.position - centerTarget.position).normalized, Vector3.up);

    }

    public void RightClick(CallbackContext context)
    {
        // if(context.started)
        //     orbiting = true;
        // else if(context.canceled)
        // {
        //     orbiting = false;
        //     transform.position = defaultPosition;
        //     transform.rotation = defaultRotation;
        // }
    }

}
