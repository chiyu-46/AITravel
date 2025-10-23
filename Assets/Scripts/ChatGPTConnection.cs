using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace OpenAIGPT
{
    // OpenAI GPTとの接続を管理するクラス
    [Serializable]
    public class ChatGPTConnection : MonoBehaviour
    {
        // OpenAI APIキー
        [SerializeField] private string _apiKey;

        /// <summary>
        /// 用户与AI对话的列表
        /// </summary>
        [SerializeField] private List<BaseMessageModel> _currentMessageList = new();

        /// <summary>
        /// 正在进行的方法调用的数量。如果AI发起方法调用，此值+1。每个方法调用完成，此值-1。
        /// 此值大于零时，用户的消息应进入等待列表，而不是直接交由AI处理。
        /// </summary>
        private int _toolCallCount;

        /// <summary>
        /// 当用户发送的信息不应立即处理时，将存储于此列表进行等待。
        /// </summary>
        private List<BaseMessageModel> _waitingMessageList = new();

        private List<Tool> tools;

        [SerializeField] private AMapAPI _mapAPI;

        [SerializeField] private UnityEvent<string> _onReceivedMessage;

        [SerializeField] private bool _playAudio;

        private AudioSource _audioSource;

        private void Awake()
        {
            _toolCallCount = 0;
            _currentMessageList.Add(new ChatGPTMessageModel()
                { role = "system", content = "你是一位旅行类应用的助手，你可以使用提供的工具，帮助用户确定所在位置、搜索周边地点信息、进行步行路线规划。" });
            tools = new List<Tool>
            {
                new Tool
                {
                    type = "function",
                    function = new Tool.Function()
                    {
                        name = "GetAddressUsingLatitudeAndLongitude",
                        description = "通过设备所在经纬度，获取设备所在的地址。",
                        parameters = new Tool.Parameter
                        {
                            type = "object",
                            properties = new Dictionary<string, Tool.Property>(),
                            required = new List<string>()
                        },
                    }
                },
                new Tool
                {
                    type = "function",
                    function = new Tool.Function()
                    {
                        name = "SearchAroundPlace",
                        description =
                            "搜索用户（设备）周围的场所(或地点)。返回值为一个列表，按照地点与用户的距离排序检索到的地点信息。每条地点信息包括地点ID（id）、地点名称（name）、地点经纬度（location）。地点ID和地点经纬度对用户来说没有意义，请不要在对用户的对话中使用它们，这两个参数仅用于规划步行路线的方法。",
                        parameters = new Tool.Parameter
                        {
                            type = "object",
                            properties = new Dictionary<string, Tool.Property>
                            {
                                ["types"] = new Tool.Property
                                {
                                    type = "string",
                                    description =
                                        @"要检索的地点的类型。这是一个数字编号，你需要根据用户的意图，从下面的表格中找到合适的编号。如果需要同时使用多个类型，请将这些类型用“|”分隔。
TYPE	大类	中类	小类
010100	汽车服务	加油站	加油站
050100	餐饮服务	中餐厅	中餐厅
050101	餐饮服务	中餐厅	综合酒楼
050102	餐饮服务	中餐厅	四川菜(川菜)
050103	餐饮服务	中餐厅	广东菜(粤菜)
050104	餐饮服务	中餐厅	山东菜(鲁菜)
050105	餐饮服务	中餐厅	江苏菜
050106	餐饮服务	中餐厅	浙江菜
050107	餐饮服务	中餐厅	上海菜
050108	餐饮服务	中餐厅	湖南菜(湘菜)
050109	餐饮服务	中餐厅	安徽菜(徽菜)
050110	餐饮服务	中餐厅	福建菜
050111	餐饮服务	中餐厅	北京菜
050112	餐饮服务	中餐厅	湖北菜(鄂菜)
050113	餐饮服务	中餐厅	东北菜
050114	餐饮服务	中餐厅	云贵菜
050115	餐饮服务	中餐厅	西北菜
050116	餐饮服务	中餐厅	老字号
050117	餐饮服务	中餐厅	火锅店
050118	餐饮服务	中餐厅	特色/地方风味餐厅
050119	餐饮服务	中餐厅	海鲜酒楼
050120	餐饮服务	中餐厅	中式素菜馆
050121	餐饮服务	中餐厅	清真菜馆
050122	餐饮服务	中餐厅	台湾菜
050123	餐饮服务	中餐厅	潮州菜
050200	餐饮服务	外国餐厅	外国餐厅
050201	餐饮服务	外国餐厅	西餐厅(综合风味)
050202	餐饮服务	外国餐厅	日本料理
050203	餐饮服务	外国餐厅	韩国料理
050204	餐饮服务	外国餐厅	法式菜品餐厅
050205	餐饮服务	外国餐厅	意式菜品餐厅
050206	餐饮服务	外国餐厅	泰国/越南菜品餐厅
050207	餐饮服务	外国餐厅	地中海风格菜品
050208	餐饮服务	外国餐厅	美式风味
050209	餐饮服务	外国餐厅	印度风味
050210	餐饮服务	外国餐厅	英国式菜品餐厅
050211	餐饮服务	外国餐厅	牛扒店(扒房)
050212	餐饮服务	外国餐厅	俄国菜
050213	餐饮服务	外国餐厅	葡国菜
050214	餐饮服务	外国餐厅	德国菜
050215	餐饮服务	外国餐厅	巴西菜
050216	餐饮服务	外国餐厅	墨西哥菜
050217	餐饮服务	外国餐厅	其它亚洲菜
050300	餐饮服务	快餐厅	快餐厅
050301	餐饮服务	快餐厅	肯德基
050302	餐饮服务	快餐厅	麦当劳
050303	餐饮服务	快餐厅	必胜客
050304	餐饮服务	快餐厅	永和豆浆
050305	餐饮服务	快餐厅	茶餐厅
050306	餐饮服务	快餐厅	大家乐
050307	餐饮服务	快餐厅	大快活
050308	餐饮服务	快餐厅	美心
050309	餐饮服务	快餐厅	吉野家
050310	餐饮服务	快餐厅	仙跡岩
050311	餐饮服务	快餐厅	呷哺呷哺
050400	餐饮服务	休闲餐饮场所	休闲餐饮场所
050500	餐饮服务	咖啡厅	咖啡厅
050501	餐饮服务	咖啡厅	星巴克咖啡
050502	餐饮服务	咖啡厅	上岛咖啡
050503	餐饮服务	咖啡厅	Pacific Coffee Company
050504	餐饮服务	咖啡厅	巴黎咖啡店
050600	餐饮服务	茶艺馆	茶艺馆
050700	餐饮服务	冷饮店	冷饮店
050800	餐饮服务	糕饼店	糕饼店
050900	餐饮服务	甜品店	甜品店
060000	购物服务	购物相关场所	购物相关场所
060100	购物服务	商场	商场
060101	购物服务	商场	购物中心
060102	购物服务	商场	普通商场
060103	购物服务	商场	免税品店
060400	购物服务	超级市场	超市
100101	住宿服务	宾馆酒店	奢华酒店
100102	住宿服务	宾馆酒店	五星级宾馆
100103	住宿服务	宾馆酒店	四星级宾馆
100104	住宿服务	宾馆酒店	三星级宾馆
100105	住宿服务	宾馆酒店	经济型连锁酒店
110100	风景名胜	公园广场	公园广场
110101	风景名胜	公园广场	公园
110102	风景名胜	公园广场	动物园
110103	风景名胜	公园广场	植物园
110104	风景名胜	公园广场	水族馆
110105	风景名胜	公园广场	城市广场
110106	风景名胜	公园广场	公园内部设施
110200	风景名胜	风景名胜	风景名胜
110201	风景名胜	风景名胜	世界遗产
110202	风景名胜	风景名胜	国家级景点
110203	风景名胜	风景名胜	省级景点
110204	风景名胜	风景名胜	纪念馆
110205	风景名胜	风景名胜	寺庙道观
110206	风景名胜	风景名胜	教堂
110207	风景名胜	风景名胜	回教寺
110208	风景名胜	风景名胜	海滩
110209	风景名胜	风景名胜	观景点
110210	风景名胜	风景名胜	红色景区",
                                },
                                ["keywords"] = new Tool.Property
                                {
                                    type = "string",
                                    description = @"如果用户指定了要检索的地点名称，则将地点名称传入。",
                                }
                            },
                            required = new List<string> { "types" }
                        },
                    }
                },
                new Tool
                {
                    type = "function",
                    function = new Tool.Function()
                    {
                        name = "PlanningWalkingPaths",
                        description = "规划用户从用户所在位置前往目的地的步行路线。返回一个列表，列表中依次记录了用户前往目的地应进行的步骤。",
                        parameters = new Tool.Parameter
                        {
                            type = "object",
                            properties = new Dictionary<string, Tool.Property>
                            {
                                ["destination"] = new Tool.Property
                                {
                                    type = "string",
                                    description = @"目的地的经纬度。此值通过“SearchAroundPlace”方法获取。"
                                },
                                ["destination_id"] = new Tool.Property
                                {
                                    type = "string",
                                    description = @"目的地的ID。此值通过“SearchAroundPlace”方法获取。",
                                }
                            },
                            required = new List<string> { "destination", "destination_id" }
                        },
                    }
                },
            };
        }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_onReceivedMessage == null)
            {
                _onReceivedMessage = new UnityEvent<string>();
            }
        }

        [Serializable]
        public class BaseMessageModel
        {
        }

        /// <summary>
        /// Message Model
        /// </summary>
        [Serializable]
        public class ChatGPTMessageModel : BaseMessageModel
        {
            [Serializable]
            public class Function
            {
                public string name;
                public string arguments;
            }

            /// <summary>
            /// 如果AI响应为方法调用，此类为方法调用Model
            /// </summary>
            [Serializable]
            public class ToolCall
            {
                public string id;
                public string type;
                public Function function;
            }

            public string role;
            public string content;
            public string refusal;
            public List<ToolCall> tool_calls;
        }

        /// <summary>
        /// 方法调用完成后，向AI发送消息的模型
        /// </summary>
        [Serializable]
        public class CallbackMessageModel : BaseMessageModel
        {
            public string role;
            public string content;
            public string tool_call_id;
        }

        public void UserSendMessageToGPT(string userMessage)
        {
            // 如果正在处理方法调用，则等待方法调用完成，再处理用户消息。
            if (_toolCallCount > 0)
            {
                _waitingMessageList.Add(new ChatGPTMessageModel { role = "user", content = userMessage });
            }
            else
            {
                _currentMessageList.Add(new ChatGPTMessageModel { role = "user", content = userMessage });
                StartCoroutine(ChatWithGPT());
            }
        }

        // 用于发送 OpenAI GPT API 用户信息请求的异步方法。
        public IEnumerator ChatWithGPT()
        {
            // OpenAI GPT APIのエンドポイント
            var apiUrl = "https://aihubmix.com/v1/chat/completions";

            // OpenAI APIリクエストに必要なヘッダー情報
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _apiKey },
                { "Content-type", "application/json" }
            };

            // APIリクエストのオプション（モデルとメッセージリスト）
            var options = new ChatGPTCompletionRequestModel()
            {
                model = "gpt-4o",
                messages = _currentMessageList,
                tools = tools,
            };
            var jsonOptions = JsonConvert.SerializeObject(options);
            Debug.Log("将向AI发送以下信息：" + jsonOptions);
            // UnityWebRequestを使用してOpenAI GPT APIにリクエストを送信
            using var request = new UnityWebRequest(apiUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonOptions)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            // リクエストヘッダーを設定
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            // リクエストを送信し、応答を待機
            yield return request.SendWebRequest();

            // リクエストの結果を処理
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);
                throw new Exception();
            }
            else
            {
                var responseString = request.downloadHandler.text;
                Debug.Log("AI回复：" + responseString);
                var responseObject = JsonConvert.DeserializeObject<ChatGPTResponseModel>(responseString);
                _currentMessageList.Add(responseObject.choices[0].message);
                if (responseObject.choices[0].finish_reason == "tool_calls")
                {
                    foreach (var toolCall in responseObject.choices[0].message.tool_calls)
                    {
                        var tool_name = toolCall.function.name;
                        var tool_call_id = toolCall.id;
                        var arguments =
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(toolCall.function.arguments);
                        if (tool_name == "GetAddressUsingLatitudeAndLongitude")
                        {
                            StartCoroutine(_mapAPI.GetAddressUsingLatitudeAndLongitude(tool_call_id));
                        }
                        else if (tool_name == "SearchAroundPlace")
                        {
                            string keywords;
                            if (arguments.TryGetValue("keywords", out keywords))
                            {
                                StartCoroutine(_mapAPI.SearchAroundPlace(tool_call_id, arguments["types"],
                                                                keywords));
                            }
                            else
                            {
                                StartCoroutine(_mapAPI.SearchAroundPlace(tool_call_id, arguments["types"], ""));
                            }
                            
                        }
                        else if (tool_name == "PlanningWalkingPaths")
                        {
                            StartCoroutine(_mapAPI.PlanningWalkingPaths(tool_call_id, arguments["destination"],
                                arguments["destination_id"]));
                        }

                        _toolCallCount++;
                    }
                }
                else
                {
                    if (responseObject.choices[0].message.refusal is not null &&
                        responseObject.choices[0].message.refusal != "")
                    {
                        Debug.LogError("AI拒绝了请求：" + responseObject.choices[0].message.refusal);
                        _onReceivedMessage.Invoke(responseObject.choices[0].message.refusal);
                        if (_playAudio)
                        {
                            StartCoroutine(TextToSpeech(responseObject.choices[0].message.refusal));
                        }
                    }
                    else
                    {
                        Debug.Log("接收：" + responseObject.choices[0].message.content);
                        _onReceivedMessage.Invoke(responseObject.choices[0].message.content);
                        if (_playAudio)
                        {
                            StartCoroutine(TextToSpeech(responseObject.choices[0].message.content));
                        }

                        // 处理等待中的用户消息
                        if (_toolCallCount == 0 && _waitingMessageList.Count > 0)
                        {
                            _currentMessageList.AddRange(_waitingMessageList);
                        }
                    }
                }
            }
        }

        public void FunctionCallback(string tool_call_id, string result)
        {
            _toolCallCount--;
            _currentMessageList.Add(new CallbackMessageModel
                { role = "tool", content = result, tool_call_id = tool_call_id });
            if (_toolCallCount == 0)
            {
                StartCoroutine(ChatWithGPT());
            }
        }

        public IEnumerator TextToSpeech(string text)
        {
            // OpenAI GPT APIのエンドポイント
            var apiUrl = "https://aihubmix.com/v1/audio/speech";

            // OpenAI APIリクエストに必要なヘッダー情報
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _apiKey },
                { "Content-type", "application/json" }
            };

            // APIリクエストのオプション（モデルとメッセージリスト）
            var options = new ChatGPTSpeechRequestModel()
            {
                model = "tts-1-hd",
                input = text,
                voice = "coral",
            };
            var jsonOptions = JsonUtility.ToJson(options);

            // UnityWebRequestを使用してOpenAI GPT APIにリクエストを送信
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(apiUrl, AudioType.MPEG))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonOptions));
                // リクエストヘッダーを設定
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }

                Debug.Log("开始请求OpenAI TTS");
                // リクエストを送信し、応答を待機
                yield return request.SendWebRequest();
                Debug.Log("请求OpenAI TTS结束");
                // リクエストの結果を処理
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    throw new Exception();
                }
                else
                {
                    var clip = ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;
                    if (clip != null)
                    {
                        _audioSource.clip = clip;
                        _audioSource.Play();
                    }

                    yield break;
                }
            }

            ;
        }

        /// <summary>
        /// 要发送给OpenAI API 的可以调用的工具的信息。
        /// </summary>
        [Serializable]
        public class Tool
        {
            [Serializable]
            public class Property
            {
                public string type;
                public string description;
                // public List<string> @enum;
            }

            [Serializable]
            public class Parameter
            {
                public string type;
                public Dictionary<string, Property> properties;
                public List<string> required;
            }

            [Serializable]
            public class Function
            {
                public string name;
                public string description;
                public Parameter parameters;
            }

            public string type;
            public Function function;
        }

        /// <summary>
        /// OpenAI chat completions API 的请求 Model
        /// </summary>
        [Serializable]
        public class ChatGPTCompletionRequestModel
        {
            public string model; // 使用するモデル名
            public List<BaseMessageModel> messages; // メッセージリスト
            public List<Tool> tools;
        }

        /// <summary>
        /// OpenAI GPT API 文字转语音所需数据
        /// </summary>
        [Serializable]
        public class ChatGPTSpeechRequestModel
        {
            public string model; // 使用するモデル名
            public string input;
            public string voice;
        }

        /// <summary>
        /// OpenAI chat completions API 的应答 Model
        /// </summary>
        [System.Serializable]
        public class ChatGPTResponseModel
        {
            public string id; // 応答のID
            public string @object; // オブジェクトタイプ
            public int created; // 作成タイムスタンプ
            public Choice[] choices; // 応答選択肢
            public Usage usage; // 使用量情報

            [System.Serializable]
            public class Choice
            {
                public int index; // 選択肢のインデックス
                public ChatGPTMessageModel message; // メッセージ内容
                public string finish_reason; // 終了理由
            }

            [System.Serializable]
            public class Usage
            {
                public int prompt_tokens; // プロンプトに使用されたトークン数
                public int completion_tokens; // 完成に使用されたトークン数
                public int total_tokens; // 合計使用トークン数
            }
        }
    }
}