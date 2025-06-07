using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;

namespace LuckyMultiplayer.Scripts
{
    public class LoginAndMatchmaking : NetworkBehaviour
    {
        [SerializeField] private Spawner spawner;
        [SerializeField] private GameObject loginUI;
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI waitingText;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button leaveButton;

        private string loggedInUsername;
        private bool isCancelling = false;
        //private bool stopAskingForMatch = false;

        public override void OnNetworkSpawn()
        {
            // Only show UI for clients
            if (IsClient && !IsServer)
                loginUI.SetActive(true);
        }

        private void Start()
        {
            loginButton.onClick.AddListener(() => LoginButtonClicked());
            leaveButton.onClick.AddListener(() => LeaveButtonClicked());

            leaveButton.gameObject.SetActive(false);
        }

        private void LoginButtonClicked()
        {
            //stopAskingForMatch = false;
            string username = usernameInput.text;
            StartCoroutine(LoginCoroutine(username));
        }

        private void LeaveButtonClicked()
        {
            isCancelling = true; // For the coroutines to stop

            if (!string.IsNullOrEmpty(loggedInUsername))
            {
                StartCoroutine(CancelMatchmakingCoroutine(loggedInUsername));
            }
            else
            {
                Application.Quit();
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }
        }

        private IEnumerator LoginCoroutine(string username)
        {
            string url = "http://localhost:3000/login";

            WWWForm form = new WWWForm();
            form.AddField("username", username);

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                loggedInUsername = username;
                statusText.text = "Log in successful!";
                StartCoroutine(MatchmakingCoroutine(username));
            }
            else
            {
                statusText.text = "Login failed: " + request.error + "\n" + request.downloadHandler.text;
            }
        }

        private IEnumerator MatchmakingCoroutine(string username)
        {
            string url = "http://localhost:3000/matchmake";

            WWWForm form = new WWWForm();
            form.AddField("username", username);

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Matchmaking success: " + request.downloadHandler.text);

                var result = JsonUtility.FromJson<MatchResponse>(request.downloadHandler.text);
                
                if (result.success)
                {
                    
                    usernameInput.gameObject.SetActive(false);
                    loginButton.gameObject.SetActive(false);
                    leaveButton.gameObject.SetActive(true);

                    if (result.role == "waiting")
                    {
                        waitingText.text = "Waiting for another opponent...";
                        StartCoroutine(PollMatchStatusCoroutine(username));
                    }

                    if (result.role == "client")
                    {
                        waitingText.text = "Match found! Joining...";

                        StartCoroutine(StartGameSoon(result));
                    }
                }
            }
            else
            {
                Debug.LogError("Matchmaking failed: " + request.error);
            }
        }

        private IEnumerator CancelMatchmakingCoroutine(string username)
        {
            string url = "http://localhost:3000/cancel";

            WWWForm form = new WWWForm();
            form.AddField("username", username);

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Cancelled matchmaking successfully.");
            }
            else
            {
                Debug.LogError("Failed to cancel matchmaking: " + request.error);
            }

            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        private IEnumerator StartGameSoon(MatchResponse result)
        {
            yield return new WaitForSeconds(3f);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.Address = result.host;
            transport.ConnectionData.Port = (ushort)result.port;

            if (loginUI != null)
                loginUI.SetActive(false);

            spawner.StartSpawnServerRpc();
        }

        private IEnumerator PollMatchStatusCoroutine(string username)
        {
            string url = "http://localhost:3000/matchmake";

            while (!isCancelling)
            {
                yield return new WaitForSeconds(1f);

                if (isCancelling)
                    yield break;

                WWWForm form = new WWWForm();
                form.AddField("username", username);

                UnityWebRequest request = UnityWebRequest.Post(url, form);
                request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var result = JsonUtility.FromJson<MatchResponse>(request.downloadHandler.text);

                    if (result.success && result.role == "client")
                    {
                        waitingText.text = "Match found! Joining...";
                        StartCoroutine(StartGameSoon(result)); // <== Add this!
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError("Polling matchmaking failed: " + request.error);
                }
            }
        }
        
        [System.Serializable]
        public class MatchResponse
        {
            public bool success;
            public string host;
            public int port;
            public string role;
        }
    }
}
