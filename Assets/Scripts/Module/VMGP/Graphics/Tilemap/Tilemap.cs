/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Nofun.Driver.Graphics;
using Nofun.Util;
using Nofun.Util.Logging;
using Nofun.VM;
using System;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using UnityEditor;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        public const int TileMapTileWidth = 8;
        public const int TileMapTileHeight = 8;

        private const byte AnimateTwoFramesAttributeFlag = 0x40;
        private const byte AnimateFourFramesAttributeFlag = 0x80;
        private const byte TransparentAttributeFlag = 1;

        private const int AnimationMaxFrameCount = 4;

        private NativeMapHeader mapHeader;
        private ITexture mapAtlas;
        private int framePassed;
        private int frameAnimate;
        private uint currentPaletteHash;
        private bool mapExisting = false;

        private TilemapCache tilemapCache;

        private bool IsTilemapFormatSupported(TextureFormat format)
        {
            return (format >= TextureFormat.Palette2) && (format <= TextureFormat.RGB332);
        }

        private bool IsTilemapFormatCurrentlyImplemented(TextureFormat format)
        {
            return (format >= TextureFormat.Palette2) && (format <= TextureFormat.RGB332);
        }

        private void RefreshMapAtlasValidStatus(bool setStaus = false)
        {
            TextureFormat format = (TextureFormat)mapHeader.format;

            if (TextureUtil.IsPaletteFormat(format))
            {
                XxHash32 hasher = new();
                hasher.Append(MemoryMarshal.Cast<SColor, byte>(ScreenPalette));

                uint newHash = BitConverter.ToUInt32(hasher.GetCurrentHash());
                if (setStaus)
                {
                    currentPaletteHash = newHash;
                }
                else
                {
                    if (currentPaletteHash != newHash)
                    {
                        tilemapCache.Invalidate(mapHeader.tileSpriteData.Value);
                        currentPaletteHash = newHash;
                    }
                }
            }

            UpdateMapAtlas(mapHeader);
        }

        private int UpdateMapAtlas(NativeMapHeader headerData)
        {
            TextureFormat format = (TextureFormat)headerData.format;

            if (!IsTilemapFormatSupported(format))
            {
                Logger.Trace(LogClass.VMGPGraphic, $"Unsupported tilemap format {format}");
                return 0;
            }

            if (!IsTilemapFormatCurrentlyImplemented(format))
            {
                throw new UnimplementedFeatureException($"Unimplemented tilemap format {format}");
            }

            // Each tile index is 1 byte, attribute (if have is 1 byte)
            bool hasAttribute = headerData.flag != 0;
            Span<byte> mapData = headerData.mapData.AsSpan(system.Memory, headerData.mapWidth * headerData.mapHeight * (hasAttribute ? 2 : 1));

            // Find highest-index to create atlas
            byte maxIndex = 0;
            int incrementIndex = (hasAttribute ? 2 : 1);

            for (int i = 0; i < mapData.Length; i += incrementIndex)
            {
                int actualReachableIndex = mapData[i];

                if (hasAttribute)
                {
                    byte attribute = mapData[i + 1];

                    // 0x40 is animation tile
                    if ((attribute & AnimateTwoFramesAttributeFlag) != 0)
                    {
                        actualReachableIndex += 2;
                    }
                    else if ((attribute & AnimateFourFramesAttributeFlag) != 0)
                    {
                        actualReachableIndex += 4;
                    }
                }

                if (actualReachableIndex > maxIndex)
                {
                    maxIndex = (byte)actualReachableIndex;
                }
            }

            int singleTileSizeBytes = (int)((TextureUtil.GetTextureSizeInBits(TileMapTileWidth, TileMapTileHeight, format) + 7) >> 3);
            Span<byte> tileData = headerData.tileSpriteData.AsSpan(system.Memory, singleTileSizeBytes * maxIndex);

            // Can't set whole map transparent now, since some tiles still have black but not have transparent flag
            // So the shader black as transparent variable will still have to keep doing its job
            mapAtlas = tilemapCache.Retrive(system.GraphicDriver, headerData, headerData.tileSpriteData.Value, tileData, maxIndex,
                ScreenPalette, false);

            return 1;
        }

        [ModuleCall]
        private uint vMapInit(VMPtr<NativeMapHeader> header)
        {
            NativeMapHeader headerData = header.Read(system.Memory);
            if (UpdateMapAtlas(headerData) == 0)
            {
                return 0;
            }

            mapHeader = headerData;
            frameAnimate = 0;
            framePassed = 0;

            RefreshMapAtlasValidStatus(true);
            mapExisting = true;

            return 1;
        }

        [ModuleCall]
        private void vMapDispose()
        {
            mapExisting = false;
            mapAtlas = null;
        }

        [ModuleCall]
        private byte vMapGetTile(byte x, byte y)
        {
            if (!mapExisting)
            {
                return 0;
            }

            if ((x >= mapHeader.mapWidth) || (y >= mapHeader.mapHeight))
            {
                return 0;
            }

            int tileStride = (mapHeader.flag == 0) ? 1 : 2;
            return mapHeader.mapData[(y * mapHeader.mapWidth + x) * tileStride].Read(system.Memory);
        }

        [ModuleCall]
        private void vMapSetTile(byte x, byte y, byte tile)
        {
            if (!mapExisting)
            {
                return;
            }

            if ((x >= mapHeader.mapWidth) || (y >= mapHeader.mapHeight))
            {
                return;
            }

            int tileStride = (mapHeader.flag == 0) ? 1 : 2;
            mapHeader.mapData[(y * mapHeader.mapWidth + x) * tileStride].Write(system.Memory, tile);
        }

        [ModuleCall]
        private byte vMapGetAttribute(byte x, byte y)
        {
            if (!mapExisting)
            {
                return 0;
            }

            if ((x >= mapHeader.mapWidth) || (y >= mapHeader.mapHeight))
            {
                return 0;
            }

            if (mapHeader.flag == 0)
            {
                return 0;
            }

            return mapHeader.mapData[(y * mapHeader.mapWidth + x) * 2 + 1].Read(system.Memory);
        }

        [ModuleCall]
        private void vMapSetAttribute(byte x, byte y, byte attribute)
        {
            if (!mapExisting)
            {
                return;
            }

            if ((x >= mapHeader.mapWidth) || (y >= mapHeader.mapHeight))
            {
                return;
            }

            if (mapHeader.flag == 0)
            {
                return;
            }

            mapHeader.mapData[(y * mapHeader.mapWidth + x) * 2 + 1].Write(system.Memory, attribute);
        }

        [ModuleCall]
        private void vMapSetXY(short x, short y)
        {
            if (!mapExisting)
            {
                return;
            }

            mapHeader.xPos = x;
            mapHeader.yPos = y;
        }

        [ModuleCall]
        private void vUpdateMap()
        {
            if (!mapExisting || (mapHeader.mapWidth == 0) || (mapHeader.mapHeight == 0))
            {
                return;
            }

            bool blackAsTransparentGlobal = BitUtil.FlagSet(mapHeader.flag, TilemapFlags.Transparent);

            NRectangle currentClip = system.GraphicDriver.ClipRect;

            int screenPosX = mapHeader.xPan;
            int screenPosY = mapHeader.yPan;

            int xPosNormed = mapHeader.xPos;
            int yPosNormed = mapHeader.yPos;

            int startDrawX = screenPosX - xPosNormed % TileMapTileWidth;
            int startDrawY = screenPosY - yPosNormed % TileMapTileHeight;

            int mapDrawWidth = currentClip.x1 - startDrawX;
            int mapDrawHeight = currentClip.y1 - startDrawY;

            if ((mapDrawWidth == 0) || (mapDrawHeight == 0))
            {
                return;
            }

            int tileStartDrawX = xPosNormed / TileMapTileWidth;
            int tileStartDrawY = yPosNormed / TileMapTileHeight;

            int tileXCount = Math.Min((mapDrawWidth + TileMapTileWidth - 1) / TileMapTileWidth,
                mapHeader.mapWidth - Math.Max(0, tileStartDrawX));

            int tileYCount = Math.Min((mapDrawHeight + TileMapTileHeight - 1) / TileMapTileHeight,
                mapHeader.mapHeight - Math.Max(0, tileStartDrawY));

            bool hasAttribute = mapHeader.flag != 0;
            int indexingMul = hasAttribute ? 2 : 1;
            Span<byte> mapData = mapHeader.mapData.AsSpan(system.Memory, mapHeader.mapWidth * mapHeader.mapHeight * indexingMul);

            // Try to clip rect and later restore back
            system.GraphicDriver.ClipRect = new NRectangle(Math.Max(screenPosX, currentClip.x), Math.Max(screenPosY, currentClip.y),
                Math.Min(mapDrawWidth, currentClip.width),
                Math.Min(mapDrawHeight, currentClip.height));

            RefreshMapAtlasValidStatus();

            // Handle negative tile (they want to draw the screen up a bit)
            for (int y = Math.Abs(Math.Min(tileStartDrawY, 0)); y < tileYCount; y++)
            {
                for (int x = Math.Abs(Math.Min(tileStartDrawX, 0)); x < tileXCount; x++)
                {
                    int screenTilePosX = startDrawX + x * TileMapTileWidth;
                    int screenTilePosY = startDrawY + y * TileMapTileHeight;

                    int tileAccessIndex = ((tileStartDrawY + y) * mapHeader.mapWidth + (tileStartDrawX + x)) * indexingMul;

                    byte tileNum = mapData[tileAccessIndex];
                    byte attrib = 0;

                    if (hasAttribute)
                    {
                        attrib = mapData[tileAccessIndex + 1];
                    }

                    if (tileNum == 0)
                    {
                        continue;
                    }

                    // Tile index is one-based
                    tileNum -= 1;

                    if ((attrib & AnimateTwoFramesAttributeFlag) != 0)
                    {
                        tileNum += (byte)(frameAnimate % 2);
                    }
                    else if ((attrib & AnimateFourFramesAttributeFlag) != 0)
                    {
                        tileNum += (byte)(frameAnimate % 4);
                    }

                    bool blackAsTransparent = blackAsTransparentGlobal && ((attrib & TransparentAttributeFlag) != 0);

                    TilemapCache.GetTilePositionInAtlas(tileNum, out int sourceAtlasX, out int sourceAtlasY);
                    
                    system.GraphicDriver.DrawTexture(screenTilePosX, screenTilePosY, 0, 0, 0, mapAtlas,
                        sourceAtlasX, sourceAtlasY, TileMapTileWidth, TileMapTileHeight,
                        blackAsTransparent: blackAsTransparent);
                }
            }

            system.GraphicDriver.ClipRect = currentClip;
            framePassed++;

            if ((mapHeader.animationSpeed != 0) && (framePassed > mapHeader.animationSpeed))
            {
                frameAnimate = (frameAnimate + 1) % AnimationMaxFrameCount;
                framePassed = 0;
            }
            else
            {
                // TODO! Handle two cases. In older version, animate by frame, in newer: increase frame each second
            }
        }
    }
}