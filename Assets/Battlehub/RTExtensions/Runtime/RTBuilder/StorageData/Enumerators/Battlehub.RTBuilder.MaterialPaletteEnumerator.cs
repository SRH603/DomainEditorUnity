namespace Battlehub.Storage.Enumerators.Battlehub.RTBuilder
{
    [ObjectEnumerator(typeof(global::Battlehub.RTBuilder.MaterialPalette))]
    public class MaterialPaletteEnumerator : ObjectEnumerator<global::Battlehub.RTBuilder.MaterialPalette>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.Materials, 3))
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