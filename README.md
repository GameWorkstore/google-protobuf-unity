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

## Unity

At package.json, add these line of code:
> "com.gameworkstore.googleprotobufunity": "https://github.com/GameWorkstore/google-protobuf-unity.git#3.15.2012"

And wait for unity to download and compile the package.

for update package for a newer version, update end of line from 3.15.2012 to any released version on Releases.

## Unreal

In Progress

# Installing Protoc

### Windows

This package comes with preinstalled protoc for Windows 64 bits.

### MacOS

Install protobuf using brew

> brew install protobuf

Install protoc-gen-go using brew

> brew install protoc-gen-go

While the package contains solutions to fix PATH by itself,
if you run into problems, follow this advice:

Brew must simlink both protoc and protoc-gen-go binaries into /usr/local/bin folder.
Use the button on preferences to debug your PATH variable and see which folders are available.
Fixing this should get rid of any binary not found errors appearing while compiling.

# Configuring

There are two modes of configuration:

## Global
You must configure at least a ProtobufConfig file anywhere in the project folder to allow the ProtobufCompiler to compile .proto files.
This is the preferred one for simple projects.

## Local
ProtobufCompiler also accepts multiple ProtobufConfig.
Put one ProtobufConfig inside each folder containing your .proto files to allow it find them and compile on each custom path properly.
This method is used for a project with more complex configurations or sub-projects inside Assets folder.

#FAQ
### [ProtobufCompiler]:protoc-gen-go: program not found or is not executable
Please specify a program using absolute path or make sure the program is available in your PATH system variable
--go_out: protoc-gen-go: Plugin failed with status code 1.

This happens when your protoc and/or protoc-gen-go installation isn't configured properly. Verify PATH on windows,
or /usr/local/bin to see if all binaries are there.
