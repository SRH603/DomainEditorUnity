namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(global::UnityEngine.Collider))]
    public class ColliderEnumerator : ObjectEnumerator<global::UnityEngine.Collider>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.sharedMaterial, 7))
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