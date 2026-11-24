using UnityEngine;

[DisallowMultipleComponent]
public class UnitSaveIdentity : MonoBehaviour
{
    [SerializeField] private string saveId;
    public string SaveId
    {
        get
        {
            EnemyUnit enemy = GetComponent<EnemyUnit>();
            return enemy != null ? $"enemy_{enemy.EnemyId}" : saveId;
        }
    }
}
