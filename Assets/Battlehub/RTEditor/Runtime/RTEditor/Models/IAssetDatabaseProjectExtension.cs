using System.Threading.Tasks;

namespace Battlehub.RTEditor.Models
{
    public interface IAssetDatabaseProjectExtension 
    {
        Task OnProjectLoadAsync(IAssetDatabaseModel assetDatabaseModel) => Task.CompletedTask;

        Task OnProjectUnloadAsync() => Task.CompletedTask;
    }
}

   

