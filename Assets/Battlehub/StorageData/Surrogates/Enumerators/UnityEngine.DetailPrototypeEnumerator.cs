namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(global::UnityEngine.DetailPrototype))]
    public class DetailPrototypeEnumerator : ObjectEnumerator<global::UnityEngine.DetailPrototype>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.prototype, 3))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.prototypeTexture, 4))
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