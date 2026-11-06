using UnityEngine;

[DisallowMultipleComponent]
public class APComponent : MonoBehaviour
{
    [SerializeField] private int maxAP = 2;
    [SerializeField] private int currentAP;

    public int MaxAP => maxAP;
    public int CurrentAP => currentAP;

    public void ResetAP() => currentAP = maxAP;

    public bool HasEnoughAP(int amount)
    {
        if (amount <= 0 && currentAP < amount)
        {
            return false;
        }

        return true;
    }
    public bool TrySpendAP(int amount)
    {
        if (!HasEnoughAP(amount)) return false;
        currentAP -= amount;
        return true;
    }
}
