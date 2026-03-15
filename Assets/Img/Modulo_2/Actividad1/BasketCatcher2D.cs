// ===========================
// BasketCatcher2D.cs
// - Atrapa objetos (Good/Bad)
// - Permite mover la canasta en X con click + drag (solo izquierda/derecha)
// ===========================

using UnityEngine;

public class BasketCatcher2D : MonoBehaviour
{
    [Header("Game")]
    [SerializeField] private MiniGameManager1 manager;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip goodSound;
    [SerializeField] private AudioClip badSound;

    [Header("Drag Move (X only)")]
    [Tooltip("Cámara que renderiza la escena 2D (si está vacío usa Camera.main)")]
    [SerializeField] private Camera cam;

    [Tooltip("Límite mínimo y máximo en X para la canasta")]
    [SerializeField] private float minX = -7f;
    [SerializeField] private float maxX = 7f;

    [Tooltip("Suavizado opcional (0 = instantáneo). Recomendado 10-20")]
    [SerializeField] private float followSpeed = 0f;

    private bool isDragging;
    private float dragOffsetX;
    private float targetX;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        targetX = transform.position.x;
    }

    private void Update()
    {
        if (!isDragging) return;

        Vector3 screenPos = Input.mousePosition;

        if (Input.touchCount > 0)
            screenPos = Input.GetTouch(0).position;

        Vector3 world = cam.ScreenToWorldPoint(screenPos);

        targetX = world.x + dragOffsetX;
        targetX = Mathf.Clamp(targetX, minX, maxX);

        Vector3 pos = transform.position;

        if (followSpeed <= 0f)
        {
            pos.x = targetX;
        }
        else
        {
            pos.x = Mathf.Lerp(pos.x, targetX, followSpeed * Time.deltaTime);
        }

        transform.position = pos;

        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            isDragging = false;
    }

    private void OnMouseDown()
    {
        StartDrag(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    private void StartDrag(Vector3 screenPos)
    {
        if (cam == null) cam = Camera.main;

        Vector3 world = cam.ScreenToWorldPoint(screenPos);
        dragOffsetX = transform.position.x - world.x;

        isDragging = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (manager == null) return;

        if (other.CompareTag("Good"))
        {
            manager.RegisterCorrect();

            if (audioSource && goodSound)
                audioSource.PlayOneShot(goodSound);

            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Bad"))
        {
            manager.RegisterWrong();

            if (audioSource && badSound)
                audioSource.PlayOneShot(badSound);

            Destroy(other.gameObject);
        }
    }
}