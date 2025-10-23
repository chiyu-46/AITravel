using System;
using UnityEngine;
using GME;

/// <summary>
/// GME引擎轮询帮助类。
/// 用于自动触发GME系统回调、自动化GME的暂停、恢复、反初始化。
/// </summary>
/// <remarks>此类不应直接推荐到 GameObject 。应在运行时调用 CreateEnginePollHelper() 创建带有此类的 GameObject 。</remarks>
public class EnginePollHelper : MonoBehaviour
{
    public void Awake()
    {
        // 设置脚本所在 GameObject 在场景切换时不销毁。
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 创建一个带有此类的 GameObject ，并开启 GME 事件轮询。
    /// </summary>
    /// <returns>创建的此类实例。</returns>
    public static EnginePollHelper CreateEnginePollHelper()
    {
        GameObject obj = new GameObject("EnginePollHelper");
        obj.hideFlags = HideFlags.HideAndDontSave;

        DontDestroyOnLoad(obj);
        EnginePollHelper instance = obj.AddComponent<EnginePollHelper>();

        UnityEngine.Debug.LogFormat("CreateEnginePollHelper:{0},{1}", obj.GetInstanceID(), instance);
        return instance;
    }

    /// <summary>
    /// 销毁 helper 及其所在 GameObject 。
    /// </summary>
    /// <param name="helper">要销毁的此类实例。</param>
    /// <returns>销毁是否成功。</returns>
    public static bool DestroyEnginePollHelper(EnginePollHelper helper)
    {
        if (helper)
        {
            if (helper.gameObject)
            {
                Destroy(helper.gameObject);
            }

            Destroy(helper);

            UnityEngine.Debug.LogFormat("DestroyEnginePollHelper:{0},{1}", helper.gameObject, helper);
        }

        return true;
    }

    public virtual void Update()
    {
        // 开启 GME 事件轮询。
        QAVNative.QAVSDK_Poll();
    }


    void OnApplicationQuit()
    {
        // 在应用退出时，自动化 GME 去初始化。
        Debug.Log(string.Format("OnApplicationQuit"));
        ITMGContext.GetInstance().Uninit();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // 在应用焦点变化时，自动化 GME 暂停、继续。
        Debug.Log(string.Format("OnApplicationFocus {0}", hasFocus));
        if (hasFocus)
        {
            ITMGContext.GetInstance().Resume();
        }
        else
        {
            ITMGContext.GetInstance().Pause();
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log(string.Format("OnApplicationPause {0}", pauseStatus));

        if (pauseStatus)
        {
            ITMGContext.GetInstance().Pause();
        }
        else
        {
            ITMGContext.GetInstance().Resume();
        }
    }
}