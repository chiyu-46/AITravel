using System;
using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using GME;

public class SpeechToText : MonoBehaviour
{
    private string userId;
    private string appId = "1400801462";
    private string authkey = "8ce0c53ddf40486e";

    // 是否在录音
    private bool isRecording = false;

    // 录音文件位置
    private string recordFilePath;

    // 录音文件的id。作为文件名的一部分出现，依次递增。
    private int sUid = 0;
    private bool isPlaying = false;
    private bool usePttWords = false;

    // 开始/停止录音按钮
    [SerializeField]private TMP_Text pttButtonText;

    // 当语音转录完成时调用
    [SerializeField] private UnityEvent<string> onTranscriptionComplete;

    private void Start()
    {
        Debug.Log("初始化GME");
        ITMGContext.GetInstance().SetLogPath(Application.persistentDataPath);

        // 使用秒时间戳作为用户id，避免重复
        userId = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString(CultureInfo.InvariantCulture);

        // 开启 GME 事件轮询
        EnginePollHelper.CreateEnginePollHelper();

        // 初始化 GME
        int ret = ITMGContext.GetInstance().Init(appId, userId);
        if (ret != QAVError.OK)
        {
            Debug.LogError($"初始化GME失败，错误码：{ret}");
            return;
        }

        // 鉴权
        byte[] sig = UserConfig.GetAuthBuffer(appId, "0", userId, authkey);
        if (sig != null)
        {
            ITMGContext.GetInstance().GetPttCtrl().ApplyPTTAuthbuffer(sig);
            Debug.Log("ApplyPTTAuthbuffer success");
        }
        else
        {
            Debug.LogError("get ptt token failed.");
        }

        // 事件绑定
        ITMGContext.GetInstance().GetPttCtrl().OnStreamingSpeechComplete += OnStreamingSpeechComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnRecordFileComplete += OnRecordFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnUploadFileComplete += OnUploadFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnDownloadFileComplete += OnDownloadFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnDownloadFileAuditComplete += OnDownloadFileAuditComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnPlayFileComplete += OnPlayFileComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnSpeechToTextComplete += OnSpeechToTextComplete;
        ITMGContext.GetInstance().GetPttCtrl().OnSpeechToTextAuditComplete += OnSpeechToTextAuditComplete;

        // 在音频转录完成后，显示结果，便于调试
        onTranscriptionComplete.AddListener(ShowResultInDebug);
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

        onTranscriptionComplete.RemoveListener(ShowResultInDebug);
    }

    private void ShowResultInDebug(string result)
    {
        Debug.Log(result);
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
        Debug.LogFormat("流式语音识别完成:{0},{1},{2},{3}", code, fileid, filePath, result);
        if (code != 0)
        {
            Debug.LogError($"流式语音识别完成出错，错误码：({code})!详细信息显示：GME > 客户端API > Unity SDK > 语音消息及转文本 > 流式语音识别的回调 文档。");
            return;
        }

        // 记录回放文件路径
        recordFilePath = filePath;

        Debug.Log($"录音时长：{ITMGContext.GetInstance().GetPttCtrl().GetVoiceFileDuration(filePath) / 1000}");

        // 展示识别结果
        onTranscriptionComplete.Invoke(result);
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
    }

    void OnUploadFileComplete(int code, string filePath, string fileid)
    {
        Debug.LogFormat("OnUploadFileComplete:{0},{1},{2}", code, filePath, fileid);
        if (code != 0)
        {
            return;
        }

        ITMGContext.GetInstance().GetPttCtrl().SpeechToText(fileid, "cmn-Hans-CN");
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
            Debug.LogError($"OnSpeechToTextComplete出错，错误码：({code})!详细信息显示：GME > 客户端API > Unity SDK > 语音消息及转文本  文档。");
            return;
        }

        onTranscriptionComplete.Invoke(result);
    }

    void OnSpeechToTextAuditComplete(int code, string filePath, string result, string auditResult)
    {
        Debug.LogFormat("OnSpeechToTextComplete:{0},{1},{2},{3}", code, filePath, result, auditResult);
        if (code != 0)
        {
            Debug.LogError($"OnSpeechToTextComplete出错，错误码：({code})!详细信息显示：GME > 客户端API > Unity SDK > 语音消息及转文本  文档。");
            return;
        }

        onTranscriptionComplete.Invoke(result);
    }

    public void onTouchDown()
    {
        // 控制录音开始。
        Debug.LogFormat("onTouchDown");
        if (isRecording)
        {
            return;
        }

        // 如果需要回放，此处应禁止回放

        string recordPath = Application.persistentDataPath + string.Format("/{0}.silk", sUid++);
        // if (usePttWords)
        // {
        //     string inputHotwords = transform.Find("Panel/InputField_hotwords").GetComponent<InputField>().text;
        //     string pttHotWorld = "";
        //     if (!inputHotwords.Equals(""))
        //     {
        //         pttHotWorld = GetPTThotWords(inputHotwords);
        //     }
        //
        //     Debug.LogFormat("ptthotords: {0}", pttHotWorld);
        //     int ret = ITMGContext.GetInstance().SetAdvanceParams("SetPTTHotWorldList", pttHotWorld);
        //     if (ret != 0)
        //     {
        //         showWarningText(string.Format("Set pttHotWords Fail!, ret = {0}", ret));
        //     }
        // }

        ITMGContext.GetInstance().GetPttCtrl().StartRecordingWithStreamingRecognition(recordPath, "cmn-Hans-CN");
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