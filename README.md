# Google Protobuf C# Library for Unity

Unity NPM version of Google Protobuf! This repository applies the same license terms of the original version.

Original repo: [protocolbuffers](https://github.com/protocolbuffers/protobuf) 

# Current Version

You can find at Assets/Package/CHANGES.txt

# Updating this package:

This package will receive updates from time to time.
To do this:

1) Replace Google.Protobuf folder for the original Root > csharp > src > Google.Protobuf
2) Replace CHANGES.txt for the original at Root.
3) Replace Assets/Package/Protoc/{versions} for related OS versions (current MacOS 64, Windows 64 and Linux 64) from original repo.

# How to install

At package.json, add these line of code:
> "com.gameworkstore.googleprotobufunity": "git://github.com:GameWorkstore/google-protobuf-unity.git"

And wait for unity to download and compile the package.

for update package for a newer version, install UpmGitExtension and update on [ Window > Package Manager ]!
> https://github.com/mob-sakai/UpmGitExtension

# Preinstalled Protoc

The installed version of protoc is Windows 64 bits.

