using System;
using System.IO;
using UnityEngine;

[DisallowMultipleComponent]
public class SaveSystem : MonoBehaviour
{
    public string SavePath => Path.Combine(Application.persistentDataPath, "Saves", "save_01.json");

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    // Tra ve true khi ghi thanh cong; message la thong bao de UI hien thi.
    public bool SaveGame(SaveData data, out string message)
    {
        if (data == null)
        {
            message = "No battle data to save.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);

            message = "Game saved.";
            Debug.Log("[SAVE] Saved to " + SavePath, this);
            return true;
        }
        catch (Exception exception)
        {
            message = "Save failed: " + exception.Message;
            return false;
        }
    }

    public bool LoadGame(out SaveData data, out string message)
    {
        data = null;
        if (!HasSaveFile())
        {
            message = "No saved game yet.";
            return false;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            data = JsonUtility.FromJson<SaveData>(json);

            // Tran dang choi can it nhat mot Player va mot Enemy con song.
            if (data == null || string.IsNullOrEmpty(data.scenePath) || data.roundNumber < 1 ||
                data.players == null || data.players.Count == 0 || data.enemies == null || data.enemies.Count == 0)
            {
                data = null;
                message = "Saved battle data is missing.";
                return false;
            }

            message = "Save ready.";
            return true;
        }
        catch (Exception exception)
        {
            data = null;
            message = "Load failed: " + exception.Message;
            return false;
        }
    }
}
