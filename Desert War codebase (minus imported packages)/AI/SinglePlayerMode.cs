using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class SinglePlayerMode : MonoBehaviour
{
    [SerializeField] GameObject landingPagePanel;
    [SerializeField] GameObject DifficultyDisplay;
    [SerializeField] string editorAIFolder = "Desert War - version XX";

    string pathToBuildFiles = @"C:\Users\Flemming\Documents\Spil - hjemmelavede\Unity\RTS\Build Files\";


    private void OnApplicationQuit()
    {
        CommandLine.CleanUp();
    }


    public void StartSinglePlayer()
    {
        StartCoroutine(SinglePlayerRoutine());
    }

    IEnumerator SinglePlayerRoutine()
    {
        NetData.isSinglePlayer = true;
        landingPagePanel.SetActive(false);
        DifficultyDisplay.SetActive(true);
        bool difficultyChosen = false;
        while (!difficultyChosen)
        {
            for (int i = 0; i < 10; i++)
            {
                if (!Keyboard.current.DigitKey(i).wasPressedThisFrame)
                    continue;

                string path = !Application.isEditor ? "" : pathToBuildFiles + editorAIFolder + @"\";
                string startAICommand = $"\"{path}Desert War\".exe -AI {i}";
                //This command can potentially create invisible instances of desert war that eats up ram and cpu
                //This is particularly bad, when -batchmode -nographics IS USED. 
                //Singleplayer mode has been dropped for this reason.
                //CommandLine.Run(startAICommand);

                difficultyChosen = true;
                //AIActivator.aiActivated = true;
                //AIActivator.difficulty = i;

                break;
            }

            if (!NetData.isSinglePlayer)
            {
                landingPagePanel.SetActive(true);
                DifficultyDisplay.SetActive(false);
                yield break;
            }
            yield return null;
        }
        DifficultyDisplay.SetActive(false);

        //wait until ai started
        yield return new WaitForSeconds(6);

        JoinAsSinglePlayer();

    }

    void JoinAsSinglePlayer()
    {
        LobbyMenu.isHost = false;
        NetworkManager.singleton.networkAddress = "localhost";
        NetworkManager.singleton.StartClient();
    }

    public void CancelSinglePlayerMode()
    {
        NetData.isSinglePlayer = false;
    }







}
