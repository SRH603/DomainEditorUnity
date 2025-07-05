namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.RawImage))]
    public class RawImageEnumerator : ObjectEnumerator<global::UnityEngine.UI.RawImage>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.texture, 3))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.material, 11))
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