using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Micosoft.MTC.SmartInk.Package
{
    public interface IMediaPackage
    {
        Task<IStorageFile> GetMediaAsync(Guid tagId);
        Task<IStorageFile> GetMediaAsync(string tag);
        Task<IStorageFile> GetMediaByNameAsync(string tagname);
        Task SaveMediaAsync(Guid tagId, IStorageFile file);
        Task SaveMediaAsync(string tagName, IStorageFile file);
    }
}