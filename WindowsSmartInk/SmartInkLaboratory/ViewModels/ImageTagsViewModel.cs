using SmartInkLaboratory.Services;
using SmartInkLaboratory.Services.Platform;
using AMP.ViewModels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
