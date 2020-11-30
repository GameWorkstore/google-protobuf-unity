﻿using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameWorkstore.Google.Protobuf
{
    public static class ProtobufPreferences
    {
        internal const string prefProtocEnable = "protobufcompiler_enable";
        internal const string prefLogError = "protobufcompiler_logerror";
        internal const string prefLogDebug = "protobufcompiler_logstandard";

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

            if (GUILayout.Button(new GUIContent("Force Compilation")))
            {
                ProtobufCompiler.ForceRecompile();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndChangeCheck();
        }
    }
}