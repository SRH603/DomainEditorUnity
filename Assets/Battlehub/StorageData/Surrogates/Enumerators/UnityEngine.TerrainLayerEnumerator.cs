namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(global::UnityEngine.TerrainLayer))]
    public class TerrainLayerEnumerator : ObjectEnumerator<global::UnityEngine.TerrainLayer>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.diffuseTexture, 3))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.normalMapTexture, 4))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.maskMapTexture, 5))
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