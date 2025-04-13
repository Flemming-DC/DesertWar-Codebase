using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FogOfWar : NetworkBehaviour
{
    [SerializeField] float defaultSightRange = 16;
    [SerializeField] LayerMask mask;
    [SerializeField] float discoveredAlpha;
    [SerializeField] Vector3 displacementFromCameraToGround = new Vector3(0, -18f, 18f);
    
    Mesh fogMesh;
    Vector3[] localVertices;
    Vector3[] globalVertices;
    Color[] colors;
    static RTSPlayer player;
    static float defaultSightRange_;
    static bool showAll;
    bool[] isDiscovered;
    
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = true;
        player = NetworkClient.localPlayer.gameObject.GetComponent<RTSPlayer>();
        fogMesh = GetComponent<MeshFilter>().mesh;
        localVertices = fogMesh.vertices;
        isDiscovered = new bool[localVertices.Length];
        colors = new Color[localVertices.Length];
        globalVertices = new Vector3[localVertices.Length];
        for (int i=0; i<localVertices.Length; i++)
        {
            colors[i] = Color.black;
            globalVertices[i] = transform.TransformPoint(localVertices[i]);
        }
        fogMesh.colors = colors;
        defaultSightRange_ = defaultSightRange;
        showAll = false;

        GameOverHandler.clientOnGameOver += ClientOnGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.clientOnGameOver -= ClientOnGameOver;
    }


    void Update()
    {
        if (Time.timeSinceLevelLoad <= 5 * Time.deltaTime)
            return;
        if (player == null)
            return;
        if (showAll)
            gameObject.SetActive(false);

        ResetFog();
        
        foreach (Transform transform_ in player.GetAlliedOwnables())
            UpdateFog(transform_);
        //foreach (Transform transform_ in player.owned)
        //    UpdateFog(transform_);

        fogMesh.colors = colors;
    }

    void ResetFog()
    {
        for (int i = 0; i < colors.Length; i++)
            if (isDiscovered[i])
                colors[i].a = discoveredAlpha;
            else
                colors[i].a = 1;
    }

    void UpdateFog(Transform transform_)
    {
        float sightRangeSqr = GetSightRangeSqr(transform_);
        Ray ray = new Ray(transform_.position - displacementFromCameraToGround, displacementFromCameraToGround);
        
        if (Physics.Raycast(ray, out RaycastHit hit, mask))
        {
            for (int i=0; i<globalVertices.Length; i++)
            {
                float distanceSqr = Vector3.SqrMagnitude(globalVertices[i] - hit.point);
                if (distanceSqr < sightRangeSqr)
                {
                    colors[i].a = 0;
                    isDiscovered[i] = true;
                }
            }
        }
    }
    


    public static bool InSight(Vector3 position, RTSPlayer _player = null)
    {
        if (_player == null)
            _player = NetworkClient.localPlayer.gameObject.GetComponent<RTSPlayer>();
        if (_player == null)
            return false;
        if (showAll)
            return true;

        //foreach (Transform transform_ in _player.owned)
        foreach (Transform transform_ in _player.GetAlliedOwnables())
          {
            if (transform_ == null)
                continue;
            else if (Vector3.SqrMagnitude(transform_.position - position) < GetSightRangeSqr(transform_))
                return true;
        }

        return false;
    }


    static float GetSightRangeSqr(Transform transform_)
    {
        if (transform_.TryGetComponent(out StatsBehaviour statsBehaviour))
            return Mathf.Pow(statsBehaviour.stats.sightRange, 2);
        else
        {
            Debug.LogWarning($"Failed to find stats on {transform_.name}. Using sightRangee = {defaultSightRange_} instead as default.");
            return defaultSightRange_;
        }
    }

    void ClientOnGameOver(string playerName, bool hasWon)
    {
        showAll = true;
    }

}
