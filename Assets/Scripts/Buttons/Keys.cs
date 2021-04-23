using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keys : MonoBehaviour
{
    [SerializeField] private GameObject whiteKeys;
    [SerializeField] private GameObject blackKeys;

    public void SetKeys(Team team)
    {
        whiteKeys.SetActive(team == Team.White);
        blackKeys.SetActive(team == Team.Black);
    }
}
