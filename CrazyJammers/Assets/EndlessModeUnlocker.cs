using UnityEngine;
using UnityEngine.UI;

public class EndlessModeUnlocker : MonoBehaviour
{
    [Tooltip("The actual button to click for Endless Mode")]
    public Button endlessButton;
    [Tooltip("The text or object that says 'Unlocked'")]
    public GameObject unlockedTextObject;
    [Tooltip("The text or object that says 'Locked'")]
    public GameObject lockedTextObject;

    private void Start()
    {
        // 1 means unlocked, 0 means locked
        bool isUnlocked = PlayerPrefs.GetInt("EndlessModeUnlocked", 0) == 1;

        if (endlessButton != null) endlessButton.interactable = isUnlocked;
        if (unlockedTextObject != null) unlockedTextObject.SetActive(isUnlocked);
        if (lockedTextObject != null) lockedTextObject.SetActive(!isUnlocked);
    }
}