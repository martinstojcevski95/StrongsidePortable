using Newtonsoft.Json.Linq;
using StrongSideAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;


public class AppManager : MonoBehaviour
{
    public static AppManager Instance;
    public GameObject Stadium, MainMenu, LoginMenu;
    [SerializeField] List<GameObject> playerObjects = new List<GameObject>(11);
    private SingleFormationResponse currentItem;
    private bool isLandscape = false;

    [SerializeField] Canvas LoadingScreen;
    [SerializeField] TMP_Text loadingText, versionNumber;

    [SerializeField] Switch accountSwitch, positionSwitch, stanceSwitch;
    [SerializeField] TMP_Text currentAccountText, currentFormation, currentStance;

    [SerializeField] Button LogIn, loadMore, changePlayerStance, applyStance, updateFormation;

    public static Action<ApiResponse, string> OnFormationUpdated;

    private string demoemail = "Demo@strongside.app", martinemail = "martin.stojcevski@strongside.app";
    private string demopass = "Apshgc@1", martinpass = "Abcdef@1";

    public static event Action<string, string> OnLoginEvent;



    public static event Action<string> OnCloseLoading;
    public static event Action<bool> OnPlayerPosAliasStatus, OnPlayerStanceStatus;
    public static event Action<string, PlayerItem> OnChosenPlayerStance;
    public static Action OnHideLogin;

    [SerializeField] Switch updatedFormations;
    public FormationsResponse formationResult;
    public FormationsResponse unUpdatedResult;

    [SerializeField] Transform offenseListParent, unUpdatedListParent;
    [SerializeField] GameObject offenseItemPrefab;
    [SerializeField] Scrollbar scrollBar;


    public BaseFormationResponseWithStances baseFormationWithStances;

    [SerializeField] TMP_Text team, role, loadedFormationsCount, loggerText, updatedFormationTxt, saveScreenshotsText;

    private bool isDemoAccount = true;
    private string token;

    private int maxResultLimit = 250;
    private int currentResultLimit = 25;
    private int limit = 25;
    int index = 0;
    public PlayerItem currentPlayer;
    [SerializeField] Canvas SaveScreenshotsPopUp;
    [SerializeField] Button yes, no;
    

