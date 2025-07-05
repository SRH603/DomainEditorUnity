namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(global::UnityEngine.Light))]
    public class LightEnumerator : ObjectEnumerator<global::UnityEngine.Light>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.flare, 23))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.cookie, 35))
                            return true;
                        break;
                    case 2:
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