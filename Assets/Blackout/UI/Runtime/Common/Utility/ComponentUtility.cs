using Blackout.Pool;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blackout.UI
{
    public static class ComponentUtility
    {
        /// <summary>
        /// This is a helper function that recreates the GetComponentInParent(bool inactive)
        /// function found in 2020.3+.
        /// This is for backwards compatability with earlier versions of Unity
        /// </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetComponentInParent<T>(GameObject gameObject) where T : Component
        {
            #if UNITY_2020_3_OR_NEWER
            return gameObject.GetComponentInParent<T>(true);
            #else
            List<T> list = ListPool<T>.Get();
            
            gameObject.GetComponentsInParent<T>(true, list);
            
            T result = list.Count > 0 ? list[0] : null;
            
            ListPool<T>.Release(list);

            return result;
            #endif
        }

        /// <summary>
        /// This is a helper function that recreates the Object.FindObjectOfType(bool inactive)
        /// function found in 2020.1+.
        /// This is for backwards compatability with earlier versions of Unity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindSceneComponentOfType<T>() where T : Component
        {
            #if UNITY_2020_1_OR_NEWER
            return Object.FindObjectOfType<T>(true);
            #else
            return Resources.FindObjectsOfTypeAll<Transform>()
                .Select(t => t.GetComponent<T>())
                .FirstOrDefault(c => c != null);
            #endif
        }
    }
}