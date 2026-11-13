using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EasyTransition;

public class ShopMenuController : MonoBehaviour
{
    [Header("Testing")]
    [Tooltip("Check this to instantly give yourself 999,999 Gald on Start so you can test buying.")]
    public bool debugGiveInfiniteGald = false;

    [Header("Dynamic Row Parent")]
    [Tooltip("Drag 'Item Button Group' here. It will safely ignore your Buy/Multi-Select buttons.")]
    [SerializeField] private Transform itemButtonGroupParent;

    [Header("Top & Bottom Details UI")]
    [SerializeField] private TextMeshProUGUI galdCountText;
    [SerializeField] private TextMeshProUGUI selectedItemNameText;
    [SerializeField] private TextMeshProUGUI selectedItemDescText;
    [SerializeField] private TextMeshProUGUI selectedItemOwnedText;
    [SerializeField] private TextMeshProUGUI selectMultipleButtonText;
    [SerializeField] private Button selectMultipleButton;
    [SerializeField] private Button buyButton;

    [Header("Navigation")]
    public TransitionSettings transitionSettings;
    public string mainMenuSceneName = "MainMenu";
    public string endlessSceneName = "EndlessModeScene";

    private List<Button> rowButtons = new List<Button>();
    private int selectedIndex = 0;
    private int purchaseQuantity = 1;

    private void Start()
    {
        // 1. Give test money if the cheat is checked
        if (debugGiveInfiniteGald)
        {
            EndlessShopData.TotalGald = 999999;
        }

        AutoBindRowButtons();

        if (selectMultipleButton != null)
            selectMultipleButton.onClick.AddListener(OnCycleQuantityClicked);

        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        SelectItem(0);
        RefreshAllUI();
    }

    private void AutoBindRowButtons()
    {
        if (itemButtonGroupParent == null) return;

        rowButtons.Clear();
        foreach (Transform child in itemButtonGroupParent)
        {
            // 2. CRITICAL FIX: Only grab objects specifically named "Item Button"
            // This prevents the script from accidentally hijacking your Buy and Multi-Select buttons!
            if (child.name.StartsWith("Item Button"))
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null) rowButtons.Add(btn);
            }
        }

        while (rowButtons.Count < EndlessShopData.Catalog.Count && rowButtons.Count > 0)
        {
            Button clone = Instantiate(rowButtons[rowButtons.Count - 1], itemButtonGroupParent);
            // Move the clone to the bottom of the items, but ABOVE your Buy/Multi buttons
            clone.transform.SetSiblingIndex(rowButtons.Count); 
            rowButtons.Add(clone);
        }

        for (int i = 0; i < rowButtons.Count; i++)
        {
            int index = i;
            if (i < EndlessShopData.Catalog.Count)
            {
                rowButtons[i].gameObject.SetActive(true);
                rowButtons[i].onClick.RemoveAllListeners();
                rowButtons[i].onClick.AddListener(() => SelectItem(index));
            }
            else
            {
                rowButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectItem(int index)
    {
        if (index < 0 || index >= EndlessShopData.Catalog.Count) return;
        selectedIndex = index;
        purchaseQuantity = 1;
        RefreshAllUI();
    }

    public void OnCycleQuantityClicked()
    {
        if (EndlessShopData.Catalog.Count == 0) return;
        var item = EndlessShopData.Catalog[selectedIndex];
        int owned = EndlessShopData.GetPermanentItemCount(item.itemId);
        
        // Calculate max amount you can physically carry vs financially afford
        int maxCanBuyByLimit = Mathf.Max(1, item.maxCarryLimit - owned);
        int maxCanAfford = item.price > 0 ? (EndlessShopData.TotalGald / item.price) : 99;
        
        // Clamp the selection. If you have 0 money, maxAllowed drops to 1 so the UI doesn't break, but Buy stays disabled.
        int maxAllowed = Mathf.Max(1, Mathf.Min(maxCanBuyByLimit, maxCanAfford));

        purchaseQuantity++;
        if (purchaseQuantity > maxAllowed) purchaseQuantity = 1;

        RefreshAllUI();
    }

    public void OnBuyClicked()
    {
        if (EndlessShopData.Catalog.Count == 0) return;
        var item = EndlessShopData.Catalog[selectedIndex];
        int owned = EndlessShopData.GetPermanentItemCount(item.itemId);

        if (owned + purchaseQuantity > item.maxCarryLimit)
        {
            purchaseQuantity = 1;
            RefreshAllUI();
            return;
        }

        int totalCost = item.price * purchaseQuantity;
        if (EndlessShopData.TotalGald >= totalCost)
        {
            EndlessShopData.TotalGald -= totalCost;
            EndlessShopData.AddPermanentItem(item.itemId, purchaseQuantity);
            purchaseQuantity = 1;
            RefreshAllUI();
        }
    }

    public void RefreshAllUI()
    {
        if (galdCountText != null)
            galdCountText.text = $"{EndlessShopData.TotalGald}G";

        for (int i = 0; i < EndlessShopData.Catalog.Count && i < rowButtons.Count; i++)
        {
            var item = EndlessShopData.Catalog[i];
            int owned = EndlessShopData.GetPermanentItemCount(item.itemId);

            TextMeshProUGUI[] rowTexts = rowButtons[i].GetComponentsInChildren<TextMeshProUGUI>(true);
            if (rowTexts.Length > 0) rowTexts[0].text = item.displayName;
            if (rowTexts.Length > 1) rowTexts[1].text = $"{item.price}G";
            if (rowTexts.Length > 2) rowTexts[2].text = $"x{owned:00}";
        }

        if (EndlessShopData.Catalog.Count > 0 && selectedIndex < EndlessShopData.Catalog.Count)
        {
            var current = EndlessShopData.Catalog[selectedIndex];
            int owned = EndlessShopData.GetPermanentItemCount(current.itemId);
            int totalCost = current.price * purchaseQuantity;

            if (selectedItemNameText != null) selectedItemNameText.text = current.displayName;
            if (selectedItemDescText != null) selectedItemDescText.text = current.description;
            if (selectedItemOwnedText != null) selectedItemOwnedText.text = $"Owned x{owned:00}";

            if (selectMultipleButtonText != null)
                selectMultipleButtonText.text = purchaseQuantity > 1 ? $"Qty: x{purchaseQuantity}" : "Select Multiple";

            if (buyButton != null)
            {
                bool canAfford = EndlessShopData.TotalGald >= totalCost;
                bool underLimit = (owned + purchaseQuantity) <= current.maxCarryLimit;
                buyButton.interactable = canAfford && underLimit;
            }
        }
    }

    public void OnTitleScreenClicked()
    {
        if (TransitionManager.Instance() != null && transitionSettings != null)
            TransitionManager.Instance().Transition(mainMenuSceneName, transitionSettings, 0f);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnEndlessModeClicked()
    {
        if (TransitionManager.Instance() != null && transitionSettings != null)
            TransitionManager.Instance().Transition(endlessSceneName, transitionSettings, 0f);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(endlessSceneName);
    }
}