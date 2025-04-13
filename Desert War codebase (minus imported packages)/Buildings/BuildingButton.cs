using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.InputSystem;

public class BuildingButton : NetworkBehaviour, IPointerDownHandler
{
    [field: SerializeField] public Building building { get; private set; }
    [SerializeField] Image icon;
    [SerializeField] TMP_Text priceText;
    [SerializeField] LayerMask floorMask;

    Camera mainCamera;
    RTSPlayer player;
    BoxCollider buildingCollider;
    Color unModifiedBuildingColor;
    Renderer buildingRendererInstance;
    GameObject buildingPreviewInstance;
    bool previewWasMadeThisFrame;
    

    private void Start()
    {
        mainCamera = Camera.main;
        icon.sprite = building.GetComponent<StatsBehaviour>().stats.icon;
        priceText.text = building.cost.ToString();
        buildingCollider = building.GetComponent<BoxCollider>();
        if (buildingCollider == null)
            buildingCollider = building.GetComponentInChildren<BoxCollider>();
        unModifiedBuildingColor = building.GetComponentInChildren<Renderer>().sharedMaterial.color.Copy();
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    private void Update()
    {
        icon.color = building.HasRequiredBuildings(player) ? new Color(1, 1, 1) : new Color(0, 0, 0);

        if (buildingPreviewInstance == null)
            return;
        if (previewWasMadeThisFrame)
        {
            previewWasMadeThisFrame = false;
            return;
        }

        UpdateBuildingPreview();
        if (Mouse.current.leftButton.wasPressedThisFrame)
            MakeBuilding();
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            BuildRangeIndicator.instance.Hide();
            Destroy(buildingPreviewInstance);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (player.resources < building.cost)
        {
            HintManager.SetHint($"Not enough resources to contruct {building.name}.", null, true);
            return;
        }
        if (!building.HasRequiredBuildings(player))
        {
            HintManager.SetHint($"Can't contruct {building.name} due to a missing requirement", null, true);
            return;
        }

        BuildRangeIndicator.instance.Show(building);
        Destroy(buildingPreviewInstance);
        buildingPreviewInstance = Instantiate(building.buildingPreview,
                                              Mouse.current.position.ReadValue(),
                                              Quaternion.identity);
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
        buildingPreviewInstance.SetActive(false);
        previewWasMadeThisFrame = true;
    }

    void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            return;
        Vector3 previewPosition = hit.point + new Vector3(0, building.heightOfReferencePosition, 0);
        buildingPreviewInstance.transform.position = previewPosition;
        if (!buildingPreviewInstance.activeSelf)
            buildingPreviewInstance.SetActive(true);

        SetPreviewColor(previewPosition);
    }


    private void MakeBuilding()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            BuildRangeIndicator.instance.Hide();
            Destroy(buildingPreviewInstance);
            bool inSight = FogOfWar.InSight(hit.point);
            player.CmdTryPlaceBuilding(building.id, inSight, hit.point);
        }
    }




    void SetPreviewColor(Vector3 previewPosition)
    {
        Color previewColor = unModifiedBuildingColor;
        bool inSight = FogOfWar.InSight(previewPosition);
        if (player.CanPlaceBuilding(building, previewPosition, inSight, out string dummy))
            previewColor *= SharedData.instance.previewColor;
        else
            previewColor *= SharedData.instance.invalidBuildingLocationColor;

        buildingRendererInstance.material.SetColor("_Color", previewColor);
        buildingRendererInstance.material.SetColor("_BaseColor", previewColor);
    }
    

}
