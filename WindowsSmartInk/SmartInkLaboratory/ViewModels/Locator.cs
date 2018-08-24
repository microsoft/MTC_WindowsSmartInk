using SmartInkLaboratory.Services;
using SmartInkLaboratory.Services.Platform;
using SmartInkLaboratory.Services.UX;
using SmartInkLaboratory.Views.Dialogs;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInkLaboratory.ViewModels
{

    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var dialogService = CreateDialogService();
            SimpleIoc.Default.Register<SmartInkLaboratory.Services.UX.IDialogService>(() => dialogService);
            var navigationService = CreateNavigationService();
            SimpleIoc.Default.Register<INavigationService>(() => navigationService);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ImageTagsViewModel>();
            SimpleIoc.Default.Register<IconMapViewModel>();
            SimpleIoc.Default.Register<ProjectsViewModel>();
            SimpleIoc.Default.Register<ResourceKeysViewModel>();
            SimpleIoc.Default.Register<TrainViewModel>();
            SimpleIoc.Default.Register<TestViewModel>();
            SimpleIoc.Default.Register<NewPackageViewModel>();
            //SimpleIoc.Default.Register<PackageManagerViewModel>();
            SimpleIoc.Default.Register<ManageViewModel>();
            SimpleIoc.Default.Register<OpenPackageViewModel>();
            SimpleIoc.Default.Register<PackageViewModel>();

            SimpleIoc.Default.Register<IClassifierService, CustomVisionClassifierBaseService>();
            //SimpleIoc.Default.Register<IIconMappingService, LocalIconMappingService>();
            SimpleIoc.Default.Register<ITagService, CustomVisionTagService>();
            SimpleIoc.Default.Register<IImageService, CustomVisionImageService>();
            SimpleIoc.Default.Register<ITrainingService, CustomVisionTrainingService>();
            SimpleIoc.Default.Register<IPredictionService, CustomVisionPredictionService>();
            SimpleIoc.Default.Register<IProjectService, CustomVisionProjectService>();
            SimpleIoc.Default.Register<IInkModelManager, InkModelManager>();
            SimpleIoc.Default.Register<IInkManifestManager, InkManifestManager>();
            SimpleIoc.Default.Register<IMessenger,Messenger>();
            SimpleIoc.Default.Register<ISecureKeyService, UWPSecureKeyService>();
            SimpleIoc.Default.Register<IProjectPackageMapper, LocalProjectPackageMapper>();
            SimpleIoc.Default.Register<IAppStateService, AppStateService>();
        }

        private INavigationService CreateNavigationService()
        {
            var navigation = new NavigationService();
            navigation.Configure("ManageResources", typeof(Views.ManageResourcesView));

            return navigation;
        }

        public MainViewModel Main
        {
            get
            {
                var vm = ServiceLocator.Current.GetInstance<MainViewModel>();
                return vm;
            }
        }


        public ImageTagsViewModel ImageTags
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ImageTagsViewModel>();
            }
        }

        public IconMapViewModel IconMap
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IconMapViewModel>();
            }
        }

        public ProjectsViewModel Projects
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProjectsViewModel>();
            }
        }

        public ResourceKeysViewModel ResourceKeys
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ResourceKeysViewModel>();
            }
        }

        public TrainViewModel Train
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TrainViewModel>();
            }
        }

        public TestViewModel Test
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TestViewModel>();
            }
        }

        public PackageViewModel Package
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PackageViewModel>();
            }
        }

        public OpenPackageViewModel OpenPackage
        {
            get
            {
                return ServiceLocator.Current.GetInstance<OpenPackageViewModel>();
            }
        }

        public NewPackageViewModel NewPackage
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NewPackageViewModel>();
            }
        }




        private static SmartInkLaboratory.Services.UX.IDialogService CreateDialogService()
        {
            var dialogService = new SmartInkLaboratory.Services.UX.DialogService();
            dialogService.Register<ResourceListDialog>(DialogKeys.ResourceList);
            dialogService.Register<NewPackageDialog>(DialogKeys.NewPackage);
            dialogService.Register<OpenPackageDialog>(DialogKeys.OpenPackage);
            dialogService.Register<ManageProjectsDialog>(DialogKeys.ManageProjects);

            return dialogService;
        }


    }
}
