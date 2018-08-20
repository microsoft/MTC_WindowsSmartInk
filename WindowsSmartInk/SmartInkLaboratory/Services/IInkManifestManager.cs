using System.Threading.Tasks;
using SmartInkLaboratory.Models;

namespace SmartInkLaboratory.Services
{
    public interface IInkManifestManager
    {
        InkManifest Current { get; }

        Task LoadAsync(string projectId);
        Task SaveAsync();
    }
}