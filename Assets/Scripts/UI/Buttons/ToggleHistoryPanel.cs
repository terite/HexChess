using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleHistoryPanel : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button button;
    [SerializeField] private RectTransform triangleImage;

    public float openY;
    public float closeY;

    public float transitionTime;
    private float ellapsedTime = 0;

    bool transitioning = false;
    TransitionMode mode = TransitionMode.In;

    private void Awake()
    {
        button.onClick.AddListener(() => {
            mode = mode == TransitionMode.In ? TransitionMode.Out : TransitionMode.In;
            transitioning = true;
            triangleImage.rotation = mode == TransitionMode.In ? Quaternion.Euler(0,0,0) : Quaternion.Euler(180,0,0);
        });
    }

    private void Update()
    {
        if(transitioning)
        {
            ellapsedTime += Time.deltaTime;
            float startPos = mode == TransitionMode.In ? closeY : openY;
            float endPos = mode == TransitionMode.In ? openY : closeY;
            panel.position = new Vector3(panel.position.x, Mathf.Lerp(startPos, endPos, Mathf.Clamp01(ellapsedTime/transitionTime)), panel.position.z);

            if(panel.position.y == endPos)
            {
                ellapsedTime = 0;
                transitioning = false;
            }
        }
    }
}
