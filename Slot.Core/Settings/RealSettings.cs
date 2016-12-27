﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using Json;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using Slot.Core.Packages;

namespace Slot.Core.Settings
{
    using MAP = Dictionary<string, object>;
    using OMAP = Dictionary<Type, SettingsBag>;

    public sealed class RealSettings : ISettings
    {
        private const string FILE = "settings.json";

        private MAP settings;
        private MAP userSettings;
        private MAP workspaceSettings;
        private readonly OMAP bagMap = new OMAP();
        private readonly IView view;

        public RealSettings(IView view)
        {
            this.view = view;
        }

        public T Get<T>() where T : SettingsBag, new()
        {
            LoadSettings();
            var typ = typeof(T);
            SettingsBag ret;

            if (!bagMap.TryGetValue(typ, out ret))
            {
                ret = new T();
                ret.Fill(settings, userSettings, workspaceSettings);
                bagMap.Add(typ, ret);
            }

            return (T)ret;
        }

        public void ReloadSettings(SettingsScope scope)
        {
            switch (scope)
            {
                case SettingsScope.User:
                    userSettings = ReadFile(UserSettingsFile);
                    break;
                case SettingsScope.Workspace:
                    var dir = view.Workspace;
                    if (dir != null)
                        workspaceSettings = ReadFile(Path.Combine(dir.FullName, ".slot", FILE));
                    break;
                case SettingsScope.Mode:
                    break;
            }

            foreach (var b in bagMap.Values)
                b.Fill(settings, userSettings, workspaceSettings);
        }

        private void LoadSettings()
        {
            if (settings != null)
                return;

            foreach (var pkg in App.Catalog<IPackageManager>().Default().EnumeratePackages())
                foreach (var e in pkg.GetMetadata(PackageSection.Settings))
                {
                    var name = Path.Combine(pkg.Directory.FullName, "data", e.String("file"));
                    var map = ReadFile(name);

                    if (settings == null)
                        settings = map;
                    else
                    {
                        try
                        {
                            foreach (var kv in map)
                                settings.Add(kv.Key, kv.Value);
                        }
                        catch { }
                    }
                }

            userSettings = ReadFile(UserSettingsFile);

            var dir = view.Workspace;

            if (dir != null)
                workspaceSettings = ReadFile(Path.Combine(dir.FullName, ".slot", FILE));
        }

        private MAP ReadFile(string fileName)
        {
            string content;

            if (!FileUtil.ReadFile(fileName, Encoding.UTF8, out content))
                return null;

            var json = new JsonParser(content);
            return json.Parse() as MAP;
        }

        internal string UserSettingsDirectory { get; set; }

        private string UserSettingsFile => Path.Combine(UserSettingsDirectory, FILE);
    }
}
