// ===========================
// ConveyorBeltUI.cs
// - Mueve hijos del beltContainer de derecha->izquierda
// - Cuando salen por la izquierda, reaparecen por la derecha (loop infinito)
// ===========================

using UnityEngine;

public class ConveyorBeltUI : MonoBehaviour
{
    [SerializeField] private RectTransform beltContainer; // contiene las piezas como hijos
    [SerializeField] private float speed = 250f;          // px/seg
    [SerializeField] private float leftLimitX = -900f;    // límite en anchoredPosition.x
    [SerializeField] private float rightRespawnX = 900f;  // donde reaparecen

    [Tooltip("Si true, NO mueve las piezas mientras se arrastran (recomendado).")]
    [SerializeField] private bool pauseWhileDragging = true;

    private bool running = true;

    public void SetRunning(bool value) => running = value;

    private void Update()
    {
        if (!running) return;
        if (beltContainer == null) return;

        for (int i = 0; i < beltContainer.childCount; i++)
        {
            var child = beltContainer.GetChild(i) as RectTransform;
            if (child == null) continue;

            if (pauseWhileDragging && child.parent != beltContainer)
            {
                // si está siendo arrastrada (parent cambia al canvas)
                continue;
            }

            Vector2 pos = child.anchoredPosition;
            pos.x -= speed * Time.deltaTime;

            if (pos.x < leftLimitX)
                pos.x = rightRespawnX;

            child.anchoredPosition = pos;
        }
    }
}