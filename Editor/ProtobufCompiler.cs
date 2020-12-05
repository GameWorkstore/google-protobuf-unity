#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace GameWorkstore.Google.Protobuf
{
    public class ProtobufCompiler : AssetPostprocessor
    {
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
        private const string PackageName = "com.gameworkstore.googleprotobufcsharp";
        private const string ProtocRelativePath = "Protoc/Win64/protoc.exe";
        private static string ProtocPath
        {
            get
            {
                PackagePath = Directory.GetDirectories("Library/PackageCache/").FirstOrDefault(IsPackagePath);
                if (string.IsNullOrEmpty(PackagePath))
                {
                    return Application.dataPath + "/googleprotobufcsharp/" + ProtocRelativePath;
                }
                return Application.dataPath + "/../" + PackagePath + "/" + ProtocRelativePath;
            }
        }

        private static bool IsPackagePath(string path)
        {
            return path.Contains(PackageName);
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!ProtobufPreferences.IsEnabled) return;

            var anyChanges = false;
            foreach (string str in importedAssets)
            {
                if (CompileProtobufAssetPath(str, IncludePaths) == true)
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
            if (ProtobufPreferences.LogDebug)
            {
                UnityEngine.Debug.Log("Protobuf Unity : Compiling all .proto files in the project...");
            }

            foreach (string s in AllProtoFiles)
            {
                if (ProtobufPreferences.LogDebug)
                {
                    UnityEngine.Debug.Log("Protobuf Unity : Compiling " + s);
                }
                CompileProtobufSystemPath(s, IncludePaths);
            }
            UnityEngine.Debug.Log(nameof(ProtobufCompiler));
            AssetDatabase.Refresh();
        }

        private static bool CompileProtobufAssetPath(string assetPath, string[] includePaths)
        {
            string protoFileSystemPath = Directory.GetParent(Application.dataPath) + Path.DirectorySeparatorChar.ToString() + assetPath;
            return CompileProtobufSystemPath(protoFileSystemPath, includePaths);
        }

        private static bool CompileProtobufSystemPath(string protoFileSystemPath, string[] includePaths)
        {
            //Do not compile changes coming from UPM package.
            if (protoFileSystemPath.Contains("Packages/com.")) return false;

            if (Path.GetExtension(protoFileSystemPath) == ".proto")
            {
                string outputPath = Path.GetDirectoryName(protoFileSystemPath);

                string options = " --csharp_out \"{0}\" ";
                foreach (string s in includePaths)
                {
                    options += string.Format(" --proto_path \"{0}\" ", s);
                }

                // Checking if the user has set valid path (there is probably a better way)
                //if (ProtobufPreferences.grpcPath != "ProtobufUnity_GrpcPath" && ProtobufPreferences.grpcPath != string.Empty)
                    //options += $" --grpc_out={outputPath} --plugin=protoc-gen-grpc={ProtobufPreferences.grpcPath}";
                //string combinedPath = string.Join(" ", optionFiles.Concat(new string[] { protoFileSystemPath }));

                string finalArguments = string.Format("\"{0}\"", protoFileSystemPath) + string.Format(options, outputPath);

                if (ProtobufPreferences.LogDebug)
                {
                    UnityEngine.Debug.Log("Protobuf Unity : Final arguments :\n" + finalArguments);
                }

                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = ProtocPath,
                    Arguments = finalArguments
                };

                Process proc = new Process() { StartInfo = startInfo };
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (ProtobufPreferences.LogDebug)
                {
                    if (output != "")
                    {
                        UnityEngine.Debug.Log("Protobuf Unity : " + output);
                    }
                    UnityEngine.Debug.Log("Protobuf Unity : Compiled " + Path.GetFileName(protoFileSystemPath));
                }

                if (ProtobufPreferences.LogErrors && error != "")
                {
                    UnityEngine.Debug.LogError("Protobuf Unity : " + error);
                }
                return true;
            }
            return false;
        }

        public const string ProtobufTemplate =
            "syntax = \"proto3\";\n" +
            "option optimize_for = LITE_RUNTIME;\n" +
            "\n" +
            "package main;\n" +
            "\n" +
            "message NewMessege\n" +
            "{\n" +
            "   string NewField = 1;\n" +
            "}\n";

        [MenuItem("Assets/Create/Protobuf (.proto)")]
        public static void CreateProtobufFile()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            path += "/NewProtobuf.proto";

            ProjectWindowUtil.CreateAssetWithContent(path, ProtobufTemplate);
        }
    }
}
#endif