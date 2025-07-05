using Battlehub.RTEditor;

namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(RuntimeAnimationClip))]
    public class RuntimeAnimationClipEnumerator : ObjectEnumerator<RuntimeAnimationClip>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {
                    case 0:
                        if (MoveNext(TypedObject.Properties, 4))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(Object, -1))
                            return true;
                        break;
                    default:
                        return false;
                }
            }
            while (true);
        }
    }
}