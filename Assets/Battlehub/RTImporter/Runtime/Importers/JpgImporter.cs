using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public class JpgImporter : AssetDatabaseFileImporter
    {
        public override string FileExt
        {
            get { return ".jpg"; }
        }

        public override string IconPath
        {
            get { return "Importers/Jpg"; }
        }

        public override async Task ImportAsync(string filePath, string targetPath, CancellationToken cancelToken)
        {
            byte[] bytes = filePath.Contains("://") ? 
                await DownloadBytesAsync(filePath) : 
                File.ReadAllBytes(filePath); 
            
            Texture2D texture = new Texture2D(4, 4);
            try
            {
                if (texture.LoadImage(bytes, false))
                {
                    await Editor.CreateAssetAsync(texture, GetUniquePath(targetPath));
                }
                else
                {
                    throw new FileImporterException($"Unable to load image {filePath}");
                }
            }
            catch (Exception e)
            {
                throw new FileImporterException(e.Message, e);
            }
            finally
            {
                if (!Editor.IsAsset(texture))
                {
                    UnityObject.Destroy(texture);
                }
            }
        }
    }
}
