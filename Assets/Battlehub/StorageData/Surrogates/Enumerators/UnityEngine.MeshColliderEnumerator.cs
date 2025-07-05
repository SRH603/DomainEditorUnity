namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(global::UnityEngine.MeshCollider))]
    public class MeshColliderEnumerator : ObjectEnumerator<global::UnityEngine.MeshCollider>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.sharedMesh, 4))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.sharedMaterial, 10))
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