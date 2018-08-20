using Microsoft.Cognitive.CustomVision.Training;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.Services
{

    public class ImageTagsServiceException : Exception
    { }

    public class CustomVisionTagService : CustomVisionClassifierBaseService, ITagService
    {
      
        private IList<Tag> _tags;

        public CustomVisionTagService() : base()
        {
           
        }

       

        public async Task<IList<Tag>> GetTagsAsync(bool refresh = true)
        {
            if (_currentProject == null)
                throw new InvalidOperationException("Project not opened");

            if (refresh)
                await LoadTagsAsync();

            return _tags;
        }

        private async Task LoadTagsAsync()
        {
            var tags = await _trainingApi.GetTagsAsync(_currentProject.Id);
            _tags = tags.Tags;
        }

        public Task<Tag> GetTagAsync(string id, bool refresh = false)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException($"{nameof(id)} cannot be null or empty.");

            return GetTagAsync(Guid.Parse(id), refresh);
        }

        public async Task<Tag> GetTagAsync(Guid id, bool refresh = false)
        {
            if (_currentProject == null)
                throw new InvalidOperationException("Project not opened");

            if (refresh)
                await LoadTagsAsync();

            var found = (from t in _tags where t.Id == id select t).FirstOrDefault();
            return found;
        }

        public async Task<Tag> GetTagByNameAsync(string tagName, bool refresh = false)
        {
            if (_currentProject== null)
                throw new InvalidOperationException("Project not opened");

            if (refresh)
                await LoadTagsAsync();
            var found = (from t in _tags where t.Name == tagName.ToLower() select t).FirstOrDefault();
            return found;
        }

        public async Task<Tag> CreateTagAsync(string tag)
        {
            if (_currentProject == null)
                throw new InvalidOperationException("Project not opened");

            if ( string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException($"{nameof(tag)} canot be null or empty");
            try { 
                var newTag = await _trainingApi.CreateTagAsync(_currentProject.Id, tag.ToLower());
                _tags.Add(newTag);
                return newTag;
            }
            catch (Exception ex)
            {
                throw new ImageTagsServiceException(); 
            }
        }

        public Task DeleteTagAsync(string tag)
        {
            if (_currentProject== null)
                throw new InvalidOperationException("Project not opened");

            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException($"{nameof(tag)} canot be null or empty");

            return DeleteTagAsync(Guid.Parse(tag));
        }

        public async Task DeleteTagAsync(Guid tagId)
        {
            if (_currentProject== null)
                throw new InvalidOperationException("Project not opened");

            if ( tagId == Guid.Empty)
                throw new ArgumentException($"{nameof(tagId)} cannot be empty.");
            try
            {
                var tag = (from t in _tags where t.Id == tagId select t).FirstOrDefault();
                if (tag != null) {
                    await _trainingApi.DeleteTagAsync(_currentProject.Id, tag.Id);
                    _tags.Remove(tag);
                 }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
