using UnityEngine;

// Replaces the system mouse cursor with a gold wedge pointer with stepped-pyramid tail with a blue outline at startup.
public static class BookCursor
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Apply()
    {
        Texture2D texture = Resources.Load<Texture2D>("Cursor/BookCursor");
        if (texture == null) return;
        Cursor.SetCursor(texture, new Vector2(3f, 3f), CursorMode.Auto);
    }
}
