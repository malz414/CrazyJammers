using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using EasyTransition;

public class ShopMenuController : MonoBehaviour
{
    private class ShopRowUI
    {
        public Button rowButton;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI quantityText;
        public GameObject incrementGroup;
        public Button plusButton;
        public Button minusButton;
    }

    [Header("Dynamic Row Parent")]
    [Tooltip("Drag 'Item Button Group' here.")]
    [SerializeField] private Transform itemButtonGroupParent;

    [Header("Top & Bottom Details UI")]
    [SerializeField] private TextMeshProUGUI galdCountText;
    [SerializeField] private TextMeshProUGUI selectedItemNameText;
    [SerializeField] private TextMeshProUGUI selectedItemDescText;
    [SerializeField] private TextMeshProUGUI selectedItemOwnedText;
    [SerializeField] private TextMeshProUGUI selectMultipleButtonText;
    [SerializeField] private Button selectMultipleButton;
    [SerializeField] private Button buyButton;

    [Header("First Time Shop Unlock")]
    [SerializeField] private GameObject shopUnlockNotifOverlay;

    [Header("Navigation")]
    public TransitionSettings transitionSettings;
    public string mainMenuSceneName = "MainMenu";
    public string endlessSceneName = "EndlessModeScene";

    private List<EndlessItemDefinition> shopCatalog = new List<EndlessItemDefinition>();
    private List<ShopRowUI> rows = new List<ShopRowUI>();

    private int viewingIndex = -1; // -1 = nothing selected yet so all increment groups start hidden
    private bool isMultiSelectMode = false;

    // Key = shopCatalog index, Value = desired buy quantity (>= 1)
    private Dictionary<int, int> cartQuantities = new Dictionary<int, int>();

