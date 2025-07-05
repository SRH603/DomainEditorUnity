using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Storage
{
    public interface IAssetDatabase : IAssetDatabase<Guid, string>
    {
        Transform AssetsRoot
        {
            get;
        }

        IDataLayer<string> DataLayer
        {
            get;
        }

        IShaderUtil ShaderUtil
        {
            get;
        }

        Task DuplicateFolderAsync(Guid id, string newFolderID);

        Task DuplicateAssetAsync(Guid assetID, string newFileID);

        bool IsCyclicNesting(object asset, Transform parent);

        bool IsCyclicNestingAfterApplyingChanges(object instance, bool toBase = false);

        bool HasChanges(object instance, object instanceRootOpenedForEditing);

        bool CanApplyChangesAndSaveAsync(object instance, object instanceRootOpenedForEditing = null);

        bool CanApplyChangesToBaseAndSaveAsync(object instance, object instanceRootOpenedForEditing = null);

        bool CanRevertChangesToBaseAndSaveAsync(object instance, object instanceRootOpenedForEditing = null);

        Task ApplyChangesAndSaveAsync(object instance, IThumbnailCreatorContext context = null);

        Task ApplyChangesAndSaveAsync(object instance, object instanceRootOpenedForEditing, IThumbnailCreatorContext context = null);

        Task ApplyChangesToBaseAndSaveAsync(object instance, object instanceRootOpenedForEditing = null, IThumbnailCreatorContext context = null);

        Task RevertChangesToBaseAndSaveAsync(object instance, IThumbnailCreatorContext context = null);

        Task RevertChangesToBaseAndSaveAsync(object instance, object instanceRootOpenedForEditing, IThumbnailCreatorContext context = null);

        Task SaveAssetAndUpdateThumbnailsAsync(Guid assetID, IThumbnailCreatorContext context);

        Task UpdateThumbnailAsync(Guid assetID, IThumbnailCreatorContext context);
    }
}