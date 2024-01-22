using UnityEditor;
using UnityEngine;

public class EditorNotes : EditorWindow
{
    private string notesContent = "";
    private string lastSavedNotesContent = "";
    private Vector2 scrollPosition;

    // Add menu named "EditorNotes" to the BaaWolf menu
    [MenuItem("BaaWolf/EditorNotes")]
    public static void ShowWindow()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return; // Don't show window if Unity is in play mode or about to enter play mode
        }

        // Creates a new dockable window or focus if it's already open.
        EditorWindow.GetWindow<EditorNotes>("Notes");
    }

    void OnEnable()
    {
        // Load previously saved notes
        notesContent = EditorPrefs.GetString("MyUnityNotes", "");
        lastSavedNotesContent = notesContent;
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

        // Text area for notes
        notesContent = EditorGUILayout.TextArea(notesContent, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();

        // Button section
        EditorGUILayout.BeginHorizontal();

        // Save button
        if (GUILayout.Button("Save Notes"))
        {
            SaveNotes();
        }

        // Revert button
        if (GUILayout.Button("Revert"))
        {
            RevertNotes();
        }

        EditorGUILayout.EndHorizontal();
    }

    void SaveNotes()
    {
        // Save notes using EditorPrefs
        EditorPrefs.SetString("MyUnityNotes", notesContent);
        lastSavedNotesContent = notesContent;
        Debug.Log("Notes Saved!");
    }

    void RevertNotes()
    {
        // Revert notes to last saved content
        notesContent = lastSavedNotesContent;

        // Indicate that GUI has changed
        GUI.changed = true;

        // Force focus to a null control, making TextArea lose focus and potentially update its content
        GUI.FocusControl(null);
    }

    void OnDisable()
    {
        // Automatically save when window is closed
        SaveNotes();
    }
}
