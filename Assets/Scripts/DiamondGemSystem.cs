using Unity.Netcode;
using UnityEngine;

namespace LuckyMultiplayer.Scripts
{
    public class DiamondGemSystem : NetworkBehaviour
    {
        private NetworkVariable<ushort> diamonds = new NetworkVariable<ushort>(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<ushort> health = new NetworkVariable<ushort>(
            5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<ushort> level = new NetworkVariable<ushort>(
            1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                // Subscribe to value changed events
                diamonds.OnValueChanged += OnDiamondsChanged;
                health.OnValueChanged += OnHealthChanged;
                level.OnValueChanged += OnLevelChanged;

                // Update UI immediately with current values
                PlayerUI.UpdateDiamondUI?.Invoke(diamonds.Value);
                PlayerUI.UpdateHealthUI?.Invoke(health.Value);
                PlayerUI.UpdateLevelUI?.Invoke(level.Value);
            }
        }

        private void OnEnable()
        {
            diamonds.OnValueChanged += OnDiamondsChanged;
            health.OnValueChanged += OnHealthChanged;
            level.OnValueChanged += OnLevelChanged;
        }

        private void OnDisable()
        {
            diamonds.OnValueChanged -= OnDiamondsChanged;
            health.OnValueChanged -= OnHealthChanged;
            level.OnValueChanged -= OnLevelChanged;
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
                PlayerUI.UpdateHealthUI?.Invoke(next);
        }

        private void OnLevelChanged(ushort oldLevel, ushort newLevel)
        {
            if (IsOwner)
                PlayerUI.UpdateLevelUI?.Invoke(newLevel);
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

        public void ReduceHealth()
        {
            if (IsServer && health.Value > 0)
            {
                health.Value--;
            }
        }

        public void TryLevelUp()
        {
            if (IsOwner && HasEnoughDiamonds(3))
            {
                UseDiamonds(3);
                AttemptLevelUpServerRpc();
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
    }
}
