# UO Moons

[![GitHub build](https://img.shields.io/github/workflow/status/UO-Moons/UOMoons/Build?logo=github)](https://github.com/UO-Moons/UOMoons/actions)
[![GitHub issues](https://img.shields.io/github/issues/UO-Moons/UOMoons.svg)](https://github.com/UO-Moons/UOMoons/issues)
[![GitHub last commit](https://img.shields.io/github/last-commit/UO-Moons/UOMoons.svg)](https://github.com/UO-Moons/UOMoons/)
[![Github code lines](https://img.shields.io/tokei/lines/github/UO-Moons/UOMoons.svg)](https://github.com/UO-Moons/UOMoons/)
[![GitHub repo size](https://img.shields.io/github/repo-size/UO-Moons/UOMoons.svg)](https://github.com/UO-Moons/UOMoons/)

UOMOons is an Ultima Online server emulator based on RunUO [https://github.com/runuo/runuo]
The main objective of UOMoons is to update the server code to new technologies, improve the performance and give support for better customization.

For change Era configuration and some basic server configuration you can use the ```bin\settings.ini``` file
## Era Support
```
- UO:
- T2A: 		Released on October 24, 1998
- UOR: 		Released on May 4, 2000
- UOTD:		Released on March 27, 2001
- UOLBR:	Released on February 24, 2002
- AOS:		Release on February 11, 2003
- SE:		Released on November 2, 2004
- ML: 		Released on August 30, 2005
```

## .Net 6 Support
Change the core to work with .Net 6.

## Requirements to Compile
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)


### Requirements for run in Ubuntu/Debian

```shell
apt-get install zlib1g-dev
```

## Run

```shell
dotnet run bin/UOMoons.dll
```

### Misc

UOMoons supports Intel's hardware random number generator (Secure Key, Bull Mountain, rdrand, etc).
If rdrand32.dll/rdrand64.dll are present in the base directory and the hardware supports that functionality, it will be used automatically. You can find those libraries here: https://github.com/msturgill/rdrand/releases/latest
