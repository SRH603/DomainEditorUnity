namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(global::UnityEngine.Terrain))]
    public class TerrainEnumerator : ObjectEnumerator<global::UnityEngine.Terrain>
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
                        if (MoveNext(TypedObject.materialTemplate, 21))
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