using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MinimapCamera : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] RectTransform minimapRect;
    [SerializeField] float minimapHalfSize = 10;
    [SerializeField] float offSet = -6;

    Transform playerCameraTransform = null;

    private void Start()
    {
        //playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().cameraTransform;
        playerCameraTransform = NetworkClient.connection.identity.GetComponent<CameraController>().cameraTransform;
        if (playerCameraTransform == null)
            NetworkClient.connection.identity.transform.GetChild(0);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }

    void MoveCamera()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        bool mouseIsInMinimap = RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect,
                                                                                        mousePosition,
                                                                                        null,
                                                                                        out Vector2 localPoint);
        if (!mouseIsInMinimap)
            return;

        Vector2 lerp = new Vector2((localPoint.x - minimapRect.rect.x) / minimapRect.rect.width,
                                   (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        Vector3 newCameraPosition = new Vector3(Mathf.Lerp(-minimapHalfSize, minimapHalfSize, lerp.x),
                                                playerCameraTransform.position.y,
                                                Mathf.Lerp(-minimapHalfSize, minimapHalfSize, lerp.y));

        playerCameraTransform.position = newCameraPosition + new Vector3(0, 0, offSet);



    }

}
