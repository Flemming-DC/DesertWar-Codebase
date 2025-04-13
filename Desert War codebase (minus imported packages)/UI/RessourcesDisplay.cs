using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using System;

public class RessourcesDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text resourcesText;

    RTSPlayer player;
    
    void Start()
    {
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        player.ClientOnRessourcesUpdated += ClientOnRessourcesUpdated;
        resourcesText.text = player.resources.ToString();
    }

    private void OnDestroy()
    {
        player.ClientOnRessourcesUpdated -= ClientOnRessourcesUpdated;
    }

    private void ClientOnRessourcesUpdated(int resources)
    {
        resourcesText.text = resources.ToString();
    }
}
