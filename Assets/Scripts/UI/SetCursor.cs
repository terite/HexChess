using UnityEditor;
using UnityEngine;

public class SetCursor : MonoBehaviour
{
    public Texture2D cursor;
    private void Start() {
        Cursor.SetCursor(cursor, new Vector2(9, 2), CursorMode.Auto);
    }
}