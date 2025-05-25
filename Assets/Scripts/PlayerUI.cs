using TMPro;
using UnityEngine;
using System;

namespace LuckyMultiplayer.Scripts
{
    public class PlayerUI : MonoBehaviour
    {
        public static Action<ushort> UpdateDiamondUI;

        // new
        public static Action<ushort> UpdateHealthUI;

        // new
        public static Action<ushort> UpdateLevelUI;
        [SerializeField] private TextMeshProUGUI diamondsText;

        // new
        [SerializeField] private TextMeshProUGUI healthText;

        //new
        [SerializeField] private TextMeshProUGUI levelText;

        private void OnEnable()
        {
            UpdateDiamondUI += ChangeDiamondText;

            // new
            UpdateHealthUI += ChangeHealthText;

            // new
            UpdateLevelUI += ChangeLevelText;
        }

        private void OnDisable()
        {
            UpdateDiamondUI -= ChangeDiamondText;

            // new
            UpdateHealthUI -= ChangeHealthText;

            // new 
            UpdateLevelUI -= ChangeLevelText;
        }

        private void ChangeDiamondText(ushort amount)
        {
            diamondsText.text = $"Gems: {amount}";
        }

        // new
        private void ChangeHealthText(ushort amount)
        {
            healthText.text = $"HP: {amount}";
        }

        // new
        private void ChangeLevelText(ushort level)
        {
            levelText.text = $"Level: {level}";
        }
    }
}
