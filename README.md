# ABAI - Aeyos Base AI
A Space Engineers mod that adds an AI block to help you manage your base.

## TODO

* Get current grid
  * Create block finder function
  * Get list of containers
  * Get list of refineries
  * Get list of asemblers
  * Filter list for whitelisted/blacklisted containers
  * Implement simple container sorting

## Planned Features

- [X] Custom block for AI
  - [ ] Alternative model with projection (transparent lcd, based on block_console)
- [X] Custom Options for AI
  - [X] Configuration Location (Title/Data)
  - [ ] Auto-sort containers (On/Off)
  - [ ] Auto-assign containers (On/Off)
  - [ ] Sort items alphabetically (On/Off)
  - [ ] Controlled assemblers (On/Off)
  - [ ] Auto-adjust solar panels (On/Off)
  - [ ] Auto-close doors (On/Off)
  - [ ] Low item alerts (On/Off)
  - [ ] Low power alerts (On/Off)
  - [ ] Low storage alerts (On/Off)
  - [ ] Enemy proximity alerts (On/Off)
  - [ ] Base damage alerts (On/Off)
  - [ ] Turn off Refineries on low Power (On/Off)
  - [ ] Turn off Assemblers on low Power (On/Off)
- [ ] Quota for Ores/Ingots/Components/Items
- [ ] Display quota, info, power, O2, H etc on LCD's
  - [ ] Custom LCD script for eye-candy info
- [ ] Benchmark, debug info, CPU time, optimizations
- [ ] AI Customization
  - [ ] Name your AI for custom messages
  - [ ] Custom AI faces
    - [X] Abot
    - [ ] wAIfu
    - [ ] Ram/Rem (possibly addon/second mod)
    - [ ] ZeroTwo (possibly addon/second mod)
    - [ ] EVA from WALL-E (possibly addon/second mod)
    - [ ] Butler (possibly addon/second mod)
  - [ ] Custom AI Sound alerts (voiced AI)
  - [ ] AI "emotions" (animated lcd using sprites and image change time)
- [ ] Weldpad with LCD that displays components requirements
- [ ] Auto-mine drill arm with auto-stop and auto-resume
- [ ] Inter-grid funcionality (manage self/Manage self & docked)
- [ ] Airlock functionality
  - [ ] Auto depressurize, close and open doors
- [ ] Translation
  - [ ] ES
  - [ ] PT

## Bugs

[X] Block has no placeholder screen, screen only appears with content
[ ] Block has no category and only appears by searching in the G menu

Helpful/Reference links:

[Digi SE Modding Wiki](https://github.com/THDigi/SE-ModScript-Examples/wiki/Quick-Intro-to-Space-Engineers-Modding)

[cdrch Fan Docs](https://github.com/cdrch/space-engineers-fan-docs/blob/master/modding-introduction/main.md#what-can-i-mod-)

[bloc97's SEAPI Doc](https://bloc97.github.io/SpaceEngineersModAPIDocs/html/b2d609dc-672a-3d90-cdc0-3753ce60d06f.htm)

[Malware DEV](https://github.com/malware-dev/MDK-SE)
