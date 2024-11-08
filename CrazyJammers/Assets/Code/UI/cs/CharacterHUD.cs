using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Code.Core.Events;
using Code.Utility.Events;

public class CharacterStatusUpdateEvent : IBusEvent { }

public class CharacterHUD : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI nameText;

    [SerializeField] Image hpFill;
    [SerializeField] TextMeshProUGUI hpText;

    private Enemy character2;
    private Character character;

    private float HPUIlerpSpeed = 6f;
    private float HPUIlerpCurve = .5f;

    private float targetHPFill = 1f;

    public void Init(Enemy character)
    {
        this.character = character;
        this.character2 = character;

        nameText.text = character.characterName;

        hpFill.fillAmount = 1;
        hpText.text = character.maxHealth + " / " + character.maxHealth;

        EventBus.Subscribe<CharacterStatusUpdateEvent>(OnStatusUpdate);

    }

    public void Init(Character character)
    {
        this.character = character;

        nameText.text = character.characterName;

        hpFill.fillAmount = 1;
        hpText.text = character.maxHealth + " / " + character.maxHealth;

        EventBus.Subscribe<CharacterStatusUpdateEvent>(OnStatusUpdate);

    }

    private void OnStatusUpdate(CharacterStatusUpdateEvent statusUpdateEvent)
    {
        UpdateHPBar();
    }

    private void Update()
    {
        //Debug.Log($"Target percentage: {targetPercentage}");

        float percentageComplete = Mathf.Pow(Mathf.Clamp01(Mathf.Abs(hpFill.fillAmount - targetHPFill)), HPUIlerpCurve);

        // Linearly interpolate lerpSpeed based on the percentageComplete
        float adjustedLerpSpeed = Mathf.Lerp(HPUIlerpSpeed, HPUIlerpSpeed * 0.1f, percentageComplete);

        // Use Mathf.Lerp to smoothly interpolate from currentNumber to targetNumber
        hpFill.fillAmount = Mathf.Lerp(hpFill.fillAmount, targetHPFill, Time.deltaTime * adjustedLerpSpeed);
    }

    private void UpdateHPBar()
    {
        //hpFill.fillAmount = fillAmount;
        hpText.text = Mathf.Max(0, character.currentHealth) + " / " + character.maxHealth;

        targetHPFill = (float)character.currentHealth / (float)character.maxHealth;
    }

    public void AttackCharacter()
    {
        this.character2.OnMouseOverr();
    }

}
