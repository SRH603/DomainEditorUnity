using Battlehub.RTEditor.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AssetDatabaseImportStatus : MonoBehaviour
    {
        [SerializeField]
        private Sprite m_statusNone = null;

        [SerializeField]
        private Sprite m_statusNew = null;

        [SerializeField]
        private Sprite m_statusWarning = null;

        [SerializeField]
        private Sprite m_statusOverwrite = null;

        private Image m_image;

        private ImportAsset.ImportStatus m_current;
        public ImportAsset.ImportStatus Current
        {
            get { return m_current; }
            set
            {
                if(m_current != value)
                {
                    m_current = value;

                    if(m_image != null)
                    {
                        UpdateSprite();
                    }
                }
            }
        }

        private void Awake()
        {
            m_image = GetComponent<Image>();
            m_image.sprite = m_statusNone;

            if(m_image != null)
            {
                UpdateSprite();
            }
        }

        private void UpdateSprite()
        {
            switch (m_current)
            {
                case ImportAsset.ImportStatus.None:
                    m_image.sprite = m_statusNone;
                    break;
                case ImportAsset.ImportStatus.New:
                    m_image.sprite = m_statusNew;
                    break;
                case ImportAsset.ImportStatus.Conflict:
                    m_image.sprite = m_statusWarning;
                    break;
                case ImportAsset.ImportStatus.Overwrite:
                    m_image.sprite = m_statusOverwrite;
                    break;
            }
        }

    }

}

