#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GameWorkstore.Google.Protobuf
{
    public static class ProtobufPreferences
    {
        private const string prefProtocEnable = "protobufcompiler_enable";
        private const string prefLogError = "protobufcompiler_logerror";
        private const string prefLogDebug = "protobufcompiler_logstandard";

        private const string csharpEnabled = "protobufcompiler_csharpenabled";
        private const string csharpCustomPath = "protobufcompiler_csharpcustompath";

        private const string goLangEnabled = "protobufcompiler_golangenabled";
        private const string goLangCustomPath = "protobufcompiler_golangcustompath";

        internal static bool IsEnabled
        {
            get
            {
                return EditorPrefs.GetBool(prefProtocEnable, true);
            }
            set
            {
                EditorPrefs.SetBool(prefProtocEnable, value);
            }
        }

        internal static bool LogErrors
        {
            get
            {
                return EditorPrefs.GetBool(prefLogError, true);
            }
            set
            {
                EditorPrefs.SetBool(prefLogError, value);
            }
        }

        internal static bool LogDebug
        {
            get
            {
                return EditorPrefs.GetBool(prefLogDebug, false);
            }
            set
            {
                EditorPrefs.SetBool(prefLogDebug, value);
            }
        }

        internal static bool CSharpEnabled
        {
            get
            {
                return EditorPrefs.GetBool(csharpEnabled, true);
            }
            set
            {
                EditorPrefs.SetBool(csharpEnabled, value);
            }
        }

        internal static string CSharpCustomPath
        {
            get
            {
                return EditorPrefs.GetString(csharpCustomPath);
            }
            set
            {
                EditorPrefs.SetString(csharpCustomPath, value);
            }
        }

        internal static bool GoLangEnabled
        {
            get
            {
                return EditorPrefs.GetBool(goLangEnabled, true);
            }
            set
            {
                EditorPrefs.SetBool(goLangEnabled, value);
            }
        }

        internal static string GoLangCustomPath
        {
            get
            {
                return EditorPrefs.GetString(goLangCustomPath);
            }
            set
            {
                EditorPrefs.SetString(goLangCustomPath, value);
            }
        }

        internal class ProtobufUnitySettingsProvider : SettingsProvider
        {
            public ProtobufUnitySettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope)
            {
            }

            public override void OnGUI(string searchContext)
            {
                ProtobufPreference();
            }

            [SettingsProvider]
            public static SettingsProvider ProtobufPreferenceSettingsProvider()
            {
                return new ProtobufUnitySettingsProvider("Preferences/Protobuf");
            }
        }

        private static void ProtobufPreference()
        {
            EditorGUI.BeginChangeCheck();

            IsEnabled = EditorGUILayout.ToggleLeft(new GUIContent("Enable Protobuf Compiler", ""), IsEnabled);

            EditorGUI.BeginDisabledGroup(!IsEnabled);
            LogErrors = EditorGUILayout.Toggle(new GUIContent("Log Error Output", "Log compilation errors from protoc command."), LogErrors);
            LogDebug = EditorGUILayout.Toggle(new GUIContent("Log Standard Output", "Log compilation completion messages."), LogDebug);
            EditorGUILayout.Space();

            CSharpEnabled = EditorGUILayout.Toggle(new GUIContent("Compile C#", "If C# Compilation is enabled."), CSharpEnabled);
            CSharpCustomPath = EditorGUILayout.TextField(new GUIContent("Compile C# Custom Path"), CSharpCustomPath);
            GoLangEnabled = EditorGUILayout.Toggle(new GUIContent("Compile GoLang", "If Go Compilation is enabled."), GoLangEnabled);
            GoLangCustomPath = EditorGUILayout.TextField(new GUIContent("Compile GoLang Custom Path"), GoLangCustomPath);

            EditorGUILayout.HelpBox("Leave custom paths empty if it should compile right next to original.", MessageType.Info);

            if (GUILayout.Button(new GUIContent("Force Compilation")))
            {
                ProtobufCompiler.ForceRecompile();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndChangeCheck();
        }
    }
}
#endif