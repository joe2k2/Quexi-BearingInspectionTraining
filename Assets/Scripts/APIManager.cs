using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    private static APIManager _instance;
    public static APIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("APIManager");
                _instance = go.AddComponent<APIManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private const string BaseURL = "https://nskadmin.quexitechnologies.com";
    private string _authToken;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class UserData
    {
        public string id;
        public string name;
        public string email;
    }

    [Serializable]
    public class LoginResponse
    {
        public string token;
        public UserData user;
        // The server might return message/status on failure, but success uses token/user
        public bool status;
        public string message;
    }

    [Serializable]
    public class AddPointsRequest
    {
        public string bearingId;
        public int points;
    }

    [Serializable]
    public class BearingData
    {
        public string _id;
        public string name;
    }

    [Serializable]
    public class BearingsResponse
    {
        public BearingData[] bearings;
        public int total;
    }

    [Serializable]
    public class GenericResponse
    {
        public bool status;
        public string message;
    }

    [Serializable]
    public class MyPointsResponse
    {
        public bool status;
        public int points;
    }

    public void Login(string email, string password, Action<bool, string> onComplete)
    {
        StartCoroutine(LoginRoutine(email, password, onComplete));
    }

    private IEnumerator LoginRoutine(string email, string password, Action<bool, string> onComplete)
    {
        LoginRequest requestData = new LoginRequest { email = email, password = password };
        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest($"{BaseURL}/api/user/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            string rawResponse = request.downloadHandler.text;
            if (showDebugLogs) Debug.Log($"API Login Raw Response: {rawResponse}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    LoginResponse response = JsonUtility.FromJson<LoginResponse>(rawResponse);
                    // Treat presence of token as success
                    if (response != null && !string.IsNullOrEmpty(response.token))
                    {
                        _authToken = response.token;
                        string msg = !string.IsNullOrEmpty(response.message) ? response.message : "Login Successful";
                        onComplete?.Invoke(true, msg);
                    }
                    else
                    {
                        string msg = (response != null && !string.IsNullOrEmpty(response.message)) 
                            ? response.message 
                            : "Invalid credentials or server error";
                        onComplete?.Invoke(false, msg);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"API Login Parse Error: {e.Message}. Raw: {rawResponse}");
                    onComplete?.Invoke(false, "Error parsing server response");
                }
            }
            else
            {
                onComplete?.Invoke(false, $"Network Error: {request.error}");
            }
        }
    }

    public void AddPoints(string bearingId, int points, Action<bool, string> onComplete = null)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            onComplete?.Invoke(false, "Not authenticated");
            return;
        }
        StartCoroutine(AddPointsRoutine(bearingId, points, onComplete));
    }

    private IEnumerator AddPointsRoutine(string bearingId, int pointsValue, Action<bool, string> onComplete)
    {
        AddPointsRequest requestData = new AddPointsRequest 
        { 
            bearingId = bearingId, 
            points = pointsValue 
        };
        string jsonData = JsonUtility.ToJson(requestData);

        if (showDebugLogs) Debug.Log($"API Add Points Sending JSON: {jsonData}");

        using (UnityWebRequest request = new UnityWebRequest($"{BaseURL}/api/user/add-points", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {_authToken}");

            yield return request.SendWebRequest();

            string rawResponse = request.downloadHandler.text;
            if (showDebugLogs) Debug.Log($"API Add Points Raw Response: {rawResponse}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                GenericResponse response = JsonUtility.FromJson<GenericResponse>(rawResponse);
                onComplete?.Invoke(response.status, response.message);
            }
            else
            {
                onComplete?.Invoke(false, request.error);
            }
        }
    }

    public void GetMyPoints(Action<bool, int, string> onComplete)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            onComplete?.Invoke(false, 0, "Not authenticated");
            return;
        }
        StartCoroutine(GetMyPointsRoutine(onComplete));
    }

    private IEnumerator GetMyPointsRoutine(Action<bool, int, string> onComplete)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{BaseURL}/api/user/my-points"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {_authToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                MyPointsResponse response = JsonUtility.FromJson<MyPointsResponse>(request.downloadHandler.text);
                if (response.status)
                    onComplete?.Invoke(true, response.points, "Success");
                else
                    onComplete?.Invoke(false, 0, "Failed to fetch points");
            }
            else
            {
                onComplete?.Invoke(false, 0, request.error);
            }
        }
    }

    public void GetBearings(Action<bool, BearingData[], string> onComplete)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            onComplete?.Invoke(false, null, "Not authenticated");
            return;
        }
        StartCoroutine(GetBearingsRoutine(onComplete));
    }

    private IEnumerator GetBearingsRoutine(Action<bool, BearingData[], string> onComplete)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{BaseURL}/api/bearings"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {_authToken}");

            yield return request.SendWebRequest();

            string rawResponse = request.downloadHandler.text;
            if (showDebugLogs) Debug.Log($"API Get Bearings Raw Response: {rawResponse}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    BearingsResponse response = JsonUtility.FromJson<BearingsResponse>(rawResponse);
                    onComplete?.Invoke(true, response.bearings, "Success");
                }
                catch (Exception e)
                {
                    onComplete?.Invoke(false, null, $"Parse Error: {e.Message}");
                }
                Debug.Log("Success");
            }
            else
            {
                onComplete?.Invoke(false, null, request.error);
                Debug.Log("Failure");
            }
        }
    }
}
