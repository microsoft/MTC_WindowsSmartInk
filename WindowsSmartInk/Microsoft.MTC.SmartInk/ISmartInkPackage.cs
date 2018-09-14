using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Input.Inking;

namespace Micosoft.MTC.SmartInk.Package
{
    public interface ISmartInkPackage
    {
        string Author { get; set; }
        DateTimeOffset DatePublished { get; set; }
        string Description { get; set; }
        bool IsLocalModelAvailable { get; }
        SoftwareBitmap LastEvaluatedBitmap { get; set; }
        string Name { get; set; }
        IReadOnlyList<string> Tags { get; }
        string Version { get; set; }

        Task AddTagAsync(Guid tagId, string tagName);
        Task AddTagsAsync(Dictionary<Guid, string> tags);
        Task<IDictionary<string, float>> EvaluateAsync(IList<InkStroke> strokes, float threshold = 0);
        Task<IDictionary<string, float>> EvaluateAsync(SoftwareBitmap bitmap, float threshold = 0);
        Task RemoveTagAsync(Guid tagId);
        Task SaveAsync();
        Task SaveModelAsync(IStorageFile modelFile);
        Task UpdateTagsAsync(Dictionary<Guid, string> tags);
    }
}