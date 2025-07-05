using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    /// <summary>
    /// Here for compatibility
    /// </summary>
    //[Obsolete]
    [DefaultExecutionOrder(-1)]
    public class PlayerPrefsLegacyStorageModel : MonoBehaviour, IPlayerPrefsStorage
    {
        private IRuntimeEditor m_editor;

        private void Awake()
        {
            if (IOC.Resolve<IPlayerPrefsStorage>() == null)
            {
                IOC.RegisterFallback<IPlayerPrefsStorage>(this);
            }

            m_editor = IOC.Resolve<IRuntimeEditor>();
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IPlayerPrefsStorage>(this);
            m_editor = null;
        }

        public ProjectAsyncOperation<T> GetValue<T>(string key, ProjectEventHandler<T> callback = null)
        {
            var ao = new ProjectAsyncOperation<T>();

            m_editor.GetValueAsync<T>(key).ContinueWith(t =>
            {
                ao.Result = t.IsFaulted ? t.Result : default;
                ao.Error = t.IsFaulted ?
                    new Error(Error.E_Failed) { ErrorText = t.Exception?.Message } :
                    Error.NoError;

                callback?.Invoke(ao.Error, ao.Result);
                ao.IsCompleted = true;
            });

            return ao;  
        }

        public ProjectAsyncOperation SetValue<T>(string key, T obj, ProjectEventHandler callback = null)
        {
            var ao = new ProjectAsyncOperation();

            m_editor.SetValueAsync(key, obj).ContinueWith(t =>
            {
                ao.Error = t.IsFaulted ?
                    new Error(Error.E_Failed) { ErrorText = t.Exception?.Message } :
                    Error.NoError;

                callback?.Invoke(ao.Error);
                ao.IsCompleted = true;
            });

            return ao;
        }

        public ProjectAsyncOperation DeleteValue<T>(string key, ProjectEventHandler callback = null)
        {
            var ao = new ProjectAsyncOperation();

            m_editor.DeleteValueAsync<T>(key).ContinueWith(t =>
            {
                ao.Error = t.IsFaulted ?
                    new Error(Error.E_Failed) { ErrorText = t.Exception?.Message } :
                    Error.NoError;

                callback?.Invoke(ao.Error);
                ao.IsCompleted = true;
            });

            return ao;
        }
    }
}
