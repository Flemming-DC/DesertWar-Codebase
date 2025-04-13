using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BuildRangeIndicator : MonoBehaviour
{
    [SerializeField] GameObject diskPrefab;
    [SerializeField] GameObject oilPumpBackground;
    [SerializeField] GameObject otherBuildingBackground;

    public static BuildRangeIndicator instance {get; private set;}
    RTSPlayer player;
    List<GameObject> disks = new List<GameObject>();

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogWarning($"Singleton has already been initialized");

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        Hide();
    }

    public void Show(Building building)
    {
        Hide();

        bool isOilPump = building.isOilPump;
        oilPumpBackground.SetActive(isOilPump);
        otherBuildingBackground.SetActive(!isOilPump);

        // this will also show the build indicator for enemy buildings
        List<Building> buildings = isOilPump 
            ? Building.allBuildings//.Where(b => FogOfWar.InSight(b.transform.position)).ToList()
            : player.myBuildings;

        foreach (var building_ in buildings)
        {
            if (building_.isOilPump != isOilPump)
                continue;

            GameObject diskInstance = Instantiate(
                diskPrefab, 
                building_.transform.position + Vector3.up, 
                Quaternion.Euler(-90, 0, 0));
            
            float buildRange = isOilPump ? player.oilPumpMinBuildRange : player.defaultBuildRange;
            diskInstance.transform.localScale = 2 * buildRange * new Vector3(1, 1, 1);
            disks.Add(diskInstance);
        }

    }


    public void Hide()
    {
        oilPumpBackground.SetActive(false);
        otherBuildingBackground.SetActive(false);
        foreach (var disk in disks)
            Destroy(disk);
        disks.Clear();
    }



}
