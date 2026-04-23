# VR Blacksmith Forge - Group Project 5

## Project overview

This is a medieval blacksmithing simulator for VR with economy mechanics. The simulator is designed to allow users to experience forging, assembling, preparing, and selling blades.

## Features

### Smithing
- Utilize tongs to grab objects
- Heat up metals
- Heat retention and heat speed is based on material
- Hammer materials
- Smash heated ingot into a blade
- Smash blade along different directions and locations to shape it
- Easy of shaping is based on material and temperature
- Sharpen blade
- Quench blade in oil/water to cool it
- Decorate blade with guard, handle, and pommel
### Economy
- Buy materials
- Sell swords
- Reach quotas
### Audio
- Ambience - furnace, wheel grinder, forest
- Collision - wood, brick, metal
- Collision - hammer on smithing material

## Controls

Move - Left Joystick
- If object is grabbed with left controller, it must be dragged in all the way to continue using left joystick for movement
Flip to Look Back - Pull Back On Right Joystick
Increment Look Rotation - Move Right Joystick Left/Right
Smooth Look Rotation - Turn Head
Grab Object - Left/Right Grab Button
Interact with UI - Left/Right Trigger Button

## Installation/deployment instructions

- Set up SideQuest
- Download SideQuest
- Create organization in Meta website
- Use Meta Horizon App to connect to headset and set headset to developer mode
- Ensure graphics drivers are up to date
- Connect headset to PC
- Enable USB debugging in headset
- Download .apk
- Install .apk from PC to headset using SideQuest
- Open up newly installed .apk inside unknown sources apps in library

## Known issues

1. Unity project will place headset on floor and restrict movement on first play after recompiling
2. Unity project may disable all audio on first 1-2 plays after recompiling
3. Blade body can be easily misaligned with handle
4. Blade volume is not perfectly preserved because body scaling does not account for tip length but affects tip volume
5. Heated material emission produces noticeably less light in VR headset
6. Furnace heating will occasionally not start heating object until object put in different location in furnace
7. A gap in collisions exists between the blade body, tip, and handle in which small object could pass through
8. Low poly market textures are very blocky
9. Directional light passes through probuilder walls
10. No smooth transition between large hit and reset position for training dummy
12. Training dummy collider is very inaccurate
12. Heated material is too bright to determine details on object
13. Diamond pommel will keep spinning
14. Sharpening one side of a blade will sharpen the opposite side
15. Sharpening blade cannot shift edge center
16. Decorations will be unable to be used if they leave the hitbox of the blade body
17. If a user hits a decoration with the hammer but the decoration slips off, no decoration of that type can be attached and the decoration will fall through the floor.
18. Blade will not be properly released from tongs after grabbing blade from anvil.
19. Hammer can fall through floor.
20. The anvil guide does not mention shaping.


## Credits/attributions

Team members, ChatGPT, SasquatchBStudios


# PRE-MILESTONE 4 README

## Current State
The primary feature of smithing a sword is mostly complete. Work has begun on some secondary features like varying smithing by material and incorporating temperature into workflow. Other secondary features like shop mechanics, sharpening, and weapon testing are yet to be started.

- **Interactive Tongs**: Two-handed tong control that open and close based on hand distance and can grab any interactable
- **Forging Billet Into Weapon**: When a billet is placed onto an anvil, it can be hit to slowly convert its shape into that of a weapon
- **Weapon Reshaping**: When a weapon is placed onto an anvil, it can be hit to squash its shape along a certain axis
- **Anvil "Socketing"**: When a billet or weapon is placed onto an anvil, its position and rotation are frozen allowing for easy hitting of the item at various angles
- **Forge Environment**: A blacksmith's forge with doors, windows, a furnace, a bellow, an anvil, a wheel grinder, and various props
- **Multiple Test Scenes**: Various development and testing scenes including the main "Blacksmith's Forge" scene we are referrencing

## Instructions for Experiencing the Core Interaction

### Prerequisites
- Meta Quest 3 headset
- Unity 2021.3 or later (check ProjectSettings for exact version)
- XR Interaction Toolkit and XR Hands packages installed

### Setup
1. Open the project in Unity
2. Navigate to `Assets/_Project/Scenes/` and open `Main Scene`
3. Ensure your Meta Quest 3 is connected (via Link/Air Link for testing, or build to device)
4. Press Play in Unity Editor or build and deploy to your Quest 3
5. If first time starting project, camera may be stuck in floor. If so, restart scene.

### Core Interaction:
1. Attach heated billet to anvil
2. Hit billet until it becomes the shape of a sword
3. Attach sword to anvil
4. Hit sword at various angles and speeds to shape it as desired

## Known Issues and Limitations

### Current Limitations
- A lot of mesh colliders for assets do not match meshes
- Extremely difficult to create a true deforming system for ingot to weapon process

### Missing Features
- **No Heating System**: Metal heating/cooling mechanics are not yet implemented.
- **No Shop System**: No money. Can't take weapon orders. Can't sell weapons. Can't buy raw materials.
- **No Material Variety**: All weapons have the same densities and final appearance.
- **No Sword Decorating**: Can't attach guards, grips, and pommels to blade.
- **No Testing Grounds**: No dummies or props to hit to test out weapon.
- **No Weapon Sharpening**: Wheel grinder is non functional and blade cannot be sharpened.
- **No Audio**: No audio is present.
- **No Particle Effects**: No sparks and fire effects.

### Known Bugs
- Not sure if intended but when moving (locomotion), a black vignette appears around the user's peripherals
- Weapons can fall through the floor
- Weapon spawned from hitting billet appears at strange angles
- Grabbing items sometimes raises a bunch of XR errors which causes the item to shake and teleport

## Controls Reference
- While grabbing both tong components, move hands close to grab any item between the tong tips. Move hands apart to release the item.
- Drop item onto top of anvil for it to stick