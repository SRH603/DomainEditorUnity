namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(global::UnityEngine.TerrainCollider))]
    public class TerrainColliderEnumerator : ObjectEnumerator<global::UnityEngine.TerrainCollider>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.terrainData, 4))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.sharedMaterial, 8))
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