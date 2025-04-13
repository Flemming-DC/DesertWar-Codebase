using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Informer : MonoBehaviour
{
    static TMP_Text text;
    static float? remainingTime = null;

    void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        if (text == null)
            Debug.LogWarning("failed to find TMP_Text among children.");
    }


    private void Update()
    {
        if (remainingTime == null)
            return;
        else if (remainingTime > 0)
            remainingTime -= Time.deltaTime;
        else
        {
            remainingTime = null;
            text.text = "";
        }

    }


    public static void Show(string message, float? duration = null)
    {
        text.text = message;
        remainingTime = duration;
    }


}
