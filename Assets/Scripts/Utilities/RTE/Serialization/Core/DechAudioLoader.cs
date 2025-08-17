using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class DechAudioLoader
{
    // 把任意格式的音频字节落到临时文件，再让 Unity 原生解码
    public static IEnumerator LoadFromBytes(byte[] audioBytes, string extNoDot, Action<AudioClip> onOk, Action<Exception> onErr)
    {
        string ext = (extNoDot ?? "wav").Trim().TrimStart('.').ToLowerInvariant();
        string tmp = Path.Combine(Application.temporaryCachePath, "dech_audio_" + Guid.NewGuid().ToString("N") + "." + ext);

        // 注意：不能在包含 yield 的 try 里写 catch，所以把可能抛异常的文件写入放在无 yield 的 try-catch 中处理
        try
        {
            File.WriteAllBytes(tmp, audioBytes);
        }
        catch (Exception ex)
        {
            onErr?.Invoke(ex);
            yield break;
        }

        // 下面这个 try 使用 finally（不使用 catch），从而可以在其中 yield
        try
        {
            using (var req = UnityWebRequestMultimedia.GetAudioClip("file://" + tmp, AudioType.UNKNOWN))
            {
#if UNITY_2020_2_OR_NEWER
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    onErr?.Invoke(new Exception(req.error));
                    yield break;
                }
#else
                yield return req.SendWebRequest();
                if (req.isNetworkError || req.isHttpError)
                {
                    onErr?.Invoke(new Exception(req.error));
                    yield break;
                }
#endif
                var clip = DownloadHandlerAudioClip.GetContent(req);
                onOk?.Invoke(clip);
            }
        }
        finally
        {
            try { File.Delete(tmp); } catch { }
        }
    }
}