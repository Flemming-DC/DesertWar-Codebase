using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] LayerMask mask;
    [SerializeField] RectTransform selectionBox;
    [SerializeField] UnitInfoDisplayer unitInfoDisplayer;
    [SerializeField] UnitGroupInfoDisplayer unitGroupInfoDisplayer;
    [SerializeField] float doubleClickDuration = 0.5f; // when I click at my max speed, then I have 0.16 seconds between each click
    [SerializeField] float minSelectionBoxSize;
    [SerializeField] float cameraDistance = 22.8f;

    public List<Selectable> selectedUnits { get; } = new List<Selectable>();
    Vector2 selectionStartPosition;
    RTSPlayer player;
    Camera mainCamera;
    Selectable lastClickedUnit;
    float lastClickTime;

    private void Start()
    {
        mainCamera = Camera.main;
        Selectable.authorityOnUnitDespawned += AuthorityOnUnitDespawned;
        GameOverHandler.clientOnGameOver += ClientOnGameOver;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    private void OnDestroy() 
    {
        Selectable.authorityOnUnitDespawned -= AuthorityOnUnitDespawned;
        GameOverHandler.clientOnGameOver -= ClientOnGameOver;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            StartSelecting();
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
            FinishSelecting();
        else if (Mouse.current.leftButton.isPressed)
            UpdateSelectionBox();
    }

    void StartSelecting()
    {
        selectionStartPosition = Mouse.current.position.ReadValue();
        UpdateSelectionBox();
    }

    void UpdateSelectionBox()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float boxX = Mathf.Abs(mousePosition.x - selectionStartPosition.x);
        float boxY = Mathf.Abs(mousePosition.y - selectionStartPosition.y);

        selectionBox.sizeDelta = new Vector2(boxX, boxY);
        Vector2 boxCenter = (mousePosition + selectionStartPosition) / 2;
        selectionBox.anchoredPosition = boxCenter;

        if (!selectionBox.gameObject.activeSelf)
        {
            Vector3 selectionBoxWorldSize = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, cameraDistance))
                                            - mainCamera.ScreenToWorldPoint(new Vector3(selectionStartPosition.x, selectionStartPosition.y, cameraDistance));
            if (selectionBoxWorldSize.sqrMagnitude > minSelectionBoxSize * minSelectionBoxSize)
                selectionBox.gameObject.SetActive(true);
        }
    }


    void FinishSelecting()
    {

        if (selectionBox.gameObject.activeSelf)
            SelectUnitsInBox();
        else if (TryGetSelectableByClick(out Selectable selectable))
        {
            bool isDoubleClicking = Time.time - lastClickTime < doubleClickDuration;
            if (isDoubleClicking && selectable == lastClickedUnit)
                SelectUnitType();
            else
                SelectSingleUnit(selectable);

            lastClickTime = Time.time;
            lastClickedUnit = selectable;
        }

        selectionBox.gameObject.SetActive(false);
    }

    void SelectSingleUnit(Selectable selectable)
    {
        ClearSelectionUnlessShiftSelecting();
        if (selectedUnits.Contains(selectable))
            return;
        AddToSelection(selectable);
    }

    void SelectUnitType()
    {
        ClearSelectionUnlessShiftSelecting();
        foreach (Selectable selectable in player.myUnits)
            if (selectable.name == lastClickedUnit.name && !selectedUnits.Contains(selectable))
                AddToSelection(selectable);
    }

    void SelectUnitsInBox()
    {
        ClearSelectionUnlessShiftSelecting();
        Vector2 boxMin = selectionBox.anchoredPosition - selectionBox.sizeDelta / 2;
        Vector2 boxMax = selectionBox.anchoredPosition + selectionBox.sizeDelta / 2;

        foreach (Selectable selectable in player.myUnits)
        {
            Vector3 halfHorizontalSize = 0.5f * selectable.GetComponent<BoxCollider>().size.Horizontal();
            Vector2 selectableMin = mainCamera.WorldToScreenPoint(selectable.transform.position - halfHorizontalSize);
            Vector2 selectableMax = mainCamera.WorldToScreenPoint(selectable.transform.position + halfHorizontalSize);
            bool isWithinSelectionBox = selectableMax.x > boxMin.x && selectableMin.x < boxMax.x &&
                                        selectableMax.y > boxMin.y && selectableMin.y < boxMax.y;
            if (!isWithinSelectionBox)
                continue;
            if (selectedUnits.Contains(selectable))
                continue;
            AddToSelection(selectable);
        }

    }

    public void AddToSelection(Selectable selectable)
    {
        selectedUnits.Add(selectable);
        selectable.Select();
        UpdateSelectionDisplay();
    }

    void UpdateSelectionDisplay()
    {
        if (selectedUnits.Count > 1)
        {
            unitInfoDisplayer.gameObject.SetActive(false);
            unitGroupInfoDisplayer.Display(selectedUnits);
        }
        else if (selectedUnits.Count == 1)
        {
            unitGroupInfoDisplayer.gameObject.SetActive(false);
            unitInfoDisplayer.Display(selectedUnits[0].GetComponent<StatsBehaviour>().stats);
        }
        else
        {
            unitInfoDisplayer.gameObject.SetActive(false);
            unitGroupInfoDisplayer.gameObject.SetActive(false);
        }
    }

    public void ClearSelectionUnlessShiftSelecting()
    {
        bool isShiftSelecting = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        if (!isShiftSelecting)
        {
            foreach (Selectable selectedUnit in selectedUnits)
                selectedUnit.Deselect();
            selectedUnits.Clear();
            UpdateSelectionDisplay();
        }
    }

    bool TryGetSelectableByClick(out Selectable selectable)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        selectable = null;

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask))
            return false;
        else if (!hit.collider.TryGetComponent(out selectable))
            return false;
        else if (!selectable.hasAuthority)
            return false;
        else
            return true;
    }

    void AuthorityOnUnitDespawned(Selectable selectable)
    {
        selectedUnits.Remove(selectable);
        if (selectable != null && NetworkClient.active)
            selectable.Deselect();
    }


    private void ClientOnGameOver(string playerName, bool hasWon)
    {
        enabled = false;
    }



}
