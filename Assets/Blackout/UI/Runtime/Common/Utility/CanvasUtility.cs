using Blackout.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace Blackout.UI
{
    public static class CanvasUtility
    {
        /// <summary>
        /// The the root Canvas of the specified GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static Canvas GetRootCanvas(GameObject gameObject)
        {
            List<Canvas> list = ListPool<Canvas>.Get();
            gameObject.GetComponentsInParent(false, list);
            if (list.Count == 0)
                return null;

            Canvas rootCanvas = list[list.Count - 1];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].isRootCanvas)
                {
                    rootCanvas = list[i];
                    break;
                }
            }

            ListPool<Canvas>.Release(list);

            return rootCanvas;
        }
    }
}