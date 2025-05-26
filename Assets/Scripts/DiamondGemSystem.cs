using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace LuckyMultiplayer.Scripts
{
    public class DiamondGemSystem : NetworkBehaviour
    {
        [SerializeField] private GameObject healthDisplay;
        [SerializeField] private Image fill;
        [SerializeField] private TextMeshProUGUI levelWorldText;
        [SerializeField] private GameObject shieldVisual;
        private NetworkVariable<ushort> diamonds = new NetworkVariable<ushort>(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<ushort> health = new NetworkVariable<ushort>(
            5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<ushort> level = new NetworkVariable<ushort>(
            1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> shieldActive = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private float maxHealth;

        private void Awake()
        {
            maxHealth = 5f;
        }

        public override void OnNetworkSpawn()
        {
            maxHealth = CalculateMaxHealth(level.Value); // Moved here so it runs for all clients

            // Subscribe to value changed events
            diamonds.OnValueChanged += OnDiamondsChanged;
            health.OnValueChanged += OnHealthChanged;
            level.OnValueChanged += OnLevelChanged;
            shieldActive.OnValueChanged += OnShieldActiveChanged;

            if (IsOwner)
            {
                // Update UI immediately with current values
                PlayerUI.UpdateDiamondUI?.Invoke(diamonds.Value);
                //PlayerUI.UpdateHealthUI?.Invoke(health.Value);
                PlayerUI.UpdateHealthUIWithMax?.Invoke(health.Value, (ushort)maxHealth);
                PlayerUI.UpdateLevelUI?.Invoke(level.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                diamonds.OnValueChanged -= OnDiamondsChanged;
                health.OnValueChanged -= OnHealthChanged;
                level.OnValueChanged -= OnLevelChanged;
                shieldActive.OnValueChanged -= OnShieldActiveChanged;
            }
        }

        private void OnShieldActiveChanged(bool previousValue, bool newValue)
        {
            UpdateShieldVisual(newValue);
        }

        private void UpdateShieldVisual(bool isActive)
        {
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(isActive);
            }
        }

        private void OnDiamondsChanged(ushort previousValue, ushort newValue)
        {
            if (IsOwner)
            {
                // static event from PlayerUI script
                PlayerUI.UpdateDiamondUI?.Invoke(newValue);
            }
        }

        // new
        private void OnHealthChanged(ushort prev, ushort next)
        {
            if (IsOwner)
            {
                //PlayerUI.UpdateHealthUI?.Invoke(next);
                PlayerUI.UpdateHealthUIWithMax?.Invoke(next, (ushort)maxHealth);
            }

            maxHealth = CalculateMaxHealth(level.Value);

            UpdateHPDisplay(); // this is outside of IsOwner because the other player needs to see the hp bar change
        }

        private void OnLevelChanged(ushort oldLevel, ushort newLevel)
        {
            if (IsOwner)
            {
                PlayerUI.UpdateLevelUI?.Invoke(newLevel);
                maxHealth = CalculateMaxHealth(newLevel);
                PlayerUI.UpdateHealthUIWithMax?.Invoke(health.Value, (ushort)maxHealth);
            }

            maxHealth = CalculateMaxHealth(newLevel);
            levelWorldText.text = newLevel.ToString();

            UpdateHPDisplay();
        }

        private float CalculateMaxHealth(ushort level)
        {
            return 5 + (level - 1); // Example logic: base 5 health + 1 per level
        }

        private void UpdateHPDisplay()
        {
            float p = Mathf.Clamp01(health.Value / maxHealth);

            if (fill)
                fill.transform.localScale = new Vector3(p, 1.0f, 1.0f);

            if (healthDisplay != null)
                healthDisplay.SetActive(p > 0.0f);

        }

        public void PickUpDiamondGem()
        {
            if (!IsServer)
                return;

            diamonds.Value++;
        }

        public void UseDiamonds(ushort amount)
        {
            if (IsOwner)
            {
                UseDiamondsServerRpc(amount);
            }
        }

        public bool HasEnoughDiamonds(ushort amount)
        {
            return diamonds.Value >= amount;
        }

        public void ReduceHealth(ushort amount)
        {
            if (IsServer && health.Value > 0)
            {
                if (shieldActive.Value)
                {
                    shieldActive.Value = false;
                    return;
                }
                
                health.Value -= amount;
            }
        }

        public int GetDamage()
        {
            float baseDamage = 1f;
            return Mathf.CeilToInt(baseDamage * Mathf.Pow(1.5f, level.Value - 1));
        }

        public void TryLevelUp()
        {
            if (IsOwner && HasEnoughDiamonds(3))
            {
                UseDiamonds(3);
                AttemptLevelUpServerRpc();
            }
        }

        public void ActivateShield()
        {
            if (IsOwner && HasEnoughDiamonds(2))
            {
                UseDiamonds(2);
                ActivateShieldServerRpc();
            }
        }

        [ServerRpc]
        private void UseDiamondsServerRpc(ushort amount)
        {
            if (diamonds.Value >= amount)
            {
                diamonds.Value -= amount;
            }
        }

        [ServerRpc]
        private void AttemptLevelUpServerRpc()
        {
            if (Random.value <= 0.5f)
            {
                level.Value++;
                health.Value++;
                UpdateSpeedClientRpc(level.Value);
            }
        }

        [ClientRpc]
        private void UpdateSpeedClientRpc(ushort newLevel)
        {
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.UpdateSpeed(newLevel);
            }
        }
        
        [ServerRpc]
        private void ActivateShieldServerRpc()
        {
            shieldActive.Value = true;
            // Optional: Start a coroutine or timer here to disable shield after some seconds automatically
        }
    }
}
