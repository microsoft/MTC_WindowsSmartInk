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

using System;
using SmartInkLaboratory.Services.Platform;
using Micosoft.MTC.SmartInk.Package;
using Microsoft.Cognitive.CustomVision.Training.Models;

namespace SmartInkLaboratory.Services
{

    public class AppStateService : IAppStateService
    {
        static ResourceKeys _currentKeys;
        static Project _currentProject;
        static ISmartInkPackage _currentPackage;
        static Tag _currentTag;
        static IClassifierService _classifier;

        public event EventHandler KeysChanged;
        public event EventHandler ProjectChanged;
        public event EventHandler TagChanged;
        public event EventHandler<TagDeletedEventArgs> TagDeleted;
        public event EventHandler PackageChanged;
        public event EventHandler IconChanged;
        public event EventHandler IterationChanged;

        public ResourceKeys CurrentKeys {
            get => _currentKeys;
            set {
                _currentKeys = value;
                _classifier.SetKeys(_currentKeys);
                KeysChanged?.Invoke(this, null);
            }
        }
        public Project CurrentProject {
            get => _currentProject;
            set
            {
                _currentProject = value;
                _classifier.SetCurrentProject(_currentProject);
                ProjectChanged?.Invoke(this, null);
            }
        }

        public Tag CurrentTag
        {
            get => _currentTag;
            set
            {
                _currentTag = value;
                TagChanged?.Invoke(this, null);
            }
        }
        public ISmartInkPackage CurrentPackage {
            get => _currentPackage;
            set
            {
                _currentPackage = value;
                PackageChanged?.Invoke(this,null);
            }
        }

        private Iteration _currentIteration;
        public Iteration CurrentIteration
        {
            get { return _currentIteration; }
            set
            {
                if (_currentIteration == value)
                    return;
                _currentIteration = value;
                IterationChanged?.Invoke(this, null);
            }
        }


        public AppStateService(IClassifierService classifier)
        {
            if (_classifier == null)
                _classifier = classifier;
            else
            {
                classifier.SetKeys(CurrentKeys);
                classifier.SetCurrentProject(CurrentProject);
                _classifier = classifier;
            }
        }

        public void IconUpdated()
        {
            IconChanged?.Invoke(this, null);
        }

        public void DeleteTag(Tag tag)
        {
            if ( _currentTag?.Id == tag.Id)
                CurrentTag = null;

            TagDeleted?.Invoke(this, new TagDeletedEventArgs { DeletedTag = tag });
        }
    }
}
