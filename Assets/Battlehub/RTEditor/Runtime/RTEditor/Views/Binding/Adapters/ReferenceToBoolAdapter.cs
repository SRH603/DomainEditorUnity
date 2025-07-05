
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(object), typeof(bool))]
    public class ReferenceToBoolAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            return valueIn != null;
        }   
    }

    [Adapter(typeof(GameObject), typeof(bool))]
    public class GameObjectReferenceToBoolAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            return (valueIn as GameObject) != null;
        }
    }
}
