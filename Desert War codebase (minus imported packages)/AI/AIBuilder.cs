using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class AIBuilder : MonoBehaviour
{
    //set tooltip and check sizes
    [SerializeField] float buildInterval = 4;
    [SerializeField] int maxSearchAttempt = 50;
    [SerializeField] int maxBuildingCount = 20;
    [SerializeField] int minOilPumpTarget = 2;
    [SerializeField] Building oilPump;
    [SerializeField] List<Building> buildings;
    [SerializeField] List<float> buildingChances;
    [SerializeField] List<float> conditionalUnitChances;

    RTSPlayer localPlayer;
    float[] cumulativeChances;
    Dictionary<Building, float> conditionalUnitChancesDict = new Dictionary<Building, float>();

    private void Start()
    {
        localPlayer = NetworkClient.localPlayer.GetComponent<RTSPlayer>();
        SetCumulativeChances();
        SetUnitGivenBuildingChancesDict();
        CheckSizes();
        Random.InitState(System.DateTime.Now.Millisecond);
        InvokeRepeating(nameof(BuildRandom), 0, buildInterval);
    }

    void SetCumulativeChances()
    {
        cumulativeChances = new float[buildingChances.Count];
        float NormalizationConstant = 1 / buildingChances.Sum();
        for(int i=0; i< buildingChances.Count; i++)
        {
            buildingChances[i] *= NormalizationConstant;
            if (i > 0)
                cumulativeChances[i] = cumulativeChances[i - 1] + buildingChances[i];
            else
                cumulativeChances[0] = buildingChances[0];
        }
        float one = cumulativeChances[cumulativeChances.Length - 1];
        one = Mathf.Round(one * 100f) / 100f;
        if (one != 1)
            Debug.Log($"the last cumulative chance is {cumulativeChances[cumulativeChances.Length - 1]}. The answer should be 1.");
    }

    void SetUnitGivenBuildingChancesDict()
    {
        for(int i=0; i< buildings.Count; i++)
            conditionalUnitChancesDict.Add(buildings[i], conditionalUnitChances[i]);
    }

    void CheckSizes()
    {
        if (buildingChances.Count != buildings.Count)
            Debug.LogWarning($"buildingChances.Count != buildings.Count, but this seems wrong.");
        if (conditionalUnitChances.Count != buildings.Count)
            Debug.LogWarning($"conditionalUnitChances.Count != buildings.Count, but this seems wrong.");
        if (conditionalUnitChancesDict.Count != buildings.Count)
            Debug.LogWarning($"conditionalUnitChancesDict.Count != buildings.Count, but this seems wrong.");
    }


    void BuildRandom()
    {
        if (!AIActivator.aiActivated)
            return;
        //if (localPlayer.resources < 250)
        //    return;

        //Building building = Building.allBuildings.Count() < 3 ? oilPump : GetRandomBuilding();
        Building building = localPlayer.myBuildings.Count() < (minOilPumpTarget + 1) ? oilPump : GetRandomBuilding();
        Build(building);
    }

    void Build(Building building)
    {
        bool hasBuilding = localPlayer.myBuildingIDs.Contains(building.id);
        bool makeUnit = hasBuilding && RandomBool(conditionalUnitChancesDict[building]);

        if (makeUnit)
        {
            Building BuildingInstance = localPlayer.GetBuildingFromID(building.id, localPlayer.myBuildings.ToArray());
            BuildingInstance.GetComponent<UnitProducer>().CmdProduceUnit();
        }
        else
        {
            if (localPlayer.resources < building.cost)
                return;
            if (!building.HasRequiredBuildings(localPlayer))
                return;
            if (localPlayer.myBuildings.Count >= maxBuildingCount)
                if (hasBuilding)
                    return;
            if (!TryFindBuildingPosition(building, out Vector3 buildPosition))
                return;

            localPlayer.CmdTryPlaceBuilding(building.id, true, buildPosition);
        }
    }

    bool TryFindBuildingPosition(Building newBuilding, out Vector3 buildPosition)
    {
        if (localPlayer.myBuildings.Count == 0)
        {
            buildPosition = Vector3.zero;
            return false;
        }
        float unscaledBuildDistance = newBuilding.TryGetComponent(out ResourceGenerator dummy1) ? 1.5f : 0.67f;
        float buildDistance = localPlayer.defaultBuildRange * unscaledBuildDistance;

        for (int i=0; i<maxSearchAttempt; i++)
        {
            Building randomBuilding = localPlayer.myBuildings[Random.Range(0, localPlayer.myBuildings.Count - 1)];
            Vector3 position = randomBuilding.transform.position + GetRandomOffSet(buildDistance);

            if (localPlayer.CanPlaceBuilding(newBuilding, position, true, out string dummy2))
            {
                buildPosition = position;
                return true;
            }
        }
        Debug.LogWarning($"maxSearchAttempt reached!");
        buildPosition = Vector3.zero;
        return false;
    }

    Vector3 GetRandomOffSet(float buildDistance)
    {
        float angle = Random.Range(0f, 2 * Mathf.PI);
        return buildDistance * new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
    }

    Building GetRandomBuilding()
    {
        float rand = Random.Range(0f, 1f);
        for (int i=0; i<cumulativeChances.Length; i++)
        {
            if (rand < cumulativeChances[i])
                return buildings[i];
        }
        Debug.LogWarning($"Couldn't find random Building");
        return null;
    }

    bool RandomBool(float probability = 0.5f)
    {
        return (Random.value < probability);
    }

}
