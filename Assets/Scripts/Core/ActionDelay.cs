using UnityEngine;

public static class ActionDelay
{
    private static float blockedUntil;

    public static bool IsWaiting => Time.time < blockedUntil;

    public static bool TryStart(float duration)
    {
        if (IsWaiting) return false;
        blockedUntil = Time.time + Mathf.Max(0f, duration);
        return true;
    }

    public static void Cancel()
    {
        blockedUntil = 0f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        blockedUntil = 0f;
    }
}
