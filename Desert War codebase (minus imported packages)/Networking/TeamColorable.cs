using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TeamColorable : NetworkBehaviour
{
    [SerializeField] Renderer[] colorRenderers = new Renderer[0];
    [SerializeField] int[] materialIndices;

    [SyncVar(hook = nameof(HandleTeamColorUpdated))] Color teamColor = new Color();


    #region server

    public override void OnStartServer()
    {
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
        teamColor = player.teamColor;
    }


    #endregion




    #region client

    void HandleTeamColorUpdated(Color oldColor, Color newColor)
    {
        CheckMaterialIndices();
        for (int i = 0; i < colorRenderers.Length; i++)
        {
            colorRenderers[i].materials[materialIndices[i]].SetColor("_Color", newColor);
            colorRenderers[i].materials[materialIndices[i]].SetColor("_BaseColor", newColor);
        }
    }

    void CheckMaterialIndices()
    {
        if (materialIndices.Length == 0)
        {
            materialIndices = new int[colorRenderers.Length];
            for (int i = 0; i < materialIndices.Length; i++)
                materialIndices[i] = 0;
        }
        else if (materialIndices.Length != colorRenderers.Length)
            Debug.LogWarning($"materialIndices.Length must either equal colorRenderers.Length or be zero");
    }

    #endregion



}
