using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class StickerDragWorld : MonoBehaviour
{
    [Header("Drop area (world)")]
    [SerializeField] private Collider2D drawAreaCollider;

    [Header("Behavior")]
    [SerializeField] private bool cloneOnDrag = true;       // tipo sticker-pack
    [SerializeField] private bool destroyIfDroppedOutside = true;

    [Header("Placement")]
    [SerializeField] private bool snapZToZero = true;       // útil si tu escena usa plano z=0
    [SerializeField] private float dragZ = 0f;              // z mientras arrastras (si estás en 2.5D)

    private Camera cam;
    private bool dragging;

    // Si clonamos, movemos el clon. Si no, movemos este mismo.
    private GameObject draggedObj;
    private SpriteRenderer draggedSR;
    private Collider2D draggedCol;

    // Para devolver si no se pega (cuando cloneOnDrag=false)
    private Vector3 originalPos;
    private Transform originalParent;

    private void Awake()
    {
        cam = Camera.main;

        // Recomendación: el collider del sticker no debe bloquear clicks de otros objetos.
        // Lo usamos igual para interacción, pero en drag no necesitamos colisiones físicas.
        // Si quieres, deja el collider como Trigger.
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnMouseDown()
    {
        if (!cam || drawAreaCollider == null) return;

        dragging = true;

        originalPos = transform.position;
        originalParent = transform.parent;

        if (cloneOnDrag)
        {
            draggedObj = Instantiate(gameObject);
            draggedObj.name = gameObject.name + "_Placed";

            draggedSR = draggedObj.GetComponent<SpriteRenderer>();
            draggedCol = draggedObj.GetComponent<Collider2D>();

            // No queremos que el clon vuelva a actuar como “paleta” (arrastrar copias infinitas de copias)
            // Entonces desactivamos cloneOnDrag en el clon:
            var cloneScript = draggedObj.GetComponent<StickerDragWorld>();
            if (cloneScript != null)
            {
                cloneScript.cloneOnDrag = false;
                cloneScript.destroyIfDroppedOutside = false; // ya es colocado/arrastrable si quisieras
                cloneScript.drawAreaCollider = drawAreaCollider;
            }
        }
        else
        {
            draggedObj = gameObject;
            draggedSR = GetComponent<SpriteRenderer>();
            draggedCol = GetComponent<Collider2D>();
        }

        // Evita que el collider interfiera durante drag (opcional)
        if (draggedCol != null) draggedCol.enabled = false;
    }

    private void OnMouseDrag()
    {
        if (!dragging || draggedObj == null || !cam) return;

        Vector3 mp = Input.mousePosition;
        mp.z = Mathf.Abs(cam.transform.position.z - dragZ);
        Vector3 world = cam.ScreenToWorldPoint(mp);

        if (snapZToZero) world.z = 0f;

        draggedObj.transform.position = world;
    }

    private void OnMouseUp()
    {
        if (!dragging || draggedObj == null || drawAreaCollider == null) return;

        dragging = false;

        // Validar si soltó dentro del área
        Vector3 pos = draggedObj.transform.position;
        bool inside = drawAreaCollider.OverlapPoint(pos);

        if (inside)
        {
            // ? Asignar sorting layer/order para apilar igual que líneas
            if (Linea.Instance != null && draggedSR != null)
            {
                // Usa la sorting layer del linePrefab si quieres consistencia
                draggedSR.sortingLayerID = Linea.Instance.GetSortingLayerID();
                draggedSR.sortingOrder = Linea.Instance.AllocateNextOrderInLayer();

                // ? Registrar Undo global
                Linea.Instance.RegisterUndo(draggedObj);
            }

            // Habilitar collider ya pegado (trigger) por si quieres seleccionarlo luego
            if (draggedCol != null) draggedCol.enabled = true;
        }
        else
        {
            if (cloneOnDrag)
            {
                // Si era clon y cae fuera: destruirlo
                Destroy(draggedObj);
            }
            else
            {
                // Si estás moviendo el original
                if (destroyIfDroppedOutside)
                {
                    Destroy(draggedObj);
                }
                else
                {
                    draggedObj.transform.position = originalPos;
                    draggedObj.transform.SetParent(originalParent, true);
                    if (draggedCol != null) draggedCol.enabled = true;
                }
            }
        }

        draggedObj = null;
        draggedSR = null;
        draggedCol = null;
    }
}