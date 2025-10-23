
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GME;

class UserConfig
{
    public static string GetExperientialAppID()
    {
        return GetAppID();
    }

    public static string GetExperientialauthkey()
    {
        return GetAuthKey();
    }

    public static bool GetTestEnv() {
		return PlayerPrefs.GetInt("TestEnv", 0) != 0;
	}
    
    public static string GetAppID() {
        return PlayerPrefs.GetString("AppID");
	}

    public static string GetAuthKey()
    {
        return PlayerPrefs.GetString("AuthKey");
    }

    public static string GetAdvParamConfig()
    {
        return PlayerPrefs.GetString("AdvParamConfig", "");
    }
    public static void SetAdvParamConfig(string config)
    {
        PlayerPrefs.SetString("AdvParamConfig", config);
    }

    public static void SetMaxLogCount(string maxLogCount)
    {
        PlayerPrefs.SetString("MaxLogCount", maxLogCount);
    }

    public static void SetAuthKey(string AuthKey)
    {
        PlayerPrefs.SetString("AuthKey", AuthKey);
    }

    public static void SetAppID(string appID) {
		PlayerPrefs.SetString("AppID", appID);
	}

	public static string GetUserID() {
		int randomUId = UnityEngine.Random.Range(1, 10000);
		return PlayerPrefs.GetString("UserID", randomUId.ToString() );
	}

	public static void SetUserID(string userID) {
		PlayerPrefs.SetString("UserID", userID);
	}


    public static string GetRoomID() {
		return PlayerPrefs.GetString("strRoomID", "banana");
	}

	public static void SetRoomID(string roomID) {
		PlayerPrefs.SetString("strRoomID", roomID);
	}

	public static ITMGRoomType GetRoomType() {
		return (ITMGRoomType)PlayerPrefs.GetInt("RoomType", 1);
	}

	public static void SetRoomType(ITMGRoomType roomtype) {
		PlayerPrefs.SetInt("RoomType", (int)roomtype);
	}

	public static byte[] GetAuthBuffer(string sdkAppID, string roomID, string userID, string authKey)
	{
        string key = "";
        key = authKey;
        return QAVAuthBuffer.GenAuthBuffer(int.Parse(sdkAppID), roomID, userID, key);
	}


	public static void SetAudioRole(string AudioRole) {
		PlayerPrefs.SetString("AudioRole", AudioRole);
	}

	public static string GetAudioRole() {
		return PlayerPrefs.GetString("AudioRole", "0");
	}

	public static void SetTeamID(string teamID) {
		PlayerPrefs.SetString("TeamID", teamID);
	}



	public static string GetTeamID() {
		return PlayerPrefs.GetString("TeamID", "");
	}

	public static void SetTeamMode(string teamMode)
	{
		PlayerPrefs.SetString("TeamMode", teamMode);
	}

	public static string GetTeamMode()
	{
		return PlayerPrefs.GetString("TeamMode", "");
	}
	public static void SetSpeakerEnabled(bool speakerEnable)
    {
		int enable = 0;
		if (speakerEnable)
        {
			enable = 1;

		}
		PlayerPrefs.SetInt("SpeakerEnabled", enable);
	}
	public static bool GetSpeakerEnabled()
	{
		int enable = PlayerPrefs.GetInt("SpeakerEnabled",0);
		if (enable == 1)
        {
			return true;
        }
		return false;
	}
}
