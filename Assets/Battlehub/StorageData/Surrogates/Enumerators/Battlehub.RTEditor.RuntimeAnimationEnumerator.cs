namespace Battlehub.Storage.Enumerators.Battlehub.RTEditor
{
    [ObjectEnumerator(typeof(global::Battlehub.RTEditor.RuntimeAnimation))]
    public class RuntimeAnimationEnumerator : ObjectEnumerator<global::Battlehub.RTEditor.RuntimeAnimation>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.Clips, 6))
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