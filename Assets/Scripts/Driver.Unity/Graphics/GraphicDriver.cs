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
using UnityEngine;

using System.Collections.Generic;
using System;

using UnityEngine.UI;
using UnityEngine.Rendering;
using Nofun.Module.VMGP3D;
using System.Linq;

namespace Nofun.Driver.Unity.Graphics
{
    public class GraphicDriver : MonoBehaviour, IGraphicDriver
    {
        private readonly int NGAGE_PPI = 130;
        private const int MEASURED_CACHE_LIMIT = 4000;
        private const string BlackTransparentUniformName = "_Black_Transparent";
        private const string MainTexUniformName = "_MainTex";

        private const string UnlitZTestUniformName = "_ZTest";
        private const string UnlitCullUniformName = "_Cull";
        private const string UnlitBlendSourceFactorUniformName = "_SourceBlendFactor";
        private const string UnlitBlendDestFactorUniformName = "_DestBlendFactor";
        private const string UnlitTexturelessUniformName = "_Textureless";
        private const string UnlitTextureBlendModeUniformName = "_TextureBlendMode";

        private const string ZClearValueUniformName = "_ClearValue";

        private const int TMPSpawnPerAlloc = 5;

        private RenderTexture screenTextureBackBuffer;

        private bool began = false;
        private Action stopProcessor;
        private Texture2D whiteTexture;
        private Vector2 screenSize;
        private Dictionary<string, int> measuredCache;

        private TMPro.FontStyles selectedFontStyles;
        private float selectedFontSize = 11.5f;
        private float selectedOutlineWidth = 0.0f;
        private int fontMeshUsed = 0;

        private ClientState serverSideState;
        private ClientState clientSideState;
        private bool fixedStateChanged = true;

        private Matrix4x4 orthoMatrix = Matrix4x4.identity;
        private bool in3DMode = false;

        private MeshCache meshCache;
        private MeshBatcher meshBatcher;

        [SerializeField]
        private TMPro.TMP_Text[] textRenders;

        [SerializeField]
        private TMPro.TMP_Text textMeasure;

        [SerializeField]
        private UnityEngine.UI.RawImage displayImage;

        [SerializeField]
        private Material mophunDrawTextureMaterial;

        [SerializeField]
        private Material mophunUnlitMaterial;

        [SerializeField]
        private Material clearZMaterial;

        [SerializeField]
        private Camera mophunCamera;

        [SerializeField]
        private bool coverScreen = false;

        private CommandBuffer commandBuffer;
        private Mesh quadMesh;

        private List<TMPro.TMP_Text> textRenderInternals;
        private Dictionary<ulong, Material> unlitMaterialCache;

        private Material currentMaterial;

        private enum BatchingMode
        {
            None,
            Render2D,
            Render3D
        }

        private BatchingMode currentBatching = BatchingMode.None;
        private Texture2D current2DTexture = null;
        private bool currentBlackAsTransparent = false;

        [HideInInspector]
        public float FpsLimit { get; set; }

        private float SecondPerFrame => 1.0f / FpsLimit;

        private void PrepareUnlitMaterial()
        {
            if (fixedStateChanged)
            {
                ulong stateIdentifier = serverSideState.MaterialIdentifier;
                if (!unlitMaterialCache.ContainsKey(stateIdentifier))
                {
                    Material mat = new Material(mophunUnlitMaterial);
                    mat.SetFloat(UnlitCullUniformName, (float)serverSideState.cullMode.ToUnity());
                    mat.SetFloat(UnlitZTestUniformName, (float)serverSideState.depthCompareFunc.ToUnity());

                    Tuple<BlendMode, BlendMode> blendFactors = serverSideState.blendMode.ToUnity();
                    mat.SetFloat(UnlitBlendSourceFactorUniformName, (float)blendFactors.Item1);
                    mat.SetFloat(UnlitBlendDestFactorUniformName, (float)blendFactors.Item2);

                    unlitMaterialCache.Add(stateIdentifier, mat);
                }
                currentMaterial = unlitMaterialCache[stateIdentifier];
                fixedStateChanged = false;
            }
        }

        private void HandleFixedStateChangedClient()
        {
            FlushBatch();
        }

        public Action StopProcessorAction
        {
            set => stopProcessor = value;
        }

        private void SetupWhiteTexture()
        {
            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }

        private void Flush3DBatch()
        {
            if (meshBatcher.Flush())
            {
                JobScheduler.Instance.RunOnUnityThread(() =>
                {
                    BeginRender(mode2D: false);

                    commandBuffer.DrawMesh(meshBatcher.Pop(), Matrix4x4.identity, currentMaterial, 0, 0, UnlitPropertyBlock);
                });
            }
        }

        private void Flush2DBatch()
        {
            if (meshBatcher.Flush())
            {
                Texture2D currentTexCopy = current2DTexture;
                bool blackAsTransparentCopy = currentBlackAsTransparent;

                JobScheduler.Instance.RunOnUnityThread(() =>
                {
                    BeginRender(mode2D: true);

                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    block.SetTexture(MainTexUniformName, currentTexCopy);
                    block.SetFloat(BlackTransparentUniformName, blackAsTransparentCopy ? 1.0f : 0.0f);

                    commandBuffer.DrawMesh(meshBatcher.Pop(), Matrix4x4.identity, mophunDrawTextureMaterial, 0, 0, block);
                });
            }
        }

        private void FlushBatch()
        {
            switch (currentBatching)
            {
                case BatchingMode.Render2D:
                    Flush2DBatch();
                    break;

                case BatchingMode.Render3D:
                    Flush3DBatch();
                    break;

                case BatchingMode.None:
                    break;

                default:
                    throw new ArgumentException("Invalid batch mode!");
            }
        }

        private void BeginBatching(BatchingMode mode = BatchingMode.Render2D)
        {
            if (currentBatching == BatchingMode.None)
            {
                currentBatching = mode;
                current2DTexture = null;

                return;
            }

            if (mode != currentBatching)
            {
                FlushBatch();

                currentBatching = mode;
                current2DTexture = null;
            }
        }

        private void Begin2DBatching(Texture2D currentTex, bool blackAsTransparent = false)
        {
            BeginBatching(BatchingMode.Render2D);

            if (((current2DTexture != null) && (current2DTexture != currentTex)) || (currentBlackAsTransparent != blackAsTransparent))
            {
                Flush2DBatch();
            }

            current2DTexture = currentTex;
            currentBlackAsTransparent = blackAsTransparent;
        }

        private void SetupQuadMesh()
        {
            quadMesh = new Mesh();

            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(1, 1, 0),
                new Vector3(1, 0, 0)
            };

            quadMesh.vertices = vertices;

            int[] indicies =
            {
                0, 1, 2,
                0, 2, 3
            };

            quadMesh.triangles = indicies;

            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1)
            };

            quadMesh.uv = uv;
        }

        public void Initialize(Vector2 size)
        {
            screenTextureBackBuffer = new RenderTexture((int)size.x, (int)size.y, 32);
            
            meshCache = new();
            meshBatcher = new();
            unlitMaterialCache = new();
            measuredCache = new();
            clientSideState = new();
            serverSideState = new();

            clientSideState.scissorRect = serverSideState.scissorRect = new Rect(0, 0, size.x, size.y);
            clientSideState.viewportRect = serverSideState.viewportRect = new Rect(0, 0, size.x, size.y);

            SetupWhiteTexture();
            SetupQuadMesh();

            displayImage.texture = screenTextureBackBuffer;
            displayImage.gameObject.SetActive(false);

            textRenderInternals = new(textRenders);

            this.screenSize = size;

            orthoMatrix = Matrix4x4.Ortho(0, ScreenWidth, 0, ScreenHeight, 1, -100);
        }

        private void Start()
        {
            RectTransform transform = displayImage.GetComponent<RectTransform>();
            Vector2 presetSize = new Vector2(176, 208);

            transform.anchoredPosition = Vector2.zero;
            transform.offsetMin = Vector2.zero;

            if (coverScreen)
            {
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.one;
                transform.offsetMax = Vector2.zero;
            }
            else
            {
                transform.anchorMin = transform.anchorMax = Vector2.one / 2;
                transform.sizeDelta = presetSize;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            Initialize(coverScreen ? transform.rect.size * displayImage.canvas.scaleFactor : presetSize);
        }

        private Rect GetUnityScreenRect(Rect curRect)
        {
            return new Rect(curRect.x, screenSize.y - (curRect.y + curRect.height), curRect.width, curRect.height);
        }

        private Vector2 GetUnityCoords(float x, float y)
        {
            return new Vector2(x, screenSize.y - y);
        }

        private void DrawTexture(ITexture tex, Rect destRect, Rect sourceRect, float centerX, float centerY, float rotation, SColor color, bool blackIsTransparent, bool flipX, bool flipY)
        {
            DrawTexture(((Texture)tex).NativeTexture, destRect, sourceRect, centerX, centerY, rotation, color, blackIsTransparent, flipX, flipY);
        }

        private void DrawRectBoardGeneral(Rect destRect, Vector2[] uvs, float centerX, float centerY, float rotation, Color[] color, bool flipX = false, bool flipY = false, float z = 0.0f)
        {
            float orgX = destRect.x;
            float orgY = destRect.y;

            float sinRot = (float)Math.Sin(MathUtil.Degs2Rad(rotation));
            float cosRot = (float)Math.Cos(MathUtil.Degs2Rad(rotation));
            centerX *= -1;

            var vertices = new Vector3[]
            {
                new Vector3(orgX + centerX * cosRot - centerY * sinRot, orgY + centerX * sinRot + centerY * cosRot, z),
                new Vector3(orgX + (centerX + destRect.width) * cosRot - centerY * sinRot, orgY + (destRect.width + centerX) * sinRot + centerY * cosRot, z),
                new Vector3(orgX + centerX * cosRot - (centerY + destRect.height) * sinRot, orgY + centerX * sinRot + (centerY + destRect.height) * cosRot, z),
                new Vector3(orgX + (centerX + destRect.width) * cosRot - (centerY + destRect.height) * sinRot, orgY + (centerX + destRect.width) * sinRot + (centerY + destRect.height) * cosRot, z)
            };

            var triangles = new int[]
            {
                0, 2, 3,
                0, 3, 1
            };

            if (flipX)
            {
                (uvs[0].x, uvs[1].x) = (uvs[1].x, uvs[0].x);
                (uvs[2].x, uvs[3].x) = (uvs[3].x, uvs[2].x);
            }

            if (flipY)
            {
                (uvs[0].y, uvs[2].y) = (uvs[2].y, uvs[0].y);
                (uvs[1].y, uvs[3].y) = (uvs[3].y, uvs[1].y);
            }

            meshBatcher.AddBasic(vertices, uvs, color, triangles);
        }

        private void DrawRectBoardGeneral2D(Rect destRect, Rect sourceRect, float centerX, float centerY, float rotation, SColor color, bool flipX = false, bool flipY = false, float z = 0.0f)
        {
            if (!destRect.Intersects(clientSideState.scissorRect, out Rect drawArea))
            {
                return;
            }

            if (drawArea != destRect)
            {
                float xRatio = (drawArea.x - destRect.x) / destRect.width;
                float yRatio = (drawArea.y - destRect.y) / destRect.height;
                float widthRatio = drawArea.width / destRect.width;
                float heightRatio = drawArea.height / destRect.height;

                destRect = drawArea;

                sourceRect.x += xRatio * sourceRect.width;
                sourceRect.y += yRatio * sourceRect.height;
                sourceRect.width *= widthRatio;
                sourceRect.height *= heightRatio;
            }

            destRect = GetUnityScreenRect(destRect);

            var uvs = new Vector2[]
            {
                new Vector2(sourceRect.x, sourceRect.y + sourceRect.height),
                new Vector2(sourceRect.x + sourceRect.width, sourceRect.y + sourceRect.height),
                new Vector2(sourceRect.x, sourceRect.y),
                new Vector2(sourceRect.x + sourceRect.width, sourceRect.y)
            };

            DrawRectBoardGeneral(destRect, uvs, centerX, centerY, rotation, Enumerable.Repeat(color.ToUnityColor(), 4).ToArray(), flipX, flipY, z);
        }

        private void DrawTexture(Texture2D tex, Rect destRect, Rect sourceRect, float centerX, float centerY, float rotation, SColor color, bool blackIsTransparent, bool flipX = false, bool flipY = false, float z = 0.0f)
        {
            Begin2DBatching(tex, blackIsTransparent);
            DrawRectBoardGeneral2D(destRect, sourceRect, centerX, centerY, rotation, color, flipX, flipY);
        }

        private void UpdateRenderMode()
        {
            commandBuffer.DisableScissorRect();

            if (in3DMode)
            {
                commandBuffer.SetProjectionMatrix(serverSideState.projectionMatrix3D);
                commandBuffer.SetViewMatrix(serverSideState.viewMatrix3D);
            }
            else
            {
                commandBuffer.SetViewMatrix(Matrix4x4.identity);
                commandBuffer.SetProjectionMatrix(orthoMatrix);
            }
        }

        private void BeginRender(bool mode2D = true)
        {
            if (began)
            {
                if (mode2D != !in3DMode)
                {
                    in3DMode = !mode2D;
                    UpdateRenderMode();
                }

                PrepareUnlitMaterial();
                return;
            }

            if (commandBuffer == null)
            {
                commandBuffer = new CommandBuffer();
                commandBuffer.name = "Mophun render buffer";
            }

            if (mophunCamera.renderingPath == RenderingPath.DeferredShading)
            {
                mophunCamera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, commandBuffer);
            }
            else
            {
                mophunCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
            }

            commandBuffer.Clear();

            commandBuffer.SetRenderTarget(screenTextureBackBuffer);
            commandBuffer.SetViewport(GetUnityScreenRect(serverSideState.viewportRect));
            
            began = true;
            in3DMode = !mode2D;

            UpdateRenderMode();
            PrepareUnlitMaterial();
        }

        public void EndFrame()
        {
        }

        public void ClearScreen(SColor color)
        {
            FlushBatch();

            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                BeginRender();
                commandBuffer.ClearRenderTarget(false, true, color.ToUnityColor());
            });
        }

        public void ClearDepth(float value)
        {
            FlushBatch();

            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                BeginRender();

                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetFloat(ZClearValueUniformName, value);

                commandBuffer.DrawMesh(quadMesh, Matrix4x4.identity, clearZMaterial, 0, 0, block);
            });
        }

        public ITexture CreateTexture(byte[] data, int width, int height, int mipCount, Driver.Graphics.TextureFormat format, Memory<SColor> palettes = new Memory<SColor>(), bool zeroAsTransparent = false)
        {
            ITexture result = null;

            JobScheduler.Instance.RunOnUnityThreadSync(() =>
            {
                result = new Texture(data, width, height, mipCount, format, palettes, zeroAsTransparent);
            });

            return result;
        }

        #region 2D draw library functions

        public void DrawText(int posX, int posY, int sizeX, int sizeY, List<int> positions, ITexture atlas, TextDirection direction, SColor textColor)
        {
            if (positions.Count % 2 != 0)
            {
                throw new ArgumentException("The list of bound values are not aligned by 2!");
            }

            // Just draw them one by one, verticies this small should not be instancing.
            // Batching them would be cool though :)
            int advX = (direction == TextDirection.Horizontal) ? sizeX : 0;
            int advY = (direction == TextDirection.VerticalUp) ? -sizeY : (direction == TextDirection.VerticalDown) ? sizeY : 0;

            Texture2D nativeTex = ((Texture)atlas).NativeTexture;

            float sizeXNormed = (float)sizeX / atlas.Width;
            float sizeYNormed = (float)sizeY / atlas.Height;

            for (int i = 0; i < positions.Count; i += 2)
            {
                Rect destRect = new Rect(posX, posY, sizeX, sizeY);
                Rect sourceRect = new Rect((float)positions[i] / atlas.Width, (float)positions[i + 1] / atlas.Height, sizeXNormed, sizeYNormed);

                DrawTexture(nativeTex, destRect, sourceRect, 0, 0, 0, textColor, false);

                posX += advX;
                posY += advY;
            }
        }

        public void DrawTexture(int posX, int posY, int centerX, int centerY, int rotation, ITexture texture,
            int sourceX = -1, int sourceY = -1, int width = -1, int height = -1, bool blackIsTransparent = false,
            bool flipX = false, bool flipY = false)
        {
            int widthToUse = (width == -1) ? texture.Width : width;
            int heightToUse = (height == -1) ? texture.Height : height;

            Rect destRect = new Rect(posX, posY, widthToUse, heightToUse);
            Rect sourceRect = new Rect(0, 0, 1, 1);

            if ((sourceX != -1) && (sourceY != -1))
            {
                sourceRect = new Rect((float)sourceX / texture.Width, (float)sourceY / texture.Height, (float)widthToUse / texture.Width, (float)heightToUse / texture.Height);
            }

            DrawTexture(texture, destRect, sourceRect, centerX, centerY, rotation, new SColor(1, 1, 1), blackIsTransparent, flipX, flipY);
        }

        private void DrawLineDetail(int x0, int y0, int x1, int y1, SColor lineColor, float lineThick)
        {
            Vector2 start = new Vector2(x0, -y0);
            Vector2 end = new Vector2(x1, -y1);

            // Our line must be reversed because the coordinate system is Y down
            Vector2 line = end - start;
            float angle = Vector2.SignedAngle(Vector2.right, line.normalized);

            Rect destRect = new Rect(x0, y0, line.magnitude, lineThick);
            DrawTexture(whiteTexture, destRect, new Rect(0, 0, 1, 1), 0, 0, angle, lineColor, false);
        }

        private void DrawLineThickScaled(int x0, int y0, int x1, int y1, SColor lineColor)
        {
            DrawLineDetail(x0, y0, x1, y1, lineColor, 1);
        }

        public void DrawLine(int x0, int y0, int x1, int y1, SColor lineColor)
        {
            DrawLineThickScaled(x0, y0, x1, y1, lineColor);
        }

        public void FillRect(int x0, int y0, int x1, int y1, SColor color)
        {
            Rect destRect = new Rect(x0, y0, x1 - x0, y1 - y0);
            Rect sourceRect = new Rect(0, 0, 1, 1);

            DrawTexture(whiteTexture, destRect, sourceRect, 0, 0, 0, color, false);
        }

        #endregion

        public void FlipScreen()
        {
            FlushBatch();

            DateTime flipStart = DateTime.Now;

            JobScheduler.Instance.RunOnUnityThreadSync(() =>
            {
                if (began)
                {
                    if (mophunCamera.renderingPath == RenderingPath.DeferredShading)
                    {
                        mophunCamera.AddCommandBuffer(CameraEvent.AfterGBuffer, commandBuffer);
                    }
                    else
                    {
                        mophunCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
                    }

                    fontMeshUsed = 0;
                    meshBatcher.Reset();

                    if (!displayImage.gameObject.activeSelf)
                    {            
                        displayImage.gameObject.SetActive(true);
                    }

                    began = false;
                }
            });

            DateTime now = DateTime.Now;
            double remaining = SecondPerFrame - (now - flipStart).TotalSeconds;

            if (remaining > 0.0f)
            {
                // Sleep to keep up with the predefined FPS
                System.Threading.Thread.Sleep((int)(remaining * 1000));
            }
        }

        private float EmulatedPointToPixels(float point)
        {
            return point * NGAGE_PPI / 72.0f;
        }

        public void SelectSystemFont(uint fontSize, uint fontFlags, int charCodeShouldBeInFont)
        {
            // TODO: Use the character hint
            float fontSizeNew = fontSize;

            if (BitUtil.FlagSet(fontSize, SystemFontSize.PixelFlag))
            {
                fontSizeNew = (fontSize & ~(uint)SystemFontSize.PixelFlag);
            }
            else if (BitUtil.FlagSet(fontSize, SystemFontSize.PointFlag))
            {
                fontSizeNew = (fontSize & ~(uint)SystemFontSize.PointFlag);
                fontSizeNew = EmulatedPointToPixels(fontSize);
            }
            else
            {
                switch ((SystemFontSize)fontSize)
                {
                    case SystemFontSize.Large:
                        {
                            fontSizeNew = 22;
                            break;
                        }

                    case SystemFontSize.Normal:
                        {
                            fontSizeNew = 17;
                            break;
                        }

                    case SystemFontSize.Small:
                        {
                            fontSizeNew = 11.5f;
                            break;
                        }
                }
            }

            selectedFontSize = fontSizeNew;

            TMPro.FontStyles styles = TMPro.FontStyles.Normal;

            if (BitUtil.FlagSet(fontFlags, SystemFontStyle.Bold))
            {
                styles |= TMPro.FontStyles.Bold;
            }

            if (BitUtil.FlagSet(fontFlags, SystemFontStyle.Italic))
            {
                styles |= TMPro.FontStyles.Italic;
            }

            if (BitUtil.FlagSet(fontFlags, SystemFontStyle.Underline))
            {
                styles |= TMPro.FontStyles.Underline;
            }

            if (BitUtil.FlagSet(fontFlags, SystemFontStyle.OutlineEffect))
            {
                selectedOutlineWidth = 0.2f;
            }
            else
            {
                selectedOutlineWidth = 0.0f;
            }

            selectedFontStyles = styles;
        }

        public void DrawSystemText(short x0, short y0, string text, SColor backColor, SColor foreColor)
        {
            FlushBatch();

            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                BeginRender();

                if (textRenderInternals.Count <= fontMeshUsed)
                {
                    // Add 5 more
                    for (int i = 0; i < TMPSpawnPerAlloc; i++)
                    {
                        GameObject newObj = Instantiate(textRenders[0].gameObject, textRenders[0].transform.parent);
                        newObj.name = $"RenderText{i + fontMeshUsed}";

                        textRenderInternals.Add(newObj.GetComponent<TMPro.TMP_Text>());
                    }
                }

                TMPro.TMP_Text textRender = textRenderInternals[fontMeshUsed];

                textRender.text = text;
                textRender.color = foreColor.ToUnityColor();
                textRender.fontStyle = selectedFontStyles;
                textRender.outlineWidth = selectedOutlineWidth;
                textRender.fontSize = selectedFontSize;
                textRender.outlineColor = backColor.ToUnityColor();
                textRender.isOverlay = true;

                LayoutRebuilder.ForceRebuildLayoutImmediate(textRender.rectTransform);
                textRender.ForceMeshUpdate();

                Matrix4x4 modelMatrix = Matrix4x4.TRS(GetUnityCoords(x0, y0), Quaternion.identity, Vector3.one);

                commandBuffer.DrawMesh(textRender.mesh, modelMatrix, textRender.fontSharedMaterial);
                fontMeshUsed++;
            });
        }

        public int GetStringExtentRelativeToSystemFont(string value)
        {
            if (measuredCache.TryGetValue(value, out int cachedLength))
            {
                return cachedLength;
            }

            if (measuredCache.Count >= MEASURED_CACHE_LIMIT)
            {
                // Purge all and redo
                measuredCache.Clear();
            }

            int resultValue = 0;

            JobScheduler.Instance.RunOnUnityThreadSync(() =>
            {
                textMeasure.fontStyle = selectedFontStyles;
                textMeasure.outlineWidth = selectedOutlineWidth;
                textMeasure.fontSize = selectedFontSize;

                Vector2 size = textMeasure.GetPreferredValues(value);
                if (value == " ")
                {
                    size.x = textMeasure.fontSize / 4.0f;
                }

                resultValue = (ushort)Math.Round(size.x) | ((ushort)Math.Round(size.y) << 16);
            });

            measuredCache.Add(value, resultValue);
            return resultValue;
        }

        private static Vector2 GetPivotMultiplier(BillboardPivot pivot)
        {
            switch (pivot)
            {
                case BillboardPivot.Center:
                    return new Vector2(0.5f, 0.5f);

                case BillboardPivot.TopLeft:
                    return new Vector2(0.0f, 1.0f);

                case BillboardPivot.TopRight:
                    return new Vector2(1.0f, 1.0f);

                case BillboardPivot.BottomLeft:
                case BillboardPivot.Bottom:
                    return new Vector2(0.0f, 0.0f);

                case BillboardPivot.BottomRight:
                    return new Vector2(0.0f, 1.0f);

                default:
                    throw new ArgumentException($"Unhandled pivot value: {pivot}");
            }
        }

        private MaterialPropertyBlock UnlitPropertyBlock
        {
            get
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetTexture(MainTexUniformName, serverSideState.textureMode ? serverSideState.mainTexture.NativeTexture : whiteTexture);
                block.SetFloat(UnlitTexturelessUniformName, serverSideState.textureMode ? 0.0f : 1.0f);
                block.SetFloat(UnlitTextureBlendModeUniformName, (float)serverSideState.blendMode + 0.5f);
                return block;
            }
        }

        public void DrawBillboard(NativeBillboard billboard)
        {
            BeginBatching(BatchingMode.Render3D);

            MpCullMode previousCull = Cull;
            Cull = MpCullMode.CounterClockwise;

            Rect destRect = new Rect(FixedUtil.FixedToFloat(billboard.position.fixedX), FixedUtil.FixedToFloat(billboard.position.fixedY),
                FixedUtil.FixedToFloat(billboard.fixedWidth), FixedUtil.FixedToFloat(billboard.fixedHeight));

            Vector2[] uvs = new Vector2[]
            {
                billboard.uv3.ToUnity(),
                billboard.uv2.ToUnity(),
                billboard.uv0.ToUnity(),
                billboard.uv1.ToUnity(),
            };

            Color[] colors = new Color[]
            {
                billboard.color3.ToUnity(),
                billboard.color2.ToUnity(),
                billboard.color0.ToUnity(),
                billboard.color1.ToUnity()
            };

            Vector2 center = destRect.size;

            center *= GetPivotMultiplier((BillboardPivot)billboard.rotationPointFlag);
            destRect.min += center;

            DrawRectBoardGeneral(destRect, uvs, center.x, center.y, billboard.rotation, colors, z: FixedUtil.FixedToFloat(billboard.position.fixedZ));
            Cull = previousCull;
        }

        public void DrawPrimitives(MpMesh meshToDraw)
        {
            BeginBatching(BatchingMode.Render3D);

            if (meshBatcher.Batchable(meshToDraw))
            {
                meshBatcher.Add(meshToDraw);
                return;
            }
            else
            {
                FlushBatch();

                uint identifier = meshCache.GetMeshIdentifier(meshToDraw, out Mesh targetMesh);

                JobScheduler.Instance.RunOnUnityThread(() =>
                {
                    BeginRender(mode2D: false);
                    if (targetMesh == null)
                    {
                        targetMesh = meshCache.GetMesh(identifier);
                    }

                    commandBuffer.DrawMesh(targetMesh, Matrix4x4.identity, currentMaterial, 0, 0, UnlitPropertyBlock);
                });
            }
        }

        public MpCullMode Cull
        {
            set
            {
                if (clientSideState.cullMode != value)
                {
                    clientSideState.cullMode = value;
                    HandleFixedStateChangedClient();

                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        serverSideState.cullMode = value;
                        fixedStateChanged = true;
                    });
                }
            }
            get
            {
                return clientSideState.cullMode;
            }
        }

        public MpCompareFunc DepthFunction
        {
            set
            {
                if (clientSideState.depthCompareFunc != value)
                {
                    clientSideState.depthCompareFunc = value;
                    HandleFixedStateChangedClient();

                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        serverSideState.depthCompareFunc = value;
                        fixedStateChanged = true;
                    });
                }
            }
            get => clientSideState.depthCompareFunc;
        }

        public MpBlendMode ColorBufferBlend
        {
            set
            {
                if (clientSideState.blendMode != value)
                {
                    clientSideState.blendMode = value;
                    HandleFixedStateChangedClient();

                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        serverSideState.blendMode = value;
                        fixedStateChanged = true;
                    });
                }
            }
            get => clientSideState.blendMode;
        }

        public bool TextureMode
        {
            set
            {
                if (clientSideState.textureMode != value)
                {
                    clientSideState.textureMode = value;
                    HandleFixedStateChangedClient();

                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        serverSideState.textureMode = value;
                    });
                }
            }
            get => clientSideState.textureMode;
        }

        public NRectangle ClipRect
        {
            set
            {
                Rect unityRect = value.ToUnity();

                if (clientSideState.scissorRect != unityRect)
                {
                    clientSideState.scissorRect = unityRect;

                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        serverSideState.scissorRect = unityRect;
                    });
                }
            }
            get => clientSideState.scissorRect.ToMophun();
        }

        public NRectangle Viewport
        {
            set
            {
                Rect unityRect = value.ToUnity();

                if (clientSideState.viewportRect != unityRect)
                {
                    clientSideState.viewportRect = unityRect;

                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        serverSideState.viewportRect = unityRect;

                        if (began)
                        {
                            commandBuffer.SetViewport(GetUnityScreenRect(unityRect));
                        }
                    });
                }
            }
            get => clientSideState.viewportRect.ToMophun();
        }

        public Matrix4x4 ProjectionMatrix3D
        {
            get => clientSideState.projectionMatrix3D;
            set
            {
                if (clientSideState.projectionMatrix3D != value)
                {
                    clientSideState.projectionMatrix3D = value;
                    HandleFixedStateChangedClient();

                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        serverSideState.projectionMatrix3D = value;

                        if (began && in3DMode)
                        {
                            commandBuffer.SetProjectionMatrix(value);
                        }
                    });
                }
            }
        }

        public Matrix4x4 ViewMatrix3D
        {
            get => clientSideState.viewMatrix3D;
            set
            {
                if (clientSideState.viewMatrix3D != value)
                {
                    clientSideState.viewMatrix3D = value;
                    HandleFixedStateChangedClient();

                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        serverSideState.viewMatrix3D = value;

                        if (began && in3DMode)
                        {
                            commandBuffer.SetViewMatrix(value);
                        }
                    });
                }
            }
        }

        public ITexture MainTexture
        {
            get => clientSideState.mainTexture;
            set
            {
                Texture casted = value as Texture;
                if (clientSideState.mainTexture != casted)
                {
                    clientSideState.mainTexture = casted;
                    HandleFixedStateChangedClient();

                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        serverSideState.mainTexture = casted;
                    });
                }
            }
        }

        public MpTextureBlendMode TextureBlendMode
        {
            get => clientSideState.textureBlendMode;
            set
            {
                if (clientSideState.textureBlendMode != value)
                {
                    clientSideState.textureBlendMode = value;
                    HandleFixedStateChangedClient();

                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        serverSideState.textureBlendMode = value;
                    });
                }
            }
        }

        public int ScreenWidth => (int)screenSize.x;

        public int ScreenHeight => (int)screenSize.y;
    } 
}