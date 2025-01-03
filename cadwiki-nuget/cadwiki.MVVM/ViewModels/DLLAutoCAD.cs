using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.MVVM.ViewModels
{

    public class DLLAutoCADViewModel : Caliburn.Micro.Screen
    {

        private List<Assembly> _loadedAssemblies;
        private List<CustomAssemblyInfo> _customAssemblyInfo;
        private bool _showToUser;

        public List<Assembly> LoadedAssemblies
        {
            get => _loadedAssemblies;
            set
            {
                var sorted = value
                    .OrderByDescending(a => a.FullName)
                    .ToList();

                var customList = new List<CustomAssemblyInfo>();
                foreach (var assembly in sorted)
                {
                    var custom = new CustomAssemblyInfo(assembly);
                    customList.Add(custom);
                }
                _loadedAssemblies = sorted;
                _customAssemblyInfo = customList;
                NotifyOfPropertyChange(nameof(LoadedAssemblies));
            }
        }

        public List<CustomAssemblyInfo> CustomAssemblyInfoList
        {
            get => _customAssemblyInfo;
            set
            {
                _customAssemblyInfo = value;
                NotifyOfPropertyChange(nameof(CustomAssemblyInfoList));
            }
        }

        public bool ShowToUser
        {
            get => _showToUser;
            set
            {
                _showToUser = value;
                NotifyOfPropertyChange(nameof(ShowToUser));
            }
        }

        public DLLAutoCADViewModel()
        {
            
        }

        public class CustomAssemblyInfo
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string DateModified { get; set; }
            public string FilePath { get; set; }

            public CustomAssemblyInfo(Assembly assembly)
            {
                Name = assembly.GetName().Name;
                Version = assembly.GetName().Version.ToString();
                try
                {
                    var location = assembly.Location;
                    var dateMod = System.IO.File.GetLastWriteTimeUtc(location);
                    string timeStamp = dateMod.ToString("yyyy-MM-dd HH:mm:ss");
                    DateModified = timeStamp;
                    FilePath = location;
                }
                catch(Exception)
                {
                    DateModified = "Error";
                    FilePath = "Error";
                }
            }
        }
    }
}
