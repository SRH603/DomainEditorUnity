using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.Storage;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public class ProjectListLegacyModel : IProjectListModel
    {
        public string RootPath
        {
            get;
            set;
        }

        private ProjectList m_projectList;

        public async Task<ProjectListEntry> CreateProjectAsync(string projectPath)
        {
            if (m_projectList == null)
            {
                await GetProjectsAsync();
            }

            var existingProject = m_projectList.Entries.Where(entry => Normalize(entry.ProjectPath).ToLower() == Normalize(projectPath).ToLower()).FirstOrDefault();
            if (existingProject != null)
            {
                return existingProject;
            }

            await IOC.Resolve<IProjectAsync>().Safe.CreateProjectAsync(projectPath);

            var entry = new ProjectListEntry()
            {
                Name = Path.GetFileName(projectPath),
                DisplayName = Path.GetFileName(projectPath),
                ProjectPath = projectPath
            };

            m_projectList.Entries.Add(entry);
            return entry;
        }

        public async Task<ProjectListEntry[]> GetProjectsAsync()
        {
            m_projectList = new ProjectList();

            var projects = await IOC.Resolve<IProjectAsync>().Safe.GetProjectsAsync();
            foreach (var project in projects)
            {
                var entry = new ProjectListEntry();
                entry.ProjectPath = project.Name;
                entry.Name = project.Name;
                entry.DisplayName = project.Name;
                entry.Version = "RTSL";
                m_projectList.Entries.Add(entry);
            }

            return m_projectList.Entries.ToArray();
        }

        public async Task<ProjectListEntry> DeleteProjectAsync(string projectPath)
        {
            if (m_projectList == null)
            {
                await GetProjectsAsync();
            }

            var query = m_projectList.Entries.Select((entry, index) => (entry, index)).Where(pair => pair.entry.ProjectPath.ToLower() == projectPath.ToLower());
            if (query.Any())
            {
                var (entry, index) = query.First();
                m_projectList.Entries.RemoveAt(index);

                await IOC.Resolve<IProjectAsync>().Safe.DeleteProjectAsync(entry.Name);
                return entry;
            }
            return null;
        }

        private string Normalize(string path)
        {
            return path.Replace('\\', '/');
        }

        public async Task ImportProjectAsync(Stream istream, string projectPath, string password = null)
        {
            string tempPath = $"{Application.persistentDataPath}/{Guid.NewGuid()}";
            try
            {
                using (var fs = File.OpenWrite(tempPath))
                {
                    await TaskUtils.Run(() => istream.CopyTo(fs));
                }

                await IOC.Resolve<IProjectAsync>().Safe.ImportProjectAsync(projectPath, tempPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        public async Task ExportProjectAsync(Stream ostream, string projectPath, string password = null)
        {
            string tempPath = $"{Application.persistentDataPath}/{Guid.NewGuid()}";
           
            try
            {
                await IOC.Resolve<IProjectAsync>().Safe.ExportProjectAsync(projectPath, tempPath);

                using (var fs = File.OpenRead(tempPath))
                {
                    await TaskUtils.Run(() => fs.CopyTo(ostream));
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }

        }
    }

}
