using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

using FreePIE.Core.Common.Extensions;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Persistence.Paths;

namespace FreePIE.Core.Persistence
{
    internal class SettingsManager : ISettingsManager
    {
        private readonly IPaths paths;
        private const string filename = "settings.xml";

        public SettingsManager(IPaths paths)
        {
            this.paths = paths;
        }

        public bool Load()
        {
            var path = paths.GetDataPath(filename);

            if(!File.Exists(path))
            {
                Settings = new Settings();
            }
            else
            {
                var serializer = new DataContractSerializer(typeof(Settings));
                

                using (var stream = new FileStream(path, FileMode.Open))
                {
                    try
                    {

                        Settings = serializer.ReadObject(stream) as Settings;
                    }
                    catch
                    {
                        Settings = new Settings();
                        return false;
                    }
                }
            }

            FixBackwardCompatibility();
            return true;
        }

        private void FixBackwardCompatibility()
        {
            Settings.Curves.Where(c => !c.ValidateCurve.HasValue).ForEach(c => c.ValidateCurve = true);
        }

        public void Save()
        {
            var serializer = new DataContractSerializer(typeof(Settings));
            var xmlWriterSettings = new XmlWriterSettings { Indent = true };

            string backup = null;
            if (File.Exists(paths.GetDataPath(filename)))
                backup = File.ReadAllText(paths.GetDataPath(filename));
            try
            {
                using (var stream = new FileStream(paths.GetDataPath(filename), FileMode.Create))
                {
                    using (var w = XmlWriter.Create(stream, xmlWriterSettings))
                        serializer.WriteObject(w, Settings);
                }
            }
            catch
            {
                if (backup != null)
                {
                    File.WriteAllText(paths.GetDataPath(filename), backup);
                }
            }
        }

        public PluginSetting GetPluginSettings(IPlugin plugin)
        {
            var pluginTypeName = plugin.GetType().FullName;
            var pluginSetting = Settings.PluginSettings.Single(ps => ps.PluginType == pluginTypeName);
            return pluginSetting;
        }

        public IEnumerable<PluginSetting> ListConfigurablePluginSettings()
        {
            return Settings
                .PluginSettings
                .Where(ps => ps.PluginProperties.Any())
                .OrderBy(ps => ps.FriendlyName)
                .ToList();
        }

        public IEnumerable<PluginSetting> ListPluginSettingsWithHelpFile()
        {
            return Settings
                .PluginSettings
                .Where(ps => !string.IsNullOrEmpty(ps.HelpFile))
                .OrderBy(ps => ps.FriendlyName)
                .ToList();
        }

        public void SaveAsFormattedXml()
        {
            var path = paths.GetDataPath(filename);
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineOnAttributes = false
            };

            using (var writer = XmlWriter.Create(path, settings))
            {
                var serializer = new DataContractSerializer(typeof(Settings));
                serializer.WriteObject(writer, Settings);
            }
        }

        public Settings Settings { get; private set; }

    }
}
