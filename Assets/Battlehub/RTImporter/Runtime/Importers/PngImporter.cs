using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public class PngImporter : AssetDatabaseFileImporter
    {
        public override string FileExt
        {
            get { return ".png"; }
        }

        public override string IconPath
        {
            get { return "Importers/Png"; }
        }

        public override int Priority
        {
            get { return int.MinValue; }
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
                    if (texture.format == TextureFormat.RGBA32 || texture.format == TextureFormat.ARGB32)
                    {
                        bool opaque = true;
                        Color32[] pixels = texture.GetPixels32();
                        for (int i = 0; i < pixels.Length; ++i)
                        {
                            if (pixels[i].a != 255)
                            {
                                opaque = false;
                                break;
                            }
                        }

                        if (opaque)
                        {
                            texture.LoadImage(texture.EncodeToJPG(), false);
                        }
                    }

                    await Editor.CreateAssetAsync(texture, GetUniquePath(targetPath));
                }
                else
                {
                    throw new FileImporterException($"Unable to load image {filePath}");
                }
            }
            catch(Exception e)
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