    public string Token
    {
        get { return token; }

        set
        {
            if (value == null) return;
            token = value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        versionNumber.text = "ver 0.0.8";
        currentAccountText.text = "Using Demo Account";
        LogIn.onClick.AddListener(OnLogin);
        loadMore.onClick.AddListener(LoadMore);
        accountSwitch.OnValueChanged.AddListener(OnValueChanged);
        updatedFormations.IsOn = true;
        positionSwitch.OnValueChanged.AddListener(PlayerAliasActivation);
        stanceSwitch.OnValueChanged.AddListener(PlayerStanceActivation);
        updatedFormations.OnValueChanged.AddListener(FilterOffenseItems);
        changePlayerStance.onClick.AddListener(CyclePlayerStances);
        applyStance.onClick.AddListener(SetPlayerStance);

        updateFormation.onClick.AddListener(UpdateFormation);

        yes.onClick.AddListener(SaveScreenshots);
        no.onClick.AddListener(DontSaveScreenshots);
    }

    private void DontSaveScreenshots()
    {
        ShowPopupForSaveScreenShots(false);
    }

    private void SaveScreenshots()
    {
        ShowPopupForSaveScreenShots(false);
        ScreenCaptureManager.instance.StartCapture(currentItem.id);
    }

    private void CyclePlayerStances()
    {
        if (currentPlayer == null)
        {
            LoadingManager.Instance.CloseLoadingScreen("Please select player first!", 1.5f);
            return;
        }
        index = (index + 1) % currentPlayer.playerStances.Count;
        OnClickedPlayer(currentPlayer.playerStances[index], currentPlayer.offensePersonnel.positionAlias);
    }

    private void SetPlayerStance()
    {
        if (currentPlayer == null)
        {
            LoadingManager.Instance.CloseLoadingScreen("Please select player first!", 1.5f);
            return;
        }
        OnChosenPlayerStance?.Invoke(currentPlayer.playerStances[index], currentPlayer);
        currentStance.text = currentPlayer.offensePersonnel.positionAlias + " - " + currentPlayer.playerStances[index];
    }

    private void OnLogin()
    {
        LoadingManager.Instance.OpenLoading("Login in please wait");
        var user = new UserRequest(!accountSwitch.IsOn ? demoemail : martinemail, !accountSwitch.IsOn ? demopass : martinpass);
        APIManager.Instance.POST(APIManager.log_in, APIManager.Instance.ConvertJsonToString(user), OnLogInResponse);
    }

    private void OnLogInResponse(string response)
    {
        var userResponse = JsonUtility.FromJson<UserResponse>(response);
        Token = userResponse.token;
        team.text = "Team - " + userResponse.accountName;
        role.text = "Role - " + userResponse.role;
        LoadingManager.Instance.OpenLoading("Loading formations please wait");
        APIManager.Instance.GET(APIManager.default_base_formation, OnFormationWithStancesResponseReceived);
    }

    public void LoadMore()
    {
        if (currentResultLimit < maxResultLimit)
        {
            currentResultLimit += limit;
        }
        GetFormationResults(currentResultLimit);


    }


    private void OnFormationWithStancesResponseReceived(string response)
    {
        baseFormationWithStances = JsonUtility.FromJson<BaseFormationResponseWithStances>($@"{{""Items"": {JArray.Parse(response)}}}");
        Debug.Log(JsonUtility.ToJson(baseFormationWithStances));
        foreach (var formationItem in baseFormationWithStances.Items)
        {
            foreach (var playerPosition in formationItem.playerPositions)
            {
                Debug.Log(playerPosition.playerIndex);
                var player = playerObjects[playerPosition.playerIndex].GetComponent<PlayerItem>();
                player.SetPlayerStances(playerPosition.otherStances, playerPosition.defaultStance, playerPosition.playerIndex);
            }
        }

        APIManager.Instance.GET(APIManager.offense_base_personnel, OnOffenseBasePersonnelResponseReceived);


    }

    private void GetFormationResults(int stepLimit)
    {
        LoadingManager.Instance.OpenLoading("Loading formations please wait");
        var requestBody = new FormationSearchRequestBody();
        requestBody.limit = stepLimit;
        requestBody.page = 1;
        requestBody.recentUpdate = true;
        APIManager.Instance.POST(APIManager.latest_offense_data, JsonUtility.ToJson(requestBody), OnFormationsResponseReceived);
    }



    public void ClearAllFormationItems()
    {
        foreach (Transform child in offenseListParent)
        {
            Destroy(child.gameObject);
        }

    }


    public void FilterOffenseItems(bool status)
    {
        updatedFormations.IsOn = status;

        foreach (Transform item in offenseListParent)
        {
            var offItem = item.GetComponent<OffenseItem>();
            item.gameObject.SetActive(updatedFormations.IsOn ? offItem.itemData.modifiedBy == "" : offItem.itemData.modifiedBy != "");
        }

        scrollBar.value = 1;
        updatedFormationTxt.text = !updatedFormations.IsOn ? "UPDATED FORMATIONS" : "NOT UPDATED FORMATIONS ";
    }

    private void OnFormationsResponseReceived(string response)
    {
        scrollBar.value = 1;

        formationResult = JsonUtility.FromJson<FormationsResponse>(response);

        loadedFormationsCount.text = "Loaded formations " + formationResult.result.Count;

        for (int i = 0; i < formationResult.result.Count; i++)
        {

            GameObject go = Instantiate(offenseItemPrefab);
            go.transform.SetParent(offenseListParent, false);
            go.GetComponent<OffenseItem>().SetItemData(formationResult.result[i]);

        }

        FilterOffenseItems(updatedFormations.IsOn);
        scrollBar.value = 1;
        LoginMenu.gameObject.SetActive(false);
        MainMenu.gameObject.SetActive(true);
        LoadingManager.Instance.CloseLoadingScreen("");

    }

    private void OnOffenseBasePersonnelResponseReceived(string response)
    {
        var result = JsonUtility.FromJson<BaseOffensePersonnel>("{\"positions\":" + response + "}");
        foreach (var personnel in result.positions)
        {
            var player = playerObjects[personnel.positionIndex].GetComponent<PlayerItem>();
            player.SetPersonnel(personnel);
        }

        GetFormationResults(currentResultLimit);

    }


    public void UnUpdatedFormationsLoad(bool status)
    {
        //  updatedFormationsFilter = status;
        updatedFormations.IsOn = status;

        updatedFormationTxt.text = updatedFormations.IsOn ? "Not Updated Formations" : "Updated Formations";
        updatedFormationTxt.color = updatedFormations.IsOn ? Color.red : Color.cyan;
        GetFormationResults(updatedFormations.IsOn ? 100 : currentResultLimit);

    }

    //private void OnLogin()
    //{
    //    LoginMenu.gameObject.SetActive(false);
    //    if (!isDemoAccount)
    //        OnLoginEvent?.Invoke(demoemail, demopass);
    //    else
    //        OnLoginEvent?.Invoke(martinemail, martinpass);
    //}

    private void OnValueChanged(bool isOn)
    {
        isDemoAccount = isOn;
        if (isOn)
            currentAccountText.text = "Using Martin's Account";
        else
            currentAccountText.text = "Using Demo Account";
    }

    public void ToggleOrientation(bool landscape)
    {
        if (landscape)
            SetScreenOrientation(ScreenOrientation.LandscapeLeft);
        else
            SetScreenOrientation(ScreenOrientation.Portrait);
    }

    private void SetScreenOrientation(ScreenOrientation orientation)
    {
        Screen.orientation = orientation;
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        Application.targetFrameRate = 60;
        OffenseItem.OnItemClicked += OnLoadData;
        PlayerItem.OnPlayerClicked += OnClickedPlayer;
        PlayerItem.OnPlayerSelected += OnSelectedPlayer;
        ScreenCaptureManager.OnCaptureFinished += OnCaptureAndFormationSaveFinished;
    }

    private void OnCaptureAndFormationSaveFinished()
    {
        updateFormation.enabled = true;
     
    }

    private void OnSelectedPlayer(PlayerItem obj)
    {
        currentPlayer = obj;
    }

    private void OnLoadData(SingleFormationResponse formationData)
    {
        currentItem = formationData;
        currentFormation.text = "Updating formation " + formationData.formationName;
        for (int i = 0; i < formationData.playerPositions.Count; i++)
        {
            var player = playerObjects[i].GetComponent<PlayerItem>();
            player.transform.position = new Vector3(formationData.playerPositions[i].position.x, 0, formationData.playerPositions[i].position.y);
            if (formationData != currentItem)
                player.defaultStance = formationData.playerPositions[i].playerStance;
        }

        MainMenu.gameObject.SetActive(false);
        Stadium.gameObject.SetActive(true);
        OnHideLogin?.Invoke();
    }

    private void OnClickedPlayer(string stance, string alias)
    {
        var res = stance == "" ? "" : alias + " - " + stance;
        currentStance.text = res;
    }

    private void OnDisable()
    {
        OffenseItem.OnItemClicked -= OnLoadData;
        PlayerItem.OnPlayerClicked -= OnClickedPlayer;
        PlayerItem.OnPlayerSelected -= OnSelectedPlayer;
        ScreenCaptureManager.OnCaptureFinished -= OnCaptureAndFormationSaveFinished;

    }

    public void UpdateFormation()
    {
        updateFormation.enabled = false;
        LoadingManager.Instance.OpenLoading("Updating formation " + currentItem.formationName + " please wait");
        var res = new FormationRequest();
        res.id = currentItem.id;
        res.formationName = currentItem.formationName;
        res.baseFormationId = currentItem.baseFormationId;
        res.formationSetId = currentItem.baseFormationId;
        res.captureImg = "string";
        res.thumbnailUrl = "string";

        res.playerPositions = new List<PlayerPosition>();


        for (int i = 0; i < playerObjects.Count; i++)
        {
            var player = playerObjects[i].GetComponent<PlayerItem>();

            var newPos = new Position();
            newPos.x = player.transform.position.x;
            newPos.y = player.transform.position.z;

            var playerPosition = new PlayerPosition();

            Debug.Log(player.offensePersonnel.positionIndex);
            playerPosition.playerIndex = player.offensePersonnel.positionIndex;
            playerPosition.position = newPos;
            playerPosition.playerColor = "string";
            playerPosition.playerStance = player.defaultStance;

            playerPosition.positionY = player.transform.position.y;


            res.playerPositions.Add(playerPosition);

        }

        APIManager.Instance.PUT("https://strongside-api-prod.azurewebsites.net/web-api/api/v2/formation-update", JsonUtility.ToJson(res), OnUpdateResponseReceived);

    }
    private void ShowPopupForSaveScreenShots(bool status)
    {
        SaveScreenshotsPopUp.enabled = status;
        saveScreenshotsText.text = "Do you want to save screenshots for this formation ?";
    }
    private void OnUpdateResponseReceived(ApiResponse fullResponse, string result)
    {      
        LoadingManager.Instance.CloseLoadingScreen($"Formation Updated Successfully!", 1.5f);
        ShowPopupForSaveScreenShots(true);
         OnFormationUpdated?.Invoke(fullResponse, currentItem.id);
    }


    public void PlayerStanceActivation(bool status)
    {
        OnPlayerStanceStatus?.Invoke(!status);
    }
    public void PlayerAliasActivation(bool status)
    {
        OnPlayerPosAliasStatus?.Invoke(!status);
    }
}
