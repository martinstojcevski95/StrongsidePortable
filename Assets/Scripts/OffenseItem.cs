using StrongSideAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class OffenseItem : MonoBehaviour
{
    [SerializeField] TMP_Text formationName, formationStackName, isUpdated, createdAt;
    public Button myButton; // Reference to the Button component
    public SingleFormationResponse itemData;
    private string apiPrefix = "https://strongside-api-prod.azurewebsites.net/web-api/api/v2/formation-get?id=";
    public static event Action<SingleFormationResponse> OnItemClicked;
    public static event Action<string> OnFormationUpdated;


    private void OnEnable()
    {
        AppManager.OnFormationUpdated += OnUpdatedFormation;
    }

    private void OnUpdatedFormation(ApiResponse obj, string formationId)
    {
        if (obj.success)
            APIManager.Instance.GET(apiPrefix + formationId, OnUpdateResponseReceived);

    }

    private void OnUpdateResponseReceived(string obj)
    {
        var updatedItem = JsonUtility.FromJson<SingleFormationResponse>(obj);

        if (itemData.id == updatedItem.id)
        {
            itemData = updatedItem;

            SetItemData(itemData);
 
        }
        OnFormationUpdated?.Invoke(itemData.id);
       // LoadingManager.Instance.CloseLoadingScreen("Update Successful", 1.5f);

    }

    private void Awake()
    {
        this.GetComponent<Button>().onClick.AddListener(LoadItemData);
    }

    public void SetItemData(SingleFormationResponse data)
    {
        itemData = data;
        formationName.text = data.formationName == "" ? "N/A" : data.formationName;
        var fullName = data.baseFormationName == "" ? data.formationName : data.baseFormationName;
        isUpdated.text = data.modifiedBy == "" ? "Not Updated" : "";
        createdAt.text = data.daysCount == "today" ? data.daysCount : data.daysCount + " ago";
        formationStackName.text = " \n \n" + fullName;
    }

    public void LoadItemData()
    {
        LoadingManager.Instance.OpenLoading("Loading " +  itemData.formationName + " please wait");
        // AppManager.Instance.OpenLoading("Loading please wait...");
        APIManager.Instance.GET(apiPrefix + itemData.id, OnResponseReceived);
    }

    private void OnResponseReceived(string obj)
    {
        Debug.Log("item get formation data on click " + obj);
        OnItemClicked?.Invoke(JsonUtility.FromJson<SingleFormationResponse>(obj));
        LoadingManager.Instance.CloseLoadingScreen("");
    }
}
