# VR Blacksmith Forge - Group Project 5
A virtual reality blacksmithing simulator

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