using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 实现录音转文字的类。
/// </summary>
public class WhisperSpeechToText : MonoBehaviour
{
    [SerializeField] private string openAIApiKey;
    // [SerializeField] private InputField _textInterface;
    [SerializeField] private UnityEvent<string> onTranscriptionComplete;

    public int frequency = 16000; 
    public int maxRecordingTime; // 録音最大時間

    private AudioClip clip;
    private float recordingTime;

    private void Start()
    {
        Microphone.GetDeviceCaps(null, out var minFreq, out var maxFreq);
        if (minFreq == 0 && maxFreq == 0)
        {
            frequency = 16000;
        }
        else
        {
            frequency = Mathf.Clamp(frequency, minFreq, maxFreq);
        }
    }

    void Update()
    {
        // レコーディング中であれば
        if (IsRecording())
        {
            recordingTime += Time.deltaTime;
            // レコーディング時間が超えていないことを確認する
            if (Mathf.FloorToInt(recordingTime) >= maxRecordingTime)
            {
                StopRecording();
            }
        }
    }

    public void StartRecording()
    {
        recordingTime = 0;
        // すでにレコーディング中であればレコーディングを止める
        if (IsRecording())
        {
            Microphone.End(null);
        }
        // マイクの録音を開始する
        Debug.Log("RecordingStart");
        clip = Microphone.Start(null, false, maxRecordingTime, frequency);
        // 録音が正しく開始されたかを確認
        if (clip == null)
        {
            Debug.LogError("Microphone recording failed.");
        }
    }

    public bool IsRecording()
    {
        return Microphone.IsRecording(null);
    }

    public void StopRecording()
    {
        Debug.Log("RecordingStop.");
        // マイクのレコーディングを止める
        Microphone.End(null);
        
        // AudioClipをWAV形式のバイナリデータに変換する
        var audioData = WavUtility.FromAudioClip(clip);

        // Send HTTP request to Whisper API
        StartCoroutine(SendRequest(audioData));
    }

    IEnumerator SendRequest(byte[] audioData)
    {
        Debug.Log("开始录音转文字");
        string url = "https://asr.tencentcloudapi.com/";

        // // フォームデータを作成する
        // var formData = new List<IMultipartFormSection>();
        // formData.Add(new MultipartFormDataSection("model", "whisper-large-v3"));
        // formData.Add(new MultipartFormDataSection("language", "zh"));
        // formData.Add(new MultipartFormFileSection("file", audioData, "audio.wav", "multipart/form-data"));

        // 请求头
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json; charset=utf-8" },
            { "X-TC-Version", "2019-06-14" },
            { "X-TC-Region", "ap-shanghai" },
            { "X-TC-Action", "SentenceRecognition" },
            { "X-TC-Timestamp", (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds.ToString(CultureInfo.InvariantCulture) },
            { "Authorization", "TC3-HMAC-SHA256 Credential=AKIDub3E8m9F5exzoOjslPmtslPpzuZzA7pH/2020-09-03/asr/tc3_request, SignedHeaders=content-type;host, Signature=aa5e2b8b16ced1ac9f877c9a92dab641dcd940837f5869a9e933688c181094de" }
        };

        // 请求体
        string requestBody = @"{
            ""UsrAudioKey"": ""test"",
            ""SubServiceType"": 2,
            ""ProjectId"": 0,
            ""EngSerViceType"": ""16k_zh"",
            ""VoiceFormat"": ""wav"",
            ""Data"": ""eGNmYXNkZmFzZmFzZGZhc2RmCg=="",
            ""SourceType"": 1
        }";
        
        // 创建UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // 添加请求头
        foreach (var header in headers)
        {
            request.SetRequestHeader(header.Key, header.Value);
        }

        // 发送请求
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            
            // 解析JSON响应
            ASRResponse response = JsonUtility.FromJson<ASRResponse>(request.downloadHandler.text);
            
            if (response != null && response.Response != null)
            {
                string result = response.Response.Result;
                Debug.Log("ASR Result: " + result);
                
                // 在这里可以使用result做进一步处理
                onTranscriptionComplete.Invoke(result);
            }
            else
            {
                Debug.LogError("Failed to parse response");
            }
        }
        else
        {
            Debug.LogError("Request failed: " + request.error);
        }
        
        // // UnityWebRequestを作成する
        // using (UnityWebRequest request = UnityWebRequest.Post(url, formData))
        // {
        //     // リクエストヘッダーを設定
        //     request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        //
        //     // リクエストを送信し、応答を待機
        //     yield return request.SendWebRequest();
        //
        //     // エラー処理
        //     if (request.result != UnityWebRequest.Result.Success)
        //     {
        //         Debug.LogError(request.error);
        //         yield break;
        //     }
        //
        //     // JSONデータのレスポンスをパースする
        //     string jsonResponse = request.downloadHandler.text;
        //     string recognizedText = "";
        //     try
        //     {
        //         recognizedText = JsonUtility.FromJson<WhisperResponseModel>(jsonResponse).text;
        //     }
        //     catch (System.Exception e)
        //     {
        //         Debug.LogError(e.Message);
        //     }
        //
        //     // 書き起こしされたテキストを出力する
        //     Debug.Log("录音转录完成: " + recognizedText);
        //     // _textInterface.text = recognizedText;
        //     onTranscriptionComplete.Invoke(recognizedText);
        // }
    }
}

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        // Write WAV header
        writer.Write(0x46464952); // "RIFF"
        writer.Write(0); // ChunkSize
        writer.Write(0x45564157); // "WAVE"
        writer.Write(0x20746d66); // "fmt "
        writer.Write(16); // Subchunk1Size
        writer.Write((ushort)1); // AudioFormat
        writer.Write((ushort)clip.channels); // NumChannels
        writer.Write(clip.frequency); // SampleRate
        writer.Write(clip.frequency * clip.channels * 2); // ByteRate
        writer.Write((ushort)(clip.channels * 2)); // BlockAlign
        writer.Write((ushort)16); // BitsPerSample
        writer.Write(0x61746164); // "data"
        writer.Write(0); // Subchunk2Size

        // Write audio data
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);
        short[] intData = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767f);
        }

        byte[] data = new byte[intData.Length * 2];
        Buffer.BlockCopy(intData, 0, data, 0, data.Length);
        writer.Write(data);

        // Update ChunkSize and Subchunk2Size fields
        writer.Seek(4, SeekOrigin.Begin);
        writer.Write((int)(stream.Length - 8));
        writer.Seek(40, SeekOrigin.Begin);
        writer.Write((int)(stream.Length - 44));

        // Close streams and return WAV data
        writer.Close();
        stream.Close();
        return stream.ToArray();
    }
}

public class WhisperResponseModel
{
    public string text;
}

[System.Serializable]
public class ASRResponse
{
    public ResponseData Response;
}

[System.Serializable]
public class ResponseData
{
    public string RequestId;
    public string Result;
    public int AudioDuration;
    public int WordSize;
    public WordData[] WordList;
}

[System.Serializable]
public class WordData
{
    public string Word;
    public int StartTime;
    public int EndTime;
}
