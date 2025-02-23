using UnityEngine;
using TMPro;

public class ChangeButtonTextColor : MonoBehaviour
{
 public TMP_Text text1; // The first TMP text component
    public TMP_Text text2;
     public TMP_Text text1c; // The first TMP text component
    public TMP_Text text2c; // The second TMP text component
    public int valueToCheck; // The value to check
    public Color textColorWhenZero = Color.grey; // The text color to set when the value is 0
    public Color defaultTextColor = Color.white; // The default text color when the value is not 0

    void Update()
    {
        // Check the value and change the TMP text colors accordingly
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
    }

}
