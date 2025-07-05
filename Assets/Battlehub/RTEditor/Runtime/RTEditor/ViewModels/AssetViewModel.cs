using Battlehub.RTEditor.Models;
using System;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AssetViewModel : INotifyPropertyChanged, IAsset
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Texture m_thumbnail;
        [Binding]
        public Texture Thumbnail
        {
            get { return m_thumbnail; }
            set
            {
                if (m_thumbnail != value)
                {
                    m_thumbnail = value;
                    RaisePropertyChanged(nameof(Thumbnail));
                }
            }
        }

        private string m_name;
        [Binding]
        public string Name
        {
            get { return m_name; }
            set
            {
                if (m_name != value)
                {
                    m_name = value;
                    RaisePropertyChanged(nameof(Name));
                    RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        [Binding]
        public string DisplayName
        {
            get { return Path.GetFileNameWithoutExtension(Name); }
            set
            {
                string ext = !string.IsNullOrEmpty(Name) ?
                    Path.GetExtension(Name) :
                    string.Empty;

                string newName = value + ext;
                if (Name != newName)
                {
                    Name = newName;
                }
            }
        }

        private ID m_id;
        public ID ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        public AssetViewModel(ID id, string name, Texture thumbnail = null)
        {
            m_name = name;
            m_thumbnail = thumbnail;
            m_id = id;
        }

        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return $"AssetViewModel:{m_name} {m_id}";
        }
    }
}
