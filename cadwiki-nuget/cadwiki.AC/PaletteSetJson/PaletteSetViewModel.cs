using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.IO;

namespace cadwiki.AC.PalleteSets
{

    public class PaletteSetViewModel : Caliburn.Micro.Screen
    {
        public static List<string> Messages = new List<string>();
        public static List<Exception> Exceptions = new List<Exception>();
        public static string UserSetingsFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\UserSettings";
        

        public string FileNamePrefix = "";
        private string _fileExension = ".json";
        [DataMember]
        public string Title { get; set; } = "";
        [DataMember]
        public string Version { get; set; } = "";



        private string _paletteDock;
        [DataMember]
        public string PaletteDock
        {
            get { return _paletteDock; }
            set
            {
                _paletteDock = value;
                NotifyOfPropertyChange(nameof(PaletteDock));
            }
        }

        private int _paletteHeight;
        [DataMember]
        public int PaletteHeight
        {
            get { return _paletteHeight; }
            set
            {
                _paletteHeight = value;
                NotifyOfPropertyChange(nameof(PaletteHeight));
            }
        }

        private int _paletteWidth;

        public PaletteSetViewModel()
        {
        }

        [DataMember]
        public int PaletteWidth
        {
            get { return _paletteWidth; }
            set
            {
                _paletteWidth = value;
                NotifyOfPropertyChange(nameof(PaletteWidth));
            }
        }

        /// <summary>
        /// Do not call this from the ViewModel base constructor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void LoadFirstFoundPresetFileIntoViewModel<T>() where T : class
        {
            try
            {
                string firstPresetFile = FindFirstFileInFolder(UserSetingsFolder, FileNamePrefix, _fileExension);
                LoadPresetIntoViewModel<T>(firstPresetFile);
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }

        /// <summary>
        /// Do not call this from the ViewModel base constructor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public T ReadViewModelFromJsonFile<T>(string filePath) where T : class
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    T viewModel = JsonConvert.DeserializeObject<T>(json);
                    return viewModel;
                }
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
            return null;
        }

        private void LoadPresetIntoViewModel<T>(string selectedFilePath) where T : class
        {
            try
            {
                if (!String.IsNullOrEmpty(selectedFilePath))
                {
                    var readInViewModel = ReadViewModelFromJsonFile<T>(selectedFilePath);
                    if (readInViewModel != null)
                    {
                        PropertyInfo[] properties = typeof(T).GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetCustomAttribute<DataMemberAttribute>() != null)
                            {
                                object readInValue = property.GetValue(readInViewModel);
                                try
                                {
                                    property.SetValue(this, readInValue);
                                    this.NotifyOfPropertyChange(property.Name);
                                }
                                catch (Exception ex)
                                {
                                    Exceptions.Add(ex);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }

        public void SaveViewModelToFirstJson()
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                };

                string firstPresetFile = GetFile(UserSetingsFolder, FileNamePrefix, _fileExension);
                if (!String.IsNullOrEmpty(firstPresetFile))
                {
                    string json = JsonConvert.SerializeObject(this, settings);
                    File.WriteAllText(firstPresetFile, json);
                }
            }
            catch (Exception ex)
            {
                Exceptions.Add(ex);
            }
        }


        public static string FindFirstFileInFolder(string folderPath, string fileNamePrefix, string fileExtension)
        {
            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            string[] matchingFiles = Directory.GetFiles(folderPath,
                $"{fileNamePrefix}*{fileExtension}");

            if (matchingFiles.Length > 0)
            {
                return matchingFiles[0];
            }
            else
            {
                return null; // No matching files found
            }
        }

        public static string GetFile(string folderPath, string fileNamePrefix, string fileExtension)
        {
            string existingFile = FindFirstFileInFolder(folderPath, fileNamePrefix, fileExtension);
            if (!String.IsNullOrEmpty(existingFile))
            {
                return existingFile;
            }
            var newFile = System.IO.Path.Combine(folderPath, fileNamePrefix + fileExtension);
            return newFile;
        }

    }
}
