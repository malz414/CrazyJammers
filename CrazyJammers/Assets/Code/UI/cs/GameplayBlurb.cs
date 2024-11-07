using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayBlurb : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI blurbText;

    [SerializeField] CanvasGroup blurbCanvas;

    private float lifeTimer = 0;

    private const float LIFESPAN = 2f;

    private const float BLURB_FADEOUT_DURATION = .4f;

    public void Init(string text)
    {
        blurbText.text = text;
        lifeTimer = LIFESPAN;
    }

    private void Update()
    {
        if(lifeTimer > 0)
        {
            lifeTimer -= Time.deltaTime;
            return;
        }

        //StartCoroutine(DoBlurbDeathRoutine());

        LeanTween.alphaCanvas(blurbCanvas, 0, BLURB_FADEOUT_DURATION).setDestroyOnComplete(true);
    }

/*    private IEnumerator DoBlurbDeathRoutine()
    {
    }*/

}
