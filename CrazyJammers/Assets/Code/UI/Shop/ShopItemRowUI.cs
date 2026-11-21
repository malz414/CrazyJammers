using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopItemRowUI : MonoBehaviour
{
    [Header("Row UI References")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public TextMeshProUGUI itemQuantityText;
    public Button plusButton;
    public Button minusButton;

    [Header("Row State")]
    public int unitPrice = 10;
    public int maxQuantity = 99;
    public int currentQuantity { get; private set; } = 0;

    public event Action<ShopItemRowUI> OnQuantityChanged;

    void Awake()
    {
        if (plusButton != null)
        {
            plusButton.onClick.RemoveAllListeners();
            plusButton.onClick.AddListener(Increment);
        }

        if (minusButton != null)
        {
            minusButton.onClick.RemoveAllListeners();
            minusButton.onClick.AddListener(Decrement);
        }

        UpdateUI();
    }

    public void Setup(string itemName, int price, int maxQty = 99)
    {
        unitPrice = price;
        maxQuantity = maxQty;
        currentQuantity = 0;

        if (itemNameText != null) itemNameText.text = itemName;
        if (itemPriceText != null) itemPriceText.text = $"{price}g";

        UpdateUI();
    }

    public void Increment()
    {
        if (currentQuantity >= maxQuantity) return;

        currentQuantity++;
        UpdateUI();
        OnQuantityChanged?.Invoke(this);
    }

    public void Decrement()
    {
        if (currentQuantity <= 0) return;

        currentQuantity--;
        UpdateUI();
        OnQuantityChanged?.Invoke(this);
    }

    private void UpdateUI()
    {
        if (itemQuantityText != null)
            itemQuantityText.text = currentQuantity.ToString();

        if (minusButton != null)
            minusButton.interactable = currentQuantity > 0;

        if (plusButton != null)
            plusButton.interactable = currentQuantity < maxQuantity;
    }

    public int GetRowTotalCost() => currentQuantity * unitPrice;
}