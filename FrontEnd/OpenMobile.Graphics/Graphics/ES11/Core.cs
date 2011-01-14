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

namespace OpenMobile.Graphics.ES11
{
    using System;
    using System.Text;
    using System.Runtime.InteropServices;
    #pragma warning disable 3019
    #pragma warning disable 1591

    partial class Raw
    {

        internal static partial class Core
        {

            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glActiveTexture")]
            internal extern static void ActiveTexture(OpenMobile.Graphics.ES11.All texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAlphaFunc")]
            internal extern static void AlphaFunc(OpenMobile.Graphics.ES11.All func, Single @ref);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAlphaFuncx")]
            internal extern static void AlphaFuncx(OpenMobile.Graphics.ES11.All func, int @ref);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAlphaFuncxOES")]
            internal extern static void AlphaFuncxOES(OpenMobile.Graphics.ES11.All func, int @ref);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBuffer")]
            internal extern static void BindBuffer(OpenMobile.Graphics.ES11.All target, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindFramebufferOES")]
            internal extern static void BindFramebufferOES(OpenMobile.Graphics.ES11.All target, UInt32 framebuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindRenderbufferOES")]
            internal extern static void BindRenderbufferOES(OpenMobile.Graphics.ES11.All target, UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindTexture")]
            internal extern static void BindTexture(OpenMobile.Graphics.ES11.All target, UInt32 texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendEquationOES")]
            internal extern static void BlendEquationOES(OpenMobile.Graphics.ES11.All mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendEquationSeparateOES")]
            internal extern static void BlendEquationSeparateOES(OpenMobile.Graphics.ES11.All modeRGB, OpenMobile.Graphics.ES11.All modeAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendFunc")]
            internal extern static void BlendFunc(OpenMobile.Graphics.ES11.All sfactor, OpenMobile.Graphics.ES11.All dfactor);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendFuncSeparateOES")]
            internal extern static void BlendFuncSeparateOES(OpenMobile.Graphics.ES11.All srcRGB, OpenMobile.Graphics.ES11.All dstRGB, OpenMobile.Graphics.ES11.All srcAlpha, OpenMobile.Graphics.ES11.All dstAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBufferData")]
            internal extern static void BufferData(OpenMobile.Graphics.ES11.All target, IntPtr size, IntPtr data, OpenMobile.Graphics.ES11.All usage);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBufferSubData")]
            internal extern static void BufferSubData(OpenMobile.Graphics.ES11.All target, IntPtr offset, IntPtr size, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCheckFramebufferStatusOES")]
            internal extern static OpenMobile.Graphics.ES11.All CheckFramebufferStatusOES(OpenMobile.Graphics.ES11.All target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClear")]
            internal extern static void Clear(UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearColor")]
            internal extern static void ClearColor(Single red, Single green, Single blue, Single alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearColorx")]
            internal extern static void ClearColorx(int red, int green, int blue, int alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearColorxOES")]
            internal extern static void ClearColorxOES(int red, int green, int blue, int alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearDepthf")]
            internal extern static void ClearDepthf(Single depth);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearDepthfOES")]
            internal extern static void ClearDepthfOES(Single depth);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearDepthx")]
            internal extern static void ClearDepthx(int depth);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearDepthxOES")]
            internal extern static void ClearDepthxOES(int depth);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearStencil")]
            internal extern static void ClearStencil(Int32 s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClientActiveTexture")]
            internal extern static void ClientActiveTexture(OpenMobile.Graphics.ES11.All texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClipPlanef")]
            internal extern static unsafe void ClipPlanef(OpenMobile.Graphics.ES11.All plane, Single* equation);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClipPlanefIMG")]
            internal extern static unsafe void ClipPlanefIMG(OpenMobile.Graphics.ES11.All p, Single* eqn);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClipPlanefOES")]
            internal extern static unsafe void ClipPlanefOES(OpenMobile.Graphics.ES11.All plane, Single* equation);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClipPlanex")]
            internal extern static unsafe void ClipPlanex(OpenMobile.Graphics.ES11.All plane, int* equation);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClipPlanexIMG")]
            internal extern static unsafe void ClipPlanexIMG(OpenMobile.Graphics.ES11.All p, int* eqn);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClipPlanexOES")]
            internal extern static unsafe void ClipPlanexOES(OpenMobile.Graphics.ES11.All plane, int* equation);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4f")]
            internal extern static void Color4f(Single red, Single green, Single blue, Single alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4ub")]
            internal extern static void Color4ub(Byte red, Byte green, Byte blue, Byte alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4x")]
            internal extern static void Color4x(int red, int green, int blue, int alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4xOES")]
            internal extern static void Color4xOES(int red, int green, int blue, int alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorMask")]
            internal extern static void ColorMask(bool red, bool green, bool blue, bool alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorPointer")]
            internal extern static void ColorPointer(Int32 size, OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexImage2D")]
            internal extern static void CompressedTexImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, OpenMobile.Graphics.ES11.All internalformat, Int32 width, Int32 height, Int32 border, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexSubImage2D")]
            internal extern static void CompressedTexSubImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.ES11.All format, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexImage2D")]
            internal extern static void CopyTexImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, OpenMobile.Graphics.ES11.All internalformat, Int32 x, Int32 y, Int32 width, Int32 height, Int32 border);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexSubImage2D")]
            internal extern static void CopyTexSubImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCullFace")]
            internal extern static void CullFace(OpenMobile.Graphics.ES11.All mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCurrentPaletteMatrixOES")]
            internal extern static void CurrentPaletteMatrixOES(UInt32 matrixpaletteindex);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteBuffers")]
            internal extern static unsafe void DeleteBuffers(Int32 n, UInt32* buffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteFencesNV")]
            internal extern static unsafe void DeleteFencesNV(Int32 n, UInt32* fences);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteFramebuffersOES")]
            internal extern static unsafe void DeleteFramebuffersOES(Int32 n, UInt32* framebuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteRenderbuffersOES")]
            internal extern static unsafe void DeleteRenderbuffersOES(Int32 n, UInt32* renderbuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteTextures")]
            internal extern static unsafe void DeleteTextures(Int32 n, UInt32* textures);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthFunc")]
            internal extern static void DepthFunc(OpenMobile.Graphics.ES11.All func);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthMask")]
            internal extern static void DepthMask(bool flag);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthRangef")]
            internal extern static void DepthRangef(Single zNear, Single zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthRangefOES")]
            internal extern static void DepthRangefOES(Single zNear, Single zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthRangex")]
            internal extern static void DepthRangex(int zNear, int zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthRangexOES")]
            internal extern static void DepthRangexOES(int zNear, int zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisable")]
            internal extern static void Disable(OpenMobile.Graphics.ES11.All cap);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisableClientState")]
            internal extern static void DisableClientState(OpenMobile.Graphics.ES11.All array);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisableDriverControlQCOM")]
            internal extern static void DisableDriverControlQCOM(UInt32 driverControl);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawArrays")]
            internal extern static void DrawArrays(OpenMobile.Graphics.ES11.All mode, Int32 first, Int32 count);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawElements")]
            internal extern static void DrawElements(OpenMobile.Graphics.ES11.All mode, Int32 count, OpenMobile.Graphics.ES11.All type, IntPtr indices);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawTexfOES")]
            internal extern static void DrawTexfOES(Single x, Single y, Single z, Single width, Single height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawTexfvOES")]
            internal extern static unsafe void DrawTexfvOES(Single* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawTexiOES")]
            internal extern static void DrawTexiOES(Int32 x, Int32 y, Int32 z, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawTexivOES")]
            internal extern static unsafe void DrawTexivOES(Int32* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawTexsOES")]
            internal extern static void DrawTexsOES(Int16 x, Int16 y, Int16 z, Int16 width, Int16 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawTexsvOES")]
            internal extern static unsafe void DrawTexsvOES(Int16* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawTexxOES")]
            internal extern static void DrawTexxOES(int x, int y, int z, int width, int height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawTexxvOES")]
            internal extern static unsafe void DrawTexxvOES(int* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEGLImageTargetRenderbufferStorageOES")]
            internal extern static void EGLImageTargetRenderbufferStorageOES(OpenMobile.Graphics.ES11.All target, IntPtr image);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEGLImageTargetTexture2DOES")]
            internal extern static void EGLImageTargetTexture2DOES(OpenMobile.Graphics.ES11.All target, IntPtr image);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnable")]
            internal extern static void Enable(OpenMobile.Graphics.ES11.All cap);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnableClientState")]
            internal extern static void EnableClientState(OpenMobile.Graphics.ES11.All array);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnableDriverControlQCOM")]
            internal extern static void EnableDriverControlQCOM(UInt32 driverControl);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFinish")]
            internal extern static void Finish();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFinishFenceNV")]
            internal extern static void FinishFenceNV(UInt32 fence);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFlush")]
            internal extern static void Flush();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogf")]
            internal extern static void Fogf(OpenMobile.Graphics.ES11.All pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogfv")]
            internal extern static unsafe void Fogfv(OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogx")]
            internal extern static void Fogx(OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogxOES")]
            internal extern static void FogxOES(OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogxv")]
            internal extern static unsafe void Fogxv(OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogxvOES")]
            internal extern static unsafe void FogxvOES(OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferRenderbufferOES")]
            internal extern static void FramebufferRenderbufferOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All attachment, OpenMobile.Graphics.ES11.All renderbuffertarget, UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTexture2DOES")]
            internal extern static void FramebufferTexture2DOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All attachment, OpenMobile.Graphics.ES11.All textarget, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFrontFace")]
            internal extern static void FrontFace(OpenMobile.Graphics.ES11.All mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFrustumf")]
            internal extern static void Frustumf(Single left, Single right, Single bottom, Single top, Single zNear, Single zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFrustumfOES")]
            internal extern static void FrustumfOES(Single left, Single right, Single bottom, Single top, Single zNear, Single zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFrustumx")]
            internal extern static void Frustumx(int left, int right, int bottom, int top, int zNear, int zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFrustumxOES")]
            internal extern static void FrustumxOES(int left, int right, int bottom, int top, int zNear, int zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenBuffers")]
            internal extern static unsafe void GenBuffers(Int32 n, UInt32* buffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenerateMipmapOES")]
            internal extern static void GenerateMipmapOES(OpenMobile.Graphics.ES11.All target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenFencesNV")]
            internal extern static unsafe void GenFencesNV(Int32 n, UInt32* fences);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenFramebuffersOES")]
            internal extern static unsafe void GenFramebuffersOES(Int32 n, UInt32* framebuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenRenderbuffersOES")]
            internal extern static unsafe void GenRenderbuffersOES(Int32 n, UInt32* renderbuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenTextures")]
            internal extern static unsafe void GenTextures(Int32 n, UInt32* textures);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBooleanv")]
            internal extern static unsafe void GetBooleanv(OpenMobile.Graphics.ES11.All pname, bool* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBufferParameteriv")]
            internal extern static unsafe void GetBufferParameteriv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBufferPointervOES")]
            internal extern static void GetBufferPointervOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, IntPtr @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetClipPlanef")]
            internal extern static unsafe void GetClipPlanef(OpenMobile.Graphics.ES11.All pname, Single* eqn);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetClipPlanefOES")]
            internal extern static unsafe void GetClipPlanefOES(OpenMobile.Graphics.ES11.All pname, Single* eqn);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetClipPlanex")]
            internal extern static unsafe void GetClipPlanex(OpenMobile.Graphics.ES11.All pname, int* eqn);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetClipPlanexOES")]
            internal extern static unsafe void GetClipPlanexOES(OpenMobile.Graphics.ES11.All pname, int* eqn);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetDriverControlsQCOM")]
            internal extern static unsafe void GetDriverControlsQCOM(Int32* num, Int32 size, UInt32* driverControls);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetDriverControlStringQCOM")]
            internal extern static unsafe void GetDriverControlStringQCOM(UInt32 driverControl, Int32 bufSize, Int32* length, String driverControlString);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetError")]
            internal extern static OpenMobile.Graphics.ES11.All GetError();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFenceivNV")]
            internal extern static unsafe void GetFenceivNV(UInt32 fence, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFixedv")]
            internal extern static unsafe void GetFixedv(OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFixedvOES")]
            internal extern static unsafe void GetFixedvOES(OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFloatv")]
            internal extern static unsafe void GetFloatv(OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFramebufferAttachmentParameterivOES")]
            internal extern static unsafe void GetFramebufferAttachmentParameterivOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All attachment, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetIntegerv")]
            internal extern static unsafe void GetIntegerv(OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetLightfv")]
            internal extern static unsafe void GetLightfv(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetLightxv")]
            internal extern static unsafe void GetLightxv(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetLightxvOES")]
            internal extern static unsafe void GetLightxvOES(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMaterialfv")]
            internal extern static unsafe void GetMaterialfv(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMaterialxv")]
            internal extern static unsafe void GetMaterialxv(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMaterialxvOES")]
            internal extern static unsafe void GetMaterialxvOES(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPointerv")]
            internal extern static void GetPointerv(OpenMobile.Graphics.ES11.All pname, IntPtr @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetRenderbufferParameterivOES")]
            internal extern static unsafe void GetRenderbufferParameterivOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetString")]
            internal extern static unsafe System.IntPtr GetString(OpenMobile.Graphics.ES11.All name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexEnvfv")]
            internal extern static unsafe void GetTexEnvfv(OpenMobile.Graphics.ES11.All env, OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexEnviv")]
            internal extern static unsafe void GetTexEnviv(OpenMobile.Graphics.ES11.All env, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexEnvxv")]
            internal extern static unsafe void GetTexEnvxv(OpenMobile.Graphics.ES11.All env, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexEnvxvOES")]
            internal extern static unsafe void GetTexEnvxvOES(OpenMobile.Graphics.ES11.All env, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexGenfvOES")]
            internal extern static unsafe void GetTexGenfvOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexGenivOES")]
            internal extern static unsafe void GetTexGenivOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexGenxvOES")]
            internal extern static unsafe void GetTexGenxvOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexParameterfv")]
            internal extern static unsafe void GetTexParameterfv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexParameteriv")]
            internal extern static unsafe void GetTexParameteriv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexParameterxv")]
            internal extern static unsafe void GetTexParameterxv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexParameterxvOES")]
            internal extern static unsafe void GetTexParameterxvOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glHint")]
            internal extern static void Hint(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsBuffer")]
            internal extern static bool IsBuffer(UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsEnabled")]
            internal extern static bool IsEnabled(OpenMobile.Graphics.ES11.All cap);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsFenceNV")]
            internal extern static bool IsFenceNV(UInt32 fence);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsFramebufferOES")]
            internal extern static bool IsFramebufferOES(UInt32 framebuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsRenderbufferOES")]
            internal extern static bool IsRenderbufferOES(UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsTexture")]
            internal extern static bool IsTexture(UInt32 texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightf")]
            internal extern static void Lightf(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightfv")]
            internal extern static unsafe void Lightfv(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightModelf")]
            internal extern static void LightModelf(OpenMobile.Graphics.ES11.All pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightModelfv")]
            internal extern static unsafe void LightModelfv(OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightModelx")]
            internal extern static void LightModelx(OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightModelxOES")]
            internal extern static void LightModelxOES(OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightModelxv")]
            internal extern static unsafe void LightModelxv(OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightModelxvOES")]
            internal extern static unsafe void LightModelxvOES(OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightx")]
            internal extern static void Lightx(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightxOES")]
            internal extern static void LightxOES(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightxv")]
            internal extern static unsafe void Lightxv(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightxvOES")]
            internal extern static unsafe void LightxvOES(OpenMobile.Graphics.ES11.All light, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLineWidth")]
            internal extern static void LineWidth(Single width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLineWidthx")]
            internal extern static void LineWidthx(int width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLineWidthxOES")]
            internal extern static void LineWidthxOES(int width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadIdentity")]
            internal extern static void LoadIdentity();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadMatrixf")]
            internal extern static unsafe void LoadMatrixf(Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadMatrixx")]
            internal extern static unsafe void LoadMatrixx(int* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadMatrixxOES")]
            internal extern static unsafe void LoadMatrixxOES(int* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadPaletteFromModelViewMatrixOES")]
            internal extern static void LoadPaletteFromModelViewMatrixOES();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLogicOp")]
            internal extern static void LogicOp(OpenMobile.Graphics.ES11.All opcode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapBufferOES")]
            internal extern static unsafe System.IntPtr MapBufferOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All access);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMaterialf")]
            internal extern static void Materialf(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMaterialfv")]
            internal extern static unsafe void Materialfv(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMaterialx")]
            internal extern static void Materialx(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMaterialxOES")]
            internal extern static void MaterialxOES(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMaterialxv")]
            internal extern static unsafe void Materialxv(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMaterialxvOES")]
            internal extern static unsafe void MaterialxvOES(OpenMobile.Graphics.ES11.All face, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixIndexPointerOES")]
            internal extern static void MatrixIndexPointerOES(Int32 size, OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixMode")]
            internal extern static void MatrixMode(OpenMobile.Graphics.ES11.All mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4f")]
            internal extern static void MultiTexCoord4f(OpenMobile.Graphics.ES11.All target, Single s, Single t, Single r, Single q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4x")]
            internal extern static void MultiTexCoord4x(OpenMobile.Graphics.ES11.All target, int s, int t, int r, int q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4xOES")]
            internal extern static void MultiTexCoord4xOES(OpenMobile.Graphics.ES11.All target, int s, int t, int r, int q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultMatrixf")]
            internal extern static unsafe void MultMatrixf(Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultMatrixx")]
            internal extern static unsafe void MultMatrixx(int* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultMatrixxOES")]
            internal extern static unsafe void MultMatrixxOES(int* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3f")]
            internal extern static void Normal3f(Single nx, Single ny, Single nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3x")]
            internal extern static void Normal3x(int nx, int ny, int nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3xOES")]
            internal extern static void Normal3xOES(int nx, int ny, int nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalPointer")]
            internal extern static void NormalPointer(OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glOrthof")]
            internal extern static void Orthof(Single left, Single right, Single bottom, Single top, Single zNear, Single zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glOrthofOES")]
            internal extern static void OrthofOES(Single left, Single right, Single bottom, Single top, Single zNear, Single zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glOrthox")]
            internal extern static void Orthox(int left, int right, int bottom, int top, int zNear, int zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glOrthoxOES")]
            internal extern static void OrthoxOES(int left, int right, int bottom, int top, int zNear, int zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelStorei")]
            internal extern static void PixelStorei(OpenMobile.Graphics.ES11.All pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterf")]
            internal extern static void PointParameterf(OpenMobile.Graphics.ES11.All pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterfv")]
            internal extern static unsafe void PointParameterfv(OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterx")]
            internal extern static void PointParameterx(OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterxOES")]
            internal extern static void PointParameterxOES(OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterxv")]
            internal extern static unsafe void PointParameterxv(OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterxvOES")]
            internal extern static unsafe void PointParameterxvOES(OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointSize")]
            internal extern static void PointSize(Single size);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointSizePointerOES")]
            internal extern static void PointSizePointerOES(OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointSizex")]
            internal extern static void PointSizex(int size);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointSizexOES")]
            internal extern static void PointSizexOES(int size);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPolygonOffset")]
            internal extern static void PolygonOffset(Single factor, Single units);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPolygonOffsetx")]
            internal extern static void PolygonOffsetx(int factor, int units);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPolygonOffsetxOES")]
            internal extern static void PolygonOffsetxOES(int factor, int units);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPopMatrix")]
            internal extern static void PopMatrix();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPushMatrix")]
            internal extern static void PushMatrix();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glQueryMatrixxOES")]
            internal extern static unsafe Int32 QueryMatrixxOES(int* mantissa, Int32* exponent);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glReadPixels")]
            internal extern static void ReadPixels(Int32 x, Int32 y, Int32 width, Int32 height, OpenMobile.Graphics.ES11.All format, OpenMobile.Graphics.ES11.All type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRenderbufferStorageOES")]
            internal extern static void RenderbufferStorageOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All internalformat, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRotatef")]
            internal extern static void Rotatef(Single angle, Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRotatex")]
            internal extern static void Rotatex(int angle, int x, int y, int z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRotatexOES")]
            internal extern static void RotatexOES(int angle, int x, int y, int z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSampleCoverage")]
            internal extern static void SampleCoverage(Single value, bool invert);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSampleCoveragex")]
            internal extern static void SampleCoveragex(int value, bool invert);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSampleCoveragexOES")]
            internal extern static void SampleCoveragexOES(int value, bool invert);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glScalef")]
            internal extern static void Scalef(Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glScalex")]
            internal extern static void Scalex(int x, int y, int z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glScalexOES")]
            internal extern static void ScalexOES(int x, int y, int z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glScissor")]
            internal extern static void Scissor(Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSetFenceNV")]
            internal extern static void SetFenceNV(UInt32 fence, OpenMobile.Graphics.ES11.All condition);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glShadeModel")]
            internal extern static void ShadeModel(OpenMobile.Graphics.ES11.All mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilFunc")]
            internal extern static void StencilFunc(OpenMobile.Graphics.ES11.All func, Int32 @ref, UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilMask")]
            internal extern static void StencilMask(UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilOp")]
            internal extern static void StencilOp(OpenMobile.Graphics.ES11.All fail, OpenMobile.Graphics.ES11.All zfail, OpenMobile.Graphics.ES11.All zpass);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTestFenceNV")]
            internal extern static bool TestFenceNV(UInt32 fence);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoordPointer")]
            internal extern static void TexCoordPointer(Int32 size, OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnvf")]
            internal extern static void TexEnvf(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnvfv")]
            internal extern static unsafe void TexEnvfv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnvi")]
            internal extern static void TexEnvi(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnviv")]
            internal extern static unsafe void TexEnviv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnvx")]
            internal extern static void TexEnvx(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnvxOES")]
            internal extern static void TexEnvxOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnvxv")]
            internal extern static unsafe void TexEnvxv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnvxvOES")]
            internal extern static unsafe void TexEnvxvOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGenfOES")]
            internal extern static void TexGenfOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGenfvOES")]
            internal extern static unsafe void TexGenfvOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGeniOES")]
            internal extern static void TexGeniOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGenivOES")]
            internal extern static unsafe void TexGenivOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGenxOES")]
            internal extern static void TexGenxOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGenxvOES")]
            internal extern static unsafe void TexGenxvOES(OpenMobile.Graphics.ES11.All coord, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexImage2D")]
            internal extern static void TexImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, Int32 internalformat, Int32 width, Int32 height, Int32 border, OpenMobile.Graphics.ES11.All format, OpenMobile.Graphics.ES11.All type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterf")]
            internal extern static void TexParameterf(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterfv")]
            internal extern static unsafe void TexParameterfv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameteri")]
            internal extern static void TexParameteri(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameteriv")]
            internal extern static unsafe void TexParameteriv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterx")]
            internal extern static void TexParameterx(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterxOES")]
            internal extern static void TexParameterxOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterxv")]
            internal extern static unsafe void TexParameterxv(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterxvOES")]
            internal extern static unsafe void TexParameterxvOES(OpenMobile.Graphics.ES11.All target, OpenMobile.Graphics.ES11.All pname, int* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexSubImage2D")]
            internal extern static void TexSubImage2D(OpenMobile.Graphics.ES11.All target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.ES11.All format, OpenMobile.Graphics.ES11.All type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTranslatef")]
            internal extern static void Translatef(Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTranslatex")]
            internal extern static void Translatex(int x, int y, int z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTranslatexOES")]
            internal extern static void TranslatexOES(int x, int y, int z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUnmapBufferOES")]
            internal extern static bool UnmapBufferOES(OpenMobile.Graphics.ES11.All target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexPointer")]
            internal extern static void VertexPointer(Int32 size, OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glViewport")]
            internal extern static void Viewport(Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWeightPointerOES")]
            internal extern static void WeightPointerOES(Int32 size, OpenMobile.Graphics.ES11.All type, Int32 stride, IntPtr pointer);
        }
    }
}
