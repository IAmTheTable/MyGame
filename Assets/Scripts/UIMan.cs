using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;
using Unity.Jobs;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine.Networking.Types;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using WebSocketSharp;
using UnityEngine.Networking;
using RCUnityWebRequest = Unity.Services.RemoteConfig.RCUnityWebRequest;
using Unity.Services.RemoteConfig;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.Diagnostics;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;
using static Response;

public class Response
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ChangeStatus
    {
        public int total { get; set; }
        public int additions { get; set; }
        public int deletions { get; set; }
    }

    public class Files
    {
        [JsonProperty("serverip.ip")]
        public ServeripIp serveripip { get; set; }
    }

    public class History
    {
        public User user { get; set; }
        public string version { get; set; }
        public DateTime committed_at { get; set; }
        public ChangeStatus change_status { get; set; }
        public string url { get; set; }
    }

    public class Owner
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }

    public class Root
    {
        public string url { get; set; }
        public string forks_url { get; set; }
        public string commits_url { get; set; }
        public string id { get; set; }
        public string node_id { get; set; }
        public string git_pull_url { get; set; }
        public string git_push_url { get; set; }
        public string html_url { get; set; }
        public Files files { get; set; }
        public bool @public { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string description { get; set; }
        public int comments { get; set; }
        public object user { get; set; }
        public string comments_url { get; set; }
        public Owner owner { get; set; }
        public List<object> forks { get; set; }
        public List<History> history { get; set; }
        public bool truncated { get; set; }
    }

    public class ServeripIp
    {
        public string filename { get; set; }
        public string type { get; set; }
        public object language { get; set; }
        public string raw_url { get; set; }
        public int size { get; set; }
        public bool truncated { get; set; }
        public string content { get; set; }
    }

    public class User
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }


}


public class UIMan : MonoBehaviour
{
    private static string ip = "";
    private static ushort port = 7777;
    private bool hasServer = false;
    public static UIMan singleton;
    public TextMeshProUGUI playerCount;
    public GameObject GameManager;
    public GameObject host;
    public GameObject client;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI fpsText;

    public GameObject leaderboardParent;
    public GameObject leaderboardNamePrefab;

    public GameObject svlb;
    public TMP_InputField usernameInput;
    public string Name;

    public GameObject LoadingFrame;
    public GameObject UpgradeFrame;
    public GameObject Minimap;
    public LevelUI levelUI;

    public Rpc rpc;
    private static int fps = 0;

    public struct userAttributes { }
    public struct appAttributes { }

    async Task InitializeRemoteConfigAsync()
    {
        // initialize handlers for unity game services
        await UnityServices.InitializeAsync();


        // remote config requires authentication for managing environment information
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
    private string GetLocalIPAddress()
    {
        return Dns.GetHostName();
    }

    public async void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;


        // initialize Unity's authentication and core services, however check for internet connection
        // in order to fail gracefully without throwing exception if connection does not exist
        if (Utilities.CheckForInternetConnection())
        {
            await InitializeRemoteConfigAsync();
        }
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        await RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes());   
    }
    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        ip = RemoteConfigService.Instance.appConfig.config.GetValue("server-ip").ToSafeString();
        port = ((ushort)RemoteConfigService.Instance.appConfig.config.GetValue("server-port"));

        // validate the server ip
        if (ip.Contains("127.0.0.1") || ip.IsNullOrEmpty())
        {
            hasServer = false;
        }
        else
            hasServer = true;
        Debug.Log("Got IP: " + ip);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
            /*Found Server and Done Scanning*/
            ip,  // The IP address is a string
            port,
            "127.0.0.1"// The port number is an unsigned short,
        );
        LoadingFrame.SetActive(false);

        // enable loading frame and set singleton, start FPS update loop
        singleton = this;
        StartCoroutine(fpsUpdateLoop());
    }
    private void Singleton_OnClientDisconnectCallback(ulong obj)
    {
        if (NetworkManager.Singleton.IsClient)
        {

        }
    }

    private void Update()
    {
        fps = (int)(1f / Time.unscaledDeltaTime);
    }

    public void LateUpdate()
    {
        StartCoroutine(loadingAnim());
    }
    public void FixedUpdate()
    {
        // ctrl shift t 
        bool keySeq = Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T);

        if (keySeq && uiMode == false)
        {
            uiMode = true;

            host.SetActive(false);
            client.SetActive(false);
            usernameInput.gameObject.SetActive(false);
        }
        else if (keySeq && uiMode == true)
        {
            uiMode = false;

            host.SetActive(true);
            client.SetActive(true);
            usernameInput.gameObject.SetActive(true);
        }
    }
    IEnumerator loadingAnim()
    {
        var loadFrameTxt = LoadingFrame.GetComponentInChildren<TextMeshProUGUI>();
        loadFrameTxt.text = "Connecting";
        for (int i = 0; i < 3; i++)
        {
            loadFrameTxt.text += ".";
            loadFrameTxt.SetText(loadFrameTxt.text);
            new WaitForSeconds(0.5f);
        }
        yield return null;
    }
    IEnumerator fpsUpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
            fpsText.text = "FPS: " + fps;
        }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);


    public void OnHost()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(usernameInput.text);

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnect;

        NetworkManager.Singleton.StartHost();
        client.SetActive(false);

        // spawn the game manager on the server
        GameObject gm = Instantiate(GameManager, Vector3.zero, Quaternion.identity);
        gm.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        svlb.SetActive(true);
        //rpc.RenameServerRpc(Name);
        usernameInput.gameObject.SetActive(false);
    }


    private void ClientConnect(ulong obj)
    {
        NetworkManager.Singleton.ConnectedClients[obj].PlayerObject.GetComponent<PlayerManager>().playerName.Value = Name;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Additional connection data defined by user code
        var connectionData = request.Payload;
        var remotePayload = System.Text.Encoding.ASCII.GetString(connectionData);

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Got approval(SERVER)");
            response.Approved = true;

            // Your approval logic determines the following values
            response.CreatePlayerObject = true;

            // The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
            response.PlayerPrefabHash = null;

            // Position to spawn the player object (if null it uses default of Vector3.zero)
            response.Position = Vector3.zero;
            // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
            response.Rotation = Quaternion.identity;

            // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
            // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
            response.Reason = "Some reason for not approving the client";

            // If additional approval steps are needed, set this to true until the additional steps are complete
            // once it transitions from true to false the connection approval response will be processed.
            response.Pending = false;
        }
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Got approval(CLIENT)");
            Name = remotePayload;
            UpgradeFrame.SetActive(true);
            Minimap.GetComponent<MinimapService>().Minimap.SetActive(true);
            levelUI.Base.SetActive(true);
        }
    }
    public async void OnJoin()
    {
        await RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes());

        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(usernameInput.text);

        Debug.Log("DOING CLIENT");

        // hide objects
        client.SetActive(false);
        svlb.SetActive(true);
        // set name
        usernameInput.gameObject.SetActive(false); // hide last since we still use the text.
        NetworkManager.Singleton.StartClient();
    }

    public bool uiMode = true; // true for show, false for hide
}