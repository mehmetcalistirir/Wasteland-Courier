using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RadioZone : MonoBehaviour
{
    [Header("Zone Info")]
    public string zoneId;

    [Header("Stations")]
    public RadioStation[] unlockStations;

    [Header("Signal Source")]
    public Transform signalSource;   // <<< ÖNEMLİ
    public float maxSignalDistance = 10f;

    [Range(0f, 1f)]
    public float maxSignalStrength = 1f;

    private Transform player;
    private bool playerInside = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (signalSource == null)
            Debug.LogWarning($"📡 RadioZone {zoneId} has no SignalSource!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        foreach (var station in unlockStations)
            RadioSystem.Instance.UnlockStation(station);

        Debug.Log($"📡 ENTER RadioZone: {zoneId}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        RadioSystem.Instance.SetSignalStrength(0f);

        Debug.Log($"📡 EXIT RadioZone: {zoneId}");
    }

    private void Update()
    {
        if (!playerInside || player == null || signalSource == null)
            return;

        float distance = Vector2.Distance(player.position, signalSource.position);

        float t = Mathf.Clamp01(1f - (distance / maxSignalDistance));
        float signal = t * maxSignalStrength;

        RadioSystem.Instance.SetSignalStrength(signal);
    }
}
