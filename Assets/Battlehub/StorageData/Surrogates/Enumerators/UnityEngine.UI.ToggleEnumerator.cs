namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.Toggle))]
    public class ToggleEnumerator : ObjectEnumerator<global::UnityEngine.UI.Toggle>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.graphic, 4))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.onValueChanged, 5))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.group, 6))
                            return true;
                        break;
                    case 3:
                        if (MoveNext(TypedObject.navigation, 8))
                            return true;
                        break;
                    case 4:
                        if (MoveNext(TypedObject.spriteState, 11))
                            return true;
                        break;
                    case 5:
                        if (MoveNext(TypedObject.animationTriggers, 12))
                            return true;
                        break;
                    case 6:
                        if (MoveNext(TypedObject.targetGraphic, 13))
                            return true;
                        break;
                    case 7:
                        if (MoveNext(TypedObject.image, 15))
                            return true;
                        break;
                    case 8:
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