using Battlehub.RTEditor.ViewModels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(IEnumerable), typeof(IEnumerable<AssetViewModel>))]
    public class IEnumerableToIEnumerableOfAssetViewModelAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            if (valueIn == null)
            {
                return null;
            }

            IEnumerable enumerable = (IEnumerable)valueIn;
            return enumerable.Cast<AssetViewModel>();
        }
    }
}
