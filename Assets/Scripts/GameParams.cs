using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameParams
{
    public Team localTeam;
    public bool showMovePreviews;

    public GameParams(Team localTeam, bool showMovePreviews)
    {
        this.localTeam = localTeam;
        this.showMovePreviews = showMovePreviews;
    }
}