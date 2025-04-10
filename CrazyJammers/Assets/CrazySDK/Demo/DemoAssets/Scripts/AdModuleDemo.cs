using UnityEngine;
using UnityEngine.UI;

namespace CrazyGames
{
    public class AdModuleDemo : MonoBehaviour
    {
        public Text timerText;

        private void Start()
        {
            CrazySDK.Init(() => { }); // ensure if starting this scene from editor it is initialized
        }

        private void Update()
        {
            timerText.text = "Timer: " + Time.time;

            if (Input.GetKeyDown(KeyCode.M))
            {
                ShowMidgameAd();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ShowRewardedAd();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void ShowMidgameAd()
        {
            CrazySDK.Ad.RequestAd(
                CrazyAdType.Midgame,
                () =>
                {
                    Debug.Log("Midgame ad started");
                },
                (error) =>
                {
                    Debug.Log("Midgame ad error: " + error);
                },
                () =>
                {
                    Debug.Log("Midgame ad finished");
                }
            );
        }

        public void ShowRewardedAd()
        {
            CrazySDK.Ad.RequestAd(
                CrazyAdType.Rewarded,
                () =>
                {
                    Debug.Log("Rewarded ad started");
                },
                (error) =>
                {
                    Debug.Log("Rewarded ad error: " + error);
                },
                () =>
                {
                    Debug.Log("Rewarded ad finished, reward the player here");
                }
            );
        }
    }
}
