namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.Selectable))]
    public class SelectableEnumerator : ObjectEnumerator<global::UnityEngine.UI.Selectable>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.navigation, 3))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.spriteState, 6))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.animationTriggers, 7))
                            return true;
                        break;
                    case 3:
                        if (MoveNext(TypedObject.targetGraphic, 8))
                            return true;
                        break;
                    case 4:
                        if (MoveNext(TypedObject.image, 10))
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