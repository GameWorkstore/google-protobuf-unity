#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;

namespace GameWorkstore.Google.Protobuf
{
    public class PackageUpdater
    {
        [MenuItem("Help/PackageUpdate/GameWorkstore.GoogleProtobufCsharp")]
        public static void TrackPackages()
        {
            Client.Add("git://github.com:GameWorkstore/googleprotobufcsharp.git");
        }
    }
}
#endif