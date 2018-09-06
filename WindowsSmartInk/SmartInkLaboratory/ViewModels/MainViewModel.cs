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

using System.Threading.Tasks;
using AMP.ViewModels;
using SmartInkLaboratory.Services.UX;
using SmartInkLaboratory.Services;
using System.Collections.Generic;
using Windows.UI.Input.Inking;

namespace SmartInkLaboratory.ViewModels
{
    public class MainViewModel : AsyncViewModel, IInkProcessor
    {
        private IDialogService _dialog;
        private IAppStateService _state;

        public InteractionMode Mode { get; set; }
    
        public IconMapViewModel IconMap { get; set; }
      
        public TrainViewModel Train { get; set; }
        public TestViewModel Test { get; set; }



        private bool _hasPackage;
        public bool HasPackage
        {
            get { return _hasPackage; }
            set
            {
                if (_hasPackage == value)
                    return;
                _hasPackage = value;
                RaisePropertyChanged(nameof(HasPackage));
            }
        }






        public MainViewModel(IconMapViewModel iconMap,
                             TrainViewModel train,
                             TestViewModel test,
                             IAppStateService state,
                             IDialogService dialog)
        {
            IconMap = iconMap;
            Train = train;
            Test = test;
            _state = state;
            _dialog = dialog;

            _state.PackageChanged += (s, e) =>
            {
                HasPackage = _state.CurrentPackage != null;
            };
         
        }

        //public async Task<IDictionary<string , float >> ProcessInkImageAsync(SoftwareBitmap bitmap)
        //{
        //    switch (Mode)
        //    {
        //        case InteractionMode.Mapping:
        //            return null;
        //        case InteractionMode.Training:
        //            return await Train.ProcessInkImageAsync(bitmap);
        //        case InteractionMode.Testing:
        //            return  await Test.ProcessInkImageAsync(bitmap);
        //        case InteractionMode.Packaging:
        //            break;
        //    }

        //    return null;
        //}


        public async Task<IDictionary<string, float>> ProcessInkAsync(IList<InkStroke> strokes)
        {
            switch (Mode)
            {
                case InteractionMode.Mapping:
                    return null;
                case InteractionMode.Training:
                    return await Train.ProcessInkAsync(strokes);
                case InteractionMode.Testing:
                    return await Test.ProcessInkAsync(strokes);
                case InteractionMode.Packaging:
                    break;
            }

            return null;
        }




        //protected override async Task InitializeAsync()
        //{
        //    await ResourceKeys.Initialization;
        //}
    }

    
}