using System;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputAction_Extension
{
    public static bool IsPressed(this InputAction inputAction)
    {
        return Math.Abs(inputAction.ReadValue<float>()) > 0f;
    }

    public static bool WasPressedThisFrame(this InputAction inputAction)
    {
        return inputAction.triggered && Math.Abs(inputAction.ReadValue<float>()) > 0f;
    }
    /*
    public static bool WasReleasedThisFrame(this InputAction inputAction)
    {
        return inputAction.triggered && Math.Abs(inputAction.ReadValue<float>()) == 0f;
    }
    */
    public static string FullBindingName(this InputAction action, int controlIndex)
    {
        if (action.controls.Count == 0)
            return "";
        int bindingIndex = action.GetBindingIndexForControl(action.controls[controlIndex]);
        return InputControlPath.ToHumanReadableString(action.bindings[bindingIndex].effectivePath);
    }

    public static (string, string) BindingNameAndDevice(this InputAction action, int controlIndex)
    {
        if (action.controls.Count == 0)
            return ("", "");
        string fullBindingName = action.FullBindingName(controlIndex);
        string[] splittedName = fullBindingName.Split('[');
        string bindingName = splittedName[0].TrimStart(' ').TrimEnd(' ');
        string device = splittedName[1].TrimStart('[').TrimEnd(']').TrimStart(' ').TrimEnd(' ');

        return (bindingName, device);
    }




}
