using Unity.Netcode;
using UnityEngine;

namespace LuckyMultiplayer.Scripts
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float speed = 3f;
        private Camera mainCamera;
        private Vector3 mouseInput = Vector3.zero;

        private void Initialize()
        {
            mainCamera = Camera.main;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Initialize();
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
            transform.position = Vector3.MoveTowards(transform.position, mouseWorldCoordinates, Time.deltaTime * speed);

            // Rotation
            if (mouseWorldCoordinates != transform.position)
            {
                Vector3 targetDirection = mouseWorldCoordinates - transform.position;
                targetDirection.z = 0f;
                transform.up = targetDirection;
            }
            
        }
    }
}
