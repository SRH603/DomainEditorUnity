namespace Battlehub.Storage.Enumerators.Battlehub.RTCommon
{
    [ObjectEnumerator(typeof(global::Battlehub.RTCommon.ExposeToEditor))]
    public class ExposeToEditorEnumerator : ObjectEnumerator<global::Battlehub.RTCommon.ExposeToEditor>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {
                    case 0:
                        if (MoveNext(TypedObject.BoundsObject, 4))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.Colliders, 8))
                            return true;
                        break;
                    case 2:
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