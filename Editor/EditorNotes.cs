﻿using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// A simple note-taking editor window for writing, saving, and loading text notes in the Unity Editor.
/// Supports automatic recovery across playmode reloads.
/// </summary>
public class OpenDocs : EditorWindow
{
    private const string DefaultFileName = "note.txt";

    [SerializeField] private string serializedNotesContent = "";
    [SerializeField] private string serializedCurrentFilePath = "";
    [SerializeField] private bool serializedHasSavedPath = false;

    private string notesContent = "";
    private string currentFilePath = "";
    private bool hasSavedPath = false;

    private TextField textField;
    private Button saveButton;
    private ScrollView scrollView;

    private int previousCursorIndex = -1;

    /// <summary>
    /// Opens a folder in the system file explorer.
    /// </summary>
    private static void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
#if UNITY_EDITOR_WIN
            Process.Start("explorer.exe", folderPath.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX
            Process.Start("open", folderPath);
#elif UNITY_EDITOR_LINUX
            Process.Start("xdg-open", folderPath);
#else
            Debug.LogError("Unsupported platform.");
#endif
        }
        else
        {
            Debug.LogError($"Folder not found: {folderPath}");
        }
    }

    /// <summary>
    /// Returns the default documentation folder path.
    /// </summary>
    private static string GetDocumentationFolderPath()
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Documentation");
    }

    [MenuItem("BaaWolf/Editor Notes/✏️ Notes Tab")]
    public static void ShowWindow()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        GetWindow<OpenDocs>("Notes");
    }

    [MenuItem("BaaWolf/Editor Notes/📂 Open Documentation Folder")]
    public static void OpenDocumentsFolderInExplorer()
    {
        string docPath = GetDocumentationFolderPath();
        if (!Directory.Exists(docPath))
        {
            Debug.LogWarning($"Documentation folder not found: {docPath}");
        }
        OpenFolder(docPath);
    }

    [MenuItem("BaaWolf/Editor Notes/📂 Create Documentation Folder")]
    public static void CreateDocumentationFolder()
    {
        string docPath = GetDocumentationFolderPath();
        if (!Directory.Exists(docPath))
        {
            Directory.CreateDirectory(docPath);
            Debug.Log($"Documentation folder created: {docPath}");
        }
        else
        {
            Debug.LogWarning($"Documentation folder already exists: {docPath}");
        }
        OpenFolder(docPath);
    }

    [MenuItem("BaaWolf/Editor Notes/📂 Open Parent Folder")]
    public static void OpenParentFolderInExplorer()
    {
        string folderPath = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;
        OpenFolder(folderPath);
    }

    [MenuItem("BaaWolf/Editor Notes/📂 Open Root Folder")]
    public static void OpenRootFolderInExplorer()
    {
        string folderPath = Directory.GetParent(Application.dataPath).FullName;
        OpenFolder(folderPath);
    }

    /// <summary>
    /// Creates the UI when the editor window is initialized.
    /// </summary>
    void CreateGUI()
    {
        // Restore serialized content
        notesContent = serializedNotesContent;
        currentFilePath = serializedCurrentFilePath;
        hasSavedPath = serializedHasSavedPath;

        var root = rootVisualElement;
        root.Clear();

        root.style.flexDirection = FlexDirection.Column;
        root.style.flexGrow = 1;
        root.style.paddingTop = 4;
        root.style.paddingBottom = 4;
        root.style.paddingLeft = 4;
        root.style.paddingRight = 4;

        scrollView = new ScrollView(ScrollViewMode.Vertical)
        {
            style =
            {
                flexGrow = 1,
                flexShrink = 1,
                marginBottom = 6
            }
        };

        textField = new TextField { multiline = true };
        textField.style.whiteSpace = WhiteSpace.Normal;
        textField.style.flexGrow = 1;
        textField.style.flexShrink = 1;
        textField.style.minHeight = 300;
        textField.style.unityTextAlign = TextAnchor.UpperLeft;

        textField.SetValueWithoutNotify(notesContent);

        scrollView.Add(textField);
        root.Add(scrollView);

        textField.RegisterValueChangedCallback(evt =>
        {
            notesContent = evt.newValue;
            serializedNotesContent = notesContent;
        });

        root.schedule.Execute(() =>
        {
            var lines = textField.text.Split('\n');
            int cursorIndex = textField.cursorIndex;
            int currentLine = 0, charCount = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                if (charCount + lines[i].Length >= cursorIndex)
                {
                    currentLine = i;
                    break;
                }
                charCount += lines[i].Length + 1;
            }

            float lineHeight = 18f;
            float caretY = currentLine * lineHeight;
            float viewTop = scrollView.scrollOffset.y;
            float viewBottom = viewTop + scrollView.contentRect.height;

            bool caretMoved = cursorIndex != previousCursorIndex;

            if (caretMoved)
            {
                if (caretY < viewTop)
                {
                    scrollView.scrollOffset = new Vector2(0, caretY);
                }
                else if (caretY + lineHeight > viewBottom)
                {
                    scrollView.scrollOffset = new Vector2(0, caretY + lineHeight - scrollView.contentRect.height);
                }
            }

            previousCursorIndex = cursorIndex;
        }).Every(100);

        var buttonRow = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                flexShrink = 0,
                height = 24,
                marginTop = 4
            }
        };

        buttonRow.Add(CreateButton("New", CreateNewNote));
        buttonRow.Add(CreateButton("Load", LoadNotesWithDialog));
        buttonRow.Add(CreateButton("Save As", SaveNotesWithDialog));

        saveButton = CreateButton("Save", () =>
        {
            if (hasSavedPath)
                SaveNotes(currentFilePath);
        }, addMargin: false);

        saveButton.SetEnabled(hasSavedPath);
        buttonRow.Add(saveButton);

        root.Add(buttonRow);

        if (string.IsNullOrEmpty(notesContent))
        {
            string defaultPath = Path.Combine(Application.dataPath, "../Documentation", DefaultFileName);
            if (File.Exists(defaultPath))
                LoadFromPath(defaultPath);
            else
                CreateNewNote();
        }
    }

    /// <summary>
    /// Creates a styled button for the editor UI.
    /// </summary>
    private Button CreateButton(string label, Action onClick, bool addMargin = true)
    {
        var btn = new Button(onClick) { text = label };
        btn.style.height = 24;
        if (addMargin)
            btn.style.marginRight = 4;
        return btn;
    }

    /// <summary>
    /// Creates a new blank note.
    /// </summary>
    void CreateNewNote()
    {
        NewNote("Enter your notes here");
    }

    /// <summary>
    /// Initializes a new note with optional initial text.
    /// </summary>
    void NewNote(string initialText = "")
    {
        notesContent = "";
        serializedNotesContent = "";

        currentFilePath = "";
        serializedCurrentFilePath = "";

        hasSavedPath = false;
        serializedHasSavedPath = false;

        titleContent.text = "Notes - New";
        saveButton?.SetEnabled(false);

        rootVisualElement.schedule.Execute(() =>
        {
            float availableHeight = scrollView.resolvedStyle.height;

            float lineHeight = EditorStyles.textField.lineHeight;
            if (lineHeight <= 0) lineHeight = 16f;

            int lineCount = Mathf.Max(1, Mathf.CeilToInt(availableHeight / lineHeight));

            string[] lines = new string[lineCount];
            lines[0] = initialText;
            for (int i = 1; i < lineCount; i++)
                lines[i] = "\u200B";

            notesContent = string.Join("\n", lines);
            serializedNotesContent = notesContent;

            textField.SetValueWithoutNotify(notesContent);
            textField.Focus();
            textField.SelectRange(0, initialText.Length);
        }).ExecuteLater(50);
    }

    /// <summary>
    /// Saves the current note to the specified file path.
    /// </summary>
    void SaveNotes(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        File.WriteAllText(path, notesContent);
        Debug.Log($"Notes saved to: {path}");
    }

    /// <summary>
    /// Opens a file save dialog to save the note under a new name.
    /// </summary>
    void SaveNotesWithDialog()
    {
        string directory = Path.Combine(Application.dataPath, "../Documentation");
        Directory.CreateDirectory(directory);

        string path = EditorUtility.SaveFilePanel("Save Notes As", directory, DefaultFileName, "txt");
        if (!string.IsNullOrEmpty(path))
        {
            SaveNotes(path);
            currentFilePath = path;
            serializedCurrentFilePath = currentFilePath;

            hasSavedPath = true;
            serializedHasSavedPath = true;

            saveButton.SetEnabled(true);
            titleContent.text = "Notes - " + Path.GetFileName(currentFilePath);
        }
    }

    /// <summary>
    /// Opens a file open dialog to load a note.
    /// </summary>
    void LoadNotesWithDialog()
    {
        string directory = Path.Combine(Application.dataPath, "../Documentation");
        string path = EditorUtility.OpenFilePanel("Load Notes", directory, "txt");

        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            LoadFromPath(path);
        }
        else
        {
            CreateNewNote();
        }
    }

    /// <summary>
    /// Loads note content from a specific file path.
    /// </summary>
    void LoadFromPath(string path)
    {
        notesContent = File.ReadAllText(path);
        serializedNotesContent = notesContent;

        currentFilePath = path;
        serializedCurrentFilePath = currentFilePath;

        hasSavedPath = true;
        serializedHasSavedPath = true;

        saveButton?.SetEnabled(true);
        textField.SetValueWithoutNotify(notesContent);
        titleContent.text = "Notes - " + Path.GetFileName(currentFilePath);
        Debug.Log($"Notes loaded from: {path}");
    }

    /// <summary>
    /// Automatically saves notes if a file path is known when the window closes.
    /// </summary>
    void OnDisable()
    {
        if (hasSavedPath && !string.IsNullOrEmpty(currentFilePath))
        {
            File.WriteAllText(currentFilePath, notesContent);
        }
    }
}
