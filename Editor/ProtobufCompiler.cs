using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Google.Protobuf.Config;

namespace Google.Protobuf.Editor
{
    public class ProtobufCompiler : AssetPostprocessor
    {
        private const string PackageName = "com.gameworkstore.googleprotobufunity";
#if UNITY_EDITOR_OSX
        private const string BinaryRelativePath = "Binary/MacOS/";
#else
        private const string BinaryRelativePath = "Binary/Win/";
#endif

        private const string ProtobufCompilerLog = "[" + nameof(ProtobufCompiler) + "]:";
        private const string ProtobufConfigAssetDatabaseFilter = "t:" + nameof(ProtobufConfig);

        /// <summary>
        /// Path to the file of all protobuf files in your Unity folder.
        /// </summary>
        private static string[] AllProtos
        {
            get
            {
                return Directory.GetFiles(Application.dataPath, "*.proto", SearchOption.AllDirectories);
            }
        }

        /// <summary>
        /// A parent folder of all protobuf files found in your Unity project collected together.
        /// This means all .proto files in Unity could import each other freely even if they are far apart.
        /// </summary>
        private static IEnumerable<string> IncludePaths
        {
            get
            {
                return AllProtos.Select(t => Path.GetDirectoryName(t)).Distinct();
            }
        }

        private static string BinaryPathCache = string.Empty;
        private static string BinaryPath
        {
            get
            {
                if (string.IsNullOrEmpty(BinaryPathCache))
                {
                    var libraryPath = Directory.GetDirectories("Library/PackageCache/").FirstOrDefault(IsPackagePath);
                    if (string.IsNullOrEmpty(libraryPath))
                    {
                        BinaryPathCache = Path.Combine(Application.dataPath, "Package", BinaryRelativePath);
                    }
                    else
                    {
                        BinaryPathCache = Path.Combine(Directory.GetParent(Application.dataPath).FullName, libraryPath, BinaryRelativePath);
                    }
                }
                return BinaryPathCache;
            }
        }

        private static string ProtocPath
        {
            get
            {
#if UNITY_EDITOR_OSX
                return "/usr/local/bin/protoc";
#else
                return Path.Combine(BinaryPath,"protoc");
#endif
            }
        }

        private static bool IsPackagePath(string path)
        {
            return path.Contains(PackageName);
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!UnityEditorInternal.InternalEditorUtility.isHumanControllingUs) return;
            
            var anyChanges = false;
            foreach (string import in importedAssets)
            {
                if(!import.EndsWith(".proto")) continue;
                var absolutePath = RelativeToAbsolute(import);
                var config = ProtobufCompiler.GetProtobufConfig(absolutePath);
                if(config == null)
                {
                    config = ProtobufCompiler.GetProtobufConfig();
                }
                anyChanges |= Compile(absolutePath, config);
            }
            if (anyChanges)
            {
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Called from Force Compilation button in the prefs.
        /// </summary>
        internal static void ForceRecompile()
        {
            if (ProtobufPreferences.LogDebug)
            {
                UnityEngine.Debug.Log(ProtobufCompilerLog+"Compiling all .proto files in the project...");
            }

            foreach (string protoFile in AllProtos)
            {
                if (ProtobufPreferences.LogDebug)
                {
                    UnityEngine.Debug.Log(ProtobufCompilerLog+"Compiling " + protoFile);
                }
                var config = ProtobufCompiler.GetProtobufConfig(protoFile);
                if(config == null)
                {
                    config = ProtobufCompiler.GetProtobufConfig();
                }
                Compile(protoFile,config);
            }
            AssetDatabase.Refresh();
        }

        public static IEnumerable<ProtobufCompilerConfig> GetCompilerConfigs(ProtobufConfig config)
        {
            var configs = new List<ProtobufCompilerConfig>();
            if (config.CSharpCompilerEnabled)
            {
                configs.Add(new ProtobufCompilerConfig()
                {
                    CompilerFormat = "--csharp_out=\"{0}\"",
                    TargetLocation = config.CSharpCustomPath,
                    Extension = ".cs"
                });
            }
            if (config.GoLangCompilerEnabled)
            {
                configs.Add(new ProtobufCompilerConfig()
                {
                    CompilerFormat = "--go_out=paths=source_relative:{0}",
                    TargetLocation = config.GoLangCustomPath,
                    Extension = ".pb.go"
                });
            }
			if (config.PythonCompilerEnabled)
			{
                configs.Add(new ProtobufCompilerConfig()
                {
                    CompilerFormat = "--python_out=\"{0}\"",
                    TargetLocation = config.PythonCustomPath,
                    Extension = ".py"
                });
            }

            return configs;
        }

        /// <summary>
        /// Compiles a .proto file inside Assets folder
        /// </summary>
        /// <param name="absolutePath">Absolute path to the .proto file.</param>
        /// <param name="config">Target configuration</param>
        public static bool Compile(string absolutePath, ProtobufConfig protobufConfig)
        {
            if (!protobufConfig.ProtocolCompilerEnabled) return false;
            //standalize path
            var aPath = Path.GetFullPath(absolutePath);
            var vPath = Path.GetFullPath(Application.dataPath);
            if (!aPath.StartsWith(vPath))
            {
                if (ProtobufPreferences.LogErrors)
                {
                    Debug.LogError(ProtobufCompilerLog + "Cannot compile " + absolutePath + " because isn't inside Assets folder");
                    return false;
                }
                return false;
            }
            if (!absolutePath.EndsWith(".proto"))
            {
                if (ProtobufPreferences.LogErrors)
                {
                    Debug.LogError(ProtobufCompilerLog + "Cannot compile " + absolutePath + " because isn't a protobuf file (.proto)");
                    return false;
                }
                return false;
            }

            var compilingFile = string.Format(" \"{0}\"", absolutePath);
            var parentFolder = Directory.GetParent(absolutePath).FullName;

            var includes = string.Empty;
            foreach (string include in IncludePaths)
            {
                includes += string.Format(" --proto_path=\"{0}\"", include);
            }

            foreach (var config in GetCompilerConfigs(protobufConfig))
            {
                var absoluteOutput = string.IsNullOrEmpty(config.TargetLocation) ? parentFolder : RelativeToAbsolute(config.TargetLocation);
                if (!Directory.Exists(absoluteOutput))
                {
                    Directory.CreateDirectory(absoluteOutput);
                }
                var compilationArguments = string.Format(config.CompilerFormat, absoluteOutput) + includes + compilingFile;
                if (ProtobufPreferences.LogDebug)
                {
                    UnityEngine.Debug.Log("[ProtobufCompiler]: Compilation Arguments\n" + compilationArguments);
                }

                var startInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = ProtocPath,
                    Arguments = compilationArguments,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
#if UNITY_EDITOR_OSX
                startInfo.EnvironmentVariables["PATH"] += ":/usr/local/bin";
#endif


                var proc = new System.Diagnostics.Process() { StartInfo = startInfo };
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (ProtobufPreferences.LogDebug && !string.IsNullOrEmpty(output))
                {
                    Debug.Log(ProtobufCompilerLog + output);
                }

                if (ProtobufPreferences.LogErrors && !string.IsNullOrEmpty(error))
                {
                    Debug.LogError(ProtobufCompilerLog + error);
                    return false;
                }

                if(File.Exists(absoluteOutput))
                {
                    AssetDatabase.ImportAsset(AbsoluteToRelative(absoluteOutput));
                }
            }

            return true;
        }

        public const string ProtobufTemplate =
            "syntax = \"proto3\";\n" +
            "option optimize_for = LITE_RUNTIME;\n" +
            "option go_package = \"example.com;main\";" +
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

        /// <summary>
        /// Creates a .proto file
        /// </summary>
        /// <param name="path">Full path to the new .proto</param>
        public static void CreateProtobufFile(string path)
        {
            if (!path.EndsWith(".proto"))
            {
                Debug.LogError(ProtobufCompilerLog + "path must ends with .proto extension");
                return;
            }

            var dir = Directory.GetParent(path);
            if (!Directory.Exists(dir.FullName))
            {
                Directory.CreateDirectory(dir.FullName);
            }

            File.WriteAllText(path, ProtobufTemplate);
            AssetDatabase.Refresh();
        }

        /// <summary>
		/// Retrieve Global ProtobufConfig, if exists
		/// </summary>
		/// <returns>reference to ProtobufConfig or null</returns>
        public static ProtobufConfig GetProtobufConfig()
        {
            var finds = AssetDatabase.FindAssets(ProtobufConfigAssetDatabaseFilter);
            var paths = finds.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            var selects = paths.Select(path => AssetDatabase.LoadAssetAtPath<ProtobufConfig>(path));
            return selects.FirstOrDefault();
        }

        /// <summary>
        /// Retrieve Local ProtobufConfig, if exists
        /// </summary>
        /// <param name="protoFile">Full path to the .proto</param>
        /// <returns></returns>
        public static ProtobufConfig GetProtobufConfig(string protoFilePath)
        {
            if (!File.Exists(protoFilePath)) return null;
            var dir = AbsoluteToRelative(Directory.GetParent(protoFilePath).FullName);
            var finds = AssetDatabase.FindAssets(ProtobufConfigAssetDatabaseFilter, new string[] { dir });
            var paths = finds.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            var selects = paths.Select(path => AssetDatabase.LoadAssetAtPath<ProtobufConfig>(path));
            return selects.FirstOrDefault();
        }

        /// <summary>
        /// Converts absolute path (System.File) to relative path (UnityEditor.AssetDatabase)
        /// </summary>
        /// <param name="absolutePath">Absolute path</param>
        /// <returns>Relative path or string.Empty if absolutePath is invalid.</returns>
        public static string AbsoluteToRelative(string absolutePath)
        {
            var fileUri = new System.Uri(absolutePath);
            var referenceUri = new System.Uri(Application.dataPath);
            return System.Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Converts relative path (UnityEditor.AssetDatabase) to absolute path (System.File)
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <returns>Absolute path or string.Empty if relativePath is invalid.</returns>
        public static string RelativeToAbsolute(string relativePath)
        {
            if (relativePath.StartsWith("Assets/"))
            {
                return Path.Combine(Directory.GetParent(Application.dataPath).FullName, relativePath);
            }
            return Path.Combine(Application.dataPath, relativePath);
        }
    }
}

public struct ProtobufCompilerConfig
{
    public string CompilerFormat;
    public string TargetLocation;
    internal string Extension;
}