
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace SmartInkLaboratory.Services.UX
{
    public interface IDialogService 
    {
        Task<bool?> OpenAsync(string key);
        Task<(bool? confirmed, T data)> OpenAsync<T>(string key) where T : class;
        Task<(bool? confirmed, T data)> OpenAsync<T>(string key, T dataContext) where T : class;
        Task<bool> ConfirmAsync(string title, string prompt, string confirm = "Ok", string cancel = "Cancel");
        void Register<T>(string key) where T : ContentDialog;
    }
}
