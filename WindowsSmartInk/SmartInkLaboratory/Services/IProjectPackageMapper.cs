using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Services
{
    public interface IProjectPackageMapper
    {
        Task<IList<string>> GetPackagesByProjectAsync(string project);
        Task<IList<string>> GetAllPackagesAsync();
        Task AddAsync(string package, string project);
        Task DeleteAsync(string package);
    }
}