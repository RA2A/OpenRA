This soft-fork combines soft-forks of Attacque Supérior and Over Powered Mod. The README of both downstream repositories can be found below.

***

# OpenRA Engine Modification for Over Powered Mod

If you are looking for the mod, please visit https://github.com/forcecore/yupgi_alert0/ and http://www.moddb.com/mods/over-powered-mod. My mod is like a show case for the engine modification.

This fork is like experimental branch of Linux Debian distribution: I push the boundaries of OpenRA on what it can do at the cost of compatibility from main OpenRA. AttacqueSuperior features more stable higher quality traits and modder friendly: https://github.com/AttacqueSuperior/Engine. Some of my modules are tamed and imported into their engine too! (And I use their stuff too hehe)

I do hope that one day these experimental stuff get popular and find its way into main OpenRA code...

## Notable modification to OpenRA.Game.exe, in this branch
* Supports "sky box". (needed game.exe modification argh)
* You can rename it to OpenRA.{modname}.exe and it will launch the mod.

## Commit tags
In commit message, I put these string as tags:
* [SPAWN]: code modifications for spawners or spawned ones. Used for aircraft carriers and V3-like AA shootable missiles.
* [GRINDER]: Yuri grinder logic related commits
* [IRON_EFF]: iron curtain flashing effect
* [RADIO_ACTIVITY]: radio activity layer stuff (desolator rad eruption, nuke radiation...)
* [SPACE]: codes for space themed maps
* [CMIN]: Chrono miner (ore teleporter)
* [HARV]: Harvester AI related stuff
* [IDEPLOY]: (un)deploy notification interface
* [DOCKS]: multi-dock support

***

# This repository is a customized soft-fork of OpenRA aimed towards Attacque Supérior.

This repository was made under the guidance of the recent OpenRA soft-fork approving policy and intends to maintain bleed-compatibility as much as possible with upstream OpenRA. All code alterations are under the GPLv3 license and probably will be evaluated by StyleCop to maintain OpenRA coding quality.

This repository's aim is to be used by other OpenRA modders for their advancements as well.

* Websites: [http://attsup.swr-productions.com](http://attsup.swr-productions.com) and [http://www.moddb.com/mods/attacque-suprior](http://www.moddb.com/mods/attacque-suprior)
* Discord: [https://discord.gg/7aM7Hm2](https://discord.gg/7aM7Hm2)

Below you can find the original OpenRA readme unedited.

***

# OpenRA

A Libre/Free Real Time Strategy game engine supporting early Westwood classics.

* Website: [http://www.openra.net](http://www.openra.net)
* IRC: \#openra on irc.freenode.net
* Repository: [https://github.com/OpenRA/OpenRA](https://github.com/OpenRA/OpenRA) [![Travis CI build status](https://travis-ci.org/OpenRA/OpenRA.svg?branch=bleed)](https://travis-ci.org/OpenRA/OpenRA) [![AppVeyor build status](https://ci.appveyor.com/api/projects/status/axc9k6jd25ej2o4w?svg=true)](https://ci.appveyor.com/project/OpenRA/openra) [![Coverity Scan Build Status](https://scan.coverity.com/projects/3650/badge.svg)](https://scan.coverity.com/projects/3650)

Please read the [FAQ](http://wiki.openra.net/FAQ) in our [Wiki](http://wiki.openra.net) and report problems at [http://bugs.openra.net](http://bugs.openra.net).

Join the [Forums](http://www.sleipnirstuff.com/forum/viewforum.php?f=80) for discussion.

## Play

Distributed mods include a reimagining of

* Command & Conquer: Red Alert
* Command & Conquer: Tiberian Dawn
* Dune 2000

Check our [Playing the Game](https://github.com/OpenRA/OpenRA/wiki/Playing-the-game) Guide to win multiplayer matches.

## Contribute

[![Bountysource](https://api.bountysource.com/badge/team?team_id=528&style=bounties_received)](https://www.bountysource.com/teams/openra/issues?utm_source=OpenRA&utm_medium=shield&utm_campaign=bounties_received)

* Please read [INSTALL.md](https://github.com/OpenRA/OpenRA/blob/bleed/INSTALL.md) and [Compiling](http://wiki.openra.net/Compiling) on how to set up an OpenRA development environment.
* See [Hacking](http://wiki.openra.net/Hacking) for an overview of the engine.
* To get your patches merged, please adhere to the [Contributing](https://github.com/OpenRA/OpenRA/blob/bleed/CONTRIBUTING.md) guidelines.

## Mapping

* We offer a [Mapping](http://wiki.openra.net/Mapping) Tutorial as you can change gameplay drastically with custom rules.
* For scripted mission have a look at the [Lua API](http://wiki.openra.net/Lua-API).
* If you want to share your maps with the community, upload them at the [OpenRA Resource Center](http://resource.openra.net).

## Modding

* There exists an auto-generated [Trait documentation](http://wiki.openra.net/Traits) to get started with yaml files.
* Check the [Modding Guide](http://wiki.openra.net/Modding-Guide) to create your own classic RTS.
* Some hints on how to create new OpenRA compatible [Pixelart](http://wiki.openra.net/Pixelart).
* Upload total conversions at [our ModDB profile](http://www.moddb.com/games/openra/mods).

## Support

* Sponsor a [mirror server](https://github.com/OpenRA/OpenRAWeb/tree/master/content/packages) if you have some bandwidth to spare.
* You can immediately set up a [Dedicated](http://wiki.openra.net/Dedicated) Game Server.
* Fund development by creating [Bounties](https://www.bountysource.com/trackers/36085-openra) on specific tasks.

## License
Copyright 2007-2017 The OpenRA Developers (see [AUTHORS](https://github.com/OpenRA/OpenRA/blob/bleed/AUTHORS))
This file is part of OpenRA, which is free software. It is made 
available to you under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version. For more
information, see [COPYING](https://github.com/OpenRA/OpenRA/blob/bleed/COPYING).
