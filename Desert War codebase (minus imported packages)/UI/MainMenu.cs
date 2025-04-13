using UnityEngine;
using Mirror;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject landingPagePanel;
    [SerializeField] GameObject lobbyMenu;
    [SerializeField] GameObject joinMenu;

    private void Start()
    {
        if (NetData.isAI || NetData.isRemoteServer)
            Invoke(nameof(HostLobby), 8 * Time.deltaTime);
        NetManager.ClientOnDisconnected += HandleClientDisconnected;
    }

    private void OnDestroy()
    {
        NetManager.ClientOnDisconnected -= HandleClientDisconnected;
    }


    public void HostLobby()
    {
        try
        {
            landingPagePanel.SetActive(false);
            LobbyMenu.isHost = true;
            NetworkManager.singleton.StartHost();
        }
        catch
        {
            Informer.Show("Failed to host lobby. Maybe this pc is already hosting a lobby?", 5);
            landingPagePanel.SetActive(true);
            LobbyMenu.isHost = false;
        }
    }


    public void Quit()
    {
        print("QUIT");
        Application.Quit();
    }

    void HandleClientDisconnected()
    {
        if (lobbyMenu.activeSelf)
            Informer.Show("The lobby has been destroyed.", 5);
        else
            Informer.Show("Failed to establish connection", 5);

        landingPagePanel.SetActive(true);
        lobbyMenu.SetActive(false);
        joinMenu.SetActive(false);
        
    }

}
