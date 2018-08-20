using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Cognitive.CustomVision.Training.Models;

namespace SmartInkLaboratory.Services
{
    public interface ITagService 
    {
        Task DeleteTagAsync( Guid tag);
        Task DeleteTagAsync(string tag);
        Task<Tag> CreateTagAsync(string tag);
        Task<IList<Tag>> GetTagsAsync(bool refresh = true);
        Task<Tag> GetTagAsync(string id, bool refresh = true);
        Task<Tag> GetTagAsync(Guid id, bool refresh = true);
        Task<Tag> GetTagByNameAsync(string tagName, bool refresh = true);
    }
}