using System;
using UnityEngine;

namespace Battlehub.Utils
{
    public static class EventHandlerExtensions
    {
        public static void InvokeSafe(this EventHandler eventHandler, object sender, EventArgs args)
        {
            try
            {
                eventHandler?.Invoke(sender, args);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void InvokeSafe<T>(this EventHandler<T> eventHandler, object sender, T args)
        {
            try
            {
                eventHandler?.Invoke(sender, args);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}