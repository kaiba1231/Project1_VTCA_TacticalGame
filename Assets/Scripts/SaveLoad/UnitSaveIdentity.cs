using UnityEngine;

[DisallowMultipleComponent]
public class UnitSaveIdentity : MonoBehaviour
{
    [SerializeField] private string saveId;
    public string SaveId => saveId;
}
