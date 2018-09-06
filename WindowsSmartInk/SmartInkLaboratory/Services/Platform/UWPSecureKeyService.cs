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

using System.Collections.Generic;
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
