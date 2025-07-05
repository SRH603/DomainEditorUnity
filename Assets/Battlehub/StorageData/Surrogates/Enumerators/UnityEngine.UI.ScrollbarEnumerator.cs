namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.Scrollbar))]
    public class ScrollbarEnumerator : ObjectEnumerator<global::UnityEngine.UI.Scrollbar>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.onValueChanged, 8))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.navigation, 9))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.spriteState, 12))
                            return true;
                        break;
                    case 3:
                        if (MoveNext(TypedObject.animationTriggers, 13))
                            return true;
                        break;
                    case 4:
                        if (MoveNext(TypedObject.targetGraphic, 14))
                            return true;
                        break;
                    case 5:
                        if (MoveNext(TypedObject.image, 16))
                            return true;
                        break;
                    case 6:
                        if (MoveNext(TypedObject.handleRect, 18))
                            return true;
                        break;
                    case 7:
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