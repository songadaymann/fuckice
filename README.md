# Fuck Ice - 2D Platformer

A 2D platformer game built with Unity, featuring ice-themed levels.

## Quick Start

### 1. Setup Assets
Run the setup script to copy your sprites into the project:

```bash
cd FuckIce
chmod +x setup_assets.sh
./setup_assets.sh
```

### 2. Open in Unity
1. Open **Unity Hub**
2. Click **Add** → **Add project from disk**
3. Select this `FuckIce` folder
4. Open with **Unity 2022.3 LTS** or newer (2D template recommended)

### 3. Configure Sprites for Pixel Art
After Unity imports the assets, configure them for pixel art:

1. Select all sprites in `Assets/Sprites/Tiles/`
2. In the Inspector:
   - **Filter Mode**: Point (no filter)
   - **Compression**: None
   - **Pixels Per Unit**: 16 (or match your tile size)
3. Click **Apply**
4. Repeat for other sprite folders

### 4. Set Up the Tilemap

#### Slice the Tileset
1. Select `Assets/Sprites/Tiles/Tileset.png`
2. Set **Sprite Mode** to **Multiple**
3. Click **Sprite Editor**
4. Click **Slice** → Grid by Cell Size → Set to your tile size (e.g., 16x16 or 32x32)
5. Click **Slice**, then **Apply**

#### Create Tile Palette
1. Go to **Window → 2D → Tile Palette**
2. Click **Create New Palette**
3. Name it "IceTiles", save in `Assets/Palettes/`
4. Drag sliced sprites from Tileset onto the palette
5. Save tiles in `Assets/Tiles/`

#### Create Tilemap in Scene
1. **GameObject → 2D Object → Tilemap → Rectangular**
2. This creates a Grid with a Tilemap child
3. Select the Tile Palette window
4. Choose your brush and paint tiles!

### 5. Set Up Parallax Background
For the 7 background layers:

1. Create empty GameObject called "Background"
2. For each layer (1.png through 7.png):
   - Create child GameObject with SpriteRenderer
   - Assign the layer sprite
   - Add the `ParallaxLayer` script
   - Set **Parallax Effect X**: 
     - Layer 7 (farthest): 0.9
     - Layer 6: 0.8
     - Layer 5: 0.7
     - ... down to ...
     - Layer 1 (closest): 0.3
   - Set **Sorting Layer** to "BackgroundParallax"
   - Set **Order in Layer**: -7 through -1

## Project Structure

```
FuckIce/
├── Assets/
│   ├── Scenes/           # Game scenes
│   ├── Scripts/
│   │   ├── Player/       # PlayerController.cs
│   │   ├── Camera/       # CameraFollow.cs
│   │   ├── Environment/  # ParallaxLayer.cs
│   │   ├── Collectibles/ # Collectible.cs
│   │   └── Animation/    # AnimatedSprite.cs
│   ├── Sprites/
│   │   ├── Tiles/        # Individual tiles + Tileset.png
│   │   ├── Background/   # Parallax layers 1-7
│   │   ├── Objects/      # Boxes, trees, etc.
│   │   └── AnimatedObjects/ # Coin, chest, flag sprites
│   ├── Tiles/            # Tile assets (generated)
│   ├── Palettes/         # Tile Palettes
│   └── Prefabs/          # Reusable game objects
├── Packages/
└── ProjectSettings/
```

## Included Scripts

### PlayerController.cs
- WASD/Arrow key movement
- Space to jump
- Coyote time & jump buffering
- Better jump feel with variable height

### CameraFollow.cs
- Smooth camera follow
- Optional look-ahead
- Configurable bounds

### ParallaxLayer.cs
- Per-layer parallax scrolling
- Infinite horizontal scrolling

### Collectible.cs
- Trigger-based collection
- Bob animation
- Sound & particle effects

### AnimatedSprite.cs
- Frame-by-frame animation
- For animated tiles without Animator

## Tips

### Tilemap Collision
1. Select your Tilemap GameObject
2. Add **Tilemap Collider 2D** component
3. Add **Composite Collider 2D** for better performance
4. On Tilemap Collider 2D, check **Used By Composite**

### Sorting Layers (already configured)
- Background
- BackgroundParallax
- Midground
- Default
- Platforms
- Objects
- Player
- Foreground
- UI

### Player Layer Setup
1. Put player on "Player" layer
2. Put ground/platforms on "Ground" layer
3. Configure layer collision in Physics2D settings if needed

## Animated Objects
The animated sprites (Coin.png, etc.) are sprite sheets. To use them:

1. Select the sprite (e.g., Coin.png)
2. Set **Sprite Mode** to **Multiple**
3. Use **Sprite Editor** to slice (Grid by Cell Count or Size)
4. Create a GameObject with **AnimatedSprite** component
5. Drag the sliced sprites into the Frames array

Enjoy building your ice platformer! 🧊

