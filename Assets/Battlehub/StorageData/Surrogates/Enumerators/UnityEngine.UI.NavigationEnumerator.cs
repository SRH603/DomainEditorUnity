namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.Navigation))]
    public class NavigationEnumerator : ObjectEnumerator<global::UnityEngine.UI.Navigation>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.selectOnUp, 3))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.selectOnDown, 4))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.selectOnLeft, 5))
                            return true;
                        break;
                    case 3:
                        if (MoveNext(TypedObject.selectOnRight, 6))
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