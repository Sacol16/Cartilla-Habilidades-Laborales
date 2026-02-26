// ===========================
// CustomerServiceGameManager.cs
// - UI de pedido con 3 slots (iconos)
// - Se rellenan/chequean cuando aciertas
// ===========================

using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CustomerServiceGameManager : MonoBehaviour
{
    [Header("Customer Logic")]
    [SerializeField] private CustomerOrder customerOrder;

    [Header("Order Slots UI (3)")]
    [SerializeField] private OrderSlotUI[] orderSlots = new OrderSlotUI[3];

    [Header("UI Text")]
    [SerializeField] private TMP_Text customerNameText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip correctSfx;
    [SerializeField] private AudioClip wrongCustomerSfx;

    [Header("Feedback Messages")]
    [SerializeField] private string correctMsg = "¡Perfecto!";
    [SerializeField] private string wrongMsg = "Ese no es el producto.";

    [Header("Product Catalog (id -> icon)")]
    [SerializeField] private List<ProductIcon> productIcons = new List<ProductIcon>();

    [System.Serializable]
    public class ProductIcon
    {
        public string productId;
        public Sprite icon;
    }

    [Header("Customers Orders (max 3 items each)")]
    [SerializeField] private List<CustomerOrderData> customers = new List<CustomerOrderData>();

    [System.Serializable]
    public class CustomerOrderData
    {
        public string customerName;
        public List<string> products; // máximo 3
    }

    private Dictionary<string, Sprite> iconMap;
    private int currentCustomerIndex = 0;

    private void Awake()
    {
        iconMap = new Dictionary<string, Sprite>();
        foreach (var p in productIcons)
        {
            if (p != null && !string.IsNullOrEmpty(p.productId) && p.icon != null)
            {
                if (!iconMap.ContainsKey(p.productId))
                    iconMap.Add(p.productId, p.icon);
            }
        }
    }

    private void Start()
    {
        LoadCustomer(0);
    }

    private void LoadCustomer(int index)
    {
        if (customerOrder == null) return;
        if (customers == null || customers.Count == 0) return;

        currentCustomerIndex = Mathf.Clamp(index, 0, customers.Count - 1);

        var data = customers[currentCustomerIndex];

        // Seguridad: limitar a 3
        List<string> order = new List<string>();
        for (int i = 0; i < data.products.Count && i < 3; i++)
            order.Add(data.products[i]);

        customerOrder.SetOrder(order);

        // UI nombre
        if (customerNameText != null)
            customerNameText.text = data.customerName;

        // Reset slots
        for (int i = 0; i < orderSlots.Length; i++)
            if (orderSlots[i] != null) orderSlots[i].ResetSlot();

        // Set slots con iconos
        for (int i = 0; i < order.Count && i < orderSlots.Length; i++)
        {
            var slot = orderSlots[i];
            if (slot == null) continue;

            Sprite icon = GetIcon(order[i]);
            slot.SetProduct(order[i], icon);
        }

        SetFeedback("");
    }

    private Sprite GetIcon(string productId)
    {
        if (string.IsNullOrEmpty(productId)) return null;
        if (iconMap != null && iconMap.TryGetValue(productId, out Sprite icon))
            return icon;

        return null;
    }

    public bool TryDeliverProduct(string productId)
    {
        if (customerOrder == null) return false;

        bool completed;
        bool ok = customerOrder.TryDeliver(productId, out completed);

        if (ok)
        {
            // marcar slot correspondiente
            MarkSlotDelivered(productId);

            SetFeedback(correctMsg);
            Play(correctSfx);

            // ¿pedido completo?
            if (AreAllSlotsDelivered())
            {
                SetFeedback("¡Pedido completo!");
                Invoke(nameof(NextCustomer), 0.7f);
            }

            return true;
        }
        else
        {
            SetFeedback(wrongMsg);
            Play(wrongCustomerSfx);
            return false;
        }
    }

    private void MarkSlotDelivered(string productId)
    {
        for (int i = 0; i < orderSlots.Length; i++)
        {
            var slot = orderSlots[i];
            if (slot == null) continue;

            if (!slot.Delivered && slot.ProductId == productId)
            {
                slot.MarkDelivered();
                return;
            }
        }
    }

    private bool AreAllSlotsDelivered()
    {
        // Solo cuentan slots que tengan ProductId asignado
        for (int i = 0; i < orderSlots.Length; i++)
        {
            var slot = orderSlots[i];
            if (slot == null) continue;

            if (!string.IsNullOrEmpty(slot.ProductId) && !slot.Delivered)
                return false;
        }
        return true;
    }

    private void NextCustomer()
    {
        int next = currentCustomerIndex + 1;
        if (next >= customers.Count)
        {
            SetFeedback("¡Terminaste el minijuego! ??");
            return;
        }

        LoadCustomer(next);
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null) feedbackText.text = msg;
    }

    private void Play(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}