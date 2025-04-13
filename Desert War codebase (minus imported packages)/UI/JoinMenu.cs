using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class JoinMenu : MonoBehaviour
{
    [SerializeField] GameObject landingPagePanel;
    [SerializeField] TMP_InputField addressInput;
    [SerializeField] Button joinButton;

    static string previousEncryptedAddress;

    void OnEnable()
    {
        NetManager.ClientOnConnected += HandleClientConnected;
        NetManager.ClientOnDisconnected += HandleClientDisconnected;

        addressInput.text = "";
        if (Application.isEditor)
            addressInput.text = Encrypter.Encrypt("localhost");

        if (previousEncryptedAddress != null)
            addressInput.text = previousEncryptedAddress;
    }

    void OnDisable()
    {
        NetManager.ClientOnConnected -= HandleClientConnected;
        NetManager.ClientOnDisconnected -= HandleClientDisconnected;
    }

    public void Join(bool useRemoteServer)
    {
        string encryptedAddress = useRemoteServer ? NetData.encryptedServerAddress : addressInput.text;
        if (Encrypter.TryDecrypt(encryptedAddress, out string address))
        {
            Informer.Show($"Establishing connection");
            LobbyMenu.isHost = false;
            NetworkManager.singleton.networkAddress = address;
            NetworkManager.singleton.StartClient();
            joinButton.interactable = false; 
            landingPagePanel.SetActive(false);
            previousEncryptedAddress = encryptedAddress;

        }
        else
        {
            Informer.Show("Failed to establish connection", 5);
            landingPagePanel.SetActive(true);
        }
    }

    public void Close()
    {
        LobbyMenu.LeaveLobby();

        //NetworkManager.singleton.StopClient();
        //joinButton.interactable = true;
        //gameObject.SetActive(false);
    }


    void HandleClientConnected()
    {
        joinButton.interactable = true;
        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }




}
