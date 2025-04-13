using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Encrypter : MonoBehaviour
{
    static Dictionary<char, char> encryption = new Dictionary<char, char>();
    static Dictionary<char, char> decryption = new Dictionary<char, char>();



    public static string Encrypt(string networkAddress)
    {
        if (encryption.Count == 0)
            Initialize();

        if (networkAddress == "localhost")
            return "dummyPassword";

        string encryptedAdress = "";
        foreach (char char_ in networkAddress)
            encryptedAdress += encryption[char_];

        return encryptedAdress;
    }


    public static bool TryDecrypt(string encryptedAddress, out string networkAddress)
    {
        if (encryption.Count == 0)
            Initialize();

        if (encryptedAddress == "dummyPassword")
        {
            networkAddress = "localhost";
            return true;
        }
        try
        {
            networkAddress = "";
            foreach (char char_ in encryptedAddress)
                networkAddress += decryption[char_];
            return IsValid(networkAddress);
        }
        catch
        {
            networkAddress = "";
            return false;
        }
    }


    public static bool IsValid(string networkAddress)
    {
        if (networkAddress.Length < "1.1.1.1".Length)
            return false;
        return IPAddress.TryParse(networkAddress, out IPAddress _);
    }


    static void Initialize()
    {
        encryption.Add('.', 't');
        encryption.Add('0', 'f');
        encryption.Add('1', 'a');
        encryption.Add('2', 'y');
        encryption.Add('3', 'u');
        encryption.Add('4', 'j');
        encryption.Add('5', 'q');
        encryption.Add('6', 'h');
        encryption.Add('7', '7');
        encryption.Add('8', '2');
        encryption.Add('9', 'k');

        foreach (var pair in encryption)
            decryption.Add(pair.Value, pair.Key);
    }

}
