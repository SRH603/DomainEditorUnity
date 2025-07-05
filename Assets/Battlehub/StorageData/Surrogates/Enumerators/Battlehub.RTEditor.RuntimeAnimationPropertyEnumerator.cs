namespace Battlehub.Storage.Enumerators.Battlehub.RTEditor
{
    [ObjectEnumerator(typeof(global::Battlehub.RTEditor.RuntimeAnimationProperty))]
    public class RuntimeAnimationPropertyEnumerator : ObjectEnumerator<global::Battlehub.RTEditor.RuntimeAnimationProperty>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {
                    case 0:
                        if (MoveNext(TypedObject.Children, 3))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.Curve, 4))
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