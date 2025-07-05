using UnityEngine;
using UnityWeld.Binding;
using Battlehub.RTEditor.Models;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class MenuItemViewModel : ContextMenuItem
    {
        [Binding]
        public override string Text
        {
            get;
            set;
        }

        [Binding]
        public override Sprite Icon
        {
            get;
            set;
        }

        [Binding]
        public override string Command
        {
            get;
            set;
        }
    }
}
