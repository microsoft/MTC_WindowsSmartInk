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
using SmartInkLaboratory.Services.Platform;
using SmartInkLaboratory.Services.UX;
using SmartInkLaboratory.Views.Dialogs;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;

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
            SimpleIoc.Default.Register<ManageViewModel>();
            SimpleIoc.Default.Register<OpenPackageViewModel>();
            SimpleIoc.Default.Register<PackageManagerViewModel>();

            SimpleIoc.Default.Register<IClassifierService, CustomVisionClassifierBaseService>();
            SimpleIoc.Default.Register<ITagService, CustomVisionTagService>();
            SimpleIoc.Default.Register<IImageService, CustomVisionImageService>();
            SimpleIoc.Default.Register<ITrainingService, CustomVisionTrainingService>();
            SimpleIoc.Default.Register<IPredictionService, CustomVisionPredictionService>();
            SimpleIoc.Default.Register<IProjectService, CustomVisionProjectService>();
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

        public PackageManagerViewModel PackageManager
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PackageManagerViewModel>();
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
