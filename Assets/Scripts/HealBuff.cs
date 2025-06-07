using UnityEngine;
using Unity.Netcode;

namespace LuckyMultiplayer.Scripts
{
    public class HealBuff : NetworkBehaviour
    {
        [SerializeField] private ushort healAmount = 100;
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!collider.CompareTag("Player"))
                return;

            if (!NetworkManager.Singleton.IsServer)
                return;

            if (collider.TryGetComponent(out DiamondGemSystem player))
            {
                player.PickUpHealGem(healAmount);
            }

            NetworkObject.Despawn();
        }
    }
}
