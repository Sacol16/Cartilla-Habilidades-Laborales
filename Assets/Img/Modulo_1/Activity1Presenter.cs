using System;
using System.Collections.Generic;
using UnityEngine;

public class Module1Activity1Presenter : MonoBehaviour
{
    [Header("Slots (6) - arrastra los 6 slots en orden 0..5")]
    public GameObject[] slots = new GameObject[6];

    [Header("Items (12) - posibles respuestas (arrastra cada GO)")]
    public GameObject hogar;
    public GameObject comida;
    public GameObject salud;
    public GameObject mascota;
    public GameObject trabajo;
    public GameObject estudio;
    public GameObject metas;
    public GameObject valores;
    public GameObject amigos;
    public GameObject ahorro;
    public GameObject tiempoPersonal; // en data llega "tiempo personal"
    public GameObject familia;

    // Mapa nombre (como llega en DB) -> GameObject
    private Dictionary<string, GameObject> _itemsByName;

    // Para poder "resetear" items al aplicar data varias veces (opcional)
    private Dictionary<GameObject, Transform> _originalParentByItem;
    private Dictionary<GameObject, Vector3> _originalLocalPosByItem;
    private Dictionary<GameObject, Quaternion> _originalLocalRotByItem;
    private Dictionary<GameObject, Vector3> _originalLocalScaleByItem;

    private void Awake()
    {
        BuildItemMap();
        CacheOriginalItemTransforms();
    }

    private void BuildItemMap()
    {
        _itemsByName = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase)
        {
            { "hogar", hogar },
            { "comida", comida },
            { "salud", salud },
            { "mascota", mascota },
            { "trabajo", trabajo },
            { "estudio", estudio },
            { "metas", metas },
            { "valores", valores },
            { "amigos", amigos },
            { "ahorro", ahorro },
            { "tiempo personal", tiempoPersonal }, // ojo: viene con espacio
            { "tiempopersonal", tiempoPersonal },  // fallback por si llega sin espacio
            { "familia", familia },
        };
    }

    private void CacheOriginalItemTransforms()
    {
        _originalParentByItem = new Dictionary<GameObject, Transform>();
        _originalLocalPosByItem = new Dictionary<GameObject, Vector3>();
        _originalLocalRotByItem = new Dictionary<GameObject, Quaternion>();
        _originalLocalScaleByItem = new Dictionary<GameObject, Vector3>();

        foreach (var kv in _itemsByName)
        {
            var go = kv.Value;
            if (go == null) continue;

            // Evita duplicar si el mismo GO está mapeado por dos keys
            if (_originalParentByItem.ContainsKey(go)) continue;

            _originalParentByItem[go] = go.transform.parent;
            _originalLocalPosByItem[go] = go.transform.localPosition;
            _originalLocalRotByItem[go] = go.transform.localRotation;
            _originalLocalScaleByItem[go] = go.transform.localScale;
        }
    }

    /// <summary>
    /// Aplica la data de activity1: coloca cada item en su slotIndex.
    /// </summary>
    public void Apply(SlotPlacementDto[] placements)
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("[Activity1Presenter] No hay slots asignados.");
            return;
        }

        // (Opcional) limpia/reinicia items a su parent original antes de aplicar
        ResetAllItemsToOriginalParent();

        if (placements == null || placements.Length == 0)
        {
            Debug.Log("[Activity1Presenter] placements vacío: no hay nada que colocar.");
            return;
        }

        foreach (var p in placements)
        {
            if (p == null) continue;

            int slotIndex = p.slotIndex;
            string name = NormalizeName(p.itemObjectName);

            if (slotIndex < 0 || slotIndex >= slots.Length)
            {
                Debug.LogWarning($"[Activity1Presenter] slotIndex fuera de rango: {slotIndex}");
                continue;
            }

            var slotGO = slots[slotIndex];
            if (slotGO == null)
            {
                Debug.LogWarning($"[Activity1Presenter] Slot {slotIndex} es null (no asignado).");
                continue;
            }

            if (!_itemsByName.TryGetValue(name, out var itemGO) || itemGO == null)
            {
                Debug.LogWarning($"[Activity1Presenter] itemObjectName desconocido o no asignado: '{p.itemObjectName}' (normalizado: '{name}')");
                continue;
            }

            PlaceItemIntoSlot(itemGO, slotGO.transform);
        }
    }

    /// <summary>
    /// Extrae el estado actual de los slots (lee el primer hijo de cada slot y crea placements).
    /// Útil para guardar.
    /// </summary>
    public SlotPlacementDto[] Extract()
    {
        if (slots == null) return Array.Empty<SlotPlacementDto>();

        var result = new List<SlotPlacementDto>(slots.Length);

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;

            // Busca un item dentro del slot (primer hijo)
            if (slot.transform.childCount <= 0) continue;

            var child = slot.transform.GetChild(0).gameObject;
            if (child == null) continue;

            // Encuentra el nombre “canonical” para ese GO
            string itemName = FindNameForItem(child);
            if (string.IsNullOrEmpty(itemName)) itemName = child.name;

            result.Add(new SlotPlacementDto { slotIndex = i, itemObjectName = itemName });
        }

        return result.ToArray();
    }

    private void PlaceItemIntoSlot(GameObject itemGO, Transform slotTransform)
    {
        // Reparent
        itemGO.transform.SetParent(slotTransform, worldPositionStays: false);

        // Reset transform dentro del slot (para que quede centrado)
        itemGO.transform.localPosition = Vector3.zero;
        itemGO.transform.localRotation = Quaternion.identity;
        itemGO.transform.localScale = Vector3.one;

        // Si usas UI (RectTransform), esto ayuda a que se ajuste al centro
        var rt = itemGO.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
        }
    }

    private void ResetAllItemsToOriginalParent()
    {
        foreach (var kv in _originalParentByItem)
        {
            var itemGO = kv.Key;
            if (itemGO == null) continue;

            var parent = kv.Value;
            itemGO.transform.SetParent(parent, worldPositionStays: false);

            // restaura transform local original
            if (_originalLocalPosByItem.TryGetValue(itemGO, out var pos))
                itemGO.transform.localPosition = pos;
            if (_originalLocalRotByItem.TryGetValue(itemGO, out var rot))
                itemGO.transform.localRotation = rot;
            if (_originalLocalScaleByItem.TryGetValue(itemGO, out var sca))
                itemGO.transform.localScale = sca;

            var rt = itemGO.GetComponent<RectTransform>();
            if (rt != null && _originalLocalPosByItem.TryGetValue(itemGO, out var pos2))
            {
                // Si era UI, normalmente la posición original la quieres como anchoredPosition;
                // como no sabemos tu layout, lo dejamos centrado en su parent original si prefieres.
                // rt.anchoredPosition = Vector2.zero;
            }
        }
    }

    private string NormalizeName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        return raw.Trim().ToLowerInvariant();
    }

    private string FindNameForItem(GameObject itemGO)
    {
        // Devuelve la primera key cuyo value sea ese itemGO
        foreach (var kv in _itemsByName)
        {
            if (kv.Value == itemGO)
                return kv.Key; // ojo: podría devolver "tiempopersonal" si esa fue la primera key; no pasa nada
        }
        return null;
    }
}