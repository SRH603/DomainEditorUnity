using System.Linq;

namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(global::UnityEngine.TerrainData))]
    public class TerrainDataEnumerator : ObjectEnumerator<global::UnityEngine.TerrainData>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {
                    case 0:
                        if (MoveNext(TypedObject.detailPrototypes, 10))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.treePrototypes.Where(p => p != null).Select(p => p.prefab).ToArray(), 12))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(TypedObject.terrainLayers, 15))
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