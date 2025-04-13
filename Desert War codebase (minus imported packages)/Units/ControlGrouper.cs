using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ControlGrouper : MonoBehaviour
{
    [SerializeField] SelectionManager selectionManager;

    List<KeyControl> digitKeys = new List<KeyControl>();
    Dictionary<int, List<Selectable>> controlGroups = new Dictionary<int, List<Selectable>>();

    private void Start()
    {
        MakeDigitKeys();
    }

    void Update()
    {
        for (int i = 0; i < digitKeys.Count; i++)
        {
            if (digitKeys[i].wasPressedThisFrame)
            {
                if (Keyboard.current.ctrlKey.isPressed)
                    MakeControlgroup(i);
                else if (controlGroups.ContainsKey(i))
                    if (controlGroups[i].Count > 0)
                        SelectControlGroup(i);
            }
        }
    }


    void SelectControlGroup(int i)
    {
        selectionManager.ClearSelectionUnlessShiftSelecting();
        foreach (Selectable selectable in controlGroups[i])
            selectionManager.AddToSelection(selectable);
    }

    void MakeControlgroup(int i)
    {
        controlGroups[i] = new List<Selectable>(selectionManager.selectedUnits);
    }


    void MakeDigitKeys()
    {
        digitKeys.Add(Keyboard.current.digit1Key);
        digitKeys.Add(Keyboard.current.digit2Key);
        digitKeys.Add(Keyboard.current.digit3Key);
        digitKeys.Add(Keyboard.current.digit4Key);
        digitKeys.Add(Keyboard.current.digit5Key);
        digitKeys.Add(Keyboard.current.digit6Key);
        digitKeys.Add(Keyboard.current.digit7Key);
        digitKeys.Add(Keyboard.current.digit8Key);
        digitKeys.Add(Keyboard.current.digit9Key);
    }




}
