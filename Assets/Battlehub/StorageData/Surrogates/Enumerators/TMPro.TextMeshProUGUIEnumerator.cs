namespace Battlehub.Storage.Enumerators.TMPro
{
    [ObjectEnumerator(typeof(global::TMPro.TextMeshProUGUI))]
    public class TextMeshProUGUIEnumerator : ObjectEnumerator<global::TMPro.TextMeshProUGUI>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {
                    case 0:
                        if (MoveNext(TypedObject.font, 8))
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