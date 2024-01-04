#if PHOTON_UNITY_NETWORKING
using System;
using System.Collections;
using MAGES.GameController;
using MAGES.Networking;
using MAGES.XRPlotLib;
using Photon.Pun;
using UnityEngine;
public class SyncPlot : MonoBehaviour
{
    private PhotonView plotpView;
    private PhotonView plotDataView;
    private GameObject plotData;

    void Start()
    {
        plotpView = GetComponent<PhotonView>();
        if (!plotData)
            plotData = transform.Find("PlotRoot/PlotData").gameObject;

        plotDataView = plotData.GetComponent<PhotonView>();
        
        if (MAGESControllerClass.Get.IsInNetwork)
        {
            StartCoroutine(SyncScale());
        }

    }
    
    public void PlotDataTakeover()
    {
        if (!MAGESControllerClass.Get.IsInNetwork) return;

        plotDataView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    private void EnterElementNewMode(){
        GetComponent<PrefabPlot>().EnterNewElementMode();
    }

    public void SyncEnterNewMode()
    {
        plotpView.RPC("EnterElementNewMode",RpcTarget.Others);
    }
    
    [PunRPC]
    private void SyncScaleRPC(float scale)
    {
        plotData.transform.localScale = new Vector3(scale, scale, scale);
    }

    IEnumerator SyncScale()
    {
        yield return new WaitForSeconds(4f);
        while (true)
        {
            plotpView.RPC("SyncScaleRPC",RpcTarget.Others,plotData.transform.localScale.x);
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void SyncUpdateData()
    {
        if (MAGESControllerClass.Get.IsInNetwork)
        {
            plotpView.RPC("RemoteUpdateData", RpcTarget.Others);
        }
    }

    [PunRPC]
    private void RemoteUpdateData()
    {
        this.GetComponent<PrefabPlot>().UpdateData();
    }
}
#endif