#!/bin/bash

# Setup script to copy ice-level assets into Unity project structure
# Run this from the FuckIce folder

SOURCE_DIR="../human-materials-ignore/ice-level"
SPRITES_DIR="Assets/Sprites"

echo "Setting up FuckIce Unity project assets..."

# Create directories
mkdir -p "$SPRITES_DIR/Tiles"
mkdir -p "$SPRITES_DIR/Background"
mkdir -p "$SPRITES_DIR/Objects/Boxes"
mkdir -p "$SPRITES_DIR/Objects/Bushes"
mkdir -p "$SPRITES_DIR/Objects/Fence"
mkdir -p "$SPRITES_DIR/Objects/Igloo"
mkdir -p "$SPRITES_DIR/Objects/Ladders"
mkdir -p "$SPRITES_DIR/Objects/Other"
mkdir -p "$SPRITES_DIR/Objects/Pointers"
mkdir -p "$SPRITES_DIR/Objects/Stones"
mkdir -p "$SPRITES_DIR/Objects/Trees"
mkdir -p "$SPRITES_DIR/AnimatedObjects"

# Copy Tiles
echo "Copying tiles..."
cp "$SOURCE_DIR/1 Tiles/"*.png "$SPRITES_DIR/Tiles/"

# Copy Background layers
echo "Copying background layers..."
cp "$SOURCE_DIR/2 Background/Background.png" "$SPRITES_DIR/Background/"
cp "$SOURCE_DIR/2 Background/Layers/"*.png "$SPRITES_DIR/Background/"

# Copy Objects
echo "Copying objects..."
cp "$SOURCE_DIR/3 Objects/Boxes/"*.png "$SPRITES_DIR/Objects/Boxes/"
cp "$SOURCE_DIR/3 Objects/Bushes/"*.png "$SPRITES_DIR/Objects/Bushes/"
cp "$SOURCE_DIR/3 Objects/Fence/"*.png "$SPRITES_DIR/Objects/Fence/"
cp "$SOURCE_DIR/3 Objects/Igloo/"*.png "$SPRITES_DIR/Objects/Igloo/"
cp "$SOURCE_DIR/3 Objects/Ladders/"*.png "$SPRITES_DIR/Objects/Ladders/"
cp "$SOURCE_DIR/3 Objects/Other/"*.png "$SPRITES_DIR/Objects/Other/"
cp "$SOURCE_DIR/3 Objects/Pointers/"*.png "$SPRITES_DIR/Objects/Pointers/"
cp "$SOURCE_DIR/3 Objects/Stones/"*.png "$SPRITES_DIR/Objects/Stones/"
cp "$SOURCE_DIR/3 Objects/Trees/"*.png "$SPRITES_DIR/Objects/Trees/"

# Copy Animated Objects
echo "Copying animated objects..."
cp "$SOURCE_DIR/4 Animated objects/"*.png "$SPRITES_DIR/AnimatedObjects/"

echo ""
echo "✓ Assets copied successfully!"
echo ""
echo "Next steps:"
echo "1. Open FuckIce folder in Unity Hub"
echo "2. Unity will import assets and generate .meta files"
echo "3. Select sprites in Tiles folder → set Filter Mode to 'Point (no filter)'"
echo "4. Set Compression to 'None' for pixel-perfect sprites"
echo "5. For Tileset.png: Sprite Mode = Multiple, then use Sprite Editor to slice"

