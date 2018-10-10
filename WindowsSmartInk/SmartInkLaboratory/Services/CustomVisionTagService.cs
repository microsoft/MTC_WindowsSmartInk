
/*
 * Copyright(c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var reponse = await _trainingApi.GetTagsWithHttpMessagesAsync(_currentProject.Id);
            _tags = (from t in reponse.Body select t).OrderBy(t => t.Name).ToList();
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
                var newTag = await _trainingApi.CreateTagWithHttpMessagesAsync(_currentProject.Id, tag.ToLower());
                _tags.Add(newTag.Body);
                return newTag.Body;
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
                    await _trainingApi.DeleteTagWithHttpMessagesAsync(_currentProject.Id, tag.Id);
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
