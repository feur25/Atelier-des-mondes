using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Threading;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class GameManager : MonoBehaviour {
    [Header("API Configuration")]
    [SerializeField] private string _apiBaseUrl = "https://apinetimmersizeq1.azurewebsites.net/api/game/objectives";
    [SerializeField] private float _pollInterval = 1f;
    
    [Header("Prefab Management")]
    [SerializeField] private Transform _objectivesContainer;
    [SerializeField] private GameObject _defaultPrefab;
    [SerializeField] private List<GameObject> _prefabsList = new List<GameObject>();
    [SerializeField] private string _prefabsPath = "Assets/Scripts/Assets/Editor";
    
    private Dictionary<string, GameObject> _activePrefabs = new Dictionary<string, GameObject>();
    private List<string> _completedObjectives = new List<string>();
    private List<string> _pendingObjectives = new List<string>();
    private Dictionary<string, bool> _objectiveProgress = new Dictionary<string, bool>();
    private CancellationTokenSource _cancelSource;
    private CancellationTokenSource _pollingCts;
    void Start() {
        _objectivesContainer ??= new GameObject("ObjectivesContainer").transform;
        
        if (Initialize()) Debug.Log("GameManager initialized successfully.");
        else Debug.LogError("Failed to initialize GameManager.");
    }

    protected bool Initialize() {
        LoadAllPrefabs();

        _cancelSource = new CancellationTokenSource();
        _ = SyncObjectivesAsync(_cancelSource.Token);

        _pollingCts = new CancellationTokenSource();
        _ = PollObjectivesAsync(_pollingCts.Token);

        return true;
    }
    private void LoadAllPrefabs() {
        #if UNITY_EDITOR
            foreach (var path in System.IO.Directory.GetFiles(_prefabsPath, "*.prefab", System.IO.SearchOption.AllDirectories)) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\', '/'));
                if (prefab && !_prefabsList.Contains(prefab)) _prefabsList.Add(prefab);
            }
        #endif
    }

    public bool this[string objectiveName] {
        get => _objectiveProgress.TryGetValue(objectiveName, out bool value) ? value : false;
        set {
            if (!_objectiveProgress.ContainsKey(objectiveName) || _objectiveProgress[objectiveName] != value) {
                _objectiveProgress[objectiveName] = value;
                _ = UpdateObjectiveAsync(objectiveName, value);
            }
        }
    }

    private async Task PollObjectivesAsync(CancellationToken token) {
        for (; !token.IsCancellationRequested ;) {
            try { await FetchObjectivesAsync(); }
            catch (Exception ex) { Debug.LogError($"Polling error: {ex.Message}"); }
            try { await Task.Delay(TimeSpan.FromSeconds(_pollInterval), token); }
            catch (TaskCanceledException) { break; }
        }
    }
    private async Task FetchObjectivesAsync() {
        try {
            var completedUrl = $"{_apiBaseUrl}/canvas/pipeline/progress/true";
            var pendingUrl = $"{_apiBaseUrl}/canvas/pipeline/progress/false";

            using var completedRequest = UnityWebRequest.Get(completedUrl);
            await completedRequest.SendWebRequest();
            
            if (completedRequest.result == UnityWebRequest.Result.Success) {
                var newCompleted = JsonUtility.FromJson<StringListWrapper>("{\"items\":" + completedRequest.downloadHandler.text + "}").items;
                foreach (var obj in newCompleted.Except(_completedObjectives).ToList()) { _completedObjectives.Add(obj); HandleCompletedObjective(obj); }
                foreach (var obj in _completedObjectives.Except(newCompleted).ToList()) { _completedObjectives.Remove(obj); RemoveObjectiveInstance(obj); }
            } else Debug.LogError(completedRequest.error);

            using var pendingRequest = UnityWebRequest.Get(pendingUrl);
            await pendingRequest.SendWebRequest();

            if (pendingRequest.result == UnityWebRequest.Result.Success) _pendingObjectives = JsonUtility.FromJson<StringListWrapper>("{\"items\":" + pendingRequest.downloadHandler.text + "}").items;
            else Debug.LogError(pendingRequest.error);
            
        } catch (Exception e) { Debug.LogError(e.Message); }
    }
    
    private void HandleCompletedObjective(string objectiveName) {
        if (_activePrefabs.ContainsKey(objectiveName) || (_activePrefabs.Count > 0 && !CanInstantiateMultipleObjectives())) return;

        var prefab = FindPrefabByName(objectiveName) ?? LoadPrefabDynamically(objectiveName) ?? _defaultPrefab;
        if (prefab == null) return;

        if (prefab == _defaultPrefab) Debug.LogWarning($"Using default prefab for {objectiveName}");

        var instance = Instantiate(prefab, _objectivesContainer);
        instance.name = objectiveName;
        _activePrefabs.Add(objectiveName, instance);
        Debug.Log($"Instantiated prefab for objective: {objectiveName}");
    }
    
    private GameObject LoadPrefabDynamically(string objectiveName) {
        #if UNITY_EDITOR
            foreach (var guid in AssetDatabase.FindAssets($"t:Prefab {objectiveName}")) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (prefab != null) {
                    if (!_prefabsList.Contains(prefab)) _prefabsList.Add(prefab);
                    return prefab;
                }
            }
        #endif
        return null;
    }
    
    private void RemoveObjectiveInstance(string objectiveName) {
        if (_activePrefabs.Remove(objectiveName, out GameObject instance)) {
            if (instance) Destroy(instance);
            Debug.Log($"Removed instance for objective: {objectiveName}");
            CheckPendingObjectives();
        }
    }

    private void CheckPendingObjectives() {
        if (_activePrefabs.Count == 0 || CanInstantiateMultipleObjectives()) {
            var next = _completedObjectives.FirstOrDefault(o => !_activePrefabs.ContainsKey(o));
            if (next != null) HandleCompletedObjective(next);
        }
    }
    
    private GameObject FindPrefabByName(string objectiveName) {
        GameObject exactMatch = _prefabsList.FirstOrDefault(p => p != null && p.name == objectiveName);
        if (exactMatch != null) return exactMatch;
        
        return _prefabsList.FirstOrDefault(p => p != null && 
            string.Equals(p.name, objectiveName, StringComparison.OrdinalIgnoreCase));
    }
    
    private bool CanInstantiateMultipleObjectives() {
        return _activePrefabs.Count < 3;
    }
    
    public void CompleteObjective(string objectiveName) => this[objectiveName] = true;
    public void ResetObjective(string objectiveName) => this[objectiveName] = false;

    private async Task SyncObjectivesAsync(CancellationToken token) {
        for (; !token.IsCancellationRequested ;) {
            try {
                using var req = UnityWebRequest.Get(_apiBaseUrl);
                await req.SendWebRequest();
                
                if (req.result == UnityWebRequest.Result.Success) {
                    var json = req.downloadHandler.text;
                    var objectives = JsonUtility.FromJson<ObjectiveList>("{\"items\":" + json + "}");
                    foreach (var o in objectives.items) {
                        if (!string.IsNullOrEmpty(o.Name)) _objectiveProgress[o.Name] = o.Progress;
                        else Debug.LogWarning("Objective with null or empty Name encountered and skipped.");
                    }
                } else Debug.LogError($"Error fetching objectives: {req.error}");
            } catch (Exception ex) { Debug.LogError($"Sync error: {ex.Message}"); }

            await Task.Delay(30000, token);
        }
    }

    private async Task UpdateObjectiveAsync(string name, bool completed) {
        try {
            using var req = UnityWebRequest.Get(_apiBaseUrl);
            await req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) { Debug.LogError(req.error); return; }

            var obj = JsonUtility.FromJson<ObjectiveList>("{\"items\":" + req.downloadHandler.text + "}").items
                .FirstOrDefault(o => o.Name == name);
            if (obj == null) { Debug.LogError($"Objective not found: {name}"); return; }

            using var upd = new UnityWebRequest($"{_apiBaseUrl}/progress/{obj.Id}", "POST") {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(completed ? "1" : "0")),
                downloadHandler = new DownloadHandlerBuffer()
            };
            upd.SetRequestHeader("Content-Type", "application/json");
            await upd.SendWebRequest();
            if (upd.result != UnityWebRequest.Result.Success && _objectiveProgress.ContainsKey(name)) _objectiveProgress[name] = !completed;
        } catch (Exception ex) { Debug.LogError(ex.Message); }
    }
    
    // private void RemoveObjectiveInstance(string objectiveName) {
    //     if (_activePrefabs.Remove(objectiveName, out GameObject instance)) {
    //         if (instance) Destroy(instance);

    //         Debug.Log($"Removed instance for objective: {objectiveName}");
    //         CheckPendingObjectives();
    //     }
    // }

    // private void CheckPendingObjectives() {
    //     if (_activePrefabs.Count == 0 || CanInstantiateMultipleObjectives()) {
    //         var next = _completedObjectives.FirstOrDefault(o => !_activePrefabs.ContainsKey(o));
    //         if (next != null) HandleCompletedObjective(next);
    //     }
    // }
    // private GameObject FindPrefabByName(string objectiveName) {
    //     GameObject exactMatch = _prefabsList.FirstOrDefault(p => p != null && p.name == objectiveName);
    //     if (exactMatch != null) return exactMatch;
        
    //     return _prefabsList.FirstOrDefault(p => p != null && 
    //         string.Equals(p.name, objectiveName, StringComparison.OrdinalIgnoreCase));
    // }
    // private bool CanInstantiateMultipleObjectives() {
    //     return _activePrefabs.Count < 3;
    // }
    // public void CompleteObjective(string objectiveName) => this[objectiveName] = true;
    // public void ResetObjective(string objectiveName) => this[objectiveName] = false;

    // private async Task SyncObjectivesAsync(CancellationToken token) {
    //     for (; !token.IsCancellationRequested ;) {
    //         try {
    //             using var req = UnityWebRequest.Get(_apiBaseUrl);
    //             await req.SendWebRequest();
                
    //             if (req.result == UnityWebRequest.Result.Success) {
    //                 var json = req.downloadHandler.text;
    //                 var objectives = JsonUtility.FromJson<ObjectiveList>("{\"items\":" + json + "}");
    //                 foreach (var o in objectives.items) _objectiveProgress[o.Name] = o.Progress;
    //             } else Debug.LogError($"Error fetching objectives: {req.error}");
    //         } catch (Exception ex) { Debug.LogError($"Sync error: {ex.Message}"); }

    //         await Task.Delay(30000, token);
    //     }
    // }

    // private async Task UpdateObjectiveAsync(string name, bool completed) {
    //     try {
    //         using var req = UnityWebRequest.Get(_apiBaseUrl);
    //         await req.SendWebRequest();

    //         if (req.result != UnityWebRequest.Result.Success) { Debug.LogError(req.error); return; }
    //         var obj = JsonUtility.FromJson<ObjectiveList>("{\"items\":" + req.downloadHandler.text + "}").items.FirstOrDefault(o => o.Name == name);

    //         if (obj == null) { Debug.LogError(name); return; }
            
    //         using var upd = new UnityWebRequest($"{_apiBaseUrl}/progress/{obj.Id}", "POST") {
    //             uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(completed ? "1" : "0")),
    //             downloadHandler = new DownloadHandlerBuffer()
    //         };

    //         upd.SetRequestHeader("Content-Type", "application/json");
    //         await upd.SendWebRequest();

    //         if (upd.result != UnityWebRequest.Result.Success)
    //             if (_objectiveProgress.ContainsKey(name)) _objectiveProgress[name] = !completed;
    //     }
    //     catch (Exception ex) { Debug.LogError(ex.Message); }
    // }
    private void OnDestroy() {
        _cancelSource?.Cancel();
        _cancelSource?.Dispose();
    }
    
    private void OnEnable() {
        Initialize();
    }

    private void OnDisable() {
        StopPollingObjectives();
    }
    private void StopPollingObjectives() {
        _pollingCts?.Cancel();
        _pollingCts?.Dispose();
        _pollingCts = null;
    }

    
    [Serializable]
    private class Objective {
        public int Id;
        public string Name;
        public bool Progress;
        public int Orded;
    }

    [Serializable]
    private class ObjectiveList {
        public List<Objective> items;
    }
    
    [Serializable]
    private class StringListWrapper {
        public List<string> items;
    }
    
}
