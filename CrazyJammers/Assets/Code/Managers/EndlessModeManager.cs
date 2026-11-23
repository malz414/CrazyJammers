using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using EasyTransition;
#if USE_CRAZYGAMES
using CrazyGames;
#endif

public class EndlessModeManager : MonoBehaviour
{
    public static EndlessModeManager Instance { get; private set; }

    [Header("Battle Item Menu & Pagination")]
    [Tooltip("Drag the 'Item Group' parent Transform containing your generic item button slots.")]
    [SerializeField] private Transform battleItemGroupParent;
    [Tooltip("Number of item buttons shown per page (e.g., 4).")]
    [SerializeField] private int itemsPerPage = 4;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TextMeshProUGUI pageCountText;
    [SerializeField] private TextMeshProUGUI itemMenuDescriptionText;

    [Header("Endless End-of-Run UI Groups")]
    [SerializeField] private GameObject firstTimeEndlessEndGroup;
    [SerializeField] private GameObject regularEndlessEndGroup;
    [SerializeField] private GameObject shopUnlockNotifOverlay;
    [SerializeField] private GameObject results;
    [SerializeField] private GameObject resultsbutton;
    [SerializeField] private GameObject image;

    [HideInInspector] public bool hasUsedItemThisTurn = false;
    public bool freeItemActionActive = false;
    public string shopSceneName = "ShopScene";

    private Dictionary<string, int> sessionInventory = new Dictionary<string, int>();
    private List<Button> slotButtons = new List<Button>();
    private int currentPage = 0;
    private bool wasShopAlreadyUnlockedThisRun = false;
    [Header("First Time Endless Unlock")]
    [SerializeField] private GameObject endlessUnlockNotifOverlay;
    [SerializeField] private GameObject shopUnlockNotifCard;
    [Tooltip("Seconds to wait after entering Endless Mode before pausing for the unlock popup.")]
    [SerializeField] private float endlessUnlockNotifDelay = 1.5f;
    private bool hasScheduledEndlessNotif = false;
    
    private class PauseWhilePopupOpen : MonoBehaviour
    {
        private void OnEnable() => Time.timeScale = 0f;
        private void OnDisable() => Time.timeScale = 1f;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        EnsurePageButtonsBound();

        if (EndlessShopData.IsEndlessUnlocked && !EndlessShopData.HasShownEndlessUnlockNotif && endlessUnlockNotifOverlay != null)
        {
            endlessUnlockNotifOverlay.SetActive(true);
            EndlessShopData.HasShownEndlessUnlockNotif = true;
        }
    }
    
    private void TryScheduleEndlessUnlockPopup(string caller)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string modeName = TurnManager.Instance != null ? TurnManager.Instance.currentGameMode.ToString() : "TurnManager_NULL";
        bool isEndlessScene = (TurnManager.Instance != null && TurnManager.Instance.currentGameMode == TurnManager.GameMode.Endless)
                              || sceneName == "EndlessModeScene";

        bool isUnlocked = EndlessShopData.IsEndlessUnlocked;
        bool alreadyShown = EndlessShopData.HasShownEndlessUnlockNotif;
        bool hasOverlay = endlessUnlockNotifOverlay != null;

        Debug.Log($"[EndlessNotifDebug] ({caller}) Scheduled={hasScheduledEndlessNotif} | Scene='{sceneName}' | Mode={modeName} | IsEndlessScene={isEndlessScene} | IsUnlocked={isUnlocked} | AlreadyShown={alreadyShown} | OverlayAssigned={hasOverlay}");

        if (hasScheduledEndlessNotif) return;

        if (isEndlessScene && isUnlocked && !alreadyShown && hasOverlay)
        {
            hasScheduledEndlessNotif = true;
            EndlessShopData.HasShownEndlessUnlockNotif = true;
            Debug.Log($"[EndlessNotifDebug] Conditions met! Waiting {endlessUnlockNotifDelay}s (realtime) to show '{endlessUnlockNotifOverlay.name}'...");
            StartCoroutine(ShowEndlessUnlockNotifDelayed(endlessUnlockNotifDelay));
        }
    }
    
    private void EnsurePageButtonsBound()
    {
        if (battleItemGroupParent != null && battleItemGroupParent.parent != null)
        {
            Transform panelRoot = battleItemGroupParent.parent;
            if (prevPageButton == null)
            {
                Transform prevTf = panelRoot.Find("Page Turner Icons/PrevPageButton");
                if (prevTf != null) prevPageButton = prevTf.GetComponent<Button>();
            }
            if (nextPageButton == null)
            {
                Transform nextTf = panelRoot.Find("Page Turner Icons/NextPageButton");
                if (nextTf != null) nextPageButton = nextTf.GetComponent<Button>();
            }
        }

        if (prevPageButton != null)
        {
            prevPageButton.onClick.RemoveAllListeners();
            prevPageButton.onClick.AddListener(OnPrevPageClicked);
            ForceButtonClickable(prevPageButton);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.AddListener(OnNextPageClicked);
            ForceButtonClickable(nextPageButton);
        }

        // Prevent decorative texts from stealing mouse raycasts over PrevPageButton
        if (pageCountText != null) pageCountText.raycastTarget = false;
        if (itemMenuDescriptionText != null) itemMenuDescriptionText.raycastTarget = false;
    }

    private void ForceButtonClickable(Button btn)
    {
        if (btn == null) return;
        btn.enabled = true;
        btn.interactable = true;
        foreach (var graphic in btn.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = true;
        }
    }


    public void OnBattleStart()
    {
        freeItemActionActive = false;
        hasUsedItemThisTurn = false;
        currentPage = 0;
        sessionInventory.Clear();

        bool isEndless = TurnManager.Instance != null && TurnManager.Instance.currentGameMode == TurnManager.GameMode.Endless;

        foreach (var item in EndlessShopData.Catalog)
        {
            sessionInventory[item.itemId] = isEndless
                ? EndlessShopData.GetTotalEndlessOwned(item.itemId)
                : EndlessShopData.GetCampaignItemCount(item.itemId);
        }

        BuildOrRefreshBattleItemButtons();
    }

    private System.Collections.IEnumerator ShowEndlessUnlockNotifDelayed(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (endlessUnlockNotifOverlay != null)
        {
            if (shopUnlockNotifCard != null) shopUnlockNotifCard.SetActive(false);

            Transform p = endlessUnlockNotifOverlay.transform.parent;
            while (p != null && p.GetComponent<Canvas>() == null)
            {
                p.gameObject.SetActive(true);
                p = p.parent;
            }

            Time.timeScale = 0f;
            endlessUnlockNotifOverlay.SetActive(true);
        }
    }
    public void CloseEndlessUnlockNotif()
    {
        if (endlessUnlockNotifOverlay != null)
            endlessUnlockNotifOverlay.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;
    }

    public void ResetToFirstPage()
    {
        currentPage = 0;
    }

    public void QuitEndlessRunNow()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.QuitEndlessRunNow();
        }
    }

    public void OnPrevPageClicked()
    {
        Debug.Log($"[Pagination] Prev clicked! Current page before decrement: {currentPage}");
        if (currentPage > 0)
        {
            currentPage--;
            BuildOrRefreshBattleItemButtons();
        }
    }

    public void OnNextPageClicked()
    {
        currentPage++;
        BuildOrRefreshBattleItemButtons();
    }

    public void BuildOrRefreshBattleItemButtons()
    {
        if (battleItemGroupParent == null) return;
        EnsurePageButtonsBound();

        slotButtons.Clear();
        for (int i = 0; i < battleItemGroupParent.childCount; i++)
        {
            Button btn = battleItemGroupParent.GetChild(i).GetComponent<Button>();
            if (btn != null && btn != prevPageButton && btn != nextPageButton)
            {
                slotButtons.Add(btn);
            }
        }

        List<EndlessItemDefinition> visibleItems = new List<EndlessItemDefinition>();
        foreach (var item in EndlessShopData.Catalog)
        {
            int count = sessionInventory.ContainsKey(item.itemId) ? sessionInventory[item.itemId] : 0;
            if (count > 0) visibleItems.Add(item);
        }

        int pageSize = Mathf.Max(1, Mathf.Min(itemsPerPage, slotButtons.Count > 0 ? slotButtons.Count : itemsPerPage));
        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)visibleItems.Count / pageSize));
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        if (prevPageButton != null)
        {
            bool showPrev = totalPages > 1 && currentPage > 0;
            prevPageButton.gameObject.SetActive(showPrev);
            if (showPrev) ForceButtonClickable(prevPageButton);
        }

        if (nextPageButton != null)
        {
            bool showNext = totalPages > 1 && currentPage < totalPages - 1;
            nextPageButton.gameObject.SetActive(showNext);
            if (showNext) ForceButtonClickable(nextPageButton);
        }

        if (pageCountText != null)
        {
            pageCountText.gameObject.SetActive(totalPages > 1);
            pageCountText.text = $"{currentPage + 1}/{totalPages}";
        }

        int startIndex = currentPage * pageSize;

        for (int i = 0; i < slotButtons.Count; i++)
        {
            Button btn = slotButtons[i];
            int itemIndex = startIndex + i;

            if (i < pageSize && itemIndex < visibleItems.Count)
            {
                EndlessItemDefinition itemDef = visibleItems[itemIndex];
                int remainingCount = sessionInventory[itemDef.itemId];

                btn.gameObject.SetActive(true);

                bool isActionLocked = hasUsedItemThisTurn || (itemDef.effectType == EndlessItemEffectType.FreeItemAction && freeItemActionActive);
                bool canUse = !isActionLocked;

                btn.interactable = canUse;
                Color targetTextColor = canUse ? Color.white : Color.gray;

                TextMeshProUGUI[] texts = btn.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (texts.Length > 0)
                {
                    texts[0].text = itemDef.displayName;
                    texts[0].color = targetTextColor;
                }
                if (texts.Length > 1)
                {
                    texts[1].text = $"x{remainingCount}";
                    texts[1].color = targetTextColor;
                }

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => UseBattleItem(itemDef));

                SetupDynamicHover(btn.gameObject, itemDef);
            }
            else
            {
                btn.gameObject.SetActive(false);
            }
        }
    }

    private void SetupDynamicHover(GameObject btnObj, EndlessItemDefinition itemDef)
    {
        EventTrigger trigger = btnObj.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btnObj.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((_) =>
        {
            if (itemMenuDescriptionText == null) return;

            if (hasUsedItemThisTurn)
            {
                itemMenuDescriptionText.text = "You already used an item this turn!";
                return;
            }
            if (itemDef.effectType == EndlessItemEffectType.FreeItemAction && freeItemActionActive)
            {
                itemMenuDescriptionText.text = "Blessed Amulet is already active!";
                return;
            }

            int count = sessionInventory.ContainsKey(itemDef.itemId) ? sessionInventory[itemDef.itemId] : 0;
            itemMenuDescriptionText.text = count > 0 ? itemDef.description : itemDef.emptyDescription;
        });
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((_) =>
        {
            if (itemMenuDescriptionText != null) itemMenuDescriptionText.text = "Select a command.";
        });
        trigger.triggers.Add(exit);
    }

    private void UseBattleItem(EndlessItemDefinition itemDef)
    {
        TurnManager tm = TurnManager.Instance;
        if (tm == null || tm.hero == null) return;

        if (hasUsedItemThisTurn)
        {
            if (itemMenuDescriptionText != null) itemMenuDescriptionText.text = "You already used an item this turn!";
            return;
        }

        int count = sessionInventory.ContainsKey(itemDef.itemId) ? sessionInventory[itemDef.itemId] : 0;
        if (count <= 0)
        {
            if (itemMenuDescriptionText != null) itemMenuDescriptionText.text = itemDef.emptyDescription;
            return;
        }

        bool needsHeal = tm.hero.currentHealth < tm.hero.maxHealth;
        bool hasStatus = tm.hero.burning > 0 || tm.hero.paralysisEffect != null;

        switch (itemDef.effectType)
        {
            case EndlessItemEffectType.HealPercent:
                if (!needsHeal)
                {
                    if (itemMenuDescriptionText != null) itemMenuDescriptionText.text = "Hero Health Full.";
                    return;
                }
                ConsumeItem(itemDef.itemId);
                tm.ExecuteHealingOrCureItem(itemDef);
                BuildOrRefreshBattleItemButtons();
                break;

            case EndlessItemEffectType.CureStatus:
                if (!hasStatus)
                {
                    if (itemMenuDescriptionText != null) itemMenuDescriptionText.text = "No Status Effects to Cure.";
                    return;
                }
                ConsumeItem(itemDef.itemId);
                tm.ExecuteHealingOrCureItem(itemDef);
                BuildOrRefreshBattleItemButtons();
                break;

            case EndlessItemEffectType.HealPercentAndCure:
                if (!needsHeal && !hasStatus)
                {
                    if (itemMenuDescriptionText != null) itemMenuDescriptionText.text = "HP and Status already full!";
                    return;
                }
                ConsumeItem(itemDef.itemId);
                tm.ExecuteHealingOrCureItem(itemDef);
                BuildOrRefreshBattleItemButtons();
                break;

            case EndlessItemEffectType.FreeItemAction:
                if (freeItemActionActive)
                {
                    if (itemMenuDescriptionText != null) itemMenuDescriptionText.text = "Blessed Amulet already active!";
                    return;
                }
                ConsumeItem(itemDef.itemId);
                freeItemActionActive = true;
                tm.ExecuteBlessedAmuletEffect();
                break;
        }
    }

    private void ConsumeItem(string itemId)
    {
        sessionInventory[itemId] = Mathf.Max(0, sessionInventory[itemId] - 1);
    }
    public void CommitCampaignInventory()
    {
        foreach (var kvp in sessionInventory)
        {
            EndlessShopData.SetCampaignItemCount(kvp.Key, kvp.Value);
        }
    }

    public int GetSessionItemCount(string itemId)
    {
        return sessionInventory.ContainsKey(itemId) ? sessionInventory[itemId] : 0;
    }

    public void OnEndlessRunEnded(int earnedGald, int finalScore = 0)
    {
        EndlessShopData.AddGald(earnedGald);

        wasShopAlreadyUnlockedThisRun = EndlessShopData.IsShopUnlocked;
        EndlessShopData.IsShopUnlocked = true;

#if USE_CRAZYGAMES
        if (CrazySDK.IsAvailable && finalScore > 0)
        {
            string encryptionKey = "YOUR_32_BYTE_BASE64_KEY_HERE";
            try
            {
                string encryptedScore = EncryptScore(finalScore, encryptionKey);
                CrazySDK.User.SubmitScore(encryptedScore, finalScore);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CrazyGames] Score submission skipped/failed: {e.Message}");
            }
        }
#endif
    }

#if USE_CRAZYGAMES
    private string EncryptScore(float score, string keyBase64)
    {
        byte[] key = Convert.FromBase64String(keyBase64);
        byte[] plaintext = System.Text.Encoding.UTF8.GetBytes(score.ToString(System.Globalization.CultureInfo.InvariantCulture));
        byte[] iv = new byte[12];

        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(iv);
        }

        byte[] result = new byte[iv.Length + plaintext.Length + 1];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            using (var encryptor = aes.CreateEncryptor())
            {
                byte[] counter = new byte[16];
                Buffer.BlockCopy(iv, 0, counter, 0, 12);
                counter[15] = 1;

                for (int i = 0; i < plaintext.Length; i += 16)
                {
                    byte[] keystream = encryptor.TransformFinalBlock(counter, 0, 16);
                    int blockSize = Math.Min(16, plaintext.Length - i);

                    for (int j = 0; j < blockSize; j++)
                        result[iv.Length + i + j] = (byte)(plaintext[i + j] ^ keystream[j]);

                    for (int j = 15; j >= 0; j--)
                        if (++counter[j] != 0) break;
                }
            }
        }

        result[result.Length - 1] = 0x55;
        return Convert.ToBase64String(result);
    }
#endif
private GameObject GetBattleItemPanelRoot()
    {
        if (battleItemGroupParent == null) return null;

        Transform current = battleItemGroupParent;
        GameObject mainUI = TurnManager.Instance != null ? TurnManager.Instance.gameObject : null;

        while (current.parent != null)
        {
            // Stop before reaching a Canvas or the root HUD parent
            if (current.parent.GetComponent<Canvas>() != null ||
                current.parent.name.Contains("MainUI") ||
                current.parent.name.Contains("GameplayUI"))
            {
                return current.gameObject;
            }
            current = current.parent;
        }

        return battleItemGroupParent.parent != null ? battleItemGroupParent.parent.gameObject : battleItemGroupParent.gameObject;
    }

    public void OpenBattleItemPanel()
    {
        GameObject panelRoot = GetBattleItemPanelRoot();
        if (panelRoot != null) panelRoot.SetActive(true);
        currentPage = 0;
        BuildOrRefreshBattleItemButtons();
    }

    public void CloseBattleItemPanel()
    {
        GameObject panelRoot = GetBattleItemPanelRoot();
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void ShowPostRunShopOptions()
    {
        if (results != null) results.SetActive(false);
        if (resultsbutton != null) resultsbutton.SetActive(false);
        if (image != null) image.SetActive(true);

        if (firstTimeEndlessEndGroup != null) firstTimeEndlessEndGroup.SetActive(!wasShopAlreadyUnlockedThisRun);
        if (regularEndlessEndGroup != null) regularEndlessEndGroup.SetActive(wasShopAlreadyUnlockedThisRun);
    }

    public void GoToShopScene()
    {
        if (TransitionManager.Instance() != null && TurnManager.Instance != null)
            TransitionManager.Instance().Transition(shopSceneName, TurnManager.Instance.sequenceTransition, 0f);
        else
            SceneManager.LoadScene(shopSceneName);
    }
}