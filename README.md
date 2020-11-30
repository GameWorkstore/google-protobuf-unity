# Google Protobuf C# Library for Unity

Unity NPM version of Google Protobuf! This repository applies the same APACHE 2.0 license terms of the original version.

Original repo: https://github.com/protocolbuffers/protobuf

# Current Version

You can find at CHANGES.txt

# Updating this package:

This package will receive updates from time to time.
To do this:

1) Replace Google.Protobuf folder for the original Root > csharp > src > Google.Protobuf
2) Replace CHANGES.txt for the original at Root.
3) Replace Bin/protoc.exe for the Windows 64 bits version from original repo.

# How to install

At package.json, add these line of code:
> "com.gameworkstore.googleprotobufcsharp": "git://github.com:GameWorkstore/googleprotobufcsharp.git"

And wait for unity to download and compile the package.

for update package for a newer version, click in [Help->PackageUpdate->GameWorkstore.GoogleProtobufCsharp]

# Preinstalled Protoc

The installed version of protoc is Windows 64 bits.

# Modifications

Current version required to add modification to #define GOOGLE_PROTOBUF_SUPPORT_SYSTEM_MEMORY on top of few files:
* IBufferMessage.cs
* WritingPrimitives.cs

