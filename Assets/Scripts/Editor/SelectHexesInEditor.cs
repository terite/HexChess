using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using Extensions;

[InitializeOnLoad]
public static class SelectHexesInEditor 
{
    static List<Hex> lastSelectedHexes = new List<Hex>();

    static SelectHexesInEditor()
    {
        Selection.selectionChanged += () => {
            List<Hex> currentHexes = Selection.GetFiltered<Hex>(SelectionMode.Unfiltered).ToList();
            
            // Deselect/remove hexes from lastSelectedHexes that are not in currentSelected
            List<Hex> toRemove = new List<Hex>();
            foreach(Hex hex in lastSelectedHexes)
            {
                if(!currentHexes.Contains(hex))
                {
                    // hex.ToggleSelect();
                    toRemove.Add(hex);
                }
            }

            if(toRemove.Count > 0)
                foreach(Hex hex in toRemove)
                    lastSelectedHexes.Remove(hex);

            // Select/Add hexes to lastSeletedHexes that are in currentSelection
            foreach(Hex hex in currentHexes)
            {
                if(!lastSelectedHexes.Contains(hex))
                {
                    lastSelectedHexes.Add(hex);
                    // hex.ToggleSelect();
                }
            }
        };
    }
}
