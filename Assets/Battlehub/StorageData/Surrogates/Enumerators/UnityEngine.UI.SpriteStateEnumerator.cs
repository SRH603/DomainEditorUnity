namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.SpriteState))]
    public class SpriteStateEnumerator : ObjectEnumerator<global::UnityEngine.UI.SpriteState>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.highlightedSprite, 1))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.pressedSprite, 2))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.selectedSprite, 3))
                            return true;
                        break;
                    case 3:
                        if (MoveNext(TypedObject.disabledSprite, 4))
                            return true;
                        break;
                    case 4:
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