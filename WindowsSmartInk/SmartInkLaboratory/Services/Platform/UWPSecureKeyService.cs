using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace SmartInkLaboratory.Services.Platform
{
    public class ResourceKeys
    {
        public string Resource { get; set; }
        public string TrainingKey { get; set; }
        public string PredicationKey { get; set; }
    }

    public class UWPSecureKeyService : ISecureKeyService
    {
        public void SaveKeys(string resourceName, string trainingKey, string predictionKey)
        {
            var secret = $"{trainingKey}-{predictionKey}";
            var vault = new PasswordVault();
            var credential = new PasswordCredential(resourceName, resourceName, secret);
            vault.Add(credential);
        }

        public IList<ResourceKeys> GetKeys()
        {
            List<ResourceKeys> keys = new List<ResourceKeys>();
            var vault = new PasswordVault();
            
            var creds = vault.RetrieveAll();
            foreach (var cred in creds)
            {
                var key = new ResourceKeys();
                key.Resource = cred.Resource;
                cred.RetrievePassword();
                var secret = cred.Password.Split("-");
                key.TrainingKey = secret[0];
                key.PredicationKey = secret[1];
                keys.Add(key);
            }

            return keys;
        }

        public void DeleteKey(string resourceName)
        {
            List<ResourceKeys> keys = new List<ResourceKeys>();
            var vault = new PasswordVault();
            var creds = vault.FindAllByResource(resourceName);
            foreach (var c in creds)
            {
                vault.Remove(c);
            }
        }
        public void ClearKeys()
        {
            var vault = new PasswordVault();
            foreach (var cred in vault.RetrieveAll())
            {
                vault.Remove(cred);
            }
        }
    }
}
