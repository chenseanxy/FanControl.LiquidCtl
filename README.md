# FanControl.LiquidCtl

[![Release](https://github.com/chenseanxy/FanControl.LiquidCtl/actions/workflows/release.yml/badge.svg)](https://github.com/chenseanxy/FanControl.LiquidCtl/actions/workflows/release.yml)

> A one-nighter project, born after me being annoyed by NZXT's stupid CAM software for 2 hours  
> Therefore compatibility might be problematic, only verified on NZXT Kraken X (X53, X63 or X73) devices.

**v0.1.0**: liquidctl section is now executed using pythonnet, potentially resulting in lower power & cpu usage
> compared to v0.0.* lanuching a python interpreter twice every second (which is really expensive)

**v0.2.0**: bumped support for liquidctl 1.13.0, supports multiple devices

[liquidctl](https://github.com/liquidctl/liquidctl) plugin for [FanControl](https://github.com/Rem0o/FanControl.Releases)

## Installation

- Download & extract [FanControl](https://github.com/Rem0o/FanControl.Releases#installation)
- Install python (has to be 64bit, add to PATH when installing) & [liquidctl](https://github.com/liquidctl/liquidctl#windows-system-level-dependencies)
  - Latest version validated on `liquidctl==1.13.0`, earlier or later versions might not be supported
- Set system environment variable: `PYTHONNET_PYDLL` to `python39.dll` (`or python37.dll`, `python38.dll`, depending on your python version)
  - This dll has to be in your path, or you can specify the full path for `PYTHONNET_PYDLL` like `path/to/python/install/python39.dll`
  - [See here](https://superuser.com/questions/949560/how-do-i-set-system-environment-variables-in-windows-10)
  - If using admin powershell: `[Environment]::SetEnvironmentVariable("PYTHONNET_PYDLL", "python39.dll", "Machine")`
- Download release files (two dlls, one .py) from [Releases](https://github.com/chenseanxy/FanControl.LiquidCtl/releases)
- Put the release files in your FanControl folder/plugins

## Current Problems

- Pyenv-Win installed pythons might not work (silent crash on `PythonEngine.Initialize`), if you have a wacky setup like I do, I recommend having a official Python install from python.org, and you can leave it out of PATH and reference the full path in `PYTHONNET_PYDLL`.

## Filing issues

Please provide the following:

- Error logs in log.txt
- Versions of: this plugin, Python and FanControl
- Output of `liquidctl list --json` and `liquidctl status --json`

## Contrib

Contributions welcome! In the csproj file you can update the `OutputPath` to your FanControl install path. Beware that debug-lanuching would not load plugins, you'll have to build and then lanuch manually.

## TODOs

[] Get gud in C# & read more of liquidctl codebases, replace current hackey solutions with REAL ones
[x] Move to embedded Python (Pythonnet) instead of calling liquidctl processes
