using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DestinationMarker : MonoBehaviour
{
    [SerializeField] AnimationClip markerAnimation;

    Image markerImage;
    Animator animator;
    Camera mainCamera;

    private void Start()
    {
        markerImage = GetComponent<Image>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
    }


    public void MarkPosition(Color color)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            return;
        transform.position = hit.point - ray.direction;
        markerImage.color = color;
        animator.Play(markerAnimation.name, 0, 0);
    }


}
