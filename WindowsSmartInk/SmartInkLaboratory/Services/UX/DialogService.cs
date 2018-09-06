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
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace SmartInkLaboratory.Services.UX
{
    public class DialogService : IDialogService
    {
        Dictionary<string, Type> _map = new Dictionary<string, Type>();


        public DialogService()
        {

        }

        public void Register<T>(string key) where T : ContentDialog
        {
            if (_map.ContainsKey(key))
                throw new ArgumentException($"Key: {key} is already registered");

            _map.Add(key, typeof(T));
        }

        public async Task<bool?> OpenAsync(string key)
        {
            var result = await OpenAsync<object>(key);
            return result.confirmed;
        }

        public async Task<(bool? confirmed, T data)> OpenAsync<T>(string key) where T : class
        {
            return await OpenAsync<T>(key, null);
        }

        public async Task<(bool? confirmed, T data)> OpenAsync<T>(string key, T dataContext) where T : class
        {
            bool? result = null;

            var dialog = await CreateDialog(key, dataContext);
            var dialogResult = await dialog.ShowAsync();

            switch (dialogResult)
            {
                case ContentDialogResult.None:
                    result = null;
                    break;
                case ContentDialogResult.Primary:
                    result = true;
                    break;
                case ContentDialogResult.Secondary:
                    result = false;
                    break;
            }


            return (result, dialog.DataContext as T);
        }


        public async Task<bool> ConfirmAsync(string title, string prompt, string confirm = "Ok", string cancel = "Cancel")
        {
            var dialog = new ContentDialog() { Title = title, Content = prompt, PrimaryButtonText = confirm, SecondaryButtonText = cancel };
            var result = await dialog.ShowAsync().AsTask();
            return result == ContentDialogResult.Primary;
        }



        private async Task<ContentDialog> CreateDialog<T>(string key, T dataContext) where T : class
        {
            if (!_map.ContainsKey(key))
                throw new InvalidOperationException($"key: {key} not found");

            var type = _map[key];
            var dialog = (ContentDialog)Activator.CreateInstance(type);

            if (dataContext != null)
                dialog.DataContext = dataContext;

            return dialog;

        }
    }
}
