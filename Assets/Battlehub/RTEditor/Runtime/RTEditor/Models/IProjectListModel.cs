using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    [Serializable]
    public class ProjectListEntry
    {
        [SerializeField]
        private string m_projectPath;
        public string ProjectPath
        {
            get { return m_projectPath; }
            set { m_projectPath = value; }
        }

        [SerializeField]
        private string m_name;

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        [SerializeField]
        private string m_displayName;

        public string DisplayName
        {
            get { return m_displayName; }
            set { m_displayName = value; }
        }

        [SerializeField]
        private string m_version;

        public string Version
        {
            get { return m_version; }
            set { m_version = value; }
        }

        
    }

    [Serializable]
    public class ProjectList
    {
        [SerializeField]
        private List<ProjectListEntry> m_items = new List<ProjectListEntry>();

        public List<ProjectListEntry> Entries
        {
            get { return m_items; }
            set { m_items = value; }
        }
    }

    public interface IProjectListModel 
    {
        string RootPath
        {
            get;
            set;
        }
        
        public Task<ProjectListEntry[]> GetProjectsAsync();

        public Task<ProjectListEntry> CreateProjectAsync(string projectPath);

        public Task<ProjectListEntry> DeleteProjectAsync(string projectPath);

        public Task ImportProjectAsync(Stream istream, string projectPath, string password = null);

        public Task ExportProjectAsync(Stream ostream, string projectPath, string password = null);
    }
}
