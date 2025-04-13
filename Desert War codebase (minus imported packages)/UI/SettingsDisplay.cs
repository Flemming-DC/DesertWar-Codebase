using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsDisplay : MonoBehaviour
{
    [SerializeField] Settings settings;

    [SerializeField] Toggle inDebugMode;
    [SerializeField] Toggle useScreenBorderCameraMotion;
    [SerializeField] Toggle useFullScreen;
    [SerializeField] Slider screenBorderThickness;
    [SerializeField] Slider cameraSpeed;
    [SerializeField] Slider soundVolume;

    private void Start()
    {
        inDebugMode.isOn = settings.inDebugMode;
        useScreenBorderCameraMotion.isOn = settings.useScreenBorderCameraMotion;
        useFullScreen.isOn = settings.useFullScreen;
        screenBorderThickness.value = settings.screenBorderThickness;
        cameraSpeed.value = settings.cameraSpeed;
        soundVolume.value = settings.soundVolume;

        if (NetData.staticUseFakeServer)
            soundVolume.value = 0;

        OnToggleInDebugMode();
        OnToggleUseScreenBorderCameraMotion();
        OnToggleUseFullScreen();
        SetScreenBorderThickness();
        SetCameraSpeed();
        SetSoundVolume();
        
    }


    public void OnToggleInDebugMode()
    {
        settings.inDebugMode = inDebugMode.isOn;
    }

    public void OnToggleUseScreenBorderCameraMotion()
    {
        settings.useScreenBorderCameraMotion = useScreenBorderCameraMotion.isOn;
    }

    public void OnToggleUseFullScreen()
    {
        settings.useFullScreen = useFullScreen.isOn;
        
        if (useFullScreen.isOn) 
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        else
            Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    public void SetScreenBorderThickness()
    {
        settings.screenBorderThickness = screenBorderThickness.value;
    }

    public void SetCameraSpeed()
    {
        settings.cameraSpeed = cameraSpeed.value;
    }

    public void SetSoundVolume()
    {
        settings.soundVolume = soundVolume.value;
    }


}
