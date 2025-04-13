using System;
using System.Reflection;
using UnityEngine;
using Mirror;

public class RPC : NetworkBehaviour
{
    
    public static RPC instance;

    public override void OnStartServer()
    {
        print("RPC.start");
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public override void OnStartClient()
    {
        print("RPC.start");
        DontDestroyOnLoad(gameObject);
        instance = this;
        ToServer(GetType(), dum);

    }

    public void dum()
    {
        print("dum");
    }





    public static void ToServer(Type type, Action action)
    {
        print($"RPC.ToServer: {instance}, {type}, {action}");

        //instance.OnServer(nameof(type), nameof(action));
        //instance.OnServer("RPC", "dum");
    }

    [Command(requiresAuthority = false)]
    void OnServer(object behaviour, string action_str)
    {
        //print($"RPC.OnServer: {type_str}, {action_str}");

        //Type type = Type.GetType(type_str);
        Type type = GetType();
        print("RPC.OnServer: " + type);

        MethodInfo action = type?.GetMethod(action_str);
        print("RPC.OnServer: " + action);
        action?.Invoke(this, null);

    }





}
