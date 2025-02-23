using UnityEngine;
using UnityEngine.UI;

public class BideButtonController : MonoBehaviour
{
    public Hero hero; 
    private Image buttonImage;
    public Image buttonImage2;

    void Awake()
    {
        buttonImage = GetComponent<Image>();

        FindHeroInScene();
        
        UpdateButtonOpacity();
    }

    void OnEnable()
    {
        UpdateButtonOpacity();
    }

    private void UpdateButtonOpacity()
    {
        if (hero == null || buttonImage == null) return;

        if (hero.bideLevel == 2.0f)
        {
            SetButtonAlpha(0.5f, 0.5f);
        }
        else
        {
            SetButtonAlpha(1.0f, 0.7921569f );
        }
    }

     private void FindHeroInScene()
    {
        // Find the first GameObject with a Hero component
        Hero[] heroes = FindObjectsOfType<Hero>();
        if (heroes.Length > 0)
        {
            hero = heroes[0]; // Use the first Hero found
        }
        else
        {
            Debug.LogError("No Hero object found in the scene!");
        }
    }

    private void SetButtonAlpha(float alpha, float alpha2)
    {
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = alpha;
            buttonImage.color = color;

            Color color1 = buttonImage2.color;
            color1.a = alpha2;
            buttonImage2.color = color1;
        }
    }
}