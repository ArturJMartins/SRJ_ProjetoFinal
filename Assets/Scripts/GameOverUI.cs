using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

namespace LuckyMultiplayer.Scripts
{
    public class GameOverUI : NetworkBehaviour
    {
        public static GameOverUI Instance;

        [SerializeField] private Spawner spawner;

        [SerializeField] private GameObject gameOverBG;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private Button rematchButton;
        [SerializeField] private Button leaveRematchButton;

        private void Awake()
        {
            Instance = this;

            gameOverBG.SetActive(false);

            rematchButton.onClick.AddListener(() => OnRematchButtonPressed());
            leaveRematchButton.onClick.AddListener(() => OnLeaveButtonPressed());
        }

        public void ShowGameOver(bool didWin)
        {
            gameOverBG.SetActive(true);
            gameOverText.text = didWin ? "You Win!" : "You Lose!";
        }

        private void OnRematchButtonPressed()
        {
            var diamondSystem = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<DiamondGemSystem>();
            diamondSystem.RequestRematchServerRpc(); 
        }

        public void Hide()
        {
            gameOverBG.SetActive(false);
        }

        public void RemoveAllGems()
        {
            spawner.RemoveAllGems();
        }

        private void OnLeaveButtonPressed()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
