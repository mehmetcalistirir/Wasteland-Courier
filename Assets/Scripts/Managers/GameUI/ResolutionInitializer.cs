using UnityEngine;
using System.Collections;

public class ResolutionInitializer : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(ApplyDelayed());
    }

    IEnumerator ApplyDelayed()
    {
        yield return null; // 1 frame gecikme
        CanvasScalerUpdater.UpdateAllCanvasScalers();
    }
}
