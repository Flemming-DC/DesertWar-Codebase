using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class AIActivator : MonoBehaviour
{
    [SerializeField] Settings settings;

    public static bool aiActivated;
    public static int difficulty;
    RTSPlayer localPlayer;

    private void Start()
    {
        localPlayer = NetworkClient.localPlayer.GetComponent<RTSPlayer>();
        if (NetData.isAI)
            this.Delay(() => ToggleAI(1));
        if (difficulty != 0)
            localPlayer.resources += difficulty * 200;
    }

    private void OnDestroy()
    {
        aiActivated = false;
    }

    void Update()
    {
        //if (!settings.inDebugMode && !NetData.isOnFlemmingsPC)
        //    return;
        if (!NetData.isOnFlemmingsPC)
            return;
        if (!Keyboard.current.spaceKey.isPressed)
            return;

        for (int i = 0; i < 10; i++)
        {
            if (!Keyboard.current.DigitKey(i).wasPressedThisFrame)
                continue;

            ToggleAI(i);
        }

    }


    void ToggleAI(int _difficulty)
    {
        aiActivated = !aiActivated;
        if (aiActivated)
        {
            HintManager.SetHint("AI is activated");
            AudioManager.soundIsOn = false;
            print($"SetDifficulty({_difficulty})");
            localPlayer.CmdSetResources(localPlayer.resources + 200 * _difficulty);
            /*
            if (!NetworkServer.active)
            {
                Debug.LogWarning("the AI has been activated on a client, rather than the server / host. This can cause bugs.");
                HintManager.SetHint("the AI has been activated on a client, rather than the server / host. This can cause bugs.");
            }*/
        }
        else
        {
            HintManager.SetHint("AI is deactivated");
            AudioManager.soundIsOn = true;
        }
    }
    


}
