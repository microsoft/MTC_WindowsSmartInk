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
 
using SmartInkLaboratory.Services.Platform;
using Micosoft.MTC.SmartInk.Package;
using System;
using SmartInkLaboratory.ViewModels;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;

namespace SmartInkLaboratory.Services
{
    public class TagDeletedEventArgs : EventArgs
    {
        public Tag DeletedTag { get; set; }
    }

    public interface IAppStateService
    {
        event EventHandler KeysChanged;
        event EventHandler ProjectChanged;
        event EventHandler TagChanged;
        event EventHandler<TagDeletedEventArgs> TagDeleted;
        event EventHandler PackageChanged;
        event EventHandler IconChanged;
        event EventHandler IterationChanged;

        ResourceKeys CurrentKeys{ get; set; }
        Project CurrentProject { get; set; }
        Tag CurrentTag { get; set; }
        SmartInkPackageViewModel CurrentPackage { get;  }
        Iteration CurrentIteration { get; set; }
        void SetCurrentPackage(ISmartInkPackage package);
        void IconUpdated();
        void DeleteTag(Tag tag);
    }
}
