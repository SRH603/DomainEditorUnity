using Battlehub.RTEditor.Models;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(ImportAsset), typeof(ImportAsset.ImportStatus))]
    public class ImportAssetToImportStatusAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            ImportAsset importAsset = (ImportAsset)valueIn;
            return importAsset.Status;
        }
    }

}
