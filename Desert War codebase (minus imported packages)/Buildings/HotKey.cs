using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class HotKey : MonoBehaviour
{
    [SerializeField] string unitHotkey;

    RTSPlayer player;
    int buildingID;
    bool canProduceUnits;

    private void Start()
    {
        player = NetworkClient.localPlayer.gameObject.GetComponent<RTSPlayer>();
        buildingID = GetComponent<BuildingButton>().building.id;
        canProduceUnits = GetComponent<BuildingButton>().building.TryGetComponent(out UnitProducer _);
    }


    private void Update()
    {
        if (!canProduceUnits)
            return;
        if (unitHotkey == "")
            return;
        if (!Input.GetKeyDown(unitHotkey))
            return;

        GetProducerWithMinQueue()?.CmdProduceUnit();
    }

    UnitProducer GetProducerWithMinQueue()
    {
        UnitProducer[] unitProducers = player.myBuildings
            .Where(b => b.id == buildingID)
            .Select(b => b.GetComponent<UnitProducer>())
            .ToArray();

        if (unitProducers.Count() == 0)
            return null;

        int minQueue = unitProducers.Select(b => b.QueueUnitCount).Min();
        return unitProducers.First(b => b.QueueUnitCount == minQueue);
    }


}
