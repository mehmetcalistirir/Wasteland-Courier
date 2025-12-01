using UnityEngine;
using UnityEngine.UI;

public static class CanvasScalerUpdater
{
    public static void UpdateAllCanvasScalers()
    {
        float x = PlayerPrefs.GetFloat("ResX", 1920);
        float y = PlayerPrefs.GetFloat("ResY", 1080);

        CanvasScaler[] scalers = Resources.FindObjectsOfTypeAll<CanvasScaler>();

        foreach (CanvasScaler scaler in scalers)
        {
            // Sadece sahnedeki CanvasScaler’lara uygula (Prefab değil)
            if (scaler.gameObject.scene.isLoaded)
                scaler.referenceResolution = new Vector2(x, y);
        }
    }
}
