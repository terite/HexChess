using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LiftOnHover : MonoBehaviour
{
    [SerializeField] private Hex hex;
    public Color orangeColor;
    private Color orgColor;
    public float raiseDuration = 0.333f;
    public float power = 0.333f;
    private float ellapsedDuration;
    private bool transitioning = false;
    private TransitionMode mode;

    float from = 1f;
    float to = 3f;

    private void Update()
    {
        if(transitioning)
        {
            float goal = mode == TransitionMode.In ? to : from;
            float t = Mathf.Clamp01(ellapsedDuration/raiseDuration);
            
            float pow = Mathf.Lerp(0, power, t);
            hex.SetHighlightPower(pow);

            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(from, to, t), transform.localPosition.z);
            
            if(transform.localPosition.y == goal)
            {
                transitioning = false;
                ellapsedDuration = 0;
            } 
            else
            {
                int mod = mode == TransitionMode.In ? 1 : -1;
                ellapsedDuration += Time.deltaTime * mod;
            }
        }
    }

    public void Lift()
    {
        // y = toY;
        transitioning = true;
        mode = TransitionMode.In;
        orgColor = hex.GetOutlineColor();
        hex.SetOutlineColor(orangeColor);
    }

    public void Reset()
    {
        // y = fromY;
        transitioning = true;
        if(ellapsedDuration == 0)
            ellapsedDuration = raiseDuration;
        mode = TransitionMode.Out;
        hex.SetOutlineColor(orgColor);
    }
}