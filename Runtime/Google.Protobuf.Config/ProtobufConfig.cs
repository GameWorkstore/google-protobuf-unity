using UnityEngine;

namespace Google.Protobuf.Config
{
    [CreateAssetMenu(fileName = "ProtobufConfig", menuName = "Protobuf/ProtobufConfig")]
    public class ProtobufConfig : ScriptableObject
    {
        public bool ProtocolCompilerEnabled = false;

        [Header("C#")]
        public bool CSharpCompilerEnabled = false;
        [Tooltip("Leave custom paths empty if it should compile right next to original.")]
        public string CSharpCustomPath = string.Empty;

        [Header("GoLang#")]
        public bool GoLangCompilerEnabled = false;
        [Tooltip("Leave custom paths empty if it should compile right next to original.")]
        public string GoLangCustomPath = string.Empty;

        [Header("Python#")]
        public bool PythonCompilerEnabled = false;
        [Tooltip("Make protobuf local inside python exportations. [ 'from google' --> 'from . google'] and [ 'import' --> 'from . import']")]
        public bool PythonLocalLibrary = false;
        [Tooltip("Leave custom paths empty if it should compile right next to original.")]
        public string PythonCustomPath = string.Empty;

        [Header("Cpp#")]
        public bool CppCompilerEnabled = false;
        [Tooltip("Leave custom paths empty if it should compile right next to original.")]
        public string CppCustomPath = string.Empty;
    }
}
