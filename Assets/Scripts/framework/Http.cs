
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BoBao.Framework
{
    public class Http : BaseMonoSingleton<Http>
    {

        public void Get(string url, Hashtable args, Action<string> cb, Action<string> failure, int timeout = 10)
        {
            string parameters = "";

            if (args != null && args.Count > 0)
            {
                bool first = true;

                parameters += "?";
                foreach (DictionaryEntry de in args)
                {
                    if (first)
                        first = false;
                    else
                        parameters += "&";

                    parameters += de.Key + "=" + de.Value.ToString();
                }
            }

            var path = url + parameters;
            StartCoroutine(doGet(path, cb, failure, timeout));
        }

        IEnumerator doGet(string url, Action<string> cb, Action<string> failure, int timeout = 10)
        {
            var request = UnityWebRequest.Get(url);
            request.timeout = timeout;
            yield return request.SendWebRequest();

            if (request.isNetworkError)
            {
                failure.Invoke(request.error);
            }
            else
            {
                cb.Invoke(request.downloadHandler.text);
            }
        }

        public void Post(string url, Hashtable args, Action<string> success, Action<string> failure, int timeout = 10)
        {
            byte[] bytes = System.Text.Encoding.Default.GetBytes(args.toJson());

            StartCoroutine(doPost(url, bytes, success, failure, timeout));
        }

        IEnumerator doPost(string url, byte[] bytes, Action<string> success, Action<string> failure, int timeout = 10)
        {
            var www = new UnityWebRequest(url, "POST");
            www.uploadHandler = new UploadHandlerRaw(bytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = timeout;
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                failure.Invoke(www.error);
            }
            else
            {
                success.Invoke(www.downloadHandler.text);
            }
        }
    }
}
