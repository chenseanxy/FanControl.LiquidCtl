# FanControl.LiquidCtl

> A one-nighter project, born after me being annoyed by NZXT's stupid CAM software for 2 hours  
> Therefore compatibility might be problematic, only verified on NZXT Kraken X (X53, X63 or X73) devices.

[liquidctl](https://github.com/liquidctl/liquidctl) plugin for [FanControl](https://github.com/Rem0o/FanControl.Releases)

## Installation

- Download & extract [FanControl](https://github.com/Rem0o/FanControl.Releases#installation)
- Install [liquidctl](https://github.com/liquidctl/liquidctl#windows-system-level-dependencies): I recommend using [pipx](https://github.com/pypa/pipx#on-windows-install-via-pip-requires-pip-190-or-later)
- Download release dlls from [Releases](https://github.com/chenseanxy/FanControl.LiquidCtl/releases)
- Put the DLLS in your FanControl folder/plugins

## Filing issues

Please provide error logs (if any) and `liquidctl list --json` output

## TODOs

- Get gud in C# & read more of liquidctl codebases, replace current hackey solutions with REAL ones
- Move to embedded Python (Pythonnet) instead of calling liquidctl processes 
