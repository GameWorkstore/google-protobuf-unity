using UnityEngine;

namespace Google.Protobuf.Config
{
    [CreateAssetMenu(fileName = "GoogleProtobufConfig", menuName = "GoogleProtobuf/GoogleProtobufConfig")]
    public class GoogleProtobufConfig : ScriptableObject
    {
        public bool ProtocolCompilerEnabled = true;

        [Header("C#")]
        public bool CSharpCompilerEnabled = true;
        public string CSharpCustomPath = string.Empty;

        [Header("GoLang#")]
        public bool GoLangCompilerEnabled = true;
        public string GoLangCustomPath = string.Empty;
    }
}
