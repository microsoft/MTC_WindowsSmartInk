using SmartInkLaboratory.Services.Platform;
using SmartInkLaboratory.Services.UX;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Micosoft.MTC.SmartInk.Package;
using Micosoft.MTC.SmartInk.Package.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.ViewModels
{
    public class PackageManagerViewModel : ViewModelBase
    {
        PackageManager _manager;
        private ISecureKeyService _keys;
        private IDialogService _dialog;

        public RelayCommand NewPackage { get; set; }
        public RelayCommand OpenPackage { get; set; }

        public PackageManagerViewModel(ISecureKeyService keys, IDialogService dialog)
        {
            _manager = new PackageManager();
            _keys = keys;
            _dialog = dialog;

            this.NewPackage = new RelayCommand(() => {  });
        }



    }
}
