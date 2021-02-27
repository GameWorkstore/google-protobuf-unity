#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Google.Protobuf.Config;

namespace GameWorkstore.Google.Protobuf
{
    public class ProtobufCompiler : AssetPostprocessor
    {

        private const string PackageName = "com.gameworkstore.googleprotobufunity";
#if UNITY_EDITOR_OSX
        private const string ProtocRelativePath = "Protoc/MacOS/protoc";
#else
        private const string ProtocRelativePath = "Protoc/Win/protoc.exe";
#endif

        /// <summary>
        /// Path to the file of all protobuf files in your Unity folder.
        /// </summary>
        private static string[] AllProtoFiles
        {
            get
            {
                string[] protoFiles = Directory.GetFiles(Application.dataPath, "*.proto", SearchOption.AllDirectories);
                return protoFiles;
            }
        }

        /// <summary>
        /// A parent folder of all protobuf files found in your Unity project collected together.
        /// This means all .proto files in Unity could import each other freely even if they are far apart.
        /// </summary>
        private static string[] IncludePaths
        {
            get
            {
                string[] protoFiles = AllProtoFiles;

                string[] includePaths = new string[protoFiles.Length];
                for (int i = 0; i < protoFiles.Length; i++)
                {
                    string protoFolder = Path.GetDirectoryName(protoFiles[i]);
                    includePaths[i] = protoFolder;
                }
                return includePaths;
            }
        }

        private static string PackagePath = string.Empty;
        private static string ProtocPath
        {
            get
            {
                if (string.IsNullOrEmpty(PackagePath))
                {
                    var libraryPath = Directory.GetDirectories("Library/PackageCache/").FirstOrDefault(IsPackagePath);
                    if (string.IsNullOrEmpty(libraryPath))
                    {
                        PackagePath = Path.Combine(Application.dataPath, "Package", ProtocRelativePath);
                    }
                    else
                    {
                        PackagePath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, libraryPath, ProtocRelativePath);
                    }
                }
                return PackagePath;
            }
        }

        private static bool IsPackagePath(string path)
        {
            return path.Contains(PackageName);
        }

        private static ProtobufConfig GetProtobufConfig()
        {
            var finds = AssetDatabase.FindAssets("t:GoogleProtobufConfig");
            var paths = finds.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            var selects = paths.Select(path => AssetDatabase.LoadAssetAtPath<ProtobufConfig>(path));
            return selects.FirstOrDefault();
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!UnityEditorInternal.InternalEditorUtility.isHumanControllingUs) return;
            if (!File.Exists(ProtocPath)) return;
            var config = GetProtobufConfig();
            if (config == null) return;
            if (!config.ProtocolCompilerEnabled) return;

            var langConfigs = GetCompilerConfigs(config);

            var anyChanges = false;
            foreach (string str in importedAssets)
            {
                if (CompileProtobufAssetPath(langConfigs, str, IncludePaths) == true)
                {
                    anyChanges = true;
                }
            }

            if (anyChanges)
            {
                UnityEngine.Debug.Log(nameof(ProtobufCompiler));
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Called from Force Compilation button in the prefs.
        /// </summary>
        internal static void ForceRecompile()
        {
            if (!File.Exists(ProtocPath)) return;
            var config = GetProtobufConfig();
            if (config == null) return;
            if (!config.ProtocolCompilerEnabled) return;

            var langConfigs = GetCompilerConfigs(config);

            if (ProtobufPreferences.LogDebug)
            {
                UnityEngine.Debug.Log("[ProtobufCompiler]: Compiling all .proto files in the project...");
            }

            foreach (string protoFile in AllProtoFiles)
            {
                if (ProtobufPreferences.LogDebug)
                {
                    UnityEngine.Debug.Log("[ProtobufCompiler]: Compiling " + protoFile);
                }
                CompileProtobufSystemPath(langConfigs, protoFile, IncludePaths);
            }
            UnityEngine.Debug.Log(nameof(ProtobufCompiler));
            AssetDatabase.Refresh();
        }

        private static bool CompileProtobufAssetPath(IEnumerable<ProtobufCompilerConfig> langConfigs, string assetPath, string[] includePaths)
        {
            string protoFileSystemPath = Directory.GetParent(Application.dataPath) + Path.DirectorySeparatorChar.ToString() + assetPath;
            return CompileProtobufSystemPath(langConfigs, protoFileSystemPath, includePaths);
        }

        private static readonly string ProjectPath = Directory.GetParent(Application.dataPath).FullName + Path.DirectorySeparatorChar.ToString();

        private static bool CompileProtobufSystemPath(IEnumerable<ProtobufCompilerConfig> langConfigs, string absoluteProtoFilePath, string[] includePaths)
        {
            //Do not compile changes coming from UPM package.
            if (absoluteProtoFilePath.Contains("Packages/com.")) return false;

            if (Path.GetExtension(absoluteProtoFilePath) != ".proto") return false;

            var absoluteNeightborOutputPath = Path.GetDirectoryName(absoluteProtoFilePath);

            var compilingFile = string.Format(" \"{0}\"", absoluteProtoFilePath);
            var includes = string.Empty;
            foreach (string include in includePaths)
            {
                includes += string.Format(" --proto_path=\"{0}\"", include);
            }

            foreach (var langConfig in langConfigs)
            {
                var absoluteLocationOutput = string.IsNullOrEmpty(langConfig.RelativeLocation) ? absoluteNeightborOutputPath : Application.dataPath + "/" + langConfig.RelativeLocation;
                if (!Directory.Exists(absoluteLocationOutput))
                {
                    Directory.CreateDirectory(absoluteLocationOutput);
                }
                var compiledOutput = string.Format(langConfig.Lang, absoluteLocationOutput);

                var finalArguments = compiledOutput + includes + compilingFile;
                if (ProtobufPreferences.LogDebug)
                {
                    UnityEngine.Debug.Log("[ProtobufCompiler]: Final Arguments\n" + finalArguments);
                }

                var startInfo = new ProcessStartInfo()
                {
                    FileName = ProtocPath,
                    Arguments = finalArguments,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                var proc = new Process() { StartInfo = startInfo };
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                var generatedFile = "Assets/" + langConfig.RelativeLocation + Path.GetFileNameWithoutExtension(absoluteProtoFilePath) + langConfig.Extension;
                if (ProtobufPreferences.LogDebug)
                {
                    if (output != "")
                    {
                        UnityEngine.Debug.Log("Protobuf Unity : " + output);
                    }
                    UnityEngine.Debug.Log("[ProtobufCompiler]: Compiled " + Path.GetFileName(absoluteProtoFilePath));
                    UnityEngine.Debug.Log("[ProtobufCompiler]: Generated " + generatedFile);
                }

                if (ProtobufPreferences.LogErrors && error != "")
                {
                    UnityEngine.Debug.LogError("[ProtobufCompiler]: " + error);
                }
                AssetDatabase.ImportAsset(generatedFile);
            }

            // Checking if the user has set valid path (there is probably a better way)
            //if (ProtobufPreferences.grpcPath != "ProtobufUnity_GrpcPath" && ProtobufPreferences.grpcPath != string.Empty)
            //options += $" --grpc_out={outputPath} --plugin=protoc-gen-grpc={ProtobufPreferences.grpcPath}";
            //string combinedPath = string.Join(" ", optionFiles.Concat(new string[] { protoFileSystemPath }));
            return true;
        }

        private static readonly List<ProtobufCompilerConfig> configs = new List<ProtobufCompilerConfig>();

        public static IEnumerable<ProtobufCompilerConfig> GetCompilerConfigs(ProtobufConfig config)
        {
            configs.Clear();
            if (config.CSharpCompilerEnabled)
            {
                configs.Add(new ProtobufCompilerConfig()
                {
                    Lang = "--csharp_out=\"{0}\"",
                    RelativeLocation = config.CSharpCustomPath,
                    Extension = ".cs"
                });
            }
            if (config.GoLangCompilerEnabled)
            {
                configs.Add(new ProtobufCompilerConfig()
                {
                    Lang = "--go_out=paths=source_relative:{0}",
                    RelativeLocation = config.GoLangCustomPath,
                    Extension = ".pb.go"
                });
            }

            return configs;
        }

        public const string ProtobufTemplate =
            "syntax = \"proto3\";\n" +
            "option optimize_for = LITE_RUNTIME;\n" +
            "option go_package = \"./ main\";"+
            "\n" +
            "package main;\n" +
            "\n" +
            "message NewMessege\n" +
            "{\n" +
            "   string NewField = 1;\n" +
            "}\n";

        [MenuItem("Assets/Create/Protobuf/Protobuf (.proto)")]
        public static void CreateProtobufFile()
        {
            var target = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (AssetDatabase.IsValidFolder(target))
            {
                var path = target + "/newprotobuf.proto";
                ProjectWindowUtil.CreateAssetWithContent(path, ProtobufTemplate);
            }
            else if (File.Exists(target))
            {
                target = target.Remove(target.LastIndexOf(Path.DirectorySeparatorChar));
                var path = target + "/newprotobuf.proto";
                ProjectWindowUtil.CreateAssetWithContent(path, ProtobufTemplate);
            }
        }
    }
}

public struct ProtobufCompilerConfig
{
    public string Lang;
    public string RelativeLocation;
    internal string Extension;
}
#endif