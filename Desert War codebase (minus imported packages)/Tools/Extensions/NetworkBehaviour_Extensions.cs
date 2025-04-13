using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public static class NetworkBehaviour_Extensions
{
    
    public static GameObject Spawn(this NetworkBehaviour behaviour, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject instance = NetworkBehaviour.Instantiate(prefab, position, rotation);
        NetworkServer.Spawn(instance, behaviour.connectionToClient);
        return instance;
    }

    public static GameObject Spawn(this NetworkBehaviour behaviour, GameObject prefab, Vector3 position)
    {
        GameObject instance = NetworkBehaviour.Instantiate(prefab, position, Quaternion.identity);
        NetworkServer.Spawn(instance, behaviour.connectionToClient);
        return instance;
    }

    public static RTSPlayer ServerGetPlayer(this NetworkBehaviour behaviour)
    {
        if (behaviour.connectionToClient == null)
        {
            Debug.LogWarning("connectionToClient is null. Perhaps you called ServerGetPlayer from a client?");
            return null;
        }
        if (behaviour.connectionToClient.identity == null)
            return null;

        return behaviour.connectionToClient.identity.GetComponent<RTSPlayer>();
    }





}
