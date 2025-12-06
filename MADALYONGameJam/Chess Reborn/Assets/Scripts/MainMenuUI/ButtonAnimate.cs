using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Animation")]
    public RectTransform target;
    public float scaleAmount = 1.07f;
    public float scaleSpeed = 10f;
    public float rotateAmount = 4f;
    public float rotateSpeed = 6f;
    public bool idleSway = false;

    [Header("Sounds")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public float volume = 0.7f;

    private AudioSource audioSource;

    private Vector3 originalScale;
    private Quaternion originalRot;
    private bool hovering = false;

    void Start()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        originalScale = target.localScale;
        originalRot = target.localRotation;

        // AudioSource otomatik ekle
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (hovering)
        {
            // Scale büyüt
            Vector3 targetScale = originalScale * scaleAmount;
            target.localScale = Vector3.Lerp(target.localScale, targetScale, Time.deltaTime * scaleSpeed);

            // Hafif sağ-sol dönme
            float z = Mathf.Sin(Time.time * rotateSpeed) * rotateAmount;
            Quaternion targetRot = Quaternion.Euler(0, 0, z);
            target.localRotation = Quaternion.Lerp(target.localRotation, targetRot, Time.deltaTime * rotateSpeed);
        }
        else
        {
            // Eski haline dön
            target.localScale = Vector3.Lerp(target.localScale, originalScale, Time.deltaTime * scaleSpeed);
            target.localRotation = Quaternion.Lerp(target.localRotation, originalRot, Time.deltaTime * rotateSpeed);

            // Idle animasyonu
            if (idleSway)
            {
                float z = Mathf.Sin(Time.time * rotateSpeed * 0.5f) * (rotateAmount * 0.2f);
                target.localRotation = Quaternion.Euler(0, 0, z);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;

        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound, volume);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound, volume);
    }
}
