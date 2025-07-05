namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(global::UnityEngine.SkinnedMeshRenderer))]
    public class SkinnedMeshRendererEnumerator : ObjectEnumerator<global::UnityEngine.SkinnedMeshRenderer>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.bones, 8))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.sharedMesh, 9))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.sharedMaterials, 35))
                            return true;
                        break;
                    case 3:
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