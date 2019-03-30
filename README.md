# WobCapacityLimits

![Version](https://img.shields.io/badge/Rimworld-1.0-brightgreen.svg)

A mod for [Rimworld](https://rimworldgame.com/), that allows you to change the carrying capacity of pawns and transports, as well as maximum stack sizes of most items.

The capacities and stack limits are changed in the mod's settings menu.

## Notes

### Pawn Carry Capacity

Rimworld uses a simple calculation for how much stuff a pawn can carry:
body size * 35 = capacity in kg

This mod allows you to change that 35 to any number via the options menu.

Note that this is only for the inventory and caravans - in your base your pawns will still only carry one stack of items.

### Transport Pods

The maximum transportable mass per pod can be edited via the options menu.

The amount of chemfuel a pod consumes per tile traveled can also be edited via the options menu, which will affect the maximum range of pods.

### Stack Sizes

Each category of item/resource has a multiplier in the options menu. Each item/resource definition has its stack limit multiplied by this to get the new stack limit.

The maximum size of stack that a pawn can carry will be automatically set to the new maximum stack size.

## Credits

This mod uses [HugsLib](https://github.com/UnlimitedHugs/RimworldHugsLib) and [Harmony](https://github.com/pardeike/Harmony).
