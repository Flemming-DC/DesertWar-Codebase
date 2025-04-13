using UnityEngine;
using TMPro;
using Mirror;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text winnerNameText;
    [SerializeField] GameObject GameOverUI;

    void Start()
    {
        GameOverHandler.clientOnGameOver += ClientOnGameOver;
        GameOverUI.SetActive(false);
    }

    void OnDestroy()
    {
        GameOverHandler.clientOnGameOver -= ClientOnGameOver;
    }

    private void Update()
    {
        if (NetManager.instance.players.Count > 1)
            return;
        if (NetData.isRemoteServer || NetData.isAI)
            LeaveGame();
    }

    public void LeaveGame()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
            NetworkManager.singleton.StopHost();
        else
            NetworkManager.singleton.StopClient();

    }

    private void ClientOnGameOver(string playerName, bool hasWon)
    {
        winnerNameText.text = hasWon ? $"{playerName} has won" : $"{playerName} has lost";
        GameOverUI.SetActive(true);
    }
}
