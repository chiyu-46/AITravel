using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class AMapAPI : MonoBehaviour
{
    [SerializeField] private string key;
    [SerializeField] private UnityEvent<string, string> onFunctionComplete;

    private void OnEnable()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("设备不支持位置服务，或应用没有访问位置的权限。");
        }
        else
        {
            StartCoroutine(StartLocation());
        }
    }

    IEnumerator StartLocation()
    {
        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.LogError("位置服务启动超时");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("位置服务初始化失败");
            yield break;
        }
    }

    private void OnDisable()
    {
        Input.location.Stop();
    }

    [Serializable]
    public class AddressInfo
    {
        public string status;
        public string info;
        public string infocode;

        [Serializable]
        public class Regeocode
        {
            public string formatted_address;
        }
        
        public Regeocode regeocode;
    }
    
    public IEnumerator GetAddressUsingLatitudeAndLongitude(string tool_call_id)
    {
        Debug.Log("任务开始：经纬度转地址");
        var apiUrl =
            $"https://restapi.amap.com/v3/geocode/regeo?key={key}&location={Input.location.lastData.longitude},{Input.location.lastData.latitude}&extensions=base";

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        // リクエストの結果を処理
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            throw new Exception();
        }
        else
        {
            var responseString = request.downloadHandler.text;
            AddressInfo addressInfo = JsonUtility.FromJson<AddressInfo>(responseString);
            if (addressInfo.status == "1")
            {
                // 对结果进行处理
                string address = addressInfo.regeocode.formatted_address;
                Dictionary<string, string> result = new Dictionary<string, string>
                {
                    ["result"] = address,
                };
                onFunctionComplete?.Invoke(tool_call_id, JsonConvert.SerializeObject(result));
            }
            else
            {
                Debug.LogError("info:" + addressInfo.info);
                Debug.LogError("infocode:" + addressInfo.infocode);
            }
        }
    }

    [Serializable]
    public class AroundPlaceInfo
    {
        public string status;
        public string info;
        public string infocode;
        public int count;

        [Serializable]
        public class Poi
        {
            public string name;
            public string id;
            public string location;
        }
        
        public List<Poi> pois;
    }
    
    public IEnumerator SearchAroundPlace(string tool_call_id, string types, string keywords)
    {
        Debug.Log("任务开始：搜索周边场所");
        var apiUrl =
            $"https://restapi.amap.com/v5/place/around?key={key}&location={Input.location.lastData.longitude},{Input.location.lastData.latitude}&types={types}&keywords={keywords}";

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        // リクエストの結果を処理
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            throw new Exception();
        }
        else
        {
            var responseString = request.downloadHandler.text;
            AroundPlaceInfo aroundPlaceInfo = JsonUtility.FromJson<AroundPlaceInfo>(responseString);
            if (aroundPlaceInfo.status == "1")
            {
                // 对结果进行处理
                string result = JsonConvert.SerializeObject(aroundPlaceInfo.pois);
                onFunctionComplete?.Invoke(tool_call_id, result);
            }
            else
            {
                Debug.LogError("info:" + aroundPlaceInfo.info);
                Debug.LogError("infocode:" + aroundPlaceInfo.infocode);
            }
        }
    }
    
    [Serializable]
    public class WalkingPathsInfo
    {
        public string status;
        public string info;
        public string infocode;
        public int count;

        [Serializable]
        public class Step
        {
            public string instruction;
        }
        
        [Serializable]
        public class Path
        {
            public List<Step> steps;
        }
        
        [Serializable]
        public class Route
        {
            public List<Path> paths;
        }
        
        public Route route;
    }
    
    public IEnumerator PlanningWalkingPaths(string tool_call_id, string destination, string destination_id)
    {
        Debug.Log("任务开始：规划步行路径");
        var apiUrl =
            $"https://restapi.amap.com/v5/direction/walking?key={key}&origin={Input.location.lastData.longitude},{Input.location.lastData.latitude}&destination={destination}&destination_id={destination_id}";

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        // リクエストの結果を処理
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            throw new Exception();
        }
        else
        {
            var responseString = request.downloadHandler.text;
            WalkingPathsInfo walkingPathsInfo = JsonUtility.FromJson<WalkingPathsInfo>(responseString);
            if (walkingPathsInfo.status == "1")
            {
                // 对结果进行处理
                string result = JsonConvert.SerializeObject(walkingPathsInfo.route.paths);
                onFunctionComplete?.Invoke(tool_call_id, result);
            }
            else
            {
                Debug.LogError("info:" + walkingPathsInfo.info);
                Debug.LogError("infocode:" + walkingPathsInfo.infocode);
            }
        }
    }
}