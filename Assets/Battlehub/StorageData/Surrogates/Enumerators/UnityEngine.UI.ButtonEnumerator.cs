namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.Button))]
    public class ButtonEnumerator : ObjectEnumerator<global::UnityEngine.UI.Button>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.onClick, 3))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.navigation, 4))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.spriteState, 7))
                            return true;
                        break;
                    case 3:
                        if (MoveNext(TypedObject.animationTriggers, 8))
                            return true;
                        break;
                    case 4:
                        if (MoveNext(TypedObject.targetGraphic, 9))
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