using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using GME;

public class ExPttViewController : MonoBehaviour
{
    private int sUid = 0;
    private bool isRecording;
    private string recordFilePath;

    private Text speachText;
    private Text pttButtonText;
    private Button playerButton;
    string[,] supportLanguages;
    private Dropdown languageDropdown;
    private bool isPlaying = false;
    private int nSelectedIndex = 0;
    private bool usePttWords = false;


    // Use this for initialization
    public void Start()
    {
        supportLanguages = new string[,]
        {
            { "普通话 (中国大陆)", "cmn-Hans-CN" },
            { "普通話 (香港)", "cmn-Hans-HK" },
            { "廣東話 (香港)", "yue-Hant-HK" },
            { "國語 (台灣)", "cmn-Hant-TW" },
            { "한국어 (대한민국)", "ko-KR" },
            { "日本語", "ja-JP" },
            { "English (United States)", "en-US" },
            { "English (Great Britain)", "en-GB" },
            { "Русский (Россия)", "ru-RU" },
            { "Italiano (Italia)", "it-IT" },
            { "Français (France)", "fr-FR" },
            { "Español (España)", "es-ES" },
            { "Português (Portugal)", "pt-PT" },
            { "Filipino (Pilipinas)", "fil-PH" },
        };


        pttButtonText = transform.Find("Btn_Record/Text").GetComponent<Text>();
        playerButton = transform.Find("Btn_Play").GetComponent<Button>();
        speachText = transform.Find("Panel/Text_Translate").GetComponent<Text>();
        languageDropdown = transform.Find("Panel/languageDropdown").GetComponent<Dropdown>();


        playerButton.gameObject.SetActive(false);
        playerButton.onClick.AddListener(delegate() { this.OnPlay(); });
        languageDropdown.ClearOptions();

        List<string> languageNames = new List<string>();

        for (int i = 0; i < supportLanguages.GetLength(0); i++)
        {
            string languageName = supportLanguages[i, 0];
            string languageId = supportLanguages[i, 1];

            languageNames.Add(languageName);
        }

        languageDropdown.AddOptions(languageNames);
        isRecording = false;
        ITMGContext.GetInstance().GetPttCtrl().OnStreamingSpeechComplete += OnStreamingSpeechComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnRecordFileComplete += OnRecordFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnUploadFileComplete += OnUploadFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnDownloadFileComplete += OnDownloadFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnDownloadFileAuditComplete += OnDownloadFileAuditComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnPlayFileComplete += OnPlayFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnSpeechToTextComplete += OnSpeechToTextComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnSpeechToTextAuditComplete += OnSpeechToTextAuditComplete;

        Toggle hotWordToggle = transform.Find("Panel/Hotword_Toggle").GetComponent<Toggle>();

        if (hotWordToggle)
        {
            hotWordToggle.onValueChanged.AddListener(delegate(bool value) { onHotWordToggle(value); });
        }

        transform.Find("Panel/InputField_hotwords").GetComponent<InputField>().text = "万剑、辰皇、蜜制、真石、奇奇、托尔、争锋路、斩妖御魔行";
    }

    // Update is called once per frame
    public  void Update()
    {
#if UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE
        //update select circle
        transform.Find("recordBtnBgImage").gameObject.GetComponent<Image>().enabled = false;
        transform.Find("playBtnBgImage").gameObject.GetComponent<Image>().enabled = false;
        if (nSelectedIndex == 0)
        {
            transform.Find("recordBtnBgImage").gameObject.GetComponent<Image>().enabled = true;
        } else if(nSelectedIndex == 1)
        {
            transform.Find("playBtnBgImage").gameObject.GetComponent<Image>().enabled = true;
        }

#endif
    }

    public void OnDestroy()
    {
        ITMGContext.GetInstance().GetPttCtrl().OnStreamingSpeechComplete -= OnStreamingSpeechComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnRecordFileComplete -= OnRecordFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnUploadFileComplete -= OnUploadFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnDownloadFileComplete -= OnDownloadFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnDownloadFileAuditComplete -= OnDownloadFileAuditComplete;

        ITMGContext.GetInstance().GetPttCtrl().OnPlayFileComplete -= OnPlayFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnSpeechToTextComplete -= OnSpeechToTextComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnSpeechToTextAuditComplete -= OnSpeechToTextAuditComplete;
    }

    public virtual void closeScene()
    {
        UnityEngine.Application.LoadLevel("ExHomeScene");
    }

    /// <summary>
    /// 在停止录制并完成识别后才返回文字。
    /// </summary>
    /// <param name="code">用于判断流式语音识别是否成功的返回码</param>
    /// <param name="fileid">录音在后台的 url 地址，录音在服务器存放90天。监听 ITMG_MAIN_EVNET_TYPE_PTT_STREAMINGRECOGNITION_IS_RUNNING 消息时，file_id 为空。</param>
    /// <param name="filePath">录音存放的本地地址</param>
    /// <param name="result">语音转文字识别的文本</param>
    void OnStreamingSpeechComplete(int code, string fileid, string filePath, string result)
    {
        Debug.LogFormat("OnStreamingSpeechComplete:{0},{1},{2},{3}", code, fileid, filePath, result);
        if (code != 0)
        {
            speachText.text = string.Format("error({0})!", code);
            return;
        }

        recordFilePath = filePath;

        // 激活回放键
        playerButton.gameObject.SetActive(true);
        Text text = playerButton.gameObject.GetComponentInChildren<Text>();
        if (text)
        {
            // 显示要回放的录音的时长（秒）
            text.text = string.Format("{0}s",
                ITMGContext.GetInstance().GetPttCtrl().GetVoiceFileDuration(filePath) / 1000);
        }

        // 展示识别结果
        speachText.text = result;
    }

    void OnRecordFileComplete(int code, string filePath)
    {
        Debug.LogFormat("OnRecordFileComplete:{0},{1}", code, filePath);

        if (code != 0)
        {
            return;
        }

        recordFilePath = filePath;
        ITMGContext.GetInstance().GetPttCtrl().UploadRecordedFile(filePath);

        playerButton.gameObject.SetActive(true);
        Text text = playerButton.gameObject.GetComponentInChildren<Text>();
        if (text)
        {
            text.text = string.Format("{0}s",
                ITMGContext.GetInstance().GetPttCtrl().GetVoiceFileDuration(filePath) / 1000);
        }
    }

    void OnUploadFileComplete(int code, string filePath, string fileid)
    {
        Debug.LogFormat("OnUploadFileComplete:{0},{1},{2}", code, filePath, fileid);
        if (code != 0)
        {
            return;
        }

        string speechLanguage = supportLanguages[languageDropdown.value, 1];


        ITMGContext.GetInstance().GetPttCtrl().SpeechToText(fileid, speechLanguage);
    }

    void OnDownloadFileComplete(int code, string filePath, string fileid)
    {
        Debug.LogFormat("OnDownloadFileComplete:{0},{1},{2}", code, filePath, fileid);
    }

    void OnDownloadFileAuditComplete(int code, string filePath, string fileid, string auditResult)
    {
        Debug.LogFormat("OnDownloadFileComplete:{0},{1},{2},{3}", code, filePath, fileid, auditResult);
    }

    void OnPlayFileComplete(int code, string filePath)
    {
        isPlaying = false;
        Debug.LogFormat("OnPlayFileComplete:{0},{1}", code, filePath);
    }

    void OnSpeechToTextComplete(int code, string filePath, string result)
    {
        Debug.LogFormat("OnSpeechToTextComplete:{0},{1},{2}", code, filePath, result);
        if (code != 0)
        {
            speachText.text = string.Format("error({0})!", code);
            return;
        }

        speachText.text = result;
    }

    void OnSpeechToTextAuditComplete(int code, string filePath, string result, string auditResult)
    {
        Debug.LogFormat("OnSpeechToTextComplete:{0},{1},{2},{3}", code, filePath, result, auditResult);
        if (code != 0)
        {
            speachText.text = string.Format("error({0})!", code);
            return;
        }

        speachText.text = result;
    }

    public void showWarningText(string text)
    {
        GameObject obj = transform.Find("Panel/Text_waring").gameObject;
        if (obj)
        {
            Text warningLabel = obj.GetComponent<Text>();
            warningLabel.text = text;
        }
    }

    public void onTouchDown()
    {
        // 控制录音开始。
        Debug.LogFormat("onTouchDown");
        if (isRecording)
        {
            return;
        }

        playerButton.gameObject.SetActive(false);
        speachText.text = "";
#if UNITY_SWITCH
        string recordPath = Application.streamingAssetsPath + string.Format ("/{0}.silk", sUid++);
#elif UNITY_PS4
        string recordPath = string.Format("/data/GMECacheDir/{0}.silk", sUid++);
#elif UNITY_PS5
        string recordPath = string.Format("/devlog/app/GMECacheDir/{0}.silk", sUid++);
#else
        string recordPath = Application.persistentDataPath + string.Format("/{0}.silk", sUid++);
#endif
        if (usePttWords)
        {
            string inputHotwords = transform.Find("Panel/InputField_hotwords").GetComponent<InputField>().text;
            string pttHotWorld = "";
            if (!inputHotwords.Equals(""))
            {
                pttHotWorld = GetPTThotWords(inputHotwords);
            }

            Debug.LogFormat("ptthotords: {0}", pttHotWorld);
            int ret = ITMGContext.GetInstance().SetAdvanceParams("SetPTTHotWorldList", pttHotWorld);
            if (ret != 0)
            {
                showWarningText(string.Format("Set pttHotWords Fail!, ret = {0}", ret));
            }
        }

        string speechLanguage = supportLanguages[languageDropdown.value, 1];
        ITMGContext.GetInstance().GetPttCtrl().StartRecordingWithStreamingRecognition(recordPath, speechLanguage);
//		ITMGContext.GetInstance().GetPttCtrl().StartRecording(recordPath);

        isRecording = true;
        pttButtonText.text = "Release To Send";
    }

    public void onTouchUp()
    {
        // 控制录音停止。
        Debug.LogFormat("onTouchUp");
        ITMGContext.GetInstance().GetPttCtrl().StopRecording();

        isRecording = false;
        pttButtonText.text = "Push To Talk";
    }

    public void onTouchExit()
    {
        // 控制录音取消
        pttButtonText.text = "Push To Talk";

        if (isRecording == false)
        {
            return;
        }

        Debug.LogFormat("onTouchExit");
        ITMGContext.GetInstance().GetPttCtrl().CancelRecording();
    }

    /// <summary>
    /// 由回放按钮调用，回放录制的音频。
    /// </summary>
    public void OnPlay()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            ITMGContext.GetInstance().GetPttCtrl().PlayRecordedFile(recordFilePath);
            Debug.LogFormat("PlayRecordedFile:{0}", recordFilePath);
        }
        else
        {
            ITMGContext.GetInstance().GetPttCtrl().StopPlayFile();
        }
    }

    public string GetPTThotWords(string str)
    {
        if (str == "")
        {
            return "";
        }

        string hotwords = "";
        string strFlag = "|10";
        string[] substrings = str.Split('、');
        foreach (string s in substrings)
        {
            if (hotwords != "")
            {
                hotwords += ",";
            }

            hotwords += s;
            hotwords += strFlag;
        }

        return hotwords;
    }

    public void onHotWordToggle(bool value)
    {
        Debug.LogFormat("SetPTTHotWorldList: {0}", value);
        usePttWords = value;

        if (!value)
        {
            ITMGContext.GetInstance().SetAdvanceParams("SetPTTHotWorldList", "");
        }
    }

#if UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE
    public override void OnKeyDown(GamepadKeyCode keyCode, GamepadKeyEvent keyEvent)
    {
        if (keyEvent == GamepadKeyEvent.KeyEventDown)
        {
            if (keyCode == GamepadKeyCode.X) 
            {
                closeScene();
            } else if(keyCode == GamepadKeyCode.Circle)
            {
                if (nSelectedIndex == 0)
                {
                    onTouchDown();
                } else if(nSelectedIndex == 1)
                {
                    OnPlay();
                }
                
            }

        } else if(keyEvent == GamepadKeyEvent.KeyEventUp)
        {
            switch(keyCode)
            {
            case GamepadKeyCode.Circle:
                onTouchUp();
                break;
            case GamepadKeyCode.Up:
                if(nSelectedIndex == 0)
                {
                    nSelectedIndex = 1;
                }
                break;
            case GamepadKeyCode.Down:
                if (nSelectedIndex == 1)
                {
                    nSelectedIndex = 0;
                }
                break;
            }
        }
    }
#endif
}