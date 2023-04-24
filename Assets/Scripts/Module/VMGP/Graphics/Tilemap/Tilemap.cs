using Nofun.Driver.Graphics;
using Nofun.Util;
using Nofun.Util.Logging;
using Nofun.VM;
using System;

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

        private VMPtr<NativeMapHeader> mapHeaderPtr;
        private ITexture mapAtlas;
        private int framePassed;
        private int frameAnimate;

        private TilemapCache tilemapCache;

        private bool IsTilemapFormatSupported(TextureFormat format)
        {
            return (format >= TextureFormat.Palette2) && (format <= TextureFormat.RGB332);
        }

        private bool IsTilemapFormatCurrentlyImplemented(TextureFormat format)
        {
            return (format == TextureFormat.RGB332);
        }

        [ModuleCall]
        private uint vMapInit(VMPtr<NativeMapHeader> header)
        {
            NativeMapHeader headerData = header.Read(system.Memory);
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

            mapAtlas = tilemapCache.Retrive(system.GraphicDriver, headerData, headerData.tileSpriteData.Value, tileData, maxIndex);
            mapHeaderPtr = header;
            frameAnimate = 0;
            framePassed = 0;

            return 1;
        }

        [ModuleCall]
        private byte vMapGetAttribute(byte x, byte y)
        {
            if (mapHeaderPtr.IsNull)
            {
                return 0;
            }

            NativeMapHeader mapHeader = mapHeaderPtr.Read(system.Memory);

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
        private void vMapSetXY(short x, short y)
        {
            if (mapHeaderPtr.IsNull)
            {
                return;
            }

            Span<NativeMapHeader> mapHeader = mapHeaderPtr.AsSpan(system.Memory);

            mapHeader[0].xPos = x;
            mapHeader[0].yPos = y;
        }

        [ModuleCall]
        private void vUpdateMap()
        {
            NativeMapHeader mapHeader = mapHeaderPtr.Read(system.Memory);
            bool blackAsTransparentGlobal = BitUtil.FlagSet(mapHeader.flag, TilemapFlags.Transparent);

            system.GraphicDriver.GetClipRect(out int x0, out int y0, out int x1, out int y1);

            int screenPosX = mapHeader.xPan;
            int screenPosY = mapHeader.yPan;

            int xPosNormed = mapHeader.xPos;
            int yPosNormed = mapHeader.yPos;

            if (mapHeader.yPan != 0)
            {
                Logger.Trace(LogClass.VMGPGraphic, "Damn");
            }

            int startDrawX = screenPosX - xPosNormed % TileMapTileWidth;
            int startDrawY = screenPosY - yPosNormed % TileMapTileHeight;

            int mapDrawWidth = (int)x1 - startDrawX;
            int mapDrawHeight = (int)y1 - startDrawY;

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
            system.GraphicDriver.SetClipRect(Math.Max(screenPosX, x0), Math.Max(screenPosY, y0),
                Math.Min(screenPosX + mapDrawWidth, x1),
                Math.Min(screenPosY + mapDrawHeight, y1));

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

            system.GraphicDriver.SetClipRect(x0, y0, x1, y1);
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