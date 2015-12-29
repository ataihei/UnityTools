﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ProjectModels
{
    /// <summary>
    /// Manipulate a *.csproj file.
    /// </summary>
    public class Csproj : ChildItem
    {
        private const string PackagesConfName = "packages.config";

        public static readonly XNamespace Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public Csproj(string basePath, string relativePath)
            : base(basePath, relativePath)
        {
        }

        /// <summary>
        /// Get moniker of Target Framework Version.
        /// </summary>
        public string TargetFrameworkVersion => _target ?? (_target = ReadTargetFrameworkVersion(Path));
        private string _target;

        private static string ReadTargetFrameworkVersion(string csprojPath)
        {
            var targetVersionLine = File.ReadAllLines(csprojPath).First(x => x.Contains("TargetFrameworkVersion"));

            var targetVersion =
                targetVersionLine.Contains("v2.0") ? "net20" :
                targetVersionLine.Contains("v3.0") ? "net30" :
                targetVersionLine.Contains("v3.5") ? "net35" :
                targetVersionLine.Contains("v4.5") ? "net45" :
                targetVersionLine.Contains("v4.6") ? "net46" :
                "dotnet";
            return targetVersion;
        }

        /// <summary>
        /// Save changes in <see cref="Content"/>
        /// </summary>
        public void Save()
        {
            if(_content != null)
            {
                _content.Save(Path);
            }
        }

        private string PackagesConfigPath => System.IO.Path.Combine(Folder, PackagesConfName);

        /// <summary>
        /// true if this csproj contains a packages.config file.
        /// </summary>
        public bool HasPackagesConfig => File.Exists(PackagesConfigPath);

        /// <summary>
        /// Content in packages.config
        /// </summary>
        public PackagesConfig PackagesConfig => _config ?? (_config = new PackagesConfig(PackagesConfigPath));
        private PackagesConfig _config;

        private string ProjectJsonPath => System.IO.Path.Combine(Folder, ProjectJson.ProjectJsonName);

        /// <summary>
        /// true if this csproj contains a packages.config file.
        /// </summary>
        public bool HasProjectJson => File.Exists(ProjectJsonPath);

        /// <summary>
        /// Content in packages.config
        /// </summary>
        public ProjectJson ProjectJson => _proj ?? (_proj = new ProjectJson(ProjectJsonPath));
        private ProjectJson _proj;

        /// <summary>
        /// Contents in this csproj file.
        /// </summary>
        public XDocument Content => _content ?? (_content = XDocument.Load(Path));
        private XDocument _content;

        public CsprojOutputType OutputType
        {
            get
            {
                var type = Content.Root.Elements(Namespace + "PropertyGroup").SelectMany(x => x.Elements(Namespace + "OutputType")).FirstOrDefault()?.Value;

                switch (type)
                {
                    default:
                    case "Exe": return CsprojOutputType.Exe;
                    case "WinExe": return CsprojOutputType.WinExe;
                    case "Library": return CsprojOutputType.Library;
                }
            }
        }

        public IEnumerable<XElement> GetItemGroups() => Content.Root.Elements(Namespace + "ItemGroup");

        public IEnumerable<XElement> GetElements(string elementName) => GetItemGroups().SelectMany(g => g.Elements(Namespace + elementName)).ToArray();

        /// <summary>
        /// Enumerate *.cs files in this project.
        /// </summary>
        public IEnumerable<ChildItem> CsFiles => _csFiles ?? (_csFiles = GetCsFiles());
        private IEnumerable<ChildItem> _csFiles;

        private IEnumerable<ChildItem> GetCsFiles()
        {
            foreach (var c in GetElements("Compile"))
            {
                var item = c.Attribute("Include").Value;
                if(item.EndsWith(".cs"))
                {
                    yield return new ChildItem(Folder, item);
                }
            }
        }

        /// <summary>
        /// Enumerate depenndencies:
        /// - <see cref="ProjectReferences"/>
        /// - <see cref="ProjectJson.Packages"/>
        /// - <see cref="PackagesConfig.Packages"/>
        /// </summary>
        public IEnumerable<Package> Dependencies => _dependencies ?? (_dependencies = GetDependencies());
        private IEnumerable<Package> _dependencies;

        private IEnumerable<Package> GetDependencies()
        {
            foreach (var r in ProjectReferences)
            {
                yield return new Package(r, "1.0.0-*");
            }

            if(HasPackagesConfig)
                foreach (var p in PackagesConfig.Packages)
                    yield return p;

            if (HasProjectJson)
                foreach (var p in ProjectJson.Packages)
                    yield return p;
        }

        /// <summary>
        /// Enumerate project references.
        /// </summary>
        public IEnumerable<string> ProjectReferences => _references ?? (_references = GetProjectReferences());
        private IEnumerable<string> _references;

        private IEnumerable<string> GetProjectReferences()
        {
            foreach (var r in GetElements("ProjectReference"))
            {
                var path = r.Attribute("Include").Value;
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                yield return name;
            }
        }

        /// <summary>
        /// Rewrite this csproj file to migrate from packages.config to project.json:
        /// - Generate project.json
        /// - Replace {None Include="packages.confing"} to {None Include="project.json"}
        /// - Remove {References} reletated to NuGet packages
        /// - Remove packages.config
        /// </summary>
        /// <returns>true if any change</returns>
        public bool MigrateToProjectJson()
        {
            if (!HasPackagesConfig)
                return false;

            ProjectJson.GeneratePackageJson(TargetFrameworkVersion, Folder, PackagesConfig.Packages);

            foreach (var r in GetElements("Reference"))
            {
                if (PackagesConfig.Packages.Any(x => r.Attribute("Include").Value.StartsWith(x.Id + ",")))
                {
                    r.Remove();
                }
            }

            foreach (var n in GetElements("None"))
            {
                var include = n.Attribute("Include");
                if (include.Value == PackagesConfName)
                {
                    n.ReplaceAll(new XAttribute("Include", ProjectJson.ProjectJsonName));
                }
            }

            File.Delete(PackagesConfig.Path);

            return true;
        }

        /// <summary>
        /// Generate a wrap project.json file for *.xproj.
        /// </summary>
        /// <param name="wrapFolder"></param>
        public void GenerateWrapJson(string wrapFolder)
        {
            ProjectJson.GenerateWrapJson(TargetFrameworkVersion, wrapFolder, RelativePath, Dependencies);
        }

        /// <summary>
        /// Copy all C# files.
        /// </summary>
        /// <param name="destinationFolder">A folder to which files are copied.</param>
        /// <param name="createProjectFolder">if true, create folder in <paramref name="destinationFolder"/> with <see cref="Item.Name"/>.</param>
        public void CopySources(string destinationFolder, bool createProjectFolder = true)
        {
            var baseFolder = createProjectFolder ?
                System.IO.Path.Combine(destinationFolder, NameWithoutExtension) :
                destinationFolder;

            foreach (var cs in CsFiles)
            {
                if (cs.Name == "AssemblyInfo.cs") continue;

                var relative = cs.RelativePath.Replace("..", "parent");
                var dest = System.IO.Path.Combine(baseFolder, relative);
                var csFolder = System.IO.Path.GetDirectoryName(dest);

                Directory.CreateDirectory(csFolder);

                File.Copy(cs.Path, dest);
            }
        }
    }
}
