using TMPro;
using UnityEngine;
using System;

namespace LuckyMultiplayer.Scripts
{
    public class PlayerUI : MonoBehaviour
    {
        public static Action<ushort> UpdateDiamondUI;
        public static Action<ushort, ushort> UpdateHealthUIWithMax;
        public static Action<ushort> UpdateLevelUI;
        public static Action<ushort> UpdateAttackUI;

        [SerializeField] private TextMeshProUGUI diamondsText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI attackText;

        private void OnEnable()
        {
            UpdateDiamondUI += ChangeDiamondText;
            UpdateHealthUIWithMax += ChangeHealthTextWithMax;
            UpdateLevelUI += ChangeLevelText;
            UpdateAttackUI += ChangeAttackText;
        }

        private void OnDisable()
        {
            UpdateDiamondUI -= ChangeDiamondText;
            UpdateHealthUIWithMax -= ChangeHealthTextWithMax;
            UpdateLevelUI -= ChangeLevelText;
            UpdateAttackUI -= ChangeAttackText;
        }

        private void ChangeDiamondText(ushort amount)
        {
            diamondsText.text = $"{amount}";
        }

        private void ChangeHealthTextWithMax(ushort current, ushort max)
        {
            healthText.text = $"HP: {current}/{max}";
        }
        private void ChangeLevelText(ushort level)
        {
            levelText.text = $"Level: {level}";
        }

        private void ChangeAttackText(ushort atkAmount)
        {
            attackText.text = $"Attack: {atkAmount}";
        }
    }
}
