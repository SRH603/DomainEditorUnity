namespace Battlehub.Storage.Enumerators.Battlehub.RTTerrain
{
    [ObjectEnumerator(typeof(global::Battlehub.RTTerrain.TerrainBrushSource))]
    public class TerrainBrushSourceEnumerator : ObjectEnumerator<global::Battlehub.RTTerrain.TerrainBrushSource>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.UserBrushes, 3))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.UserTextures, 4))
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