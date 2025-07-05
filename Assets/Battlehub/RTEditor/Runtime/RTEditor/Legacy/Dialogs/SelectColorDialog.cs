using UnityEngine;
using System;
using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using Battlehub.RTEditor.ViewModels;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*System.Obsolete*/]
    public class SelectColorDialog : RuntimeWindow, ISelectColorDialog
    {
        public event EventHandler<Color> ColorChanged;

        private Color m_selectedColor = Color.white;
        public Color SelectedColor
        {
            get { return m_selectedColor; }
            set { m_selectedColor = value; }
        }

        [SerializeField]
        private ColorPicker m_colorPicker = null;

        private Dialog m_parentDialog;

        private ILocalization m_localization;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.SelectColor;
            base.AwakeOverride();

            m_localization = IOC.Resolve<ILocalization>();
        }

        private void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.Ok += OnOk;
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SelectColorDialog_Select", "Select");
            m_parentDialog.IsCancelVisible = true;
            m_parentDialog.CancelText = m_localization.GetString("ID_RTEditor_SelectColorDialog_Cancel", "Cancel");
            m_colorPicker.CurrentColor = SelectedColor;
            m_colorPicker.onValueChanged?.AddListener(OnColorPickerValueChanged);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
            }

            m_colorPicker?.onValueChanged?.RemoveListener(OnColorPickerValueChanged);
        }

        private void OnColorPickerValueChanged(Color color)
        {
            ColorChanged?.Invoke(this, color);
        }

        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            SelectedColor = m_colorPicker.CurrentColor;
        }
    }
}
