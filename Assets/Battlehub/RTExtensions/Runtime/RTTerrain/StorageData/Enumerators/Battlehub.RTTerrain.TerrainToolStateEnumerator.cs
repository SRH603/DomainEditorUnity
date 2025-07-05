namespace Battlehub.Storage.Enumerators.Battlehub.RTTerrain
{
    [ObjectEnumerator(typeof(global::Battlehub.RTTerrain.TerrainToolState))]
    public class TerrainToolStateEnumerator : ObjectEnumerator<global::Battlehub.RTTerrain.TerrainToolState>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.CutoutTexture, 12))
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