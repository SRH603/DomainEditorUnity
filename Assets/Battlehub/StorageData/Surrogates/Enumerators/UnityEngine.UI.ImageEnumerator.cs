namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.Image))]
    public class ImageEnumerator : ObjectEnumerator<global::UnityEngine.UI.Image>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.sprite, 3))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.overrideSprite, 4))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.material, 15))
                            return true;
                        break;
                    case 3:
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