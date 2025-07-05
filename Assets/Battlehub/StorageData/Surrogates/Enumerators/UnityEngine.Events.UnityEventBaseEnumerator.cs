using Battlehub.Storage.UnityExtensions;
using System.Collections.Generic;

namespace Battlehub.Storage.Enumerators.UnityEngine.Events
{
    public class UnityEventBaseEnumerator : ObjectEnumerator<global::UnityEngine.Events.UnityEventBase>
    {
        protected override IEnumerator<(object, int)> GetNext()
        {
            var calls = TypedObject.GetPersistentCalls();
            if (calls != null)
            {
                for (int i = 0; i < calls.Length; ++i)
                {
                    var call = calls[i];
                    if (call == null)
                    {
                        continue;
                    }

                    var cache = call?.ArgumentsCache;
                    var arg = cache?.ObjectArgument;
                    if (arg != null)
                    {
                        yield return (arg, i << 16);
                    }
                    
                    var target = call.Target;
                    if (target != null)
                    {
                        yield return (target, i);
                    }
                }
            }

            yield return (Object, -1);
        }
    }
}