// ===========================
// SectorsMatchCompleteActivator.cs
// - Activa un GameObject cuando los 3 slots están correctos (ocupados)
// - Úsalo para mostrar panel final / botón continuar
// ===========================

using UnityEngine;

public class SectorsMatchCompleteActivator : MonoBehaviour
{
    [Header("Slots (3)")]
    [Tooltip("Arrastra aquí tus 3 DropSlotUI2 (Primario/Secundario/Terciario).")]
    [SerializeField] private DropSlotUI2[] slots = new DropSlotUI2[3];

    [Header("On Complete")]
    [SerializeField] private GameObject toActivate;
    [SerializeField] private GameObject toDeactivate; // opcional

    [Tooltip("Si quieres que revise cada frame. Si lo apagas, puedes llamar CheckNow() desde un botón o evento.")]
    [SerializeField] private bool checkContinuously = true;

    private bool fired = false;

    private void Awake()
    {
        if (toActivate != null) toActivate.SetActive(false);
    }

    private void Update()
    {
        if (!checkContinuously) return;
        if (fired) return;

        if (AreAllPlaced())
            FireComplete();
    }

    public void CheckNow()
    {
        if (fired) return;
        if (AreAllPlaced())
            FireComplete();
    }

    private bool AreAllPlaced()
    {
        if (slots == null || slots.Length == 0) return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) return false;
            if (!slots[i].IsOccupied) return false; // <- clave
        }
        return true;
    }

    private void FireComplete()
    {
        fired = true;

        if (toActivate != null)
            toActivate.SetActive(true);

        if (toDeactivate != null)
            toDeactivate.SetActive(false);
    }
}