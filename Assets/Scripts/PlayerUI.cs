using TMPro;
using UnityEngine;
using System;

namespace LuckyMultiplayer.Scripts
{
    public class PlayerUI : MonoBehaviour
    {
        public static Action<ushort> UpdateDiamondUI;

        // new
        //public static Action<ushort> UpdateHealthUI;

        public static Action<ushort, ushort> UpdateHealthUIWithMax;

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
            //UpdateHealthUI += ChangeHealthText;
            UpdateHealthUIWithMax += ChangeHealthTextWithMax;

            // new
            UpdateLevelUI += ChangeLevelText;
        }

        private void OnDisable()
        {
            UpdateDiamondUI -= ChangeDiamondText;

            // new
            //UpdateHealthUI -= ChangeHealthText;
            UpdateHealthUIWithMax -= ChangeHealthTextWithMax;

            // new 
            UpdateLevelUI -= ChangeLevelText;
        }

        private void ChangeDiamondText(ushort amount)
        {
            diamondsText.text = $"{amount}";
        }

        private void ChangeHealthTextWithMax(ushort current, ushort max)
        {
            healthText.text = $"HP: {current}/{max}";
        }

        // new
        private void ChangeLevelText(ushort level)
        {
            levelText.text = $"Level: {level}";
        }
    }
}
