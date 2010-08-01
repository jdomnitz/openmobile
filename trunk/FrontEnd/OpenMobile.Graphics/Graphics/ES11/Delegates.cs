#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion
#if EXPERIMENTAL
namespace OpenMobile.Graphics.ES11
{
    using System;
    using System.Text;
    using System.Runtime.InteropServices;
    #pragma warning disable 0649
    #pragma warning disable 3019
    #pragma warning disable 1591

    partial class GL
    {
        internal static partial class Delegates
        {
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ActiveTexture(OpenMobile.Graphics.ES11.All texture);
            internal static ActiveTexture glActiveTexture;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void AlphaFunc(OpenMobile.Graphics.ES11.All func, Single @ref);
            internal static AlphaFunc glAlphaFunc;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void AlphaFuncx(OpenMobile.Graphics.ES11.All func, int @ref);
            internal static AlphaFuncx glAlphaFuncx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void AlphaFuncxOES(OpenMobile.Graphics.ES11.All func, int @ref);
            internal static AlphaFuncxOES glAlphaFuncxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void BindBuffer(OpenMobile.Graphics.ES11.All target, UInt32 buffer);
            internal static BindBuffer glBindBuffer;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void BindFramebufferOES(OpenMobile.Graphics.ES11.All target, UInt32 framebuffer);
            internal static BindFramebufferOES glBindFramebufferOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void BindRenderbufferOES(OpenMobile.Graphics.ES11.All target, UInt32 renderbuffer);
            internal static BindRenderbufferOES glBindRenderbufferOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void BindTexture(OpenMobile.Graphics.ES11.All target, UInt32 texture);
            internal static BindTexture glBindTexture;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void BlendEquationOES(OpenMobile.Graphics.ES11.All mode);
            internal static BlendEquationOES glBlendEquationOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void BlendEquationSeparateOES(OpenMobile.Graphics.ES11.All modeRGB, OpenMobile.Graphics.ES11.All modeAlpha);
            internal static BlendEquationSeparateOES glBlendEquationSeparateOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void BlendFunc(OpenMobile.Graphics.ES11.All sfactor, OpenMobile.Graphics.ES11.All dfactor);
            internal static BlendFunc glBlendFunc;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void BlendFuncSeparateOES(OpenMobile.Graphics.ES11.All srcRGB, OpenMobile.Graphics.ES11.All dstRGB, OpenMobile.Graphics.ES11.All srcAlpha, OpenMobile.Graphics.ES11.All dstAlpha);
            internal static BlendFuncSeparateOES glBlendFuncSeparateOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void BufferData(OpenMobile.Graphics.ES11.All target, IntPtr size, IntPtr data, OpenMobile.Graphics.ES11.All usage);
            internal static BufferData glBufferData;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void BufferSubData(OpenMobile.Graphics.ES11.All target, IntPtr offset, IntPtr size, IntPtr data);
            internal static BufferSubData glBufferSubData;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate OpenMobile.Graphics.ES11.All CheckFramebufferStatusOES(OpenMobile.Graphics.ES11.All target);
            internal static CheckFramebufferStatusOES glCheckFramebufferStatusOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Clear(UInt32 mask);
            internal static Clear glClear;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ClearColor(Single red, Single green, Single blue, Single alpha);
            internal static ClearColor glClearColor;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ClearColorx(int red, int green, int blue, int alpha);
            internal static ClearColorx glClearColorx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ClearColorxOES(int red, int green, int blue, int alpha);
            internal static ClearColorxOES glClearColorxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ClearDepthf(Single depth);
            internal static ClearDepthf glClearDepthf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ClearDepthfOES(Single depth);
            internal static ClearDepthfOES glClearDepthfOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ClearDepthx(int depth);
            internal static ClearDepthx glClearDepthx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ClearDepthxOES(int depth);
            internal static ClearDepthxOES glClearDepthxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ClearStencil(Int32 s);
            internal static ClearStencil glClearStencil;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ClientActiveTexture(OpenMobile.Graphics.ES11.All texture);
            internal static ClientActiveTexture glClientActiveTexture;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void ClipPlanef(OpenMobile.Graphics.ES11.All plane, Single* equation);
            internal unsafe static ClipPlanef glClipPlanef;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void ClipPlanefIMG(OpenMobile.Graphics.ES11.All p, Single* eqn);
            internal unsafe static ClipPlanefIMG glClipPlanefIMG;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void ClipPlanefOES(OpenMobile.Graphics.ES11.All plane, Single* equation);
            internal unsafe static ClipPlanefOES glClipPlanefOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void ClipPlanex(OpenMobile.Graphics.ES11.All plane, int* equation);
            internal unsafe static ClipPlanex glClipPlanex;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void ClipPlanexIMG(OpenMobile.Graphics.ES11.All p, int* eqn);
            internal unsafe static ClipPlanexIMG glClipPlanexIMG;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void ClipPlanexOES(OpenMobile.Graphics.ES11.All plane, int* equation);
            internal unsafe static ClipPlanexOES glClipPlanexOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Color4f(Single red, Single green, Single blue, Single alpha);
            internal static Color4f glColor4f;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Color4ub(Byte red, Byte green, Byte blue, Byte alpha);
            internal static Color4ub glColor4ub;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Color4x(int red, int green, int blue, int alpha);
            internal static Color4x glColor4x;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Color4xOES(int red, int green, int blue, int alpha);
            internal static Color4xOES glColor4xOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ColorMask(bool red, bool green, bool blue, bool alpha);
            internal static ColorMask glColorMask;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ColorPointer(Int32 size, OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            internal static ColorPointer glColorPointer;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void CompressedTexImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, OpenMobile.Graphics.ES11.All internalformat, Int32 width, Int32 height, Int32 border, Int32 imageSize, IntPtr data);
            internal static CompressedTexImage2D glCompressedTexImage2D;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void CompressedTexSubImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.ES11.All format, Int32 imageSize, IntPtr data);
            internal static CompressedTexSubImage2D glCompressedTexSubImage2D;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void CopyTexImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, OpenMobile.Graphics.ES11.All internalformat, Int32 x, Int32 y, Int32 width, Int32 height, Int32 border);
            internal static CopyTexImage2D glCopyTexImage2D;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void CopyTexSubImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            internal static CopyTexSubImage2D glCopyTexSubImage2D;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void CullFace(OpenMobile.Graphics.ES11.All mode);
            internal static CullFace glCullFace;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void CurrentPaletteMatrixOES(UInt32 matrixpaletteindex);
            internal static CurrentPaletteMatrixOES glCurrentPaletteMatrixOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void DeleteBuffers(Int32 n, UInt32* buffers);
            internal unsafe static DeleteBuffers glDeleteBuffers;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void DeleteFencesNV(Int32 n, UInt32* fences);
            internal unsafe static DeleteFencesNV glDeleteFencesNV;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void DeleteFramebuffersOES(Int32 n, UInt32* framebuffers);
            internal unsafe static DeleteFramebuffersOES glDeleteFramebuffersOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void DeleteRenderbuffersOES(Int32 n, UInt32* renderbuffers);
            internal unsafe static DeleteRenderbuffersOES glDeleteRenderbuffersOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void DeleteTextures(Int32 n, UInt32* textures);
            internal unsafe static DeleteTextures glDeleteTextures;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DepthFunc(OpenMobile.Graphics.ES11.All func);
            internal static DepthFunc glDepthFunc;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DepthMask(bool flag);
            internal static DepthMask glDepthMask;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DepthRangef(Single zNear, Single zFar);
            internal static DepthRangef glDepthRangef;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DepthRangefOES(Single zNear, Single zFar);
            internal static DepthRangefOES glDepthRangefOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DepthRangex(int zNear, int zFar);
            internal static DepthRangex glDepthRangex;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DepthRangexOES(int zNear, int zFar);
            internal static DepthRangexOES glDepthRangexOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Disable(OpenMobile.Graphics.ES11.All cap);
            internal static Disable glDisable;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DisableClientState(OpenMobile.Graphics.ES11.All array);
            internal static DisableClientState glDisableClientState;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DisableDriverControlQCOM(UInt32 driverControl);
            internal static DisableDriverControlQCOM glDisableDriverControlQCOM;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DrawArrays(OpenMobile.Graphics.ES11.All mode, Int32 first, Int32 count);
            internal static DrawArrays glDrawArrays;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DrawElements(OpenMobile.Graphics.ES11.All mode, Int32 count, OpenMobile.Graphics.ES11.All type, IntPtr indices);
            internal static DrawElements glDrawElements;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DrawTexfOES(Single x, Single y, Single z, Single width, Single height);
            internal static DrawTexfOES glDrawTexfOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void DrawTexfvOES(Single* coords);
            internal unsafe static DrawTexfvOES glDrawTexfvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DrawTexiOES(Int32 x, Int32 y, Int32 z, Int32 width, Int32 height);
            internal static DrawTexiOES glDrawTexiOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void DrawTexivOES(Int32* coords);
            internal unsafe static DrawTexivOES glDrawTexivOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DrawTexsOES(Int16 x, Int16 y, Int16 z, Int16 width, Int16 height);
            internal static DrawTexsOES glDrawTexsOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void DrawTexsvOES(Int16* coords);
            internal unsafe static DrawTexsvOES glDrawTexsvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void DrawTexxOES(int x, int y, int z, int width, int height);
            internal static DrawTexxOES glDrawTexxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void DrawTexxvOES(int* coords);
            internal unsafe static DrawTexxvOES glDrawTexxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void EGLImageTargetRenderbufferStorageOES(OpenMobile.Graphics.ES11.All target, IntPtr image);
            internal static EGLImageTargetRenderbufferStorageOES glEGLImageTargetRenderbufferStorageOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void EGLImageTargetTexture2DOES(OpenMobile.Graphics.ES11.All target, IntPtr image);
            internal static EGLImageTargetTexture2DOES glEGLImageTargetTexture2DOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Enable(OpenMobile.Graphics.ES11.All cap);
            internal static Enable glEnable;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void EnableClientState(OpenMobile.Graphics.ES11.All array);
            internal static EnableClientState glEnableClientState;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void EnableDriverControlQCOM(UInt32 driverControl);
            internal static EnableDriverControlQCOM glEnableDriverControlQCOM;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Finish();
            internal static Finish glFinish;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void FinishFenceNV(UInt32 fence);
            internal static FinishFenceNV glFinishFenceNV;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Flush();
            internal static Flush glFlush;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Fogf(OpenMobile.Graphics.ES11.All pname, Single param);
            internal static Fogf glFogf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void Fogfv(OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static Fogfv glFogfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Fogx(OpenMobile.Graphics.ES11.All pname, int param);
            internal static Fogx glFogx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void FogxOES(OpenMobile.Graphics.ES11.All pname, int param);
            internal static FogxOES glFogxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void Fogxv(OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static Fogxv glFogxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void FogxvOES(OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static FogxvOES glFogxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void FramebufferRenderbufferOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All attachment, OpenMobile.Graphics.ES11.All renderbuffertarget, UInt32 renderbuffer);
            internal static FramebufferRenderbufferOES glFramebufferRenderbufferOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void FramebufferTexture2DOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All attachment, OpenMobile.Graphics.ES11.All textarget, UInt32 texture, Int32 level);
            internal static FramebufferTexture2DOES glFramebufferTexture2DOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void FrontFace(OpenMobile.Graphics.ES11.All mode);
            internal static FrontFace glFrontFace;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Frustumf(Single left, Single right, Single bottom, Single top, Single zNear, Single zFar);
            internal static Frustumf glFrustumf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void FrustumfOES(Single left, Single right, Single bottom, Single top, Single zNear, Single zFar);
            internal static FrustumfOES glFrustumfOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Frustumx(int left, int right, int bottom, int top, int zNear, int zFar);
            internal static Frustumx glFrustumx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void FrustumxOES(int left, int right, int bottom, int top, int zNear, int zFar);
            internal static FrustumxOES glFrustumxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GenBuffers(Int32 n, UInt32* buffers);
            internal unsafe static GenBuffers glGenBuffers;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void GenerateMipmapOES(OpenMobile.Graphics.ES11.All target);
            internal static GenerateMipmapOES glGenerateMipmapOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GenFencesNV(Int32 n, UInt32* fences);
            internal unsafe static GenFencesNV glGenFencesNV;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GenFramebuffersOES(Int32 n, UInt32* framebuffers);
            internal unsafe static GenFramebuffersOES glGenFramebuffersOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GenRenderbuffersOES(Int32 n, UInt32* renderbuffers);
            internal unsafe static GenRenderbuffersOES glGenRenderbuffersOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GenTextures(Int32 n, UInt32* textures);
            internal unsafe static GenTextures glGenTextures;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetBooleanv(OpenMobile.Graphics.ES11.All pname, bool* @params);
            internal unsafe static GetBooleanv glGetBooleanv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetBufferParameteriv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static GetBufferParameteriv glGetBufferParameteriv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void GetBufferPointervOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, IntPtr @params);
            internal static GetBufferPointervOES glGetBufferPointervOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetClipPlanef(OpenMobile.Graphics.ES11.All pname, Single* eqn);
            internal unsafe static GetClipPlanef glGetClipPlanef;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetClipPlanefOES(OpenMobile.Graphics.ES11.All pname, Single* eqn);
            internal unsafe static GetClipPlanefOES glGetClipPlanefOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetClipPlanex(OpenMobile.Graphics.ES11.All pname, int* eqn);
            internal unsafe static GetClipPlanex glGetClipPlanex;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetClipPlanexOES(OpenMobile.Graphics.ES11.All pname, int* eqn);
            internal unsafe static GetClipPlanexOES glGetClipPlanexOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetDriverControlsQCOM(Int32* num, Int32 size, UInt32* driverControls);
            internal unsafe static GetDriverControlsQCOM glGetDriverControlsQCOM;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetDriverControlStringQCOM(UInt32 driverControl, Int32 bufSize, Int32* length, String driverControlString);
            internal unsafe static GetDriverControlStringQCOM glGetDriverControlStringQCOM;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate OpenMobile.Graphics.ES11.All GetError();
            internal static GetError glGetError;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetFenceivNV(UInt32 fence, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static GetFenceivNV glGetFenceivNV;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetFixedv(OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetFixedv glGetFixedv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetFixedvOES(OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetFixedvOES glGetFixedvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetFloatv(OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static GetFloatv glGetFloatv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetFramebufferAttachmentParameterivOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All attachment, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static GetFramebufferAttachmentParameterivOES glGetFramebufferAttachmentParameterivOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetIntegerv(OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static GetIntegerv glGetIntegerv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetLightfv(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static GetLightfv glGetLightfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetLightxv(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetLightxv glGetLightxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetLightxvOES(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetLightxvOES glGetLightxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetMaterialfv(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static GetMaterialfv glGetMaterialfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetMaterialxv(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetMaterialxv glGetMaterialxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetMaterialxvOES(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetMaterialxvOES glGetMaterialxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void GetPointerv(OpenMobile.Graphics.ES11.All pname, IntPtr @params);
            internal static GetPointerv glGetPointerv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetRenderbufferParameterivOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static GetRenderbufferParameterivOES glGetRenderbufferParameterivOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate System.IntPtr GetString(OpenMobile.Graphics.ES11.All name);
            internal unsafe static GetString glGetString;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexEnvfv(OpenMobile.Graphics.ES11.All env, OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static GetTexEnvfv glGetTexEnvfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexEnviv(OpenMobile.Graphics.ES11.All env, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static GetTexEnviv glGetTexEnviv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexEnvxv(OpenMobile.Graphics.ES11.All env, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetTexEnvxv glGetTexEnvxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexEnvxvOES(OpenMobile.Graphics.ES11.All env, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetTexEnvxvOES glGetTexEnvxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexGenfvOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static GetTexGenfvOES glGetTexGenfvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexGenivOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static GetTexGenivOES glGetTexGenivOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexGenxvOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetTexGenxvOES glGetTexGenxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexParameterfv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static GetTexParameterfv glGetTexParameterfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexParameteriv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static GetTexParameteriv glGetTexParameteriv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexParameterxv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetTexParameterxv glGetTexParameterxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void GetTexParameterxvOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static GetTexParameterxvOES glGetTexParameterxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Hint(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All mode);
            internal static Hint glHint;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate bool IsBuffer(UInt32 buffer);
            internal static IsBuffer glIsBuffer;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate bool IsEnabled(OpenMobile.Graphics.ES11.All cap);
            internal static IsEnabled glIsEnabled;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate bool IsFenceNV(UInt32 fence);
            internal static IsFenceNV glIsFenceNV;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate bool IsFramebufferOES(UInt32 framebuffer);
            internal static IsFramebufferOES glIsFramebufferOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate bool IsRenderbufferOES(UInt32 renderbuffer);
            internal static IsRenderbufferOES glIsRenderbufferOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate bool IsTexture(UInt32 texture);
            internal static IsTexture glIsTexture;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Lightf(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, Single param);
            internal static Lightf glLightf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void Lightfv(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static Lightfv glLightfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void LightModelf(OpenMobile.Graphics.ES11.All pname, Single param);
            internal static LightModelf glLightModelf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void LightModelfv(OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static LightModelfv glLightModelfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void LightModelx(OpenMobile.Graphics.ES11.All pname, int param);
            internal static LightModelx glLightModelx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void LightModelxOES(OpenMobile.Graphics.ES11.All pname, int param);
            internal static LightModelxOES glLightModelxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void LightModelxv(OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static LightModelxv glLightModelxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void LightModelxvOES(OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static LightModelxvOES glLightModelxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Lightx(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int param);
            internal static Lightx glLightx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void LightxOES(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int param);
            internal static LightxOES glLightxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void Lightxv(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static Lightxv glLightxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void LightxvOES(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static LightxvOES glLightxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void LineWidth(Single width);
            internal static LineWidth glLineWidth;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void LineWidthx(int width);
            internal static LineWidthx glLineWidthx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void LineWidthxOES(int width);
            internal static LineWidthxOES glLineWidthxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void LoadIdentity();
            internal static LoadIdentity glLoadIdentity;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void LoadMatrixf(Single* m);
            internal unsafe static LoadMatrixf glLoadMatrixf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void LoadMatrixx(int* m);
            internal unsafe static LoadMatrixx glLoadMatrixx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void LoadMatrixxOES(int* m);
            internal unsafe static LoadMatrixxOES glLoadMatrixxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void LoadPaletteFromModelViewMatrixOES();
            internal static LoadPaletteFromModelViewMatrixOES glLoadPaletteFromModelViewMatrixOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void LogicOp(OpenMobile.Graphics.ES11.All opcode);
            internal static LogicOp glLogicOp;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate System.IntPtr MapBufferOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All access);
            internal unsafe static MapBufferOES glMapBufferOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Materialf(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, Single param);
            internal static Materialf glMaterialf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void Materialfv(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static Materialfv glMaterialfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Materialx(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int param);
            internal static Materialx glMaterialx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void MaterialxOES(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int param);
            internal static MaterialxOES glMaterialxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void Materialxv(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static Materialxv glMaterialxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void MaterialxvOES(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static MaterialxvOES glMaterialxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void MatrixIndexPointerOES(Int32 size, OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            internal static MatrixIndexPointerOES glMatrixIndexPointerOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void MatrixMode(OpenMobile.Graphics.ES11.All mode);
            internal static MatrixMode glMatrixMode;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void MultiTexCoord4f(OpenMobile.Graphics.ES11.All target, Single s, Single t, Single r, Single q);
            internal static MultiTexCoord4f glMultiTexCoord4f;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void MultiTexCoord4x(OpenMobile.Graphics.ES11.All target, int s, int t, int r, int q);
            internal static MultiTexCoord4x glMultiTexCoord4x;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void MultiTexCoord4xOES(OpenMobile.Graphics.ES11.All target, int s, int t, int r, int q);
            internal static MultiTexCoord4xOES glMultiTexCoord4xOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void MultMatrixf(Single* m);
            internal unsafe static MultMatrixf glMultMatrixf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void MultMatrixx(int* m);
            internal unsafe static MultMatrixx glMultMatrixx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void MultMatrixxOES(int* m);
            internal unsafe static MultMatrixxOES glMultMatrixxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Normal3f(Single nx, Single ny, Single nz);
            internal static Normal3f glNormal3f;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Normal3x(int nx, int ny, int nz);
            internal static Normal3x glNormal3x;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Normal3xOES(int nx, int ny, int nz);
            internal static Normal3xOES glNormal3xOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void NormalPointer(OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            internal static NormalPointer glNormalPointer;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Orthof(Single left, Single right, Single bottom, Single top, Single zNear, Single zFar);
            internal static Orthof glOrthof;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void OrthofOES(Single left, Single right, Single bottom, Single top, Single zNear, Single zFar);
            internal static OrthofOES glOrthofOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Orthox(int left, int right, int bottom, int top, int zNear, int zFar);
            internal static Orthox glOrthox;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void OrthoxOES(int left, int right, int bottom, int top, int zNear, int zFar);
            internal static OrthoxOES glOrthoxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PixelStorei(OpenMobile.Graphics.ES11.All pname, Int32 param);
            internal static PixelStorei glPixelStorei;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PointParameterf(OpenMobile.Graphics.ES11.All pname, Single param);
            internal static PointParameterf glPointParameterf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void PointParameterfv(OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static PointParameterfv glPointParameterfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PointParameterx(OpenMobile.Graphics.ES11.All pname, int param);
            internal static PointParameterx glPointParameterx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PointParameterxOES(OpenMobile.Graphics.ES11.All pname, int param);
            internal static PointParameterxOES glPointParameterxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void PointParameterxv(OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static PointParameterxv glPointParameterxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void PointParameterxvOES(OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static PointParameterxvOES glPointParameterxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PointSize(Single size);
            internal static PointSize glPointSize;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PointSizePointerOES(OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            internal static PointSizePointerOES glPointSizePointerOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PointSizex(int size);
            internal static PointSizex glPointSizex;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PointSizexOES(int size);
            internal static PointSizexOES glPointSizexOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PolygonOffset(Single factor, Single units);
            internal static PolygonOffset glPolygonOffset;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PolygonOffsetx(int factor, int units);
            internal static PolygonOffsetx glPolygonOffsetx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PolygonOffsetxOES(int factor, int units);
            internal static PolygonOffsetxOES glPolygonOffsetxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PopMatrix();
            internal static PopMatrix glPopMatrix;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void PushMatrix();
            internal static PushMatrix glPushMatrix;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate Int32 QueryMatrixxOES(int* mantissa, Int32* exponent);
            internal unsafe static QueryMatrixxOES glQueryMatrixxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ReadPixels(Int32 x, Int32 y, Int32 width, Int32 height, OpenMobile.Graphics.ES11.All format, OpenMobile.Graphics.ES11.All type, IntPtr pixels);
            internal static ReadPixels glReadPixels;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void RenderbufferStorageOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All internalformat, Int32 width, Int32 height);
            internal static RenderbufferStorageOES glRenderbufferStorageOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Rotatef(Single angle, Single x, Single y, Single z);
            internal static Rotatef glRotatef;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Rotatex(int angle, int x, int y, int z);
            internal static Rotatex glRotatex;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void RotatexOES(int angle, int x, int y, int z);
            internal static RotatexOES glRotatexOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void SampleCoverage(Single value, bool invert);
            internal static SampleCoverage glSampleCoverage;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void SampleCoveragex(int value, bool invert);
            internal static SampleCoveragex glSampleCoveragex;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void SampleCoveragexOES(int value, bool invert);
            internal static SampleCoveragexOES glSampleCoveragexOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Scalef(Single x, Single y, Single z);
            internal static Scalef glScalef;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Scalex(int x, int y, int z);
            internal static Scalex glScalex;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ScalexOES(int x, int y, int z);
            internal static ScalexOES glScalexOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Scissor(Int32 x, Int32 y, Int32 width, Int32 height);
            internal static Scissor glScissor;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void SetFenceNV(UInt32 fence, OpenMobile.Graphics.ES11.All condition);
            internal static SetFenceNV glSetFenceNV;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void ShadeModel(OpenMobile.Graphics.ES11.All mode);
            internal static ShadeModel glShadeModel;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void StencilFunc(OpenMobile.Graphics.ES11.All func, Int32 @ref, UInt32 mask);
            internal static StencilFunc glStencilFunc;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void StencilMask(UInt32 mask);
            internal static StencilMask glStencilMask;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void StencilOp(OpenMobile.Graphics.ES11.All fail, OpenMobile.Graphics.ES11.All zfail, OpenMobile.Graphics.ES11.All zpass);
            internal static StencilOp glStencilOp;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate bool TestFenceNV(UInt32 fence);
            internal static TestFenceNV glTestFenceNV;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexCoordPointer(Int32 size, OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            internal static TexCoordPointer glTexCoordPointer;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexEnvf(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Single param);
            internal static TexEnvf glTexEnvf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexEnvfv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static TexEnvfv glTexEnvfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexEnvi(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32 param);
            internal static TexEnvi glTexEnvi;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexEnviv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static TexEnviv glTexEnviv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexEnvx(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int param);
            internal static TexEnvx glTexEnvx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexEnvxOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int param);
            internal static TexEnvxOES glTexEnvxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexEnvxv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static TexEnvxv glTexEnvxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexEnvxvOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static TexEnvxvOES glTexEnvxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexGenfOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Single param);
            internal static TexGenfOES glTexGenfOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexGenfvOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static TexGenfvOES glTexGenfvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexGeniOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Int32 param);
            internal static TexGeniOES glTexGeniOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexGenivOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static TexGenivOES glTexGenivOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexGenxOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, int param);
            internal static TexGenxOES glTexGenxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexGenxvOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static TexGenxvOES glTexGenxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, Int32 internalformat, Int32 width, Int32 height, Int32 border, OpenMobile.Graphics.ES11.All format, OpenMobile.Graphics.ES11.All type, IntPtr pixels);
            internal static TexImage2D glTexImage2D;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexParameterf(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Single param);
            internal static TexParameterf glTexParameterf;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexParameterfv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Single* @params);
            internal unsafe static TexParameterfv glTexParameterfv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexParameteri(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32 param);
            internal static TexParameteri glTexParameteri;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexParameteriv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            internal unsafe static TexParameteriv glTexParameteriv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexParameterx(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int param);
            internal static TexParameterx glTexParameterx;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexParameterxOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int param);
            internal static TexParameterxOES glTexParameterxOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexParameterxv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static TexParameterxv glTexParameterxv;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal unsafe delegate void TexParameterxvOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            internal unsafe static TexParameterxvOES glTexParameterxvOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TexSubImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.ES11.All format, OpenMobile.Graphics.ES11.All type, IntPtr pixels);
            internal static TexSubImage2D glTexSubImage2D;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Translatef(Single x, Single y, Single z);
            internal static Translatef glTranslatef;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Translatex(int x, int y, int z);
            internal static Translatex glTranslatex;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void TranslatexOES(int x, int y, int z);
            internal static TranslatexOES glTranslatexOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate bool UnmapBufferOES(OpenMobile.Graphics.ES11.All target);
            internal static UnmapBufferOES glUnmapBufferOES;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void VertexPointer(Int32 size, OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            internal static VertexPointer glVertexPointer;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void Viewport(Int32 x, Int32 y, Int32 width, Int32 height);
            internal static Viewport glViewport;
            [System.Security.SuppressUnmanagedCodeSecurity()]
            internal delegate void WeightPointerOES(Int32 size, OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            internal static WeightPointerOES glWeightPointerOES;
        }
    }
}
#endif