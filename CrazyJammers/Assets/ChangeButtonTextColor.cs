using UnityEngine;
using TMPro;

public class ChangeButtonTextColor : MonoBehaviour
{
    public TMP_Text text1; // The first TMP text component (Potion Name)
    public TMP_Text text2; // The second TMP text component (Potion Amount)
    public TMP_Text text1c; // The first TMP text component (Panacea Name)
    public TMP_Text text2c; // The second TMP text component (Panacea Amount)
    
    public Color textColorWhenZero = Color.grey; // The text color to set when unusable
    public Color defaultTextColor = Color.white; // The default text color when usable

    private Hero hero; // Cache the hero reference

    void Update()
    {
        // Try to get the hero reference if we don't have it
        if (hero == null && TurnManager.Instance != null && TurnManager.Instance.hero != null)
        {
            hero = TurnManager.Instance.hero;
        }

        // If hero is still null (battle not started?), just run the original logic
        if (hero == null)
        {
            // Original Potion Logic
            if (PotionData.Instance.Potion == 0)
            {
                text1.color = textColorWhenZero;
                text2.color = textColorWhenZero;
            }
            else
            {
                text1.color = defaultTextColor;
                text2.color = defaultTextColor;
            }

            // Original Panacea Logic
            if (PotionData.Instance.Panacea == 0)
            {
                text1c.color = textColorWhenZero;
                text2c.color = textColorWhenZero;
            }
            else
            {
                text1c.color = defaultTextColor;
                text2c.color = defaultTextColor;
            }
            return; // Exit update early
        }

        // --- Updated Logic with Hero reference ---

        // Check Potion: Gray out if 0 OR health is full
        if (PotionData.Instance.Potion == 0 || hero.currentHealth >= hero.maxHealth)
        {
            text1.color = textColorWhenZero;
            text2.color = textColorWhenZero;
        }
        else
        {
            text1.color = defaultTextColor;
            text2.color = defaultTextColor;
        }

        // Check Panacea: Gray out if 0 OR no status effects
        bool hasStatus = hero.burning > 0 || hero.paralysisEffect != null;
        if (PotionData.Instance.Panacea == 0 || !hasStatus)
        {
            text1c.color = textColorWhenZero;
            text2c.color = textColorWhenZero;
        }
        else
        {
            text1c.color = defaultTextColor;
            text2c.color = defaultTextColor;
        }
    }
}