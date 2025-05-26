using Unity.Netcode;
using UnityEngine;

namespace LuckyMultiplayer.Scripts
{
    public class PlayerController : NetworkBehaviour
    {
        private Camera mainCamera;
        private Vector3 mouseInput = Vector3.zero;
        [SerializeField] private float baseSpeed = 3f;
        private float currentSpeed;

        private DiamondGemSystem diamondGemSystem;

        private void Initialize()
        {
            mainCamera = Camera.main;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Initialize();
            diamondGemSystem = GetComponent<DiamondGemSystem>();
        }

        private void Start()
        {
            currentSpeed = baseSpeed;
        }

        private void Update()
        {
            if (!NetworkObject.IsLocalPlayer || !Application.isFocused)
                return;

            // Movement
            mouseInput.x = Input.mousePosition.x;
            mouseInput.y = Input.mousePosition.y;
            mouseInput.z = mainCamera.nearClipPlane;

            Vector3 mouseWorldCoordinates = mainCamera.ScreenToWorldPoint(mouseInput);
            mouseWorldCoordinates.z = 0f;
            transform.position = Vector3.MoveTowards(transform.position, mouseWorldCoordinates, Time.deltaTime * currentSpeed);

            // Rotation
            if (mouseWorldCoordinates != transform.position)
            {
                Vector3 targetDirection = mouseWorldCoordinates - transform.position;
                targetDirection.z = 0f;
                transform.up = targetDirection;
            }

            // Input
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TryUseDiamonds();
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                TryLevelUp();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                TryToShield();
            }
        }

        private void TryUseDiamonds()
        {
            if (diamondGemSystem != null && diamondGemSystem.HasEnoughDiamonds(1))
            {
                diamondGemSystem.UseDiamonds(1);

                // Deal damage to another player
                TryDealDamageToOtherPlayer();
            }
        }

        // new
        private void TryDealDamageToOtherPlayer()
        {
            int damage = diamondGemSystem.GetDamage();
            foreach (var playerObj in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            {
                if (playerObj != this)
                {
                    playerObj.ReduceHealthServerRpc((ushort)damage);
                }
            }
        }

        // new
        [ServerRpc(RequireOwnership = false)]
        public void ReduceHealthServerRpc(ushort damage)
        {
            diamondGemSystem.ReduceHealth(damage);
        }

        private void TryLevelUp()
        {
            diamondGemSystem?.TryLevelUp();
        }

        // Called by server to update speed
        public void UpdateSpeed(ushort newLevel)
        {
            currentSpeed = baseSpeed + newLevel * 1.5f;
        }

        private void TryToShield()
        {
            diamondGemSystem?.ActivateShield();
        }
    }
}
