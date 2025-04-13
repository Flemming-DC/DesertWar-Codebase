using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Settings))]
public class EditorButton : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Settings settings = (Settings) target;

        if (GUILayout.Button("Reset"))
        {
            settings.inDebugMode = settings.defaultInDebugMode;
            settings.useScreenBorderCameraMotion = settings.defaultUseScreenBorderCameraMotion;
            settings.screenBorderThickness = settings.defaultScreenBorderThickness;
            settings.cameraSpeed = settings.defaultCameraSpeed;
            settings.soundVolume = settings.defaultSoundVolume;
        }
    }


}
