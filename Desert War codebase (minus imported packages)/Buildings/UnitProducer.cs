using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;

public class UnitProducer : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] TMP_Text remainingUnitsText;
    [SerializeField] Image unitProgressImage;
    [SerializeField] int maxQueuedUnitCount;
    [SerializeField] float spawnMoveRange;

    public int QueueUnitCount {get => queueUnitCount; }
    [SyncVar(hook = nameof(OnQueueUnitsChanged))] int queueUnitCount;
    [SyncVar] float unitTimer;
    [SyncVar] float unitBuildTime;
    private float progressVelocity;
    GameObject unitProgressCanvas;
    Selectable unitPrefab;
    RTSPlayer player;

    private void Update()
    {
        unitTimer -= Time.deltaTime;

        if (isServer)
            if (unitTimer <= 0 && queueUnitCount > 0)
                ProduceUnit();
        
        if (isClient)
            UpdateTimerDisplay();
    }

    #region server

    public override void OnStartServer()
    {
        unitPrefab = GetComponent<StatsBehaviour>().stats.producedUnit.GetComponent<Selectable>();
        unitBuildTime = unitPrefab.GetComponent<StatsBehaviour>().stats.unitBuildTime;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();
    }

    [Server]
    void ProduceUnit()
    {
        if (queueUnitCount == 0)
            return;

        GameObject unitInstance = this.Spawn(unitPrefab.gameObject, spawnPoint.position);
        StartCoroutine(MoveOnNextFrame(unitInstance));
        queueUnitCount--;
        unitTimer = unitBuildTime;
    }

    IEnumerator MoveOnNextFrame(GameObject unitInstance)
    {
        yield return new WaitForSeconds(3 * Time.deltaTime);
        if (unitInstance == null)
            yield break;

        Vector3 spawnOffSet = UnityEngine.Random.insideUnitSphere;
        spawnOffSet.y = spawnPoint.position.y;
        spawnOffSet = spawnOffSet.normalized * spawnMoveRange;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(spawnPoint.position + spawnOffSet);
    }

    [Command]
    public void CmdProduceUnit()
    {
        if (queueUnitCount >= maxQueuedUnitCount)
            return;

        if (player.resources < unitPrefab.cost)
        {
            HintManager.SetHint($"Not enough resources to build {unitPrefab.name}.", connectionToClient, true);
            return;
        }

        queueUnitCount++;
        player.resources -= unitPrefab.cost;
        if (queueUnitCount == 1)
            unitTimer = unitBuildTime;
    }
    
    #endregion




    #region client

    public override void OnStartClient()
    {
        unitProgressCanvas = unitProgressImage.transform.parent.gameObject;
        unitTimer = 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (!hasAuthority)
            return;

        CmdProduceUnit();
    }

    void OnQueueUnitsChanged(int oldQueueUnits, int newQueueUnits)
    {
        remainingUnitsText.text = newQueueUnits.ToString();
    }

    private void UpdateTimerDisplay()
    {
        if (unitProgressCanvas == null)
        {
            Debug.LogWarning($"unitProgressCanvas is null for {name}");
            return;
        }
        float newProgress = unitTimer / unitBuildTime;
        
        if (queueUnitCount <= 0 || !hasAuthority)
        {
            unitProgressCanvas.SetActive(false);
            return;
        }
        else
            unitProgressCanvas.SetActive(true);


        if (newProgress < unitProgressImage.fillAmount)
            unitProgressImage.fillAmount = newProgress;
        else
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress, ref progressVelocity, 0.1f);

    }

    #endregion



}
