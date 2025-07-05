using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [CreateAssetMenu(menuName = "Unity Weld/Runtime Editor/Adapter options/GameObjectToHierarchyIcon adapter options")]
    public class GameObjectToHierarchyIconAdapterOptions : AdapterOptions
    {
        public bool MainIcon = true;
    }
}
