using Unity.Netcode;
using UnityEngine;

namespace LuckyMultiplayer.Scripts
{
    public class DiamondGem : NetworkBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!collider.CompareTag("Player"))
                return;

            if (!NetworkManager.Singleton.IsServer)
                return;

            if (collider.TryGetComponent(out DiamondGemSystem player))
            {
                player.PickUpDiamondGem();
            }

            NetworkObject.Despawn(); // removes it from the scene
        }
    }
}
