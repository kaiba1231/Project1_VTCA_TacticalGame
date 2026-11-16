using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string scenePath;
    public int roundNumber;
    public List<PlayerSaveData> players = new List<PlayerSaveData>();
    public List<EnemySaveData> enemies = new List<EnemySaveData>();
}

[Serializable]
public class PlayerSaveData
{
    public string saveId;
    public Vector3 position;
    public int currentHealth;
    public int currentAP;
    public bool hasEndedTurn;
}

[Serializable]
public class EnemySaveData
{
    public string saveId;
    public Vector3 position;
    public int currentHealth;
}
