namespace Battlehub.Storage.Enumerators.Battlehub.ProBuilderIntegration
{
    [ObjectEnumerator(typeof(global::Battlehub.ProBuilderIntegration.PBMesh))]
    public class PBMeshEnumerator : ObjectEnumerator<global::Battlehub.ProBuilderIntegration.PBMesh>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {
                    case 0:
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