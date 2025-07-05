namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.Dropdown))]
    public class DropdownEnumerator : ObjectEnumerator<global::UnityEngine.UI.Dropdown>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.captionText, 4))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.captionImage, 5))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.itemText, 6))
                            return true;
                        break;
                    case 3:
                        if (MoveNext(TypedObject.itemImage, 7))
                            return true;
                        break;
                    case 4:
                        if (MoveNext(TypedObject.options, 8))
                            return true;
                        break;
                    case 5:
                        if (MoveNext(TypedObject.onValueChanged, 9))
                            return true;
                        break;
                    case 6:
                        if (MoveNext(TypedObject.navigation, 12))
                            return true;
                        break;
                    case 7:
                        if (MoveNext(TypedObject.spriteState, 15))
                            return true;
                        break;
                    case 8:
                        if (MoveNext(TypedObject.animationTriggers, 16))
                            return true;
                        break;
                    case 9:
                        if (MoveNext(TypedObject.targetGraphic, 17))
                            return true;
                        break;
                    case 10:
                        if (MoveNext(TypedObject.image, 19))
                            return true;
                        break;
                    case 11:
                        if (MoveNext(TypedObject.template, 21))
                            return true;
                        break;
                    case 12:
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