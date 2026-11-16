using UnityEngine;

// Chi giu du lieu cho lan chuyen scene dang dien ra. JSON moi la ban luu tren dia.
[DisallowMultipleComponent]
public class PersistentSaveData : MonoBehaviour
{
    public static PersistentSaveData Instance { get; private set; }
    public bool HasPendingLoad => pendingLoad != null;
    public string Notification { get; private set; }

    [System.NonSerialized] private SaveData pendingLoad;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        pendingLoad = null;
        Notification = null;
        DontDestroyOnLoad(gameObject);
    }

    public bool QueueLoad(SaveData data)
    {
        if (data == null || HasPendingLoad) return false;
        pendingLoad = data;
        Notification = null;
        return true;
    }

    public SaveData TakePendingLoad()
    {
        SaveData data = pendingLoad;
        pendingLoad = null;
        return data;
    }

    public void ClearPendingLoad() => pendingLoad = null;
    public void SetNotification(string message) => Notification = message;

    public string TakeNotification()
    {
        string message = Notification;
        Notification = null;
        return message;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance() => Instance = null;
}
