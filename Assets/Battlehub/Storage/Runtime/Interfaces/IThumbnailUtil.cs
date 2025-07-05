using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Storage
{
    public interface IThumbnailUtil
    {
        int Layer { get;  set; }

        Task<Texture2D> CreateThumbnailAsync(object obj, bool instantiate = true);

        Task<byte[]> EncodeToPngAsync(Texture2D texture);
    }
}
