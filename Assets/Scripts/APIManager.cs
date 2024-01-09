using Newtonsoft.Json.Linq;
using StrongSideAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;




public class APIManager : MonoBehaviour
{

    public static APIManager Instance;
    //URLS
    public const string latest_offense_data = "https://strongside-api-prod.azurewebsites.net/web-api/api/v2/formation-search";
    public const string log_in = "https://strongside-api-prod.azurewebsites.net/web-api/api/v2/login";
    public const string offense_base_personnel = "https://strongside-api-prod.azurewebsites.net/web-api/api/v2/personnel-group-position-base";
    public const string default_base_formation = "https://strongside-api-prod.azurewebsites.net/web-api/api/v2/formation-default";
    private string token = "";

    private int maxResultLimit = 250;
    private int currentResultLimit = 25;
    private int limit = 25;

    [SerializeField] TMP_Text team, role, loadedFormationsCount;
    private UserResponse userResponse;
    // private LatestOffenseResponse latestOffense;
    public FormationsResponse formationResult;
    public FormationsResponse unUpdatedResult;
    [SerializeField] Transform offenseListParent, unUpdatedListParent;
    [SerializeField] GameObject offenseItemPrefab;
    [SerializeField] Scrollbar scrollBar;


    public BaseFormationResponseWithStances baseFormation;
    [SerializeField] Switch updatedFormations;
    bool updatedFormationsFilter = true;
    bool initial = false;


    private void Awake()
    {
        Instance = this;
    }


    public string ConvertJsonToString(object json)
    {
        return JsonUtility.ToJson(json);
    }


    public void GET(string apiUrl, Action<string> callback)
    {
        StartCoroutine(SendGetRequest(apiUrl, callback));
    }

    public void POST(string apiUrl, string requestData, Action<string> callback)
    {
        StartCoroutine(PostRqeuest(apiUrl, requestData, callback));
    }

    public void PUT(string apiUrl, string requestData, Action<ApiResponse, string> callback)
    {
        StartCoroutine(putRequest(apiUrl, requestData, callback));
    }
    IEnumerator SendGetRequest(string apiUrl, Action<string> callback)
    {
        // Create a UnityWebRequest object with the GET method
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);

        // Set the authorization header with the bearer token
        request.SetRequestHeader("Authorization", "Bearer " + AppManager.Instance.Token);

        // Set the content type header to JSON
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {

            string responseJson = request.downloadHandler.text;
            callback?.Invoke(responseJson);
        }
    }

    IEnumerator PostRqeuest(string apiUrl, string requestData, Action<string> callback)
    {
        var uwr = new UnityWebRequest(apiUrl, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(requestData);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Authorization", "Bearer " + AppManager.Instance.Token);
        uwr.SetRequestHeader("Content-Type", "application/json");


        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            callback?.Invoke(uwr.downloadHandler.text);
        }
        else
        {
            string responseJson = uwr.downloadHandler.text;
            var toJson = JsonUtility.FromJson<ApiResponse>(uwr.downloadHandler.text);
            callback?.Invoke(responseJson);
        }
    }
    IEnumerator putRequest(string url, string requestData, Action<ApiResponse, string> callback)
    {
        byte[] dataToPut = System.Text.Encoding.UTF8.GetBytes(requestData);
        UnityWebRequest uwr = UnityWebRequest.Put(url, dataToPut);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", "Bearer " + AppManager.Instance.Token);
        yield return uwr.SendWebRequest();
        var genericResponse = new ApiResponse();

        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {

            var toJson = JsonUtility.FromJson<ApiResponse>(uwr.downloadHandler.text);
           // AppManager.Instance.OpenLoading(toJson.errorMsg);
            callback?.Invoke(toJson, uwr.downloadHandler.text);
            Debug.LogError("Error: " + uwr.error);
        }
        else
        {

            var toJson = JsonUtility.FromJson<ApiResponse>(uwr.downloadHandler.text);
           // AppManager.Instance.OpenLoading(toJson.errorMsg);
            //  Debug.Log("Response: " + JsonUtility.ToJson(toJson));
            callback?.Invoke(toJson, uwr.downloadHandler.text);
        }
    }
}

namespace StrongSideAPI
{
    [Serializable]
    public class ApiResponse
    {
        public string response;
        public int responseCode;
        public string responseMsg;
        public string errorMsg;
        public bool success;
    }


    [Serializable]
    public class FormationSearchRequestBody
    {
        public int limit;
        public int page;
        public bool recentUpdate;
    }

    [Serializable]
    public class UserRequest
    {
        public string username;
        public string password;