    private void Start()
    {
        if (EndlessShopData.IsShopUnlocked && !EndlessShopData.HasShownShopUnlockNotif && shopUnlockNotifOverlay != null)
        {
            shopUnlockNotifOverlay.SetActive(true);
            EndlessShopData.HasShownShopUnlockNotif = true;
        }

        shopCatalog = EndlessShopData.GetShopCatalog();
        AutoBindRowButtons();

        if (selectMultipleButton != null)
        {
            selectMultipleButton.onClick.RemoveAllListeners();
            selectMultipleButton.onClick.AddListener(OnToggleMultiSelect);
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        RefreshAllUI();
    }

   private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[Debug] All PlayerPrefs Wiped! Restarting scene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        // Temporary Click Raycast Debugger
        if (Input.GetMouseButtonDown(0) && EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count == 0)
            {
                Debug.Log("[ShopClickDebug] Clicked nothing on UI.");
            }
            else
            {
                string hitChain = "";
                for (int i = 0; i < results.Count; i++)
                {
                    hitChain += $"\n  #{i + 1}: {results[i].gameObject.name} (Parent: {results[i].gameObject.transform.parent?.name})";
                }
                Debug.Log($"[ShopClickDebug] Top UI hit was '{results[0].gameObject.name}':{hitChain}");
            }
        }
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Trim() == targetName) return child;
            Transform found = FindChildRecursive(child, targetName);
            if (found != null) return found;
        }
        return null;
    }

    private void AutoBindRowButtons()
    {
        if (itemButtonGroupParent == null) return;
        rows.Clear();

        List<Button> rawButtons = new List<Button>();
        foreach (Transform child in itemButtonGroupParent)
        {
            if (child.name.StartsWith("Item Button"))
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null) rawButtons.Add(btn);
            }
        }

        while (rawButtons.Count < shopCatalog.Count && rawButtons.Count > 0)
        {
            Button clone = Instantiate(rawButtons[rawButtons.Count - 1], itemButtonGroupParent);
            clone.transform.SetSiblingIndex(rawButtons.Count);
            rawButtons.Add(clone);
        }

        for (int i = 0; i < rawButtons.Count; i++)
        {
            int index = i;
            Button rowBtn = rawButtons[i];

            if (i < shopCatalog.Count)
            {
                rowBtn.gameObject.SetActive(true);
                rowBtn.onClick.RemoveAllListeners();
                rowBtn.onClick.AddListener(() => ViewItem(index));

                Transform rowTransform = rowBtn.transform;
                ShopRowUI rowUI = new ShopRowUI { rowButton = rowBtn };

                Transform priceTf = FindChildRecursive(rowTransform, "itemPriceText");
                Transform nameTf = FindChildRecursive(rowTransform, "Item Name Text");
                Transform qtyTf = FindChildRecursive(rowTransform, "ItemQuantityText");

                if (priceTf != null) rowUI.priceText = priceTf.GetComponent<TextMeshProUGUI>();
                if (nameTf != null) rowUI.nameText = nameTf.GetComponent<TextMeshProUGUI>();
                if (qtyTf != null) rowUI.quantityText = qtyTf.GetComponent<TextMeshProUGUI>();

                if (rowUI.priceText == null || rowUI.nameText == null || rowUI.quantityText == null)
                {
                    TextMeshProUGUI[] fallbackTexts = rowBtn.GetComponentsInChildren<TextMeshProUGUI>(true);
                    if (rowUI.priceText == null && fallbackTexts.Length > 0) rowUI.priceText = fallbackTexts[0];
                    if (rowUI.nameText == null && fallbackTexts.Length > 1) rowUI.nameText = fallbackTexts[1];
                    if (rowUI.quantityText == null && fallbackTexts.Length > 2) rowUI.quantityText = fallbackTexts[2];
                }

                Transform incGroupTf = FindChildRecursive(rowTransform, "Increment Button Group");
                Transform plusTf = FindChildRecursive(rowTransform, "PlusButton");
                Transform minusTf = FindChildRecursive(rowTransform, "MinusButton");

                if (incGroupTf != null)
                {
                    rowUI.incrementGroup = incGroupTf.gameObject;
                    rowUI.incrementGroup.SetActive(false);
                }

                if (plusTf != null)
                {
                    rowUI.plusButton = plusTf.GetComponent<Button>();
                    if (rowUI.plusButton != null)
                    {
                        rowUI.plusButton.onClick.RemoveAllListeners();
                        rowUI.plusButton.onClick.AddListener(() => OnIncrementUpClicked(index));
                    }
                }

                if (minusTf != null)
                {
                    rowUI.minusButton = minusTf.GetComponent<Button>();
                    if (rowUI.minusButton != null)
                    {
                        rowUI.minusButton.onClick.RemoveAllListeners();
                        rowUI.minusButton.onClick.AddListener(() => OnIncrementDownClicked(index));
                    }
                }

                Debug.Log($"[ShopBindDebug] Row {index} ({rowBtn.name}): IncGroup={(rowUI.incrementGroup != null)}, Plus={(rowUI.plusButton != null)}, Minus={(rowUI.minusButton != null)}");
                rows.Add(rowUI);
            }
            else
            {
                rowBtn.gameObject.SetActive(false);
            }
        }
    }

    public void OnIncrementUpClicked(int index)
    {
        Debug.Log($"[ShopIncDebug] Plus clicked for row {index}");
        if (index < 0 || index >= shopCatalog.Count) return;

        viewingIndex = index;
        int stockLeft = GetRemainingShopStock(index);
        int currentQty = cartQuantities.ContainsKey(index) ? cartQuantities[index] : 0;
        Debug.Log($"[ShopIncDebug] Plus check -> currentQty={currentQty}, stockLeft={stockLeft}");

        if (stockLeft <= 0) return;

        if (currentQty < stockLeft)
        {
            cartQuantities[index] = currentQty + 1;
            RefreshAllUI();
        }
    }

    public void OnIncrementDownClicked(int index)
    {
        Debug.Log($"[ShopIncDebug] Minus clicked for row {index}");
        if (index < 0 || index >= shopCatalog.Count) return;
        if (!cartQuantities.ContainsKey(index)) return;

        viewingIndex = index;
        int currentQty = cartQuantities[index];
        Debug.Log($"[ShopIncDebug] Minus check -> currentQty={currentQty}");

        if (currentQty > 1)
        {
            cartQuantities[index] = currentQty - 1;
            RefreshAllUI();
        }
    }
    public void ViewItem(int index)
    {
        if (index < 0 || index >= shopCatalog.Count) return;

        int stockLeft = GetRemainingShopStock(index);
        viewingIndex = index;

        if (isMultiSelectMode)
        {
            if (cartQuantities.ContainsKey(index))
            {
                cartQuantities.Remove(index);
            }
            else if (stockLeft > 0)
            {
                cartQuantities[index] = 1;
            }
        }
        else
        {
            cartQuantities.Clear();
            if (stockLeft > 0) cartQuantities[index] = 1;
        }

        RefreshAllUI();
    }

    public void OnToggleMultiSelect()
    {
        isMultiSelectMode = !isMultiSelectMode;
        cartQuantities.Clear();

        if (viewingIndex >= 0 && viewingIndex < shopCatalog.Count && GetRemainingShopStock(viewingIndex) > 0)
        {
            cartQuantities[viewingIndex] = 1;
        }

        RefreshAllUI();
    }



    public void OnBuyClicked()
    {
        if (cartQuantities.Count == 0) return;

        int totalCost = 0;
        foreach (var kvp in cartQuantities)
        {
            var item = shopCatalog[kvp.Key];
            int stockLeft = GetRemainingShopStock(kvp.Key);
            if (kvp.Value <= 0 || kvp.Value > stockLeft) return;
            totalCost += item.price * kvp.Value;
        }

        if (EndlessShopData.TotalGald < totalCost) return;

        EndlessShopData.TotalGald -= totalCost;
        foreach (var kvp in cartQuantities)
        {
            EndlessShopData.AddPermanentItem(shopCatalog[kvp.Key].itemId, kvp.Value);
        }

        cartQuantities.Clear();
        isMultiSelectMode = false;

        if (viewingIndex >= 0 && viewingIndex < shopCatalog.Count && GetRemainingShopStock(viewingIndex) > 0)
        {
            cartQuantities[viewingIndex] = 1;
        }

        RefreshAllUI();
    }

    private int GetRemainingShopStock(int index)
    {
        var item = shopCatalog[index];
        int owned = EndlessShopData.GetTotalEndlessOwned(item.itemId);
        return Mathf.Max(0, item.maxCarryLimit - owned);
    }

    public void RefreshAllUI()
    {
        if (galdCountText != null) galdCountText.text = $"{EndlessShopData.TotalGald}G";

        int totalCost = 0;
        bool stockValid = cartQuantities.Count > 0;

        foreach (var kvp in cartQuantities)
        {
            int stockLeft = GetRemainingShopStock(kvp.Key);
            if (kvp.Value <= 0 || kvp.Value > stockLeft) stockValid = false;
            totalCost += shopCatalog[kvp.Key].price * kvp.Value;
        }

        for (int i = 0; i < shopCatalog.Count && i < rows.Count; i++)
        {
            var item = shopCatalog[i];
            var rowUI = rows[i];
            int stockLeft = GetRemainingShopStock(i);
            bool inCart = cartQuantities.ContainsKey(i);
            int buyQty = inCart ? cartQuantities[i] : 0;

            string prefix = isMultiSelectMode ? (inCart ? "[X] " : "[ ] ") : "";

            if (rowUI.priceText != null) rowUI.priceText.text = $"{item.price}G";
            if (rowUI.nameText != null) rowUI.nameText.text = prefix + item.displayName;
            if (rowUI.quantityText != null) rowUI.quantityText.text = inCart ? $"{buyQty}/{stockLeft}" : stockLeft.ToString();

            // Show and enable this row's +/- buttons only when this item is selected and in stock
            if (rowUI.incrementGroup != null)
            {
                bool showRowIncrement = inCart && stockLeft > 0;
                rowUI.incrementGroup.SetActive(showRowIncrement);

                if (showRowIncrement)
                {
                    if (rowUI.plusButton != null) rowUI.plusButton.interactable = buyQty < stockLeft;
                    if (rowUI.minusButton != null) rowUI.minusButton.interactable = buyQty > 1;
                }
            }

            CanvasGroup cg = rowUI.rowButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = rowUI.rowButton.gameObject.AddComponent<CanvasGroup>();

            if (isMultiSelectMode) cg.alpha = inCart ? 1.0f : 0.5f;
            else cg.alpha = (i == viewingIndex) ? 1.0f : 0.5f;
        }

        bool hasValidSelection = viewingIndex >= 0 && viewingIndex < shopCatalog.Count;
        if (hasValidSelection)
        {
            var current = shopCatalog[viewingIndex];
            int owned = EndlessShopData.GetTotalEndlessOwned(current.itemId);

            if (selectedItemNameText != null) selectedItemNameText.text = current.displayName;
            if (selectedItemDescText != null) selectedItemDescText.text = current.description;
            if (selectedItemOwnedText != null) selectedItemOwnedText.text = $" x{owned}";
        }
        else
        {
            if (selectedItemNameText != null) selectedItemNameText.text = "";
            if (selectedItemDescText != null) selectedItemDescText.text = "Select an item.";
            if (selectedItemOwnedText != null) selectedItemOwnedText.text = "";
        }

        if (selectMultipleButtonText != null)
            selectMultipleButtonText.text = isMultiSelectMode ? "Cancel Multi-Select" : "Select Multiple";

        if (buyButton != null)
        {
            bool canAfford = EndlessShopData.TotalGald >= totalCost;
            buyButton.interactable = stockValid && canAfford && totalCost > 0;

            TextMeshProUGUI buyText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buyText != null) buyText.text = totalCost > 0 ? $"Buy ({totalCost}G)" : "Buy";
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