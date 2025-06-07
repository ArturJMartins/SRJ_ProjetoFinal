using Unity.Netcode;
using UnityEngine;

namespace LuckyMultiplayer.Scripts
{
    public class Spawner : NetworkBehaviour
    {
        [SerializeField] private int minPlayers = 2;
        [SerializeField] private float timeForFirstSpawn = 2;
        [SerializeField] private float spawnInterval = 7;
        [SerializeField] private int spawnCount = 6;
        [SerializeField] private float xMin = -20f;
        [SerializeField] private float xMax = 20f;
        [SerializeField] private float yMin = -8f;
        [SerializeField] private float yMax = 10f;
        [SerializeField] private DiamondGem diamondGemPrefab;
        [SerializeField] private HealBuff healGemPrefab;

        private float spawnTimer;
        private NetworkManager networkManager;
        private int currentPlayers => (networkManager == null) ? 0 : networkManager.ConnectedClients.Count;

        private bool isMatchMade = false;

        private void Start()
        {
            spawnTimer = timeForFirstSpawn;
            networkManager = FindFirstObjectByType<NetworkManager>();
        }

        private void Update()
        {
            if (networkManager.IsServer || networkManager.IsHost)
            {
                if (currentPlayers >= minPlayers && isMatchMade)
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
                    var healGems = FindObjectsByType<HealBuff>(FindObjectsSortMode.None);
                    foreach (var gem in gems)
                    {
                        Destroy(gem.gameObject);
                    }
                    foreach (var gem in healGems)
                    {
                        Destroy(gem.gameObject);
                    }
                    spawnTimer = timeForFirstSpawn;

                    isMatchMade = false;
                }
            }
        }

        private void Spawn()
        {
            for (int i = 0; i < spawnCount; i++)
            {
                float x = Random.Range(xMin, xMax);
                float y = Random.Range(yMin, yMax);

                float chance = Random.value;
                if (chance < 0.90f) // 90% chance
                {
                    var newObject = Instantiate(diamondGemPrefab, new Vector3(x, y, 0), Quaternion.identity);
                    var networkObject = newObject.GetComponent<NetworkObject>();
                    networkObject.Spawn(true);
                }
                else // 10% 
                {
                    var newObject = Instantiate(healGemPrefab, new Vector3(x, y, 0), Quaternion.identity);
                    var networkObject = newObject.GetComponent<NetworkObject>();
                    networkObject.Spawn(true);
                }
            }
        }

        public void RemoveAllGems()
        {
            var gems = FindObjectsByType<DiamondGem>(FindObjectsSortMode.None);
            var healGems = FindObjectsByType<HealBuff>(FindObjectsSortMode.None);
            
            foreach (var gem in gems)
            {
                Destroy(gem.gameObject);
            }
            
            foreach (var gem in healGems)
            {
                Destroy(gem.gameObject);
            }

            spawnTimer = timeForFirstSpawn;
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartSpawnServerRpc()
        {
            isMatchMade = true;
        }
    }
}
