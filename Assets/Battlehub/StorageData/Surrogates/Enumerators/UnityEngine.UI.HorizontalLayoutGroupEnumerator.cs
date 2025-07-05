namespace Battlehub.Storage.Enumerators.UnityEngine.UI
{
    [ObjectEnumerator(typeof(global::UnityEngine.UI.HorizontalLayoutGroup))]
    public class HorizontalLayoutGroupEnumerator : ObjectEnumerator<global::UnityEngine.UI.HorizontalLayoutGroup>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.padding, 11))
                            return true;
                        break;
                    case 1:
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