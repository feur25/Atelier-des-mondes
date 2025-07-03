using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

using System.Threading.Tasks;

[CreateAssetMenu(fileName = "ObjectiveAssetManager", menuName = "Tools/Objective Asset Manager", order = 1)]
public class ObjectiveAssetManager : ScriptableObject {
    private const string BASE_URL = "https://apinetimmersizeq1.azurewebsites.net/api/game/objectives";
    [SerializeField] public List<GameObject> selectedAssets = new List<GameObject>();
    [SerializeField] private List<string> existingObjectives = new List<string>();
    
    public void FetchExistingObjectives(Action onComplete = null) =>
        EditorCoroutine.Start(GetExistingObjectives((success, objectives) => {
            if (success) existingObjectives = objectives;
            onComplete?.Invoke();
        }));
    
    private IEnumerator GetExistingObjectives(Action<bool, List<string>> onComplete) {
        var task = GetExistingObjectivesAsync();
        for (; !task.IsCompleted ;) yield return null;
        var (success, objectives, error) = task.Result;
        if (!success) Debug.LogError($"Failed to fetch existing objectives: {error}");
        onComplete?.Invoke(success, objectives);
    }
    public async void SubmitAssetsToAPI(Action<int, int> onComplete = null) {
        var newAssets = selectedAssets.Where(a => a && !existingObjectives.Contains(a.name)).ToList();
        if (newAssets.Count == 0) { onComplete?.Invoke(0, 0); return; }
        var canvasNames = newAssets.Select(a => a.name).ToList();

        var (success, count, msg) = await AddCanvasPipelineBatchAsync(canvasNames);
        if (success) existingObjectives.AddRange(canvasNames.Take(count));
        onComplete?.Invoke(success ? count : 0, canvasNames.Count);
    }
    
    private async Task<(bool success, int successCount, string message)> AddCanvasPipelineBatchAsync(List<string> canvasNames) {
        using var www = new UnityWebRequest(BASE_URL + "/canvas/pipeline/batch", "POST");
        www.SetRequestHeader("Content-Type", "application/json");
        var jsonArray = "[" + string.Join(",", canvasNames.Select(n => $"\"{n}\"")) + "]";
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonArray));
        www.downloadHandler = new DownloadHandlerBuffer();
        var op = www.SendWebRequest();
        for (; !op.isDone ;) await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
            return (false, 0, www.error);

        try {
            var json = www.downloadHandler.text;
            int count = json.Count(c => c == '{');
            return (true, count, "Success");
        } catch (Exception e) { return (false, 0, e.Message); }
    }
    
    public async void RemoveObjectiveFromAPI(string objectiveName, Action<bool> onComplete = null) {
        var (success, message) = await RemoveObjectiveAsync(objectiveName);
        if (success) {
            existingObjectives.Remove(objectiveName);
            Debug.Log($"Removed objective: {objectiveName}");
        } else {
            Debug.LogError($"Failed to remove objective: {objectiveName}. {message}");
            FetchExistingObjectives();
        }

        onComplete?.Invoke(success);
    }
    
    private async Task<(bool success, string message)> RemoveObjectiveAsync(string objectiveName) {
        using var www = UnityWebRequest.Get(BASE_URL);
        var op = www.SendWebRequest();
        for (; !op.isDone ;) await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
            return (false, www.error);

        try {
            var json = www.downloadHandler.text;
            var objectives = JsonUtility.FromJson<ObjectiveList>("{\"items\":" + json + "}").items;
            var obj = objectives.FirstOrDefault(o => o.Name == objectiveName);
            if (obj == null) return (false, "Objective not found");

            using var del = UnityWebRequest.Delete($"{BASE_URL}/canvas/pipeline/{obj.Id}");
            var delOp = del.SendWebRequest();
            for (; !delOp.isDone ;) await Task.Yield();
            return (del.result == UnityWebRequest.Result.Success, del.result == UnityWebRequest.Result.Success ? "Success" : del.error);
        } catch (Exception e) { return (false, $"Error parsing API response: {e.Message}"); }
    }

    private async Task<(bool success, List<string> objectives, string error)> GetExistingObjectivesAsync() {
        using var www = UnityWebRequest.Get(BASE_URL + "/canvas/pipeline");
        var op = www.SendWebRequest();
        for (; !op.isDone ;) await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
            return (false, new List<string>(), www.error);

        try {
            var json = www.downloadHandler.text;
            var objectives = JsonUtility.FromJson<StringListWrapper>("{\"items\":" + json + "}").items;
            return (true, objectives, null);
        } catch (Exception e) {
            Debug.LogError($"Error parsing API response: {e.Message}");
            return (false, new List<string>(), e.Message);
        }
    }

    private async Task<(bool success, string message)> AddCanvasPipelineAsync(string canvasName) {
        using var www = new UnityWebRequest(BASE_URL + "/canvas/pipeline", "POST");

        www.SetRequestHeader("Content-Type", "application/json");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes($"\"{canvasName}\""));
        www.downloadHandler = new DownloadHandlerBuffer();

        var op = www.SendWebRequest();
        for (; !op.isDone ;) await Task.Yield();
        return (www.result == UnityWebRequest.Result.Success, www.result == UnityWebRequest.Result.Success ? www.downloadHandler.text : www.error);
    }

    [Serializable] private class StringListWrapper { public List<string> items; }
    [Serializable] private class Objective { public int Id; public string Name; public bool Progress; public int Orded; }
    [Serializable] private class ObjectiveList { public List<Objective> items; }
    
    public class EditorCoroutine {
        private static readonly List<EditorCoroutine> active = new();
        private IEnumerator routine;
        private EditorCoroutine(IEnumerator r) { routine = r; }
        public static EditorCoroutine Start(IEnumerator r) {
            var c = new EditorCoroutine(r);
            active.Add(c);
            EditorApplication.update += c.Update;
            return c;
        }
        public void Stop() {
            EditorApplication.update -= Update;
            active.Remove(this);
        }
        private void Update() {
            if (!routine.MoveNext()) Stop();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ObjectiveAssetManager))]
public class ObjectiveAssetManagerEditor : Editor {
    Vector2 scrollPos, existingScrollPos;
    bool isSubmitting, isLoading, showStatus, showExisting, showExistingObjectives, showStatusMessage = true;
    string statusMsg, statusMessage = "";
    Vector2 scrollPosition, existingObjectivesScrollPosition;

    void OnEnable() => RefreshObjectives();

    public override void OnInspectorGUI() {
        var manager = (ObjectiveAssetManager)target;
        GUILayout.Label("Objective Asset Manager", EditorStyles.boldLabel);
        if (GUILayout.Button("Refresh Objectives from API", GUILayout.Height(30))) RefreshObjectives();

        DrawDropArea(manager);
        DrawSelectedAssetsList(manager);
        DrawActionButtons(manager);
        DrawExistingObjectives(manager);

        if (showStatus) EditorGUILayout.HelpBox(statusMsg, statusMsg.Contains("Error") ? MessageType.Error : MessageType.Info);
    }

    void ShowStatus(string message) {
        statusMsg = message; showStatus = true;
        double t = EditorApplication.timeSinceStartup;
        void Clear() {
            if (EditorApplication.timeSinceStartup - t >= 5) {
                showStatus = false; Repaint();
                EditorApplication.update -= Clear;
            }
        }
        EditorApplication.update += Clear;
    }

    void RefreshObjectives() {
        var manager = (ObjectiveAssetManager)target;
        isLoading = true; ShowStatus("Loading objectives from API...");
        manager.FetchExistingObjectives(() => { isLoading = false; ShowStatus("Objectives loaded from API"); Repaint(); });
    }

    T GetPrivateField<T>(object obj, string fieldName) =>
        (T)(obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(obj) ?? default(T));

    void DrawDropArea(ObjectiveAssetManager manager) {
        var dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.9f);
        GUI.Box(dropArea, "Drop Canvas/GameObjects Here");
        GUI.backgroundColor = Color.white;

        var evt = Event.current;
        if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition)) {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform) {
                DragAndDrop.AcceptDrag();
                foreach (var obj in DragAndDrop.objectReferences)
                    if (obj is GameObject go && !manager.selectedAssets.Contains(go))
                        manager.selectedAssets.Add(go);
                EditorUtility.SetDirty(manager);
            }
            evt.Use();
        }
    }

    void DrawSelectedAssetsList(ObjectiveAssetManager manager) {
        GUILayout.Label($"Selected Assets: {manager.selectedAssets.Count}", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        for (int i = 0; i < manager.selectedAssets.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            manager.selectedAssets[i] = (GameObject)EditorGUILayout.ObjectField(manager.selectedAssets[i], typeof(GameObject), false);
            if (GUILayout.Button("Remove", GUILayout.Width(70))) {
                manager.selectedAssets.RemoveAt(i--);
                EditorUtility.SetDirty(manager);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    void DrawActionButtons(ObjectiveAssetManager manager) {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All")) { manager.selectedAssets.Clear(); EditorUtility.SetDirty(manager); }
        EditorGUI.BeginDisabledGroup(!(manager.selectedAssets.Count > 0 && !isSubmitting));
        if (GUILayout.Button(isSubmitting ? "Submitting..." : "Submit to API") && !isSubmitting) {
            isSubmitting = true;
            ShowStatusMessage("Submitting assets to API...");
            manager.SubmitAssetsToAPI((success, total) => {
                isSubmitting = false;
                ShowStatusMessage(total == 0 ? "No new assets to submit - all already exist in API" : $"Submitted {success}/{total} assets successfully");
                if (success == total && success > 0) { manager.selectedAssets.Clear(); EditorUtility.SetDirty(manager); }
                Repaint();
            });
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }

    void DrawExistingObjectives(ObjectiveAssetManager manager) {
        showExistingObjectives = EditorGUILayout.Foldout(showExistingObjectives, "Existing Objectives in API", true);
        if (!showExistingObjectives) return;

        var existingObjectives = GetPrivateField<List<string>>(manager, "existingObjectives");
        if (existingObjectives == null || existingObjectives.Count == 0) {
            EditorGUILayout.HelpBox("No objectives found in API or not yet loaded", MessageType.Info);
            return;
        }

        existingObjectivesScrollPosition = EditorGUILayout.BeginScrollView(existingObjectivesScrollPosition, GUILayout.Height(200));
        for (int i = 0; i < existingObjectives.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(existingObjectives[i]);
            if (GUILayout.Button("Remove from API", GUILayout.Width(120)) &&
                EditorUtility.DisplayDialog("Remove Objective", $"Are you sure you want to remove '{existingObjectives[i]}' from the API?", "Yes", "No")) {
                ShowStatusMessage($"Removing {existingObjectives[i]} from API...");
                manager.RemoveObjectiveFromAPI(existingObjectives[i], (success) =>
                {
                    ShowStatusMessage(success ? $"Successfully removed {existingObjectives[i]} from API" : $"Error: Failed to remove {existingObjectives[i]} from API");
                    Repaint();
                });
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
    
    private void ShowStatusMessage(string message) {
        statusMessage = message;
        showStatusMessage = true;
        double startTime = EditorApplication.timeSinceStartup;
        void Clear() {
            if (EditorApplication.timeSinceStartup - startTime >= 5.0) {
                showStatusMessage = false;
                Repaint();
                EditorApplication.update -= Clear;
            }
        }
        EditorApplication.update += Clear;
    }
}
#endif