        public UserRequest(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
    public class Child
    {
        public string path;
        public string title;
        public string icon;
        public string type;
        public bool active;
        public bool frontendMenu;
        public bool viewAccess;
        public bool fullAccess;
        public object children;
    }


    public class Menu
    {
        public string path;
        public string title;
        public string icon;
        public string type;
        public bool active;
        public bool frontendMenu;
        public bool viewAccess;
        public bool fullAccess;
        public List<Child> children;
    }

    [Serializable]
    public class UserResponse
    {
        public string userName;
        public string role;
        public string userType;
        public string accountName;
        public bool isPackagePaid;
        public bool isDemoAccount;
        public List<Menu> menus;
        public string token;
        public bool isAudioOn;
    }


    [Serializable]
    public class FormationSetImage
    {
        public string id;
        public bool isDesktopUpload;
        public bool isAutoGenarated;
        public bool isDefault;
        public bool isVideo;
        public string fileName;
        public string fileSeq;
        public string fileUrl;
        public object thumbnailSeq;
        public object thumbnailUrl;
    }

    [Serializable]
    public class OppPlayerPosition
    {
        public int playerIndex;
        public Position position;
        public string playerColor;
        public object playerStance;
        public double positionY;
    }

    [Serializable]
    public class PlayerPosition
    {
        public int playerIndex;
        public Position position;
        public string playerColor;
        public string playerStance;
        public double positionY;
    }
    [Serializable]
    public class BasePlayerPosition
    {
        public int playerIndex;
        public Position position;
        public object positionY;
        public object playerColor;
        public string defaultStance;
        public List<string> otherStances;
    }

    [Serializable]
    public class Position
    {
        public float x;
        public float y;
    }


    [Serializable]
    public class BaseFormationResponseWithStances
    {
        public BaseFormation[] Items;
    }

    [Serializable]
    public class BaseFormation
    {
        public string id;
        public string userId;
        public object teamId;
        public object baseFormationName;
        public object formationSetId;
        public object formationSetName;
        public string formationName;
        public int version;
        public bool isBase;
        public bool hasOpposite;
        public object thumbnailUrl;
        public object oppositeFormationId;
        public object oppBaseFormationName;
        public string createdBy;
        public DateTime createdAt;
        public string modifiedBy;
        public DateTime modifiedAt;
        public object tagModel;
        public List<BasePlayerPosition> playerPositions;
    }
    [Serializable]
    public class TagList
    {
        public string id;
        public string defaultTagTypeId;
        public string tagName;
        public object accountId;
        public string createdBy;
        public DateTime createdOn;
        public object types;
        public string originRefId;
        public object menuUrl;
    }

    [Serializable]
    public class FormationRequest
    {
        public string id;
        public string formationName;
        public string baseFormationId;
        public string formationSetId;
        public List<PlayerPosition> playerPositions;
        public string captureImg;
        public string thumbnailUrl;
    }


    [Serializable]
    public class ForTest
    {
        public SingleFormationResponse[] result;
        public int count;
        public object errorCode;
        public object errorMsg;
        public bool success;
    }

    [Serializable]
    public class FormationsResponse
    {
        public List<SingleFormationResponse> result;
        public int count;
        public object errorCode;
        public object errorMsg;
        public bool success;
    }


    [Serializable]
    public class SingleFormationResponse
    {
        public string id;
        public string formationName;
        public string baseFormationName;
        public string formationSetName;
        public string thumbnailUrl;
        public bool hasOpposite;
        public string oppBaseFormationName;
        public object oppThumbnailUrl;
        public string baseFormationId;
        public string formationSetId;
        public object captureImg;
        public bool isArchive;
        public bool isFavorite;
        public bool isDefault;
        public bool dislike;
        public object favoriteStatus;
        public object defaultStatus;
        public bool isDesktopUpload;
        public bool isAutoGenarated;
        public string daysCount;
        public string modifiedUser;
        public List<PlayerPosition> playerPositions;
        public List<FormationImage> formationImages;
        public List<object> formationVideos;
        public List<string> tagNames;
        public string defaultFileSeq;
        public string thumbnailSeq;
        public List<object> plays;
        public object hash;
        public object viewType;
        public string fileSeq;
        public bool isDefaultFileVideo;
        public bool isAccountData;
        public bool isDraft;
        public bool isUpdateRequired;
        public object updateMessage;
        public object fileUrl;
        public string createdBy;
        public string createdOn;
        public string modifiedBy;
        public string modifiedOn;

    }
    [Serializable]
    public class FormationImage
    {
        public string id;
        public bool isDesktopUpload;
        public bool isAutoGenarated;
        public bool isDefault;
        public bool isVideo;
        public string fileName;
        public string fileSeq;
        public string fileUrl;
        public object thumbnailSeq;
        public object thumbnailUrl;
    }

    [Serializable]
    public class OffensePersonnel
    {
        public string id;
        public string personnelGroupId;
        public string positionId;
        public string positionName;
        public string positionAlias;
        public int position;
        public int positionIndex;
        public object alignment;
    }

    [Serializable]
    public class BaseOffensePersonnel
    {
        public OffensePersonnel[] positions;
    }
}




