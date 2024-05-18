using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using cadwiki.NetUtils;

namespace cadwiki.AdminOps
{

    public partial class Form1
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var asm = Assembly.GetExecutingAssembly();
            var assemblyName = asm.GetName();
            var version = assemblyName.Version;
            string versionString = version.ToString();
            LabelCurrentVersion.Text = versionString;
            var newVersion = version;
            newVersion = newVersion.IncrementBuild();
            var time = DateTime.Now;
            string format = "HHmm";
            string timeStamp = time.ToString(format);
            newVersion = newVersion.SetVersion(newVersion.Major, newVersion.Minor, newVersion.Build, int.Parse(timeStamp));
            TextBoxNewVersion.Text = newVersion.ToString();

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var asm = Assembly.GetExecutingAssembly();
            string dllPath = asm.Location;
            string dllFolder = Path.GetDirectoryName(dllPath);
            string root = Paths.TryGetSolutionDirectoryPath(dllFolder);
            string repoFolder = new DirectoryInfo(root).Parent.FullName;
            var folders = Directory.GetDirectories(repoFolder, "*", SearchOption.AllDirectories).ToList();
            folders.Reverse();
            folders.Add(repoFolder);
            folders.Reverse();
            var projectsToUpdate = new List<string>() { "*cadwiki-nuget", "*CadDevToolsDriver*", "*cadwiki.AdminUtils*", "*cadwiki.CadDevTools*", "*cadwiki.DllReloader*", "*cadwiki.FileStore*", "*cadwiki.NetUtils*", "*cadwiki.NUnitTestRunner*", "*cadwiki.WpfUi*" };
            var wildCardPatterns = new List<string>() { "*apikeys.txt", "*README.nuget.commands.md", "*AssemblyInfo.vb", "*.nuspec", "*.targets" };
            foreach (var folder in folders)
            {
                string folderName = Path.GetFileName(folder);
                bool isThisAProjectToUpdate = false;
                foreach (var project in projectsToUpdate)
                {
                    if (StringContains(project, folder))
                    {
                        isThisAProjectToUpdate = true;
                        break;
                    }
                }

                if (isThisAProjectToUpdate)
                {
                    foreach (var fileName in Directory.GetFiles(folder))
                    {
                        foreach (var wildCardPattern in wildCardPatterns)
                        {
                            if (StringContains(wildCardPattern, fileName))
                            {
                                ReplaceText(LabelCurrentVersion.Text, TextBoxNewVersion.Text, fileName);
                            }
                        }
                    }
                }

            }

        }

        private void ReplaceText(string oldText, string newText, string fileName)
        {
            var lines = File.ReadAllLines(fileName).ToList();
            for (int index = 0, loopTo = lines.Count - 1; index <= loopTo; index++)
            {
                if (lines[index].Contains(oldText))
                {
                    lines[index] = lines[index].Replace(oldText, newText);
                }
            }
            File.WriteAllLines(fileName, lines.ToArray());
        }

        public bool StringContains(string searchPattern, string inputText)
        {
            string regexText = WildcardToRegex(searchPattern);
            var regex = new Regex(regexText, RegexOptions.IgnoreCase);

            if (regex.IsMatch(inputText))
            {
                return true;
            }
            return false;
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
        }

    }
}