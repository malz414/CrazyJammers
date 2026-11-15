    using System;
    using System.Security.Cryptography;
    #if USE_CRAZYGAMES
    using CrazyGames;
    #endif
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using TMPro;
    using EasyTransition;

    public class EndlessModeManager : MonoBehaviour
    {
        public static EndlessModeManager Instance { get; private set; }

        [Header("Battle Item Menu (Auto-Detected from Parent)")]
        [Tooltip("Drag the 'Item Group' parent Transform from Battle Menu here. Children 0 & 1 (Potion/Panacea) are skipped automatically.")]
        [SerializeField] private Transform battleItemGroupParent;
        [Tooltip("How many children at the top of Item Group are standard items (2 = Potion & Panacea).")]
        [SerializeField] private int standardItemButtonsCount = 2;
        [SerializeField] private TextMeshProUGUI itemMenuDescriptionText;

        [Header("Endless End-of-Run UI Groups")]
        [SerializeField] private GameObject firstTimeEndlessEndGroup;
        [SerializeField] private GameObject regularEndlessEndGroup;
        [SerializeField] private GameObject shopUnlockNotifOverlay;
        [SerializeField] private GameObject results;
        [SerializeField] private GameObject resultsbutton;
        [SerializeField] private GameObject image;
        [HideInInspector] public bool hasUsedItemThisTurn = false;

        
        public string shopSceneName = "ShopScene";

        // Runtime session inventory (replenished every run)
        private Dictionary<string, int> sessionInventory = new Dictionary<string, int>();
        private List<Button> spawnedBattleButtons = new List<Button>();
        public bool freeItemActionActive = false;
        private bool wasShopAlreadyUnlockedThisRun = false;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void OnBattleStart()
        {
            freeItemActionActive = false;
            sessionInventory.Clear();

            foreach (var item in EndlessShopData.Catalog)
            {
                sessionInventory[item.itemId] = EndlessShopData.GetPermanentItemCount(item.itemId);
            }

            BuildOrRefreshBattleItemButtons();
        }

        public void BuildOrRefreshBattleItemButtons()
    {
        if (battleItemGroupParent == null) return;

        spawnedBattleButtons.Clear();
        for (int i = standardItemButtonsCount; i < battleItemGroupParent.childCount; i++)
        {
            Button btn = battleItemGroupParent.GetChild(i).GetComponent<Button>();
            if (btn != null) spawnedBattleButtons.Add(btn);
        }

        List<EndlessItemDefinition> ownedCatalog = EndlessShopData.Catalog.FindAll(
            item => EndlessShopData.GetPermanentItemCount(item.itemId) > 0
        );

        while (spawnedBattleButtons.Count < ownedCatalog.Count && spawnedBattleButtons.Count > 0)
        {
            Button template = spawnedBattleButtons[spawnedBattleButtons.Count - 1];
            Button clone = Instantiate(template, battleItemGroupParent);
            spawnedBattleButtons.Add(clone);
        }

        for (int i = 0; i < spawnedBattleButtons.Count; i++)
        {
            Button btn = spawnedBattleButtons[i];
            if (i < ownedCatalog.Count)
            {
                EndlessItemDefinition itemDef = ownedCatalog[i];
                int remainingCount = sessionInventory.ContainsKey(itemDef.itemId) ? sessionInventory[itemDef.itemId] : 0;

                if (remainingCount <= 0)
                {
                    btn.gameObject.SetActive(false);
                    continue; 
                }

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
                btn.onClick.AddListener(() => UseEndlessItem(itemDef));

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

        private void UseEndlessItem(EndlessItemDefinition itemDef)
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

            switch (itemDef.effectType)
            {
                case EndlessItemEffectType.FullHealAndCure:
                    bool needsHeal = tm.hero.currentHealth < tm.hero.maxHealth;
                    bool hasStatus = tm.hero.burning > 0 || tm.hero.paralysisEffect != null;
                    if (!needsHeal && !hasStatus)
                    {
                        if (itemMenuDescriptionText != null) itemMenuDescriptionText.text = "HP and Status already full!";
                        return;
                    }

                    sessionInventory[itemDef.itemId]--;
                    tm.ExecuteMiracleDewEffect();
                    BuildOrRefreshBattleItemButtons();
                    break;

                case EndlessItemEffectType.FreeItemAction:
                    if (freeItemActionActive)
                    {
                        if (itemMenuDescriptionText != null) itemMenuDescriptionText.text = "Blessed Amulet already active!";
                        return;
                    }

                    sessionInventory[itemDef.itemId]--;
                    freeItemActionActive = true;
                    tm.ExecuteBlessedAmuletEffect();
                    break;
            }
        }

        public bool TryConsumeFreeItemAction()
        {
            if (!freeItemActionActive) return false;
            freeItemActionActive = false;
            return true;
        }
        public int GetSessionItemCount(string itemId)
        {
            return sessionInventory.ContainsKey(itemId) ? sessionInventory[itemId] : 0;
        }

    public void OnEndlessRunEnded(int earnedGald, int finalScore)
    {
        EndlessShopData.AddGald(earnedGald);

        wasShopAlreadyUnlockedThisRun = EndlessShopData.IsShopUnlocked;
        EndlessShopData.IsShopUnlocked = true;

#if USE_CRAZYGAMES
        if (CrazySDK.IsAvailable)
        {
            // IMPORTANT: Replace this string later with your actual key from the Developer Portal!
            string encryptionKey = "YOUR_32_BYTE_BASE64_KEY_HERE"; 
            
            try 
            {
                string encryptedScore = EncryptScore(finalScore, encryptionKey);
                CrazySDK.User.SubmitScore(encryptedScore, finalScore);
                Debug.Log($"[CrazyGames] Successfully sent Endless Score: {finalScore}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CrazyGames] Score encryption/submission failed: {e.Message}");
            }
        }
#endif
    }

#if USE_CRAZYGAMES
    // CrazyGames' required AES Encryption method
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
                    {
                        result[iv.Length + i + j] = (byte)(plaintext[i + j] ^ keystream[j]);
                    }

                    for (int j = 15; j >= 0; j--)
                    {
                        if (++counter[j] != 0) break;
                    }
                }
            }
        }
        
        result[result.Length - 1] = 0x55; // Unity marker byte required by the SDK
        return Convert.ToBase64String(result);
    }
#endif
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