using UnityEngine;
using Mirror;
using TMPro;

public class HintManager : NetworkBehaviour
{
    [SerializeField] string hintTag;
    [SerializeField] float hintDuration;

    public static HintManager instance;
    static TMP_Text hint;
    float timer;

    void Start()
    {
        GameObject hintCanvas = GameObject.FindWithTag(hintTag);
        instance = hintCanvas.GetComponent<HintManager>();
        hint = hintCanvas.GetComponentInChildren<TMP_Text>();

        if (hintCanvas == null)
            Debug.LogWarning($"can't find hintCanvas via tag {hintTag}");
    }


    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
            hint.text = "";
    }


    public static void SetHint(string hintString, NetworkConnection connectionToClient = null, bool useErrorSound = false)
    {
        // SetHint() should receive a connectionToClient, whenever it is called from the server but shouldn't display the hint on the server.
        if (instance.isServer && connectionToClient != null)
            instance.TargetSetHint(connectionToClient, hintString, useErrorSound);
        else
            instance.InternalSetHint(hintString, useErrorSound);
        
    }


    [TargetRpc]
    void TargetSetHint(NetworkConnection connection, string hintString, bool useErrorSound)
    {
        InternalSetHint(hintString, useErrorSound);
    }


    void InternalSetHint(string hintString, bool useErrorSound)
    {
        hint.text = hintString;
        timer = hintDuration;
        if (useErrorSound)
            AudioManager.Play("Error");
    }

}
