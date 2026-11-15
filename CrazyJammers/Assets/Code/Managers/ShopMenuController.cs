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
    [Tooltip("Drag the Notification Overlay prefab here.")]
    [SerializeField] private GameObject shopUnlockNotifOverlay;

    [Header("Navigation")]
    public TransitionSettings transitionSettings;
    public string mainMenuSceneName = "MainMenu";
    public string endlessSceneName = "EndlessModeScene";

    private List<Button> rowButtons = new List<Button>();
    private int viewingIndex = 0; 
    
    private bool isMultiSelectMode = false;
    private HashSet<int> cartIndices = new HashSet<int>(); 

    private void Start()
    {
           // 1. Give test money if the cheat is checked
        if (debugGiveInfiniteGald)
        {
            EndlessShopData.TotalGald = 999999;
        }
        if (EndlessShopData.IsShopUnlocked && !EndlessShopData.HasShownShopUnlockNotif && shopUnlockNotifOverlay != null)
        {
            shopUnlockNotifOverlay.SetActive(true);
            EndlessShopData.HasShownShopUnlockNotif = true;
        }

        AutoBindRowButtons();

        if (selectMultipleButton != null)
            selectMultipleButton.onClick.AddListener(OnToggleMultiSelect);

        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        ViewItem(0);
    }



    private void AutoBindRowButtons()
    {
        if (itemButtonGroupParent == null) return;
        rowButtons.Clear();
        foreach (Transform child in itemButtonGroupParent)
        {
            if (child.name.StartsWith("Item Button"))
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null) rowButtons.Add(btn);
            }
        }

        while (rowButtons.Count < EndlessShopData.Catalog.Count && rowButtons.Count > 0)
        {
            Button clone = Instantiate(rowButtons[rowButtons.Count - 1], itemButtonGroupParent);
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
                
                rowButtons[i].onClick.AddListener(() => ViewItem(index));
            }
            else
            {
                rowButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void ViewItem(int index)
    {
        if (index < 0 || index >= EndlessShopData.Catalog.Count) return;
        viewingIndex = index;
        
        if (isMultiSelectMode)
        {
            if (cartIndices.Contains(index)) cartIndices.Remove(index);
            else cartIndices.Add(index);
        }

        RefreshAllUI();
    }

    public void OnToggleMultiSelect()
    {
        isMultiSelectMode = !isMultiSelectMode;
        
        if (isMultiSelectMode)
        {
            cartIndices.Clear();
            cartIndices.Add(viewingIndex);
        }
        else
        {
            cartIndices.Clear();
        }
        
        RefreshAllUI();
    }

    public void OnBuyClicked()
    {
        if (EndlessShopData.Catalog.Count == 0) return;

        if (!isMultiSelectMode)
        {
            var item = EndlessShopData.Catalog[viewingIndex];
            int owned = EndlessShopData.GetPermanentItemCount(item.itemId);
            
            if (EndlessShopData.TotalGald >= item.price && owned < item.maxCarryLimit)
            {
                EndlessShopData.TotalGald -= item.price;
                EndlessShopData.AddPermanentItem(item.itemId, 1);
                RefreshAllUI();
            }
        }
        else
        {
            if (cartIndices.Count == 0) return;

            int totalCost = 0;
            bool limitsOk = true;

            foreach (int i in cartIndices)
            {
                var item = EndlessShopData.Catalog[i];
                totalCost += item.price;
                if (EndlessShopData.GetPermanentItemCount(item.itemId) >= item.maxCarryLimit)
                {
                    limitsOk = false;
                }
            }

            if (limitsOk && EndlessShopData.TotalGald >= totalCost)
            {
                EndlessShopData.TotalGald -= totalCost;
                foreach (int i in cartIndices)
                {
                    EndlessShopData.AddPermanentItem(EndlessShopData.Catalog[i].itemId, 1);
                }
                cartIndices.Clear();
                isMultiSelectMode = false; 
                RefreshAllUI();
            }
        }
    }

    public void RefreshAllUI()
    {
        if (galdCountText != null) galdCountText.text = $"{EndlessShopData.TotalGald}G";

        int totalCost = 0;
        bool overLimit = false;

        if (isMultiSelectMode)
        {
            foreach (int i in cartIndices)
            {
                var item = EndlessShopData.Catalog[i];
                totalCost += item.price;
                if (EndlessShopData.GetPermanentItemCount(item.itemId) >= item.maxCarryLimit) overLimit = true;
            }
        }
        else if (EndlessShopData.Catalog.Count > 0)
        {
            var item = EndlessShopData.Catalog[viewingIndex];
            totalCost = item.price;
            if (EndlessShopData.GetPermanentItemCount(item.itemId) >= item.maxCarryLimit) overLimit = true;
        }

        for (int i = 0; i < EndlessShopData.Catalog.Count && i < rowButtons.Count; i++)
        {
            var item = EndlessShopData.Catalog[i];
            int owned = EndlessShopData.GetPermanentItemCount(item.itemId);
            int amountLeftInShop = item.maxCarryLimit - owned;

            TextMeshProUGUI[] rowTexts = rowButtons[i].GetComponentsInChildren<TextMeshProUGUI>(true);
            
            string prefix = "";
            if (isMultiSelectMode)
            {
                prefix = cartIndices.Contains(i) ? "[X] " : "[ ] ";
            }

            if (rowTexts.Length > 0) rowTexts[0].text = $"{item.price}G"; 
            if (rowTexts.Length > 1) rowTexts[1].text = prefix + item.displayName; 
            
            // --- NEW: Shows remaining quantity available to buy ---
            if (rowTexts.Length > 2) rowTexts[2].text = amountLeftInShop.ToString(); 

            CanvasGroup cg = rowButtons[i].GetComponent<CanvasGroup>();
            if (cg == null) cg = rowButtons[i].gameObject.AddComponent<CanvasGroup>();
            
            if (isMultiSelectMode) cg.alpha = cartIndices.Contains(i) ? 1.0f : 0.5f;
            else cg.alpha = (i == viewingIndex) ? 1.0f : 0.5f;
        }

        // Refresh the Description Panel at the bottom
        if (EndlessShopData.Catalog.Count > 0 && viewingIndex < EndlessShopData.Catalog.Count)
        {
            var current = EndlessShopData.Catalog[viewingIndex];
            int owned = EndlessShopData.GetPermanentItemCount(current.itemId);

            if (selectedItemNameText != null) selectedItemNameText.text = current.displayName;
            if (selectedItemDescText != null) selectedItemDescText.text = current.description;
            if (selectedItemOwnedText != null) selectedItemOwnedText.text = $"{owned}";

            if (selectMultipleButtonText != null)
                selectMultipleButtonText.text = isMultiSelectMode ? "Cancel" : "Select Multiple";

            if (buyButton != null)
            {
                bool canAfford = EndlessShopData.TotalGald >= totalCost;
                bool hasSelection = !isMultiSelectMode || cartIndices.Count > 0;
                buyButton.interactable = canAfford && !overLimit && hasSelection;
                
                TextMeshProUGUI buyText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buyText != null) buyText.text = $"Buy ({totalCost}G)";
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