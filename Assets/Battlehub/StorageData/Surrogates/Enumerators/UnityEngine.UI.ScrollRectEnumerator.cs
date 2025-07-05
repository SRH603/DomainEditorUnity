namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.ScrollRect))]
    public class ScrollRectEnumerator : ObjectEnumerator<global::UnityEngine.UI.ScrollRect>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.horizontalScrollbar, 11))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.verticalScrollbar, 12))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.onValueChanged, 17))
                            return true;
                        break;
                    case 3:
                        if (MoveNext(TypedObject.content, 23))
                            return true;
                        break;
                    case 4:
                        if (MoveNext(TypedObject.viewport, 24))
                            return true;
                        break;
                    case 5:
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