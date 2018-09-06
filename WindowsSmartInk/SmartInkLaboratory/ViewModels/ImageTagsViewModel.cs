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

using SmartInkLaboratory.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SmartInkLaboratory.ViewModels
{
    public class TagChangedEventArgs : EventArgs
    {
        public Tag NewTag { get; set; }
    }

    public class ImageTagsViewModel : ViewModelBase
    {
        ITagService _tagService;
        private IAppStateService _state;
    

        public RelayCommand<string> AddTag { get; private set; }
        public RelayCommand<Tag> DeleteTag { get; private set; }
        public RelayCommand Refresh{get; private set;}

        public ObservableCollection<Tag> Tags { get; private set; } = new ObservableCollection<Tag>();

    
        public Tag CurrentTag
        {
            get { return _state.CurrentTag; }
            set
            {
                if (_state.CurrentTag == value)
                    return;
                _state.CurrentTag = value;
                RaisePropertyChanged(nameof(CurrentTag));
         }
        }


        public ImageTagsViewModel(ITagService tagService, IAppStateService state)
        {
            _tagService = tagService;
            _state = state;
            _state.ProjectChanged += async (s,e) => {
                await LoadTagsAsync();
            };

        

            this.AddTag = new RelayCommand<string>(async (tag) => {
                if (string.IsNullOrWhiteSpace(tag))
                    return;
                try
                {
                    var newTag = await _tagService.CreateTagAsync(tag);
                    Tags.Add(newTag);
                }
                catch (ImageTagsServiceException ex)
                {
                }
                catch (Exception ex)
                { }
                
            });
            this.DeleteTag = new RelayCommand<Tag>(async (tag) => {
                await _tagService.DeleteTagAsync(tag.Id);
                _state.DeleteTag(tag);
                Tags.Remove(tag);

            });
            this.Refresh = new RelayCommand(async() => {
                await LoadTagsAsync();

            });
        }

        public async Task<Tag> GetTagByNameAsync(string tagname)
        {

            var tag = await _tagService.GetTagByNameAsync(tagname);
            return tag;
        }

        private async Task LoadTagsAsync(bool refresh = true)
        {
           
            Tags.Clear();
            var tags = await _tagService.GetTagsAsync(refresh);
            foreach (var t in tags)
                Tags.Add(t);
        }
    }
}
