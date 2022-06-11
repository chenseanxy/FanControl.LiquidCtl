# FanControl.LiquidCtl

> A one-nighter project, born after me being annoyed by NZXT's stupid CAM software for 2 hours  
> Therefore compatibility might be problematic, only verified on NZXT Kraken X (X53, X63 or X73) devices.

**v0.1.0**: liquidctl section is now executed using pythonnet, potentially resulting in lower power & cpu usage
> compared to v0.0.* lanuching a python interpreter twice every second (which is really expensive)

[liquidctl](https://github.com/liquidctl/liquidctl) plugin for [FanControl](https://github.com/Rem0o/FanControl.Releases)

## Installation

- Download & extract [FanControl](https://github.com/Rem0o/FanControl.Releases#installation)
- Install python (has to be 64bit, add to PATH when installing) & [liquidctl](https://github.com/liquidctl/liquidctl#windows-system-level-dependencies)
- Set system environment variable: `PYTHONNET_PYDLL` to `python39.dll` (`or python37.dll`, `python38.dll`, depending on your python version)
  - This dll has to be in your path
  - [See here](https://superuser.com/questions/949560/how-do-i-set-system-environment-variables-in-windows-10)
  - If using admin powershell: `[Environment]::SetEnvironmentVariable("PYTHONNET_PYDLL", "python39.dll", "Machine")`
- Download release files (two dlls, one .py) from [Releases](https://github.com/chenseanxy/FanControl.LiquidCtl/releases)
- Put the release files in your FanControl folder/plugins

## Filing issues

Please provide the following:

- Error logs in log.txt
- Versions of: this plugin, Python and FanControl
- Output of `liquidctl list --json` and `liquidctl status --json`

## TODOs

[] Get gud in C# & read more of liquidctl codebases, replace current hackey solutions with REAL ones
[x] Move to embedded Python (Pythonnet) instead of calling liquidctl processes
