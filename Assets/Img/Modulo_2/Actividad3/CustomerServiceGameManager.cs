// ===========================
// CustomerServiceGameManager.cs
// - UI de pedido con 3 slots (iconos)
// - Se rellenan/chequean cuando aciertas
// - Cambia el sprite del cliente según el customer
// - Feedback aleatorio + se oculta solo a los X segundos
// - Al terminar: activa un GameObject (endPanel / siguiente paso) en vez de mostrar texto final
// ===========================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerServiceGameManager : MonoBehaviour
{
    [Header("Customer Logic")]
    [SerializeField] private CustomerOrder customerOrder;

    [Header("Customer UI")]
    [SerializeField] private TMP_Text customerNameText;
    [SerializeField] private Image customerImage;

    [Header("Order Slots UI (3)")]
    [SerializeField] private OrderSlotUI[] orderSlots = new OrderSlotUI[3];

    [Header("Feedback UI")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float feedbackSeconds = 1.2f;

    [Header("End (Activate GameObject)")]
    [Tooltip("Se activa cuando termina el último cliente (panel final, botón continuar, etc).")]
    [SerializeField] private GameObject onMinigameFinishedActivate;

    [Tooltip("Opcional: desactiva este GO al terminar (ej: UI del minijuego).")]
    [SerializeField] private GameObject onMinigameFinishedDeactivate;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip correctSfx;
    [SerializeField] private AudioClip wrongCustomerSfx;

    [Header("Feedback Messages (Random)")]
    [SerializeField]
    private List<string> correctMessages = new List<string>
    {
        "¡Perfecto!",
        "¡Muy bien!",
        "¡Eso es!",
        "¡Excelente elección!",
        "¡Genial, justo ese!",
        "¡Listo, aquí tienes!",
        "¡Pedido en camino!",
        "¡Buen servicio!"
    };

    [SerializeField]
    private List<string> wrongMessages = new List<string>
    {
        "Ese no era",
        "No coincide con el pedido.",
        "Ups, eso no lo pidió.",
        "Revisa el pedido otra vez.",
        "Ese producto no va aquí.",
        "Casi… pero no es.",
        "El cliente no pidió eso.",
        "Intenta con otro."
    };

    [SerializeField]
    private List<string> orderCompletedMessages = new List<string>
    {
        "¡Pedido completo!",
        "¡Todo listo, gracias!",
        "¡Perfecto, ya quedó!",
        "¡Excelente! Pedido finalizado.",
        "¡Listo! Siguiente cliente…"
    };

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
        public Sprite customerSprite;
        public List<string> products; // máximo 3
    }

    private Dictionary<string, Sprite> iconMap;
    private int currentCustomerIndex = 0;

    private Coroutine feedbackRoutine;
    private int lastCorrectIndex = -1;
    private int lastWrongIndex = -1;
    private int lastCompleteIndex = -1;

    private bool minigameFinished = false;

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

        if (feedbackText != null)
            feedbackText.text = "";

        if (onMinigameFinishedActivate != null)
            onMinigameFinishedActivate.SetActive(false);
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

        if (customerNameText != null)
            customerNameText.text = data.customerName;

        if (customerImage != null)
        {
            customerImage.sprite = data.customerSprite;
            customerImage.enabled = (data.customerSprite != null);
        }

        for (int i = 0; i < orderSlots.Length; i++)
            if (orderSlots[i] != null) orderSlots[i].ResetSlot();

        for (int i = 0; i < order.Count && i < orderSlots.Length; i++)
        {
            var slot = orderSlots[i];
            if (slot == null) continue;

            Sprite icon = GetIcon(order[i]);
            slot.SetProduct(order[i], icon);
        }

        ShowFeedback("", 0f);
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
        if (minigameFinished) return false;
        if (customerOrder == null) return false;

        bool completed;
        bool ok = customerOrder.TryDeliver(productId, out completed);

        if (ok)
        {
            MarkSlotDelivered(productId);

            ShowFeedback(GetRandomNonRepeat(correctMessages, ref lastCorrectIndex), feedbackSeconds);
            Play(correctSfx);

            if (AreAllSlotsDelivered())
            {
                ShowFeedback(GetRandomNonRepeat(orderCompletedMessages, ref lastCompleteIndex), feedbackSeconds);
                Invoke(nameof(NextCustomer), 0.7f);
            }

            return true;
        }
        else
        {
            ShowFeedback(GetRandomNonRepeat(wrongMessages, ref lastWrongIndex), feedbackSeconds);
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
            FinishMinigame(); // <-- CAMBIO CLAVE
            return;
        }

        LoadCustomer(next);
    }

    private void FinishMinigame()
    {
        if (minigameFinished) return;
        minigameFinished = true;

        // limpiar feedback en pantalla
        ShowFeedback("", 0f);

        // activar/desactivar lo que necesites
        if (onMinigameFinishedActivate != null)
            onMinigameFinishedActivate.SetActive(true);

        if (onMinigameFinishedDeactivate != null)
            onMinigameFinishedDeactivate.SetActive(false);
    }

    // ===== Feedback que se auto-oculta =====
    private void ShowFeedback(string msg, float seconds)
    {
        if (feedbackText == null) return;

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(FeedbackCoroutine(msg, seconds));
    }

    private IEnumerator FeedbackCoroutine(string msg, float seconds)
    {
        feedbackText.text = msg;

        if (seconds > 0f)
        {
            yield return new WaitForSeconds(seconds);

            if (feedbackText.text == msg)
                feedbackText.text = "";
        }
    }

    // ===== Random sin repetir el último =====
    private string GetRandomNonRepeat(List<string> list, ref int lastIndex)
    {
        if (list == null || list.Count == 0) return "";

        if (list.Count == 1)
        {
            lastIndex = 0;
            return list[0];
        }

        int idx;
        do { idx = Random.Range(0, list.Count); }
        while (idx == lastIndex);

        lastIndex = idx;
        return list[idx];
    }

    private void Play(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}