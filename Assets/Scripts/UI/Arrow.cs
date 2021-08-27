using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    [SerializeField] private LineRenderer tail;
    [SerializeField] private Transform head;
    public void Init(Vector3 from, Vector3 to, Color color)
    {
        float pos0Offset = tail.GetPosition(0).magnitude;
        float pos1Offset = tail.GetPosition(1).magnitude;
        
        Vector3 forwardDir = (to - from).normalized;
        Vector3 backwardsVec = from - to;
        head.forward = forwardDir;
        transform.position = to;

        tail.SetPosition(0, backwardsVec.normalized * pos0Offset);
        tail.SetPosition(1, backwardsVec.normalized * pos1Offset);
        tail.SetPosition(tail.positionCount - 1, backwardsVec);

        foreach(LineRenderer renderer in lineRenderers)
        {
            renderer.startColor = color;
            renderer.endColor = color;
        }
    }
}