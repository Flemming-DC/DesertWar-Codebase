using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class Keyboard_Extensions
{



    public static KeyControl DigitKey(this Keyboard keyboard, int digit)
    {
        switch (digit)
        {
            case 1:
                return Keyboard.current.digit1Key;
            case 2:
                return Keyboard.current.digit2Key;
            case 3:
                return Keyboard.current.digit3Key;
            case 4:
                return Keyboard.current.digit4Key;
            case 5:
                return Keyboard.current.digit5Key;
            case 6:
                return Keyboard.current.digit6Key;
            case 7:
                return Keyboard.current.digit7Key;
            case 8:
                return Keyboard.current.digit8Key;
            case 9:
                return Keyboard.current.digit9Key;
            case 0:
                return Keyboard.current.digit0Key;
            default:
                Debug.LogWarning($"digit key {digit} was not found. Returning digit key 0 as a default value");
                return Keyboard.current.digit0Key;
        }
    }


}
