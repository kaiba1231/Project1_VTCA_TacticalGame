using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class SaveSystem : MonoBehaviour
{
    public const int AutoSlot = 0;
    public const int ManualSlotCount = 3;

    [SerializeField] private string apiBaseUrl = "http://127.0.0.1:9999/api";

    public IEnumerator ListSlots(Action<SaveSlotInfo[], string> completed)
    {
        using var request = UnityWebRequest.Get(SavesUrl);
        request.timeout = 5;
        yield return request.SendWebRequest();

        if (TryReadJson(request, out SaveSlotList list) && list.slots != null)
            completed(list.slots, null);
        else
            completed(null, ErrorMessage(request, "Could not read save slots."));
    }

    public IEnumerator LoadGame(int slotId, Action<SaveData, string> completed)
    {
        using var request = UnityWebRequest.Get($"{SavesUrl}/{slotId}");
        request.timeout = 5;
        yield return request.SendWebRequest();

        if (TryReadJson(request, out SaveData data) && IsBattleData(data))
            completed(data, null);
        else if (request.responseCode == 404)
            completed(null, slotId == AutoSlot ? "No autosave yet." : "This slot is empty.");
        else
            completed(null, ErrorMessage(request, "Could not read the saved battle."));
    }

    public IEnumerator SaveGame(int slotId, SaveData data, Action<string> completed)
    {
        if (!IsBattleData(data))
        {
            completed("No battle data to save.");
            yield break;
        }

        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        using var request = UnityWebRequest.Put($"{SavesUrl}/{slotId}", body);
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 5;
        yield return request.SendWebRequest();

        completed(request.result == UnityWebRequest.Result.Success
            ? null
            : ErrorMessage(request, "Could not save the battle."));
    }

    private string SavesUrl => apiBaseUrl.TrimEnd('/') + "/saves";

    private static bool IsBattleData(SaveData data)
    {
        return data != null && !string.IsNullOrWhiteSpace(data.scenePath) && data.roundNumber > 0
            && data.players != null && data.players.Count > 0
            && data.enemies != null && data.enemies.Count > 0;
    }

    private static bool TryReadJson<T>(UnityWebRequest request, out T data) where T : class
    {
        data = null;
        if (request.result != UnityWebRequest.Result.Success) return false;

        try { data = JsonUtility.FromJson<T>(request.downloadHandler.text); }
        catch (ArgumentException) { return false; }
        return data != null;
    }

    private static string ErrorMessage(UnityWebRequest request, string fallback)
    {
        if (request.result == UnityWebRequest.Result.ConnectionError) return "Cannot reach the save API.";
        if (request.responseCode == 503) return "The save API cannot reach the database.";
        return fallback;
    }
}

[Serializable]
public class SaveSlotList
{
    public SaveSlotInfo[] slots;
}

[Serializable]
public class SaveSlotInfo
{
    public int slotId;
    public string scenePath;
    public int roundNumber;
    public string savedAt;
}
