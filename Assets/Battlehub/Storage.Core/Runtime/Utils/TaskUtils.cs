#if UNITY_WEBGL
#define SINGLE_THREADED
#endif 

using System;
using System.Threading.Tasks;

namespace Battlehub.Storage
{
    public static class TaskUtils 
    {
        public static Task Run(Action action)
        {
#if SINGLE_THREADED
            action();
            return Task.CompletedTask;
#else
            return Task.Run(action);
#endif
        }

        public static Task Run(Func<Task> action)
        {
#if SINGLE_THREADED
            return action();
#else
            return Task.Run(action);
#endif
        }

        public static Task<TResult> Run<TResult>(Func<TResult> function)
        {
#if SINGLE_THREADED
            var result = function();
            return Task.FromResult(result);
#else
            return Task.Run(function);
#endif
        }

        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
#if SINGLE_THREADED
            return function();
#else
            return Task.Run(function);
#endif
        }
    }
}
