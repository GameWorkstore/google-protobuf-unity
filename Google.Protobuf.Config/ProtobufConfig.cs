using UnityEngine;

namespace Google.Protobuf.Config
{
    [CreateAssetMenu(fileName = "ProtobufConfig", menuName = "Protobuf/Protobuf Config")]
    public class ProtobufConfig : ScriptableObject
    {
        public bool ProtocolCompilerEnabled = true;

        [Header("C#")]
        public bool CSharpCompilerEnabled = true;
        [Tooltip("Leave custom paths empty if it should compile right next to original.")]
        public string CSharpCustomPath = string.Empty;

        [Header("GoLang#")]
        public bool GoLangCompilerEnabled = true;
        [Tooltip("Leave custom paths empty if it should compile right next to original.")]
        public string GoLangCustomPath = string.Empty;
    }
}
