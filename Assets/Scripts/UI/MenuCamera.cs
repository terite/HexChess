using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class MenuCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private LiftOnHover lastHovered;
    private void Update() {
        if(Physics.Raycast(cam.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit, 100))
        {
            if(hit.collider == null)
            {
                if(lastHovered != null)
                {
                    lastHovered.Reset();
                    lastHovered = null;
                }
                return;
            }
            
            if(hit.collider.gameObject.TryGetComponent<LiftOnHover>(out LiftOnHover hithoverer))
            {
                if(hithoverer != lastHovered)
                {
                    lastHovered?.Reset();
                    hithoverer.Lift();
                    lastHovered = hithoverer;
                }
            }
            else if(lastHovered != null)
            {
                lastHovered.Reset();
                lastHovered = null;
            }
        }
        else if(lastHovered != null)
        {
            lastHovered.Reset();
            lastHovered = null;
        }
    }
    public void MouseClick(CallbackContext context)
    {
        if(!context.performed)
            return;
        
        if(lastHovered != null)
        {
            if(lastHovered.gameObject.TryGetComponent<IObjectButton>(out IObjectButton button))
                button.Click();
        }
    }
}