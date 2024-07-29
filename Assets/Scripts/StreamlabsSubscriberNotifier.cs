using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class StreamlabsSubscriberNotifier : MonoBehaviour
{
    [Header("Streamlabs API Settings")]
    public string clientId;
    public string clientSecret;
    public string redirectUri;
    public string authorizationCode;

    [Header("Animation Settings")]
    public Animator animator;
    public string animationTriggerName = "NewSubscriber";

    private string accessToken;
    private string refreshToken;

    void Start()
    {
        StartCoroutine(GetAccessToken());
    }

    IEnumerator GetAccessToken()
    {
        WWWForm form = new WWWForm();
        form.AddField("grant_type", "authorization_code");
        form.AddField("client_id", clientId);
        form.AddField("client_secret", clientSecret);
        form.AddField("redirect_uri", redirectUri);
        form.AddField("code", authorizationCode);

        using (UnityWebRequest www = UnityWebRequest.Post("https://streamlabs.com/api/v2.0/token", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                var jsonResponse = JsonUtility.FromJson<TokenResponse>(www.downloadHandler.text);
                accessToken = jsonResponse.access_token;
                refreshToken = jsonResponse.refresh_token;
                StartCoroutine(GetSubscriberNotification());
            }
        }
    }

    IEnumerator GetSubscriberNotification()
    {
        while (true)
        {
            using (UnityWebRequest www = UnityWebRequest.Get("https://streamlabs.com/api/v2.0/alerts?access_token=" + accessToken))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    var alerts = JsonUtility.FromJson<AlertsResponse>(www.downloadHandler.text);
                    foreach (var alert in alerts.alerts)
                    {
                        if (alert.type == "subscription")
                        {
                            TriggerAnimation(alert);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(30); // 30秒ごとにチェック
        }
    }

    void TriggerAnimation(Alert alert)
    {
        // ここでアニメーションをトリガーするコードを追加
        Debug.Log("New Subscriber: " + alert.message);
        // アニメーションコントローラーを使ってアニメーションを再生
        if (animator != null)
        {
            animator.SetTrigger(animationTriggerName);
        }
    }

    [System.Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string refresh_token;
    }

    [System.Serializable]
    public class AlertsResponse
    {
        public Alert[] alerts;
    }

    [System.Serializable]
    public class Alert
    {
        public string type;
        public string message;
    }
}
