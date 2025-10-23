using OpenAIGPT;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    // ユーザーの入力を受け取るためのInputField
    [SerializeField]
    private InputField inputField;
    // チャットメッセージを表示するコンテンツエリア
    [SerializeField]
    private GameObject content_obj;
    // チャットメッセージのプレハブオブジェクト
    [SerializeField]
    private GameObject chat_obj;
    [SerializeField]
    private ChatGPTConnection _chatGptConnection;

    // 送信ボタンが押されたときに呼び出されるメソッド
    public void OnClick()
    {
        // InputFieldからテキストを取得
        var text = inputField.GetComponent<InputField>().text;
        // メッセージを送信
        SendMessageToGPT(text);
        // InputFieldをクリア
        inputField.GetComponent<InputField>().text = "";
    }

    // メッセージを送信し、応答を取得する非同期メソッド
    private void SendMessageToGPT(string text)
    {
        var responseObj = Instantiate(chat_obj, content_obj.transform);
        responseObj.GetComponent<Text>().text = text;
        _chatGptConnection.UserSendMessageToGPT(text);
    }

    public void ReceiveMessageFromGPT(string text)
    {
        var responseObj = Instantiate(chat_obj, content_obj.transform);
        responseObj.GetComponent<Text>().text = text;
    }
}
