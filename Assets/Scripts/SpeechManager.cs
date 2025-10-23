using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用于管理录音按钮的类。
///
/// 实现按住按钮录音功能，并更新按钮文字显示。
/// </summary>
public class SpeechManager : MonoBehaviour
{
    public WhisperSpeechToText whisperSpeechToText;
    private TMP_Text recordingState;

    private void Start()
    {
        recordingState = GetComponentInChildren<TMP_Text>();
    }

    public void OnClick()
    {
        if (whisperSpeechToText.IsRecording())
        {
            recordingState.text = "Speak";
            whisperSpeechToText.StopRecording();
        }
        else
        {
            recordingState.text = "Speaking";
            whisperSpeechToText.StartRecording();
        }
    }
}
