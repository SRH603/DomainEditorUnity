using Battlehub.RTCommon;
using Battlehub.Storage;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Battlehub.RTEditor.Models
{
    public class ProjectListModel : IProjectListModel
    {
        public string RootPath
        {
            get;
            set;
        }

        private string GetRootPath()
        {
            return !string.IsNullOrEmpty(RootPath) ? RootPath : IOC.Resolve<IRuntimeEditor>().ProjectsRootFolderPath;
        }

        private ProjectList m_projectList;
        private IDataLayer<string> DataLayer
        {
            get { return RuntimeAssetDatabase.Instance.DataLayer; }
        }

        public async Task<ProjectListEntry[]> GetProjectsAsync()
        {
            m_projectList = new ProjectList();

            string rootPath = GetRootPath();
            var dataLayer = DataLayer;
            var rootTreeItems = await TaskUtils.Run(() => dataLayer.GetTreeAsync(rootPath, recursive: false));

            string[] directories = rootTreeItems.Where(item => item.IsFolder).Select(item => item.ID).ToArray();  // Directory.GetDirectories(rootPath);
            foreach (string directory in directories)
            {
                string name = Path.GetFileName(directory);
                if (string.IsNullOrEmpty(name) || name.StartsWith("."))
                {
                    continue;
                }

                var entry = new ProjectListEntry();
                entry.ProjectPath = Normalize(directory);
                entry.Name = name;
                entry.Version = await dataLayer.ExistsAsync(entry.ProjectPath + "/Project.rtmeta") ? "RTSL" : "AssetDatabase";
                entry.DisplayName = entry.Version != "AssetDatabase" ? $"{entry.Name} ({entry.Version})" : entry.Name;

                m_projectList.Entries.Add(entry);
            }

            return m_projectList.Entries.ToArray();
        }

        public async Task<ProjectListEntry> CreateProjectAsync(string projectPath)
        {
            if (m_projectList == null)
            {
                await GetProjectsAsync();
            }

            if (!Path.IsPathRooted(projectPath))
            {
                projectPath = $"{GetRootPath()}/{projectPath}";
            }
            projectPath = Normalize(projectPath);

            var existingProject = m_projectList.Entries.Where(entry => entry.ProjectPath.ToLower() == projectPath.ToLower()).FirstOrDefault();
            if (existingProject != null)
            {
                return existingProject;
            }

            await DataLayer.CreateFolderAsync(projectPath);
            var entry = new ProjectListEntry()
            {
                Name = Path.GetFileName(projectPath),
                DisplayName = Path.GetFileName(projectPath),
                ProjectPath = projectPath
            };

            m_projectList.Entries.Add(entry);
            return entry;
        }

        public async Task<ProjectListEntry> DeleteProjectAsync(string path)
        {
            if (m_projectList == null)
            {
                await GetProjectsAsync();
            }

            string projectPath = path;
            if (!Path.IsPathRooted(path))
            {
                projectPath = Normalize($"{GetRootPath()}/{path}");
            }

            var query = m_projectList.Entries.Select((entry, index) => (entry, index)).Where(pair => pair.entry.ProjectPath.ToLower() == projectPath.ToLower());
            if(query.Any())
            {
                var (entry, index) = query.First();
                m_projectList.Entries.RemoveAt(index);

                await DataLayer.DeleteFolderAsync(projectPath);
                return entry;
            }
            return null;
        }

        public async Task ImportProjectAsync(Stream istream, string projectPath, string password)
        {
            if (!Path.IsPathRooted(projectPath))
            {
                projectPath = Normalize($"{GetRootPath()}/{projectPath}");
            }

            string zipPath = $"{projectPath}.zip";
            var dataLayer = DataLayer;
            try
            {
                var zipStream = await dataLayer.OpenWriteAsync(zipPath);
                try
                {
                    await TaskUtils.Run(() => istream.CopyTo(zipStream));
                }
                finally
                {
                    await dataLayer.ReleaseAsync(zipStream);
                }

                await dataLayer.DeleteFolderAsync(projectPath);
                await TaskUtils.Run(() => dataLayer.UncompressZipAsync(zipPath, password, projectPath));
            }
            finally
            {
                await dataLayer.DeleteAsync(zipPath);
            }
        }

        public async Task ExportProjectAsync(Stream ostream, string projectPath, string password)
        {
            if (!Path.IsPathRooted(projectPath))
            {
                projectPath = Normalize($"{GetRootPath()}/{projectPath}");
            }

            string zipPath = $"{projectPath}.zip";
            var dataLayer = DataLayer;
            try
            {
                await TaskUtils.Run(() => dataLayer.CompressZipAsync(projectPath, password, zipPath));

                var zipStream = await dataLayer.OpenReadAsync(zipPath);
                try
                {
                    await TaskUtils.Run(() => zipStream.CopyTo(ostream));
                }
                finally
                {
                    await dataLayer.ReleaseAsync(zipStream);
                }
            }
            finally
            {
                await dataLayer.DeleteAsync(zipPath);
            }
        }

        private string Normalize(string path)
        {
            return path.Replace('\\', '/');
        }
    }

}
