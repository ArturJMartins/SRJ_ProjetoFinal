using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace LuckyMultiplayer.Scripts
{
    public class DiamondGemSystem : NetworkBehaviour
    {
        private static int rematchVotes = 0;
        private static HashSet<ulong> votedClientIds = new HashSet<ulong>();
        [SerializeField] private GameObject healthDisplay;
        [SerializeField] private Image fill;
        [SerializeField] private TextMeshProUGUI levelWorldText;
        [SerializeField] private GameObject shieldVisual;
        private NetworkVariable<ushort> diamonds = new NetworkVariable<ushort>(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<ushort> health = new NetworkVariable<ushort>(
            10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<ushort> level = new NetworkVariable<ushort>(
            1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> shieldActive = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private float maxHealth;
        private Flasher flasher;
        [SerializeField] Color flashColor = Color.white;
        [SerializeField] Color lvlFlashColor = Color.red;

        private void Awake()
        {
            maxHealth = 5f;
            flasher = GetComponent<Flasher>();
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

                PlayerUI.UpdateAttackUI?.Invoke((ushort)GetDamage());
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

            if (flasher) flasher.Flash(flashColor, 0.2f);
            UpdateHPDisplay(); // this is outside of IsOwner because the other player needs to see the hp bar change

            if (IsServer && next == 0)
            {
                ulong deadPlayerClientId = OwnerClientId; // The player who died

                // Notify all clients who won and who lost
                NotifyGameOverToClients(deadPlayerClientId);
            }
        }

        private void NotifyGameOverToClients(ulong deadPlayerClientId)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                bool didWin = client.ClientId != deadPlayerClientId;

                // Send the game over notification only to the target client
                ShowGameOverClientRpc(didWin, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { client.ClientId }
                    }
                });
            }
        }

        [ClientRpc]
        private void ShowGameOverClientRpc(bool didWin, ClientRpcParams clientRpcParams = default)
        {
            if (GameOverUI.Instance != null)
            {
                GameOverUI.Instance.ShowGameOver(didWin);
            }
        }

        private void OnLevelChanged(ushort oldLevel, ushort newLevel)
        {
            if (IsOwner)
            {
                PlayerUI.UpdateLevelUI?.Invoke(newLevel);
                maxHealth = CalculateMaxHealth(newLevel);
                PlayerUI.UpdateHealthUIWithMax?.Invoke(health.Value, (ushort)maxHealth);
                PlayerUI.UpdateAttackUI?.Invoke((ushort)GetDamage());
            }

            maxHealth = CalculateMaxHealth(newLevel);
            levelWorldText.text = newLevel.ToString();

            if (flasher) flasher.Flash(lvlFlashColor, 0.5f);

            UpdateHPDisplay();
        }

        private float CalculateMaxHealth(ushort level)
        {
            if (level == 1)
                return 10 + (level - 1);
            else
                return 10 * (level * 2.5f);
        }

        private void UpdateHPDisplay()
        {
            float p = Mathf.Clamp01(health.Value / maxHealth);

            if (fill)
                fill.transform.localScale = new Vector3(p, 1.0f, 1.0f);

            if (healthDisplay != null)
                healthDisplay.SetActive(p > 0.0f);

        }

        public void PickUpHealGem(ushort amount)
        {
            if (!IsServer)
                return;

            health.Value = (ushort)Mathf.Min(health.Value + amount, maxHealth);
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
                if (Random.value <= 0.75f)
                {
                    if (shieldActive.Value)
                    {
                        shieldActive.Value = false;
                        return;
                    }

                    health.Value = (ushort)Mathf.Max(health.Value - amount, 0);
                }
            }
        }

        public int GetDamage()
        {
            float baseDamage = 1f;
            return Mathf.CeilToInt(baseDamage * Mathf.Pow(1.5f, level.Value - 1));
        }

        public void TryLevelUp()
        {
            if (IsOwner && HasEnoughDiamonds(2))
            {
                UseDiamonds(2);
                AttemptLevelUpServerRpc();
            }
        }

        public void ActivateShield()
        {
            if (IsOwner && HasEnoughDiamonds(1) && !shieldActive.Value)
            {
                UseDiamonds(1);
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
                float newMaxHealth = CalculateMaxHealth(level.Value);
                health.Value = (ushort)Mathf.Min(health.Value + (newMaxHealth / 5), newMaxHealth);
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
            if (Random.value <= 0.5f)
            {
                shieldActive.Value = true;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRematchServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            if (votedClientIds.Contains(clientId))
                return; // Already voted

            votedClientIds.Add(clientId);
            rematchVotes++;

            if (rematchVotes >= NetworkManager.Singleton.ConnectedClientsList.Count)
            {
                // Reset for next time
                rematchVotes = 0;
                votedClientIds.Clear();

                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    var playerObj = client.PlayerObject;
                    var diamondSystem = playerObj.GetComponent<DiamondGemSystem>();

                    if (diamondSystem != null)
                    {
                        diamondSystem.ResetStats(); // Trigger reset for each player
                    }
                }

                if (GameOverUI.Instance != null)
                    GameOverUI.Instance.RemoveAllGems();
                
                HideGameOverUIClientRpc();
            }
        }

        public void ResetStats()
        {
            health.Value = 10;
            diamonds.Value = 0;
            level.Value = 1;
            shieldActive.Value = false;

            float newMaxHealth = CalculateMaxHealth(level.Value);
            health.Value = (ushort)newMaxHealth;

            UpdateSpeedClientRpc(level.Value);
            ResetStatsClientRpc();
        }

        [ClientRpc]
        private void HideGameOverUIClientRpc()
        {
            if (GameOverUI.Instance != null)
            {
                GameOverUI.Instance.Hide();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetStatsServerRpc(ServerRpcParams rpcParams = default)
        {
            health.Value = 10;
            diamonds.Value = 0;
            level.Value = 1;
            shieldActive.Value = false;

            float newMaxHealth = CalculateMaxHealth(level.Value);
            health.Value = (ushort)newMaxHealth;

            UpdateSpeedClientRpc(level.Value);
            ResetStatsClientRpc();
        }

        [ClientRpc]
        private void ResetStatsClientRpc()
        {
            if (IsOwner)
            {
                maxHealth = CalculateMaxHealth(level.Value);
                PlayerUI.UpdateDiamondUI?.Invoke(diamonds.Value);
                PlayerUI.UpdateHealthUIWithMax?.Invoke(health.Value, (ushort)maxHealth);
                PlayerUI.UpdateLevelUI?.Invoke(level.Value);
                PlayerUI.UpdateAttackUI?.Invoke((ushort)GetDamage());

                if (shieldVisual != null)
                    shieldVisual.SetActive(false);

                UpdateHPDisplay();
            }
        }
    }
}
