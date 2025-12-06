using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;


public class PlayerAbilities : MonoBehaviour
{
    public PlayerMovement2D movement;
    public GameObject barrierPrefab;

    bool placingBarrier = false;

    PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.PlaceClick.performed += ctx => OnMouseClick();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    // UI butonu çağırır
    public void StartPlacingBarrier()
    {
        placingBarrier = true;
        Debug.Log("Barrier placing mode enabled.");
    }

    void OnMouseClick()
{
    // UI tıklandıysa sahne tıklamasını yok say
    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
    {
        Debug.Log("UI clicked — ignoring world click.");
        return;
    }

    Debug.Log("CLICK RECEIVED | placingBarrier = " + placingBarrier);

    if (!placingBarrier)
    {
        Debug.Log("PLACEMENT MODE OFF — RETURNING");
        return;
    }

    if (Camera.main == null)
    {
        Debug.LogError("Camera.main IS NULL!");
        return;
    }

    if (Mouse.current == null)
    {
        Debug.LogError("Mouse.current IS NULL! (Input System wrong?)");
        return;
    }

    if (barrierPrefab == null)
    {
        Debug.LogError("BARRIER PREFAB NOT ASSIGNED IN INSPECTOR!");
        return;
    }

    Vector3 world = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    world.z = 0;

    Instantiate(barrierPrefab, world, Quaternion.identity);

    Debug.Log("BARRIER PLACED!");

    placingBarrier = false;
    PlayerAbilityUI.instance.RemoveAbilityButton(AbilityType.PlaceBarrier);
}



    // Speed skill aynı şekilde çalışmaya devam eder
    public void ActivateSpeedBoost()
{
    // Daha tıklanır tıklanmaz butonu kaldırıyoruz:
    PlayerAbilityUI.instance.RemoveAbilityButton(AbilityType.SpeedBoost);

    StartCoroutine(SpeedBoostRoutine());
}

    IEnumerator SpeedBoostRoutine()
{
    movement.speedBoostAmount += 3f;

    yield return new WaitForSeconds(5f);

    movement.speedBoostAmount -= 3f;

    if (movement.speedBoostAmount < 0f)
        movement.speedBoostAmount = 0f;
}

}
