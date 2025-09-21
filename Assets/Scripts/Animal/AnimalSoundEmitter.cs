using System;
using UnityEngine;

public struct SoundEvent
{
    public Vector2 position;
    public float radius;
    public float fleeDuration;
    public float speedMultiplier;

    public SoundEvent(Vector2 pos, float rad, float dur = 2f, float mult = 1.5f)
    {
        position = pos;
        radius = rad;
        fleeDuration = dur;
        speedMultiplier = mult;
    }
}

public static class AnimalSoundEmitter
{
    public static event Action<SoundEvent> OnSound;

    // 🔹 2 parametreli versiyon (default değerlerle)
    public static void EmitSound(Vector2 pos, float radius)
    {
        EmitSound(pos, radius, 2f, 1.5f);
    }

    // 🔹 4 parametreli versiyon
    public static void EmitSound(Vector2 pos, float radius, float fleeDuration, float speedMultiplier)
    {
        OnSound?.Invoke(new SoundEvent(pos, radius, fleeDuration, speedMultiplier));
    }
}
