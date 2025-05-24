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
            /*var wyzards = FindObjectsByType<Wyzard>(FindObjectsSortMode.None);
            if (wyzards.Length == 0) return;

            float xMin = wyzards[0].transform.position.x;
            float yMin = wyzards[0].transform.position.y;
            float xMax = xMin;
            float yMax = yMin;

            foreach (var wyzard in wyzards)
            {
                xMin = Mathf.Min(xMin, wyzard.transform.position.x);
                xMax = Mathf.Max(xMax, wyzard.transform.position.x);
                yMin = Mathf.Min(yMin, wyzard.transform.position.y);
                yMax = Mathf.Max(yMax, wyzard.transform.position.y);
            }

            for (int i = 0; i < spawnCount; i++)
            {
                float x = Random.Range(xMin - 20, xMax + 20);
                float y = Random.Range(yMin - 20, yMax + 20);

                var newObject = Instantiate(diamondGemPrefab, new Vector3(x, y, 0), Quaternion.identity);
                var networkObject = newObject.GetComponent<NetworkObject>();
                networkObject.Spawn(true);
            }*/
        }
    }
}
