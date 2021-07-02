using System;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BoardSensorComponent : SensorComponent
{
    private ISensor[] sensors;
    public Board board;

    public override ISensor[] CreateSensors()
    {
        Board board = GameObject.FindObjectOfType<Board>();
        if(board == null)
            return Array.Empty<ISensor>();
        
        return new BoardSensor[1]{new BoardSensor(board)};
    }
}
