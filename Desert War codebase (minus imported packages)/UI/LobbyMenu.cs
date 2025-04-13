using System.Net;
using System.Net.Sockets;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] GameObject lobbyUI;
    [SerializeField] Button startGameButton;
    [SerializeField] TMP_Text networkAddressText;
    [SerializeField] TMP_InputField[] playerNames = new TMP_InputField[4];
    [SerializeField] TMP_Dropdown[] teams = new TMP_Dropdown[4];
    [SerializeField] Image[] localPlayerIndicators = new Image[4];

    public static bool isHost;
    bool localPlayerHasAlreadyConnected;
    int localPlayerIndex;
    static string localPlayerCustomName;
    public static LobbyMenu instance;

    void OnEnable()
    {
        if (instance == null)
            instance = this;
        NetManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerUpdated += AuthorityHandlePartyOwnerUpdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
        Invoke(nameof(SetNetworkAddress), Time.deltaTime);

        for (int i = 0; i < playerNames.Length; i++)
            playerNames[i].onValueChanged.AddListener((string _) => OnPlayerNameChanged());
        

    }

    void OnDisable()
    {
        NetManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerUpdated -= AuthorityHandlePartyOwnerUpdated;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
        localPlayerHasAlreadyConnected = false;
    }



    void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
        Informer.Show("");
        startGameButton.gameObject.SetActive(true);
        // startGameButton.gameObject.SetActive(isHost || NetData.isSinglePlayer);
        networkAddressText.transform.parent.gameObject.SetActive(isHost);
        
        
    }

    void AuthorityHandlePartyOwnerUpdated(bool becomesPartyOwner)
    {
        startGameButton.gameObject.SetActive(becomesPartyOwner);
        networkAddressText.transform.parent.gameObject.SetActive(isHost);
    }

    public void ClientHandleInfoUpdated()
    {
        List<RTSPlayer> players = NetManager.instance.humanPlayers;
        startGameButton.interactable = (players.Count >= NetManager.instance.minPlayerCount);

        for (int i=0; i<players.Count; i++)
            playerNames[i].text = players[i].displayName;
        for (int i = players.Count; i < playerNames.Length; i++)
            playerNames[i].text = "Waiting For Player...";

        //for (int i = 0; i < teams.Length; i++)
        //    teams[i].SetValueWithoutNotify(i);

        this.Delay(ClientHandleInfoUpdated_part_2, 3 * Time.deltaTime);
    }

    void ClientHandleInfoUpdated_part_2()
    {
        List<RTSPlayer> players = NetManager.instance.humanPlayers;

        if (localPlayerHasAlreadyConnected)
            return;
        localPlayerHasAlreadyConnected = true;

        for (int i = 0; i < playerNames.Length; i++)
        {
            playerNames[i].enabled = false;
            localPlayerIndicators[i].enabled = false;
            teams[i].enabled = false;
        }

        if (players.Count > 0)
        {
            localPlayerIndex = players.Count - 1;
            playerNames[localPlayerIndex].enabled = true;
            localPlayerIndicators[localPlayerIndex].enabled = true;
            teams[localPlayerIndex].enabled = true;
            if (localPlayerCustomName != null)
                playerNames[localPlayerIndex].text = localPlayerCustomName;
        }

    }

    public static void LeaveLobby()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
            NetworkManager.singleton.StopHost();
        else
            NetworkManager.singleton.StopClient();
        isHost = false;
        Destroy(NetworkManager.singleton);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }


    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
    }



    void SetNetworkAddress()
    {
        IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName())
            .AddressList
            .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
            .ToArray();
        string address = addressList.Length > 1 ? addressList[1].ToString() : addressList[0].ToString();
        networkAddressText.text = "Your lobby password is: " + Encrypter.Encrypt(address);

        if (NetData.isRemoteServer)
        {
            string externalAddress = new WebClient().DownloadString("http://icanhazip.com")
                .Replace("\\r\\n", "")
                .Replace("\\n", "")
                .Trim();

            print($"\n\nthe local address is encrypter.Encrypt({address}) = {Encrypter.Encrypt(address)}\n\n");
            print($"\n\nthe public address is encrypter.Encrypt({externalAddress}) = {Encrypter.Encrypt(externalAddress)}\n\n");
        }

    }


    
    public void OnPlayerNameChanged()
    {
        List<RTSPlayer> players = NetManager.instance.humanPlayers;
        if (players.Count <= localPlayerIndex)
            return;
        if (!localPlayerHasAlreadyConnected)
            return;
        RTSPlayer localPlayer = NetworkClient.localPlayer.GetComponent<RTSPlayer>();
        if (localPlayer != players[localPlayerIndex])
            return;

        localPlayerCustomName = playerNames[localPlayerIndex].text;
        players[localPlayerIndex].CmdSetDisplayName(localPlayerCustomName);
    }


    public void OnTeamDropDownChanged(int playerIndex) // kaldes fra UI
    {
        List<RTSPlayer> players = NetManager.instance.humanPlayers;
        if (players.Count <= playerIndex)
            return;
        RTSPlayer localPlayer = NetworkClient.localPlayer.GetComponent<RTSPlayer>();
        if (localPlayer != players[localPlayerIndex])
            return;
        if (localPlayerIndex != playerIndex)
            return;
        players[playerIndex].CmdSetTeam(teams[playerIndex].value + 1);
    }


    public void OnTeamChanged(RTSPlayer player)
    {
        List<RTSPlayer> players = NetManager.instance.humanPlayers;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == player)
            {
                teams[i].gameObject.SetActive(true);
                teams[i].SetValueWithoutNotify(player.team - 1);
            }
        }

    }




}
