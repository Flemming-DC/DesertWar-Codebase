using System;
using System.Net;
using UnityEngine;


public class NetData : MonoBehaviour
{
    /*
    video tutorial for LAN: https://www.youtube.com/watch?v=kcAv4iuQ0_s&ab_channel=Epitome
    video tutorial for remote server: https://www.youtube.com/watch?v=wm18gcIoUwc&ab_channel=Epitome
    server public ip: encrypter.Encrypt(188.166.93.81) = a22tahhtkut2a
    log into server: ssh root@188.166.93.81
    server password: Esmolf1431Esmolf
    copy to server: scp -r "Desert War - server - version XX"/ root@188.166.93.81:/home
    run game on server:  ./"Desert War.x86_64" -server -batchmode -nographics & disown
    get PID of game: pidof "Desert War.x86_64"
    stop game on server: sudo kill -9 <PID of game>
    */

    [SerializeField] bool useFakeServer;
    [SerializeField] bool isFakeServer;
    [SerializeField] bool isFakeAI;
    [SerializeField] string remoteServerAddress = "188.166.93.81";
    [SerializeField] string flemmingsAddress = "192.168.8.100";
    [SerializeField] GameObject AIScreen;
    [SerializeField] float mapLoadingTime_ = 10;

    public static bool staticUseFakeServer;
    public static string encryptedServerAddress;
    public static bool isRemoteServer;
    public static bool isAI;
    public static bool isSinglePlayer;
    public static bool isOnFlemmingsPC;
    public static float mapLoadingTime;

    private void Awake()
    {
        string[] args = Environment.GetCommandLineArgs();
        foreach (var arg in args)
        {
            if (arg == "-server")
                isRemoteServer = true;
            if (arg == "-AI")
            {
                isAI = true;
            }
            if (Int32.TryParse(arg, out int difficulty))
                AIActivator.difficulty = difficulty;


        }
        if (isFakeServer)// && Application.isEditor)
            isRemoteServer = true;
        if (isFakeAI)// && Application.isEditor)
            isAI = true;



        if (!Encrypter.IsValid(remoteServerAddress))
            Debug.LogError($"remoteServerAddress = {remoteServerAddress}, but this isn't a valid ip address");
        encryptedServerAddress = Encrypter.Encrypt(useFakeServer ? "localhost" : remoteServerAddress);
        
        if (isAI)
        {
            AIActivator.aiActivated = true;
            AIScreen.SetActive(true);
        }

        DontDestroyOnLoad(gameObject);
        mapLoadingTime = mapLoadingTime_;


        IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        string address = addressList.Length > 1 ? addressList[1].ToString() : addressList[0].ToString();
        isOnFlemmingsPC = (address == flemmingsAddress);

        staticUseFakeServer = useFakeServer;
    }


}
