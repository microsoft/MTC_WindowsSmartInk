using System.Collections.Generic;
using Windows.Security.Credentials;

namespace SmartInkLaboratory.Services.Platform
{
    public interface ISecureKeyService
    {
      
        IList<ResourceKeys> GetKeys();
        void SaveKeys(string resourceName, string trainingKey, string predictionKey);
        void ClearKeys();
        void DeleteKey(string resourcename);
    }
}