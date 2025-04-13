using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UnitCommander : MonoBehaviour
{
    [SerializeField] SelectionManager unitSelectionHandler;
    [SerializeField] LayerMask mask;
    [SerializeField] DestinationMarker destinationMarker;

    Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        GameOverHandler.clientOnGameOver += ClientOnGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.clientOnGameOver -= ClientOnGameOver;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            CancelCommand();

        if (!Mouse.current.rightButton.wasPressedThisFrame)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask))
            return;

        if (HitValidTarget(hit, out Targetable targetable))
            TryTarget(targetable);
        else
            TryMove(hit.point, Keyboard.current.qKey.isPressed);
    }


    public void TryMove(Vector3 destination, bool isAttackMoving = false)
    {
        Color markerColor = isAttackMoving ? Color.red : Color.green;
        if (unitSelectionHandler.selectedUnits.Count > 0)
            destinationMarker.MarkPosition(markerColor);

        foreach (Selectable selectable in unitSelectionHandler.selectedUnits)
            selectable.unitMovement.CmdMove(destination, isAttackMoving);
    }

    public void TryTarget(Targetable targetable)
    {
        if (unitSelectionHandler.selectedUnits.Count > 0)
            destinationMarker.MarkPosition(Color.red);
        foreach (Selectable selectable in unitSelectionHandler.selectedUnits)
            selectable.targeter.CmdSetTarget(targetable);
    }

    void CancelCommand()
    {
        foreach (Selectable selectable in unitSelectionHandler.selectedUnits)
            selectable.unitMovement.CmdCancel();
    }

    void ClientOnGameOver(string playerName, bool hasWon)
    {
        enabled = false;
    }

    bool HitValidTarget(RaycastHit hit, out Targetable targetable)
    {
        if (!hit.collider.TryGetComponent(out targetable))
            return false;
        if (targetable.hasAuthority && !targetable.canBeTargetedByAllies)
            return false;
        if (!FogOfWar.InSight(targetable.transform.position))
            return false;

        return true;
    }

}
