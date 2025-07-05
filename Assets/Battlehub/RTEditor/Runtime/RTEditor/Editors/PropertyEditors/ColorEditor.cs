using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ColorEditor : PropertyEditor<Color>
    {
        [SerializeField]
        private Image MainColor = null;

        [SerializeField]
        private RectTransform Alpha = null;

        [SerializeField]
        private Button BtnSelect = null;

        private Color m_initialColor;
        private Transform m_dialogTransform;
        private IWindowManager m_wm;

        protected override void SetInputField(Color value)
        {
            if(HasMixedValues())
            {
                MainColor.color = new Color(0, 0, 0, 0);
                Alpha.gameObject.SetActive(false);
            }
            else
            {
                Color color = value;
                color.a = 1.0f;
                MainColor.color = color;
                Alpha.transform.localScale = new Vector3(value.a, 1, 1);
                Alpha.gameObject.SetActive(true);
            }   
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            BtnSelect.onClick.AddListener(OnSelect);

            m_wm = IOC.Resolve<IWindowManager>();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (BtnSelect != null)
            {
                BtnSelect.onClick.RemoveListener(OnSelect);
            }

            if (m_dialogTransform != null)
            {
                m_wm.DestroyDialogWindow();
            }

            m_wm = null;
        }

        private void OnSelect()
        {
            ILocalization localization = IOC.Resolve<ILocalization>();
            string memberInfoTypeName = localization.GetString("ID_RTEditor_PE_TypeName_" + MemberInfoType.Name, MemberInfoType.Name);
            string select = localization.GetString("ID_RTEditor_PE_ColorEditor_Select", "Select") + " ";

            ISelectColorDialog colorSelector = null;
            m_dialogTransform = m_wm.CreateDialogWindow(BuiltInWindowNames.SelectColor, select + memberInfoTypeName,
                (sender, args) =>
                {
                    if(colorSelector != null)
                    {
                        colorSelector.ColorChanged -= OnColorChanged;

                        SetColor(colorSelector.SelectedColor, record: true);
                    }

                    m_dialogTransform = null;
                }, 
                (sender, args) => 
                {
                    if (colorSelector != null)
                    {
                        colorSelector.ColorChanged -= OnColorChanged;

                        SetColor(m_initialColor, record: false);
                    }

                    m_dialogTransform = null;
                }, 
                false);

            colorSelector = m_dialogTransform.GetComponentInChildren<ISelectColorDialog>();
            if(colorSelector != null)
            {
                BeginEdit();

                m_initialColor = GetValue();
                colorSelector.SelectedColor = m_initialColor;
                colorSelector.ColorChanged += OnColorChanged;
            }
        }
       
        private void OnColorChanged(object sender, Color color)
        {
            SetValue(color);
            SetInputField(color);
        }

        public void SetColor(Color color, bool record)
        {
            SetValue(color);
            EndEdit(record);
            SetInputField(color);
        }
    }
}
