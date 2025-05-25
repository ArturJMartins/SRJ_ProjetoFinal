using Unity.Netcode;
using UnityEngine;

namespace LuckyMultiplayer.Scripts
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private int   minPlayers = 2;
        [SerializeField] private float timeForFirstSpawn = 2;
        [SerializeField] private float spawnInterval = 10;
        [SerializeField] private int   spawnCount = 5;
        [SerializeField] private DiamondGem diamondGemPrefab;

        private float          spawnTimer;
        private NetworkManager networkManager;
        private int currentPlayers => (networkManager == null) ? 0 : networkManager.ConnectedClients.Count;

        private void Start()
        {
            spawnTimer = timeForFirstSpawn;
            networkManager = FindFirstObjectByType<NetworkManager>();
        }

        private void Update()
        {
            if (networkManager.IsServer || networkManager.IsHost)
            {
                if (currentPlayers >= minPlayers)
                {
                    spawnTimer -= Time.deltaTime;
                    if (spawnTimer <= 0.0f)
                    {
                        Spawn();
                        spawnTimer = spawnInterval;
                    }
                }
                else if (currentPlayers == 0)
                {
                    var gems = FindObjectsByType<DiamondGem>(FindObjectsSortMode.None);
                    foreach (var gem in gems)
                    {
                        Destroy(gem.gameObject);
                    }
                    spawnTimer = timeForFirstSpawn;
                }
            }
        }

        private void Spawn()
        {
            // Get all players
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            if (players.Length == 0) return;

            float xMin = players[0].transform.position.x;
            float yMin = players[0].transform.position.y;
            float xMax = xMin;
            float yMax = yMin;

            foreach (var player in players)
            {
                xMin = Mathf.Min(xMin, player.transform.position.x);
                xMax = Mathf.Max(xMax, player.transform.position.x);
                yMin = Mathf.Min(yMin, player.transform.position.y);
                yMax = Mathf.Max(yMax, player.transform.position.y);
            }

            for (int i = 0; i < spawnCount; i++)
            {
                float x = Random.Range(xMin - 10, xMax + 10);
                float y = Random.Range(yMin - 10, yMax + 10);

                var newObject = Instantiate(diamondGemPrefab, new Vector3(x, y, 0), Quaternion.identity);
                var networkObject = newObject.GetComponent<NetworkObject>();
                networkObject.Spawn(true);
            }
        }
    }
}
