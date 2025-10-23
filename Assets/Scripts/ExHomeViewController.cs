
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

using GME;

#if UNITY_SWITCH
using nn.fs;
using nn.account;
#endif

public class ExHomeViewController : MonoBehaviour {
     int mClickCount = 0;
     double mFirstClickTime = 0;

#if UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE
    private bool mIsLogin = false;
    private List<Selectable> mSelectableList = new List<Selectable>();
    private int mSelectIndex = 0;
#endif

    public void Start () {
#if UNITY_SWITCH
        nn.account.Uid accountuserId = new nn.account.Uid();
        nn.account.Account.Initialize();
        nn.account.UserHandle userHandle = new nn.account.UserHandle();
        nn.account.Account.TryOpenPreselectedUser(ref userHandle);
        nn.account.Account.GetUserId(ref accountuserId, userHandle);
        nn.Result result = nn.fs.SaveData.Mount("gme_log_cache", accountuserId);
        result = nn.fs.CacheStorage.Mount("cache");
        ITMGContext.GetInstance().SetLogPath("gme_log_cache:/GME_LOG/unity");
#elif UNITY_PS4
        ITMGContext.GetInstance().SetLogPath("data/GMECacheDir");
#elif UNITY_PS5
		ITMGContext.GetInstance().SetLogPath("/devlog/app/GMECacheDir/");
#else
        ITMGContext.GetInstance().SetLogPath(Application.persistentDataPath);
#endif

#if UNITY_2021
        Screen.orientation = ScreenOrientation.LandscapeLeft;
#else
        Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif
        Debug.Log("LoginViewController start!!!");

        string info = "sdk_ver:" + ITMGContext.GetInstance().GetSDKVersion() + "           appid:" + UserConfig.GetExperientialAppID();
        transform.Find("topInfo").GetComponent<Text>().text = info;
        transform.Find("userId").GetComponent<InputField>().text = UserConfig.GetUserID();


		transform.Find ("loginBtn").GetComponent<Button>().onClick.AddListener (delegate() {
            this.OnClickLogin();
        });

		transform.Find ("luanchPanel/LaunchAV").GetComponent<Button>().onClick.AddListener (delegate() {
            UnityEngine.Application.LoadLevel("ExEnterRoomScene");
        });
		transform.Find ("luanchPanel/LaunchPtt").GetComponent<Button> ().onClick.AddListener (delegate() {
#if UNITY_WEBGL
            ShowWarnning("PTT is not supported in WebGL");
            return;
#endif
            UnityEngine.Application.LoadLevel("ExPttScene");
        });

        transform.Find("ChangeDemoBtn").GetComponent<Button>().onClick.AddListener(delegate ()
        {
            OnClickChangeDemo();
        });

#if UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE
        mSelectableList.Add(transform.Find("loginBtn").GetComponent<Button>());
        mSelectableList.Add(transform.Find("luanchPanel/LaunchAV").GetComponent<Button>());
        mSelectableList.Add(transform.Find("luanchPanel/LaunchPtt").GetComponent<Button>());
#endif
    }

    void OnDestroy()
	{
		Debug.Log ("LoginViewController, OnDestroy");
	}

	void ShowWarnning(string warningContent)
	{
		Text warningLabel = transform.Find ("warningLabel").GetComponent<Text> ();
		if (warningLabel) {
			warningLabel.text = warningContent;
		}
	}

    public void Update()
    {
#if UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE
        //update select circle
        transform.Find("loginBgImage").gameObject.GetComponent<Image>().enabled = false;
        if (mIsLogin)
        {
            transform.Find("luanchPanel/avBgImage").gameObject.GetComponent<Image>().enabled = false;
            transform.Find("luanchPanel/pttBgImage").gameObject.GetComponent<Image>().enabled = false;
        }
        switch (mSelectIndex)
        {
            case 0:
                transform.Find("loginBgImage").gameObject.GetComponent<Image>().enabled = true;
                break;
            case 1:
                transform.Find("luanchPanel/avBgImage").gameObject.GetComponent<Image>().enabled = true;
                break;
            case 2:
                transform.Find("luanchPanel/pttBgImage").gameObject.GetComponent<Image>().enabled = true;
                break;
        }
#endif

    }

#if UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE
    public override void OnKeyDown(GamepadKeyCode keyCode, GamepadKeyEvent keyEvent)
    {
        if(keyEvent == GamepadKeyEvent.KeyEventUp)
        {
            return;
        }
        switch (keyCode)
        {
            case GamepadKeyCode.Circle:
                if (mSelectableList[mSelectIndex].Equals(transform.Find("loginBtn").GetComponent<Button>()))
                {
                    OnClickLogin();
                }
                if (mSelectableList[mSelectIndex].Equals(transform.Find("luanchPanel/LaunchAV").GetComponent<Button>()))
                {
                    UnityEngine.Application.LoadLevel("ExEnterRoomScene");
                }
                if (mSelectableList[mSelectIndex].Equals(transform.Find("luanchPanel/LaunchPtt").GetComponent<Button>()))
                {
                    UnityEngine.Application.LoadLevel("ExPttScene");
                }
                break;
            case GamepadKeyCode.Up:
                mSelectIndex--;
                if (mSelectIndex < 0)
                {
                    mSelectIndex = 0;
                }
                if (mSelectIndex == 1)
                {
                    mSelectIndex--;
                }
                break;
            case GamepadKeyCode.Down:
                int limitCount = 0;
                if (mIsLogin)
                {
                    limitCount = 1;
                }
                else
                {
                    limitCount = 2;
                }
                mSelectIndex++;
                if (mSelectIndex >= mSelectableList.Count - limitCount)
                {
                    mSelectIndex = mSelectableList.Count - 1 - limitCount;
                }
                break;
            case GamepadKeyCode.Left:
                if (mIsLogin && mSelectIndex == 2)
                {
                    mSelectIndex--;
                }
                break;
            case GamepadKeyCode.Right:
                if (mIsLogin && mSelectIndex == 1)
                {
                    mSelectIndex++;
                }
                break;
            default:
                break;
        }
    }
#endif

    void OnClickChangeDemo()
    {
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        double now = t.TotalMilliseconds;
        if(mClickCount == 0){
            mFirstClickTime = now;
        }

        mClickCount++;
        if (mClickCount == 5)
        {
            mClickCount = 0;
            if (now - mFirstClickTime < 3000)
            {
                UnityEngine.Application.LoadLevel("HomeScene");
            }
        }
        
    }
	void OnClickLogin()
	{
#if UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE
        mIsLogin = true;
#endif
        string userId = transform.Find ("userId").GetComponent<InputField> ().text;
		if (userId.Equals ("")) {
			ShowWarnning("user is empty.");
			return;
		}
        EnginePollHelper.CreateEnginePollHelper();                           //// warning : Without Poll Engine won't work

        int ret = ITMGContext.GetInstance().Init(UserConfig.GetExperientialAppID(), userId);
        if (ret != QAVError.OK)
        {
            ShowWarnning(string.Format("Init Failed {0}", ret));
            return;
        }

#if UNITY_WEBGL
        ShowWarnning(string.Format("Init Success", ret));
        UserConfig.SetUserID(transform.Find("userId").GetComponent<InputField>().text);
        transform.Find("luanchPanel").gameObject.SetActive(true);
        return;
#endif

        UserConfig.SetUserID(transform.Find("userId").GetComponent<InputField>().text);
        byte[] sig = UserConfig.GetAuthBuffer(UserConfig.GetExperientialAppID(), "0", userId, UserConfig.GetExperientialauthkey());
        if (sig != null)
        {
            ITMGContext.GetInstance().GetPttCtrl().ApplyPTTAuthbuffer(sig);
            ShowWarnning("ApplyPTTAuthbuffer success");
        }
        else
        {
            ShowWarnning("get ptt token failed.");
        }

        transform.Find("luanchPanel").gameObject.SetActive(true);
    }
    }
