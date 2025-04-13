using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.SceneManagement;

public class Surrenderer : NetworkBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject SettingsMenu;
    [SerializeField] GameObject ControlsMenu;


    private void Start()
    {
        pausePanel.SetActive(false);
        if (NetData.isRemoteServer)
            Invoke(nameof(Surrender), NetData.mapLoadingTime); // this has to be long enough for the map to finish loading.
        GameOverHandler.clientOnGameOver += clientOnGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.clientOnGameOver -= clientOnGameOver;
    }


    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            ToggleActive();
        else if (Keyboard.current.f10Key.wasPressedThisFrame)
            ToggleActive();
    }


    public void Resume()
    {
        pausePanel.SetActive(false);
        SettingsMenu.SetActive(false);
        ControlsMenu.SetActive(false);
    }

    void ToggleActive()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
        if (!pausePanel.activeSelf)
        {
            SettingsMenu.SetActive(false);
            ControlsMenu.SetActive(false);
        }
    }

    public void Surrender()
    {
        if (Base.authorityBases.Count == 0)
        {
            Debug.LogWarning($"This player no bases.");
            return;
        }
        pausePanel.SetActive(false);
        Base.authorityBases[0].Surrender();
    }

    private void clientOnGameOver(string playerName, bool hasWon)
    {
        pausePanel.SetActive(false);
        SettingsMenu.SetActive(false);
        ControlsMenu.SetActive(false);
        enabled = false;
    }



}
