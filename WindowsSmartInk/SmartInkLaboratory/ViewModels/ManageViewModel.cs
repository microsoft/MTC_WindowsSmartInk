using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.ViewModels
{
    public class ManageViewModel : ViewModelBase
    {
        private ResourceKeysViewModel _keys;
        private ProjectsViewModel _projects;
        private INavigationService _nav;

        public RelayCommand GoBack { get; set; }

        public ManageViewModel(ResourceKeysViewModel keys, ProjectsViewModel projects, INavigationService nav)
        {
            _keys = keys;
            _projects = projects;
            _nav = nav;


            this.GoBack = new RelayCommand(() => {
                _nav.GoBack();
            });
        }
    }
}
