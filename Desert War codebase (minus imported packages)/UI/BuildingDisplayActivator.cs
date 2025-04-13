using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingDisplayActivator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] BuildingInfoDisplayer buildingInfoDisplayer;

    Stats stats;

    void Start()
    {
        stats = GetComponent<BuildingButton>().building.GetComponent<StatsBehaviour>().stats;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buildingInfoDisplayer.gameObject.SetActive(true);
        buildingInfoDisplayer.Display(stats);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buildingInfoDisplayer.gameObject.SetActive(false);
    }
}
