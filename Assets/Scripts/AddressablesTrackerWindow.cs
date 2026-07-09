using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesTeachingTrackerWindow : EditorWindow
{
    private class TrackedEntry
    {
        public string key;
        public readonly List<GameObject> instances = new List<GameObject>();
        public int totalLoaded;      // how many times InstantiateAsync succeeded for this key
        public bool everLoaded;      // has the underlying asset been loaded at least once
    }

    private readonly Dictionary<string, TrackedEntry> trackedEntries = new Dictionary<string, TrackedEntry>();
    private string newKeyInput = "";
    private Vector2 scrollPos;

    [MenuItem("Window/Addressables/Teaching Tracker")]
    public static void ShowWindow()
    {
        var window = GetWindow<AddressablesTeachingTrackerWindow>("Addressables Teaching Tracker");
        window.minSize = new Vector2(440, 320);
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // All instances get torn down by Unity when leaving Play Mode,
        // so our bookkeeping has to reset to match reality.
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            trackedEntries.Clear();
            Repaint();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Addressables Teaching Tracker", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Load a key, watch the console for cache-hit messages, then try deleting an " +
            "instance in the Hierarchy and clicking Release to see the rules in action.",
            MessageType.None);
        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to load/instantiate Addressables.", MessageType.Info);
        }

        // --- Load new key row ---
        EditorGUILayout.BeginHorizontal();
        newKeyInput = EditorGUILayout.TextField("Addressable Key / Address", newKeyInput);
        GUI.enabled = Application.isPlaying && !string.IsNullOrEmpty(newKeyInput);
        if (GUILayout.Button("Load / Instantiate", GUILayout.Width(140)))
        {
            LoadAndInstantiate(newKeyInput);
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        CleanupDestroyedInstances();
        EditorGUILayout.LabelField($"Tracked Keys: {trackedEntries.Count}    " +
                                    $"Active Instances (all keys): {trackedEntries.Values.Sum(e => e.instances.Count)}");
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        string rowToRemove = null;
        foreach (var kvp in trackedEntries)
        {
            var entry = kvp.Value;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(entry.key, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Active in scene: {entry.instances.Count}    Total loaded: {entry.totalLoaded}");

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Instantiate Another", GUILayout.Width(150)))
            {
                LoadAndInstantiate(entry.key);
            }

            bool canRelease = Application.isPlaying && entry.totalLoaded > 0 && entry.instances.Count == 0;
            GUI.enabled = canRelease;
            if (GUILayout.Button("Release", GUILayout.Width(90)))
            {
                Release(entry);
            }

            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Try Release (blocked demo)", GUILayout.Width(190)) && entry.instances.Count > 0)
            {
                Debug.LogWarning(
                    $"[Addressables] Cannot release '{entry.key}' — {entry.instances.Count} instance(s) " +
                    "are still active in the scene. Destroy all of them first, then Release. " +
                    "This is why Addressables uses reference counting: an asset can't be safely " +
                    "unloaded while something is still using it.");
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (!Application.isPlaying) { }
            else if (entry.instances.Count > 0)
            {
                EditorGUILayout.HelpBox($"Release is locked: {entry.instances.Count} instance(s) still exist. " +
                                         "Delete them in the Hierarchy to unlock it.", MessageType.Warning);
            }

            if (GUILayout.Button("Remove Row (releases if unlocked, ignores otherwise)"))
            {
                if (entry.instances.Count == 0)
                {
                    rowToRemove = entry.key;
                }
                else
                {
                    Debug.LogWarning($"[Addressables] Won't remove '{entry.key}' — instances still active in the scene.");
                }
            }

            EditorGUILayout.EndVertical();
        }

        if (rowToRemove != null)
        {
            trackedEntries.Remove(rowToRemove);
        }

        EditorGUILayout.EndScrollView();
    }

    private void LoadAndInstantiate(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (!trackedEntries.TryGetValue(key, out var entry))
        {
            entry = new TrackedEntry { key = key };
            trackedEntries[key] = entry;
        }

        // --- Teaching moment #1: caching ---
        if (!entry.everLoaded)
        {
            Debug.Log($"[Addressables] Loading '{key}' for the first time (from bundle/AssetDatabase)...");
        }
        else
        {
            Debug.Log($"[Addressables] '{key}' is ALREADY loaded. Addressables will reuse the cached asset " +
                       "instead of loading it again — only a new instance + reference count increase happens now.");
        }

        Addressables.InstantiateAsync(key).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                entry.instances.Add(handle.Result);
                entry.totalLoaded++;
                entry.everLoaded = true;
                Debug.Log($"[Addressables] '{key}' instance #{entry.totalLoaded} created. " +
                           $"Active in scene now: {entry.instances.Count}.");
            }
            else
            {
                Debug.LogError($"[Addressables] Failed to load/instantiate key: '{key}'. " +
                                "Check the key exists and is included in your Addressables build.");
            }
            Repaint();
        };
    }

    private void Release(TrackedEntry entry)
    {
        // By the time this is enabled, entry.instances.Count is already 0
        // (all instances were destroyed in the scene first).
        Debug.Log($"[Addressables] '{entry.key}' released. All instances were destroyed in the scene first, " +
                   "so it's now safe to free this asset from memory.");
        entry.totalLoaded = 0;
        entry.everLoaded = false;
        Repaint();
    }

    // Detects instances that were destroyed by something other than this
    // window (e.g. the student deleting them in the Hierarchy) so the
    // Active count and the Release lock stay accurate.
    private void CleanupDestroyedInstances()
    {
        foreach (var entry in trackedEntries.Values)
        {
            entry.instances.RemoveAll(instance => instance == null);
        }
    }
}