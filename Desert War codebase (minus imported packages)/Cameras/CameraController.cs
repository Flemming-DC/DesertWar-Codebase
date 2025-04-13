using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] Settings settings;
    [field: SerializeField] public Transform cameraTransform { get; private set; }

    [SerializeField] float minWorldX = -45;
    [SerializeField] float maxWorldX = 45;
    [SerializeField] float minWorldZ = -50;
    [SerializeField] float maxWorldZ = 40;

    Controls controls;
    Vector2 velocity;
    bool useScreenBorderCameraMotion;
    float screenBorderThickness;
    float cameraSpeed;


    #region client
    public override void OnStartClient()
    {
        if (!hasAuthority)
            return;

        cameraTransform.gameObject.SetActive(true);
        controls = new Controls();
        controls.Enable();
        Base.authorityOnBaseSpawned += SendCameraToBase;
    }


    public override void OnStopClient()
    {
        Base.authorityOnBaseSpawned -= SendCameraToBase;
        if (controls != null)
            controls.Disable();
    }

    [ClientCallback]
    void Update()
    {
        if (NetData.isRemoteServer || NetData.isAI)
            return;
        if (!hasAuthority || !Application.isFocused)
            return;

        ReadSettings();
        SetVelocity(controls.Player.MoveCamera.ReadValue<Vector2>());
        bool noCameraMotionYet = velocity.sqrMagnitude < 0.2f * cameraSpeed * cameraSpeed;
        if (noCameraMotionYet && useScreenBorderCameraMotion)
            SetVelocity(GetMoveInputGivenClosenessToScreen());

        MoveCamera();
    }

    private void ReadSettings()
    {
        useScreenBorderCameraMotion = settings.useScreenBorderCameraMotion;
        screenBorderThickness = settings.screenBorderThickness;
        cameraSpeed = settings.cameraSpeed;
    }

    Vector2 GetMoveInputGivenClosenessToScreen()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 moveInput = Vector2.zero;

        if (mousePosition.x > Screen.width - screenBorderThickness)
            moveInput.x = 1;
        if (mousePosition.x < screenBorderThickness)
            moveInput.x = -1;
        if (mousePosition.y > Screen.height - screenBorderThickness)
            moveInput.y = 1;
        if (mousePosition.y < screenBorderThickness)
            moveInput.y = -1;

        return moveInput;
    }

    private void SetVelocity(Vector2 moveInput)
    {
        if (!hasAuthority)
            return;

        velocity = moveInput.normalized * cameraSpeed;
    }

    void MoveCamera()
    {
        cameraTransform.position += new Vector3(velocity.x * Time.deltaTime,
                                                0,
                                                velocity.y * Time.deltaTime);

        cameraTransform.position = new Vector3(Mathf.Clamp(cameraTransform.position.x, minWorldX, maxWorldX),
                                               cameraTransform.position.y,
                                               Mathf.Clamp(cameraTransform.position.z, minWorldZ, maxWorldZ));

    }

    public void SendCameraToBase(Base base_)
    {
        if (cameraTransform == null)
            cameraTransform = transform.GetChild(0);

        cameraTransform.position = new Vector3(base_.transform.position.x,
                                               Camera.main.transform.position.y,
                                               base_.transform.position.z - Camera.main.transform.position.y);
        Base.authorityOnBaseSpawned -= SendCameraToBase;
    }

    #endregion


    #region both
    public bool PositionIsInMap(Vector3 position)
    {
        if (position.x < minWorldX)
            return false;
        if (position.x > maxWorldX)
            return false;
        if (position.z < minWorldZ)
            return false;
        if (position.z > maxWorldZ)
            return false;

        return true;
    }
    #endregion

}
