using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor.Binding
{
    public class OptionsEditorBinding : PropertyEditorBinding
    {
        [SerializeField]
        private string viewModelOptionsPropertyName;

        public string ViewModelOptionsPropertyName
        {
            get { return viewModelOptionsPropertyName; }
            set { viewModelOptionsPropertyName = value; }
        }

        protected override void InitPropertyEditor(object viewModel, string propertyName, string label)
        {
            PropertyInfo propertyInfo = viewModel.GetType().GetProperty(propertyName);

            if (PropertyEditor is OptionsEditor)
            {
                string optionsPropertyName;
                object optionsViewModel;
                ParseViewModelEndPointReference(viewModelOptionsPropertyName, out optionsPropertyName, out optionsViewModel);

                var optionsPropertyInfo = optionsViewModel.GetType().GetProperty(optionsPropertyName);
                var rangeOptions = optionsPropertyInfo.GetValue(optionsViewModel) as RangeOptions;
                if (rangeOptions != null)
                {
                    OptionsEditor optionsEditor = (OptionsEditor)PropertyEditor;
                    optionsEditor.Options = rangeOptions.Options;
                }

                var strings = optionsPropertyInfo.GetValue(optionsViewModel) as string[];
                if (strings != null)
                {
                    OptionsEditor optionsEditor = (OptionsEditor)PropertyEditor;
                    optionsEditor.Options = strings.Select(s => new RangeOptions.Option(s)).ToArray();
                }
            }

            PropertyEditor.Init(viewModel, propertyInfo, label, EnableUndo);
        }
    }
}
