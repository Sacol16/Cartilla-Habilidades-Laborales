using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinigameOrgManager : MonoBehaviour
{
    [Header("Feedback Text")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float messageSeconds = 1.5f;

    [Header("Feedback Colors")]
    [SerializeField] private Color correctTextColor = new Color(0.2f, 0.85f, 0.35f, 1f);
    [SerializeField] private Color wrongTextColor = new Color(0.95f, 0.25f, 0.25f, 1f);

    [Header("Slot Colors (optional)")]
    [SerializeField] private Color okSlotColor = new Color(0.2f, 0.85f, 0.35f, 1f);

    [Header("Employees Group")]
    [SerializeField] private string employeesGroupId = "EMPLOYEES";
    [Tooltip("Arrastra aquí los 4 DropSlots de empleados (los de abajo).")]
    [SerializeField] private List<DropSlot> employeeSlots = new List<DropSlot>();

    [Header("FX (optional)")]
    [SerializeField] private float popScale = 1.08f;
    [SerializeField] private float popTime = 0.10f;
    [SerializeField] private float shakeTime = 0.16f;
    [SerializeField] private float shakeAmount = 10f;

    private Coroutine msgCo;

    // === ENTRYPOINT ===
    public void HandleDrop(PieceDragUI piece, DropSlot droppedOn)
    {
        if (piece == null || droppedOn == null)
        {
            if (piece != null) piece.ReturnToStart();
            return;
        }

        // ===== EMPLEADOS =====
        if (piece.cargo == CargoType.Empleado)
        {
            // Solo se acepta en slots del grupo EMPLOYEES
            if (!IsEmployeesSlot(droppedOn))
            {
                Reject(piece, "Los empleados van en la parte inferior. ¡Inténtalo de nuevo!");
                return;
            }

            PlaceEmployee(piece, droppedOn);
            return;
        }

        // ===== DIRECTORES (cargos únicos) =====
        TryPlaceUnique(piece, droppedOn);
    }

    private void TryPlaceUnique(PieceDragUI piece, DropSlot slot)
    {
        // Si lo suelta en zona empleados, es incorrecto
        if (IsEmployeesSlot(slot))
        {
            Reject(piece, "Ese cargo no va en la zona de empleados. ¡Inténtalo de nuevo!");
            return;
        }

        if (slot.IsOccupied)
        {
            Reject(piece, "Ese espacio ya está ocupado. Prueba en otro lugar.");
            return;
        }

        if (piece.cargo != slot.acceptsCargo)
        {
            Reject(piece, "Ups, ese cargo no va ahí. ¡Inténtalo de nuevo!");
            return;
        }

        // Correcto
        Accept(piece, slot);
        // ✅ Registrar en manager (esto llena DG/DF/DP)
        Module3ActivityManager.Instance?.RegisterPlacement(slot.slotIndex, piece.cargo.ToString());
        // ✅ Por si quieres forzar check inmediato
        Module3ActivityManager.Instance?.ForceRecheck();
    }

    private void PlaceEmployee(PieceDragUI piece, DropSlot droppedOn)
    {
        // Si el slot donde lo soltó está libre, se queda ahí
        if (!droppedOn.IsOccupied)
        {
            Accept(piece, droppedOn);

            // Para empleados no necesitas registrar por slotIndex único,
            // pero igual puedes registrar si quieres trazabilidad:
            Module3ActivityManager.Instance?.RegisterPlacement(droppedOn.slotIndex, "Empleado");
            Module3ActivityManager.Instance?.ForceRecheck();
            return;
        }

        // Si está ocupado, busca otro libre en el grupo
        var free = FindFirstFreeEmployeeSlot();
        if (free != null)
        {
            Accept(piece, free);
            Module3ActivityManager.Instance?.RegisterPlacement(free.slotIndex, "Empleado");
            Module3ActivityManager.Instance?.ForceRecheck();
            return;
        }

        Reject(piece, "No hay espacios disponibles para empleados.");
    }

    private bool IsEmployeesSlot(DropSlot slot)
        => slot != null && slot.groupId == employeesGroupId;

    private DropSlot FindFirstFreeEmployeeSlot()
    {
        foreach (var s in employeeSlots)
            if (s != null && !s.IsOccupied) return s;
        return null;
    }

    // ===== ACCEPT / REJECT =====

    private void Accept(PieceDragUI piece, DropSlot slot)
    {
        piece.SnapToSlot(slot.Rect);
        piece.LockInPlace();

        slot.SetOccupied(true);

        // opcional: pintar el frame del slot
        if (slot.slotFrame != null)
            slot.slotFrame.color = okSlotColor;

        // opcional: animación pop
        StartCoroutine(Pop(slot.Rect));

        ShowMessage("¡Correcto!", true);
    }

    private void Reject(PieceDragUI piece, string msg)
    {
        piece.ReturnToStart();
        StartCoroutine(Shake(piece.Rect));
        ShowMessage(msg, false);
    }

    // ===== UI MESSAGE =====

    private void ShowMessage(string msg, bool correct)
    {
        if (feedbackText == null) return;

        if (msgCo != null) StopCoroutine(msgCo);
        msgCo = StartCoroutine(MessageRoutine(msg, correct));
    }

    private IEnumerator MessageRoutine(string msg, bool correct)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = msg;
        feedbackText.color = correct ? correctTextColor : wrongTextColor;

        yield return new WaitForSeconds(messageSeconds);

        feedbackText.gameObject.SetActive(false);
    }

    // ===== FX =====

    private IEnumerator Pop(RectTransform target)
    {
        if (target == null) yield break;

        Vector3 baseScale = target.localScale;
        Vector3 up = baseScale * popScale;

        float t = 0f;
        while (t < popTime)
        {
            t += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(baseScale, up, t / popTime);
            yield return null;
        }

        t = 0f;
        while (t < popTime)
        {
            t += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(up, baseScale, t / popTime);
            yield return null;
        }

        target.localScale = baseScale;
    }

    private IEnumerator Shake(RectTransform target)
    {
        if (target == null) yield break;

        Vector2 basePos = target.anchoredPosition;
        float t = 0f;

        while (t < shakeTime)
        {
            t += Time.unscaledDeltaTime;
            float x = Random.Range(-shakeAmount, shakeAmount);
            target.anchoredPosition = basePos + new Vector2(x, 0f);
            yield return null;
        }

        target.anchoredPosition = basePos;
    }
}