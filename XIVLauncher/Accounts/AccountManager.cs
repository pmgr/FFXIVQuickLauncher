﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XIVLauncher.Settings;

namespace XIVLauncher.Accounts
{
    public class AccountManager
    {
        public ObservableCollection<XivAccount> Accounts;

        public XivAccount CurrentAccount
        {
            get
            {
                return Accounts.Count > 1 ?
                    Accounts.FirstOrDefault(a => a.Id == _setting.CurrentAccountId) :
                    Accounts.FirstOrDefault();
            }
            set => _setting.CurrentAccountId = value.Id;
        }

        private ILauncherSettingsV3 _setting; 

        public AccountManager(ILauncherSettingsV3 setting)
        {
            Load();

            _setting = setting;

            Accounts.CollectionChanged += Accounts_CollectionChanged;
        }

        private void Accounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Save();
        }

        public void UpdatePassword(XivAccount account, string password)
        {
            var existingAccount = Accounts.FirstOrDefault(a => a.Id == account.Id);
            existingAccount.Password = password;
        }

        public void AddAccount(XivAccount account)
        {
            var existingAccount = Accounts.FirstOrDefault(a => a.Id == account.Id);

            if (existingAccount != null && existingAccount.Password != account.Password)
            {
                existingAccount.Password = account.Password;
                return;
            }

            if (existingAccount != null)
                return;

            Accounts.Add(account);
        }

        public void RemoveAccount(XivAccount account)
        {
            Accounts.Remove(account);
        }

        #region SaveLoad

        private static readonly string ConfigPath = Path.Combine(Paths.XIVLauncherPath, "accountsList.json");

        public void Save()
        {
            File.WriteAllText(ConfigPath,  JsonConvert.SerializeObject(Accounts, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            }));
        }

        public void Load()
        {
            if (!File.Exists(ConfigPath))
            {
                Accounts = new ObservableCollection<XivAccount>();

                // Migration from old settings?
                if (Properties.Settings.Default.Accounts == "[]") 
                    return;

                Accounts = JsonConvert.DeserializeObject<ObservableCollection<XivAccount>>(Properties.Settings.Default.Accounts, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });

                Save();

                return;
            }

            Accounts = JsonConvert.DeserializeObject<ObservableCollection<XivAccount>>(File.ReadAllText(ConfigPath), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
        }

        #endregion
    }
}
