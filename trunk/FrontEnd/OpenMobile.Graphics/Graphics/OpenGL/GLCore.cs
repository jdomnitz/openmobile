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

namespace OpenMobile.Graphics.OpenGL
{
    using System;
    using System.Text;
    using System.Runtime.InteropServices;
    #pragma warning disable 3019
    #pragma warning disable 1591

    public partial class Raw
    {

        internal static partial class Core
        {

            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAccum", ExactSpelling = true)]
            internal extern static void Accum(OpenMobile.Graphics.OpenGL.AccumOp op, Single value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glActiveStencilFaceEXT", ExactSpelling = true)]
            internal extern static void ActiveStencilFaceEXT(OpenMobile.Graphics.OpenGL.ExtStencilTwoSide face);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glActiveTexture", ExactSpelling = true)]
            internal extern static void ActiveTexture(OpenMobile.Graphics.OpenGL.TextureUnit texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glActiveTextureARB", ExactSpelling = true)]
            internal extern static void ActiveTextureARB(OpenMobile.Graphics.OpenGL.TextureUnit texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glActiveVaryingNV", ExactSpelling = true)]
            internal extern static void ActiveVaryingNV(UInt32 program, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAlphaFragmentOp1ATI", ExactSpelling = true)]
            internal extern static void AlphaFragmentOp1ATI(OpenMobile.Graphics.OpenGL.AtiFragmentShader op, UInt32 dst, UInt32 dstMod, UInt32 arg1, UInt32 arg1Rep, UInt32 arg1Mod);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAlphaFragmentOp2ATI", ExactSpelling = true)]
            internal extern static void AlphaFragmentOp2ATI(OpenMobile.Graphics.OpenGL.AtiFragmentShader op, UInt32 dst, UInt32 dstMod, UInt32 arg1, UInt32 arg1Rep, UInt32 arg1Mod, UInt32 arg2, UInt32 arg2Rep, UInt32 arg2Mod);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAlphaFragmentOp3ATI", ExactSpelling = true)]
            internal extern static void AlphaFragmentOp3ATI(OpenMobile.Graphics.OpenGL.AtiFragmentShader op, UInt32 dst, UInt32 dstMod, UInt32 arg1, UInt32 arg1Rep, UInt32 arg1Mod, UInt32 arg2, UInt32 arg2Rep, UInt32 arg2Mod, UInt32 arg3, UInt32 arg3Rep, UInt32 arg3Mod);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAlphaFunc", ExactSpelling = true)]
            internal extern static void AlphaFunc(OpenMobile.Graphics.OpenGL.AlphaFunction func, Single @ref);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glApplyTextureEXT", ExactSpelling = true)]
            internal extern static void ApplyTextureEXT(OpenMobile.Graphics.OpenGL.ExtLightTexture mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAreProgramsResidentNV", ExactSpelling = true)]
            internal extern static unsafe bool AreProgramsResidentNV(Int32 n, UInt32* programs, [OutAttribute] bool* residences);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAreTexturesResident", ExactSpelling = true)]
            internal extern static unsafe bool AreTexturesResident(Int32 n, UInt32* textures, [OutAttribute] bool* residences);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAreTexturesResidentEXT", ExactSpelling = true)]
            internal extern static unsafe bool AreTexturesResidentEXT(Int32 n, UInt32* textures, [OutAttribute] bool* residences);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glArrayElement", ExactSpelling = true)]
            internal extern static void ArrayElement(Int32 i);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glArrayElementEXT", ExactSpelling = true)]
            internal extern static void ArrayElementEXT(Int32 i);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glArrayObjectATI", ExactSpelling = true)]
            internal extern static void ArrayObjectATI(OpenMobile.Graphics.OpenGL.EnableCap array, Int32 size, OpenMobile.Graphics.OpenGL.AtiVertexArrayObject type, Int32 stride, UInt32 buffer, UInt32 offset);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAttachObjectARB", ExactSpelling = true)]
            internal extern static void AttachObjectARB(UInt32 containerObj, UInt32 obj);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glAttachShader", ExactSpelling = true)]
            internal extern static void AttachShader(UInt32 program, UInt32 shader);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBegin", ExactSpelling = true)]
            internal extern static void Begin(OpenMobile.Graphics.OpenGL.BeginMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginConditionalRender", ExactSpelling = true)]
            internal extern static void BeginConditionalRender(UInt32 id, OpenMobile.Graphics.OpenGL.ConditionalRenderType mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginConditionalRenderNV", ExactSpelling = true)]
            internal extern static void BeginConditionalRenderNV(UInt32 id, OpenMobile.Graphics.OpenGL.NvConditionalRender mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginFragmentShaderATI", ExactSpelling = true)]
            internal extern static void BeginFragmentShaderATI();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginOcclusionQueryNV", ExactSpelling = true)]
            internal extern static void BeginOcclusionQueryNV(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginPerfMonitorAMD", ExactSpelling = true)]
            internal extern static void BeginPerfMonitorAMD(UInt32 monitor);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginQuery", ExactSpelling = true)]
            internal extern static void BeginQuery(OpenMobile.Graphics.OpenGL.QueryTarget target, UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginQueryARB", ExactSpelling = true)]
            internal extern static void BeginQueryARB(OpenMobile.Graphics.OpenGL.ArbOcclusionQuery target, UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginTransformFeedback", ExactSpelling = true)]
            internal extern static void BeginTransformFeedback(OpenMobile.Graphics.OpenGL.BeginFeedbackMode primitiveMode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginTransformFeedbackEXT", ExactSpelling = true)]
            internal extern static void BeginTransformFeedbackEXT(OpenMobile.Graphics.OpenGL.ExtTransformFeedback primitiveMode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginTransformFeedbackNV", ExactSpelling = true)]
            internal extern static void BeginTransformFeedbackNV(OpenMobile.Graphics.OpenGL.NvTransformFeedback primitiveMode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBeginVertexShaderEXT", ExactSpelling = true)]
            internal extern static void BeginVertexShaderEXT();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindAttribLocation", ExactSpelling = true)]
            internal extern static void BindAttribLocation(UInt32 program, UInt32 index, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindAttribLocationARB", ExactSpelling = true)]
            internal extern static void BindAttribLocationARB(UInt32 programObj, UInt32 index, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBuffer", ExactSpelling = true)]
            internal extern static void BindBuffer(OpenMobile.Graphics.OpenGL.BufferTarget target, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBufferARB", ExactSpelling = true)]
            internal extern static void BindBufferARB(OpenMobile.Graphics.OpenGL.BufferTargetArb target, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBufferBase", ExactSpelling = true)]
            internal extern static void BindBufferBase(OpenMobile.Graphics.OpenGL.BufferTarget target, UInt32 index, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBufferBaseEXT", ExactSpelling = true)]
            internal extern static void BindBufferBaseEXT(OpenMobile.Graphics.OpenGL.ExtTransformFeedback target, UInt32 index, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBufferBaseNV", ExactSpelling = true)]
            internal extern static void BindBufferBaseNV(OpenMobile.Graphics.OpenGL.NvTransformFeedback target, UInt32 index, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBufferOffsetEXT", ExactSpelling = true)]
            internal extern static void BindBufferOffsetEXT(OpenMobile.Graphics.OpenGL.ExtTransformFeedback target, UInt32 index, UInt32 buffer, IntPtr offset);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBufferOffsetNV", ExactSpelling = true)]
            internal extern static void BindBufferOffsetNV(OpenMobile.Graphics.OpenGL.NvTransformFeedback target, UInt32 index, UInt32 buffer, IntPtr offset);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBufferRange", ExactSpelling = true)]
            internal extern static void BindBufferRange(OpenMobile.Graphics.OpenGL.BufferTarget target, UInt32 index, UInt32 buffer, IntPtr offset, IntPtr size);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBufferRangeEXT", ExactSpelling = true)]
            internal extern static void BindBufferRangeEXT(OpenMobile.Graphics.OpenGL.ExtTransformFeedback target, UInt32 index, UInt32 buffer, IntPtr offset, IntPtr size);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindBufferRangeNV", ExactSpelling = true)]
            internal extern static void BindBufferRangeNV(OpenMobile.Graphics.OpenGL.NvTransformFeedback target, UInt32 index, UInt32 buffer, IntPtr offset, IntPtr size);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindFragDataLocation", ExactSpelling = true)]
            internal extern static void BindFragDataLocation(UInt32 program, UInt32 color, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindFragDataLocationEXT", ExactSpelling = true)]
            internal extern static void BindFragDataLocationEXT(UInt32 program, UInt32 color, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindFragmentShaderATI", ExactSpelling = true)]
            internal extern static void BindFragmentShaderATI(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindFramebuffer", ExactSpelling = true)]
            internal extern static void BindFramebuffer(OpenMobile.Graphics.OpenGL.FramebufferTarget target, UInt32 framebuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindFramebufferEXT", ExactSpelling = true)]
            internal extern static void BindFramebufferEXT(OpenMobile.Graphics.OpenGL.FramebufferTarget target, UInt32 framebuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindLightParameterEXT", ExactSpelling = true)]
            internal extern static Int32 BindLightParameterEXT(OpenMobile.Graphics.OpenGL.LightName light, OpenMobile.Graphics.OpenGL.LightParameter value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindMaterialParameterEXT", ExactSpelling = true)]
            internal extern static Int32 BindMaterialParameterEXT(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.MaterialParameter value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindMultiTextureEXT", ExactSpelling = true)]
            internal extern static void BindMultiTextureEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, UInt32 texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindParameterEXT", ExactSpelling = true)]
            internal extern static Int32 BindParameterEXT(OpenMobile.Graphics.OpenGL.ExtVertexShader value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindProgramARB", ExactSpelling = true)]
            internal extern static void BindProgramARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 program);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindProgramNV", ExactSpelling = true)]
            internal extern static void BindProgramNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindRenderbuffer", ExactSpelling = true)]
            internal extern static void BindRenderbuffer(OpenMobile.Graphics.OpenGL.RenderbufferTarget target, UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindRenderbufferEXT", ExactSpelling = true)]
            internal extern static void BindRenderbufferEXT(OpenMobile.Graphics.OpenGL.RenderbufferTarget target, UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindTexGenParameterEXT", ExactSpelling = true)]
            internal extern static Int32 BindTexGenParameterEXT(OpenMobile.Graphics.OpenGL.TextureUnit unit, OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindTexture", ExactSpelling = true)]
            internal extern static void BindTexture(OpenMobile.Graphics.OpenGL.TextureTarget target, UInt32 texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindTextureEXT", ExactSpelling = true)]
            internal extern static void BindTextureEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, UInt32 texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindTextureUnitParameterEXT", ExactSpelling = true)]
            internal extern static Int32 BindTextureUnitParameterEXT(OpenMobile.Graphics.OpenGL.TextureUnit unit, OpenMobile.Graphics.OpenGL.ExtVertexShader value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindTransformFeedbackNV", ExactSpelling = true)]
            internal extern static void BindTransformFeedbackNV(OpenMobile.Graphics.OpenGL.NvTransformFeedback2 target, UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindVertexArray", ExactSpelling = true)]
            internal extern static void BindVertexArray(UInt32 array);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBindVertexShaderEXT", ExactSpelling = true)]
            internal extern static void BindVertexShaderEXT(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormal3bEXT", ExactSpelling = true)]
            internal extern static void Binormal3bEXT(SByte bx, SByte by, SByte bz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormal3bvEXT", ExactSpelling = true)]
            internal extern static unsafe void Binormal3bvEXT(SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormal3dEXT", ExactSpelling = true)]
            internal extern static void Binormal3dEXT(Double bx, Double by, Double bz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormal3dvEXT", ExactSpelling = true)]
            internal extern static unsafe void Binormal3dvEXT(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormal3fEXT", ExactSpelling = true)]
            internal extern static void Binormal3fEXT(Single bx, Single by, Single bz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormal3fvEXT", ExactSpelling = true)]
            internal extern static unsafe void Binormal3fvEXT(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormal3iEXT", ExactSpelling = true)]
            internal extern static void Binormal3iEXT(Int32 bx, Int32 by, Int32 bz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormal3ivEXT", ExactSpelling = true)]
            internal extern static unsafe void Binormal3ivEXT(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormal3sEXT", ExactSpelling = true)]
            internal extern static void Binormal3sEXT(Int16 bx, Int16 by, Int16 bz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormal3svEXT", ExactSpelling = true)]
            internal extern static unsafe void Binormal3svEXT(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBinormalPointerEXT", ExactSpelling = true)]
            internal extern static void BinormalPointerEXT(OpenMobile.Graphics.OpenGL.NormalPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBitmap", ExactSpelling = true)]
            internal extern static unsafe void Bitmap(Int32 width, Int32 height, Single xorig, Single yorig, Single xmove, Single ymove, Byte* bitmap);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendColor", ExactSpelling = true)]
            internal extern static void BlendColor(Single red, Single green, Single blue, Single alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendColorEXT", ExactSpelling = true)]
            internal extern static void BlendColorEXT(Single red, Single green, Single blue, Single alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendEquation", ExactSpelling = true)]
            internal extern static void BlendEquation(OpenMobile.Graphics.OpenGL.BlendEquationMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendEquationEXT", ExactSpelling = true)]
            internal extern static void BlendEquationEXT(OpenMobile.Graphics.OpenGL.ExtBlendMinmax mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendEquationi", ExactSpelling = true)]
            internal extern static void BlendEquationi(UInt32 buf, OpenMobile.Graphics.OpenGL.ArbDrawBuffersBlend mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendEquationIndexedAMD", ExactSpelling = true)]
            internal extern static void BlendEquationIndexedAMD(UInt32 buf, OpenMobile.Graphics.OpenGL.AmdDrawBuffersBlend mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendEquationSeparate", ExactSpelling = true)]
            internal extern static void BlendEquationSeparate(OpenMobile.Graphics.OpenGL.BlendEquationMode modeRGB, OpenMobile.Graphics.OpenGL.BlendEquationMode modeAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendEquationSeparateEXT", ExactSpelling = true)]
            internal extern static void BlendEquationSeparateEXT(OpenMobile.Graphics.OpenGL.ExtBlendEquationSeparate modeRGB, OpenMobile.Graphics.OpenGL.ExtBlendEquationSeparate modeAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendEquationSeparatei", ExactSpelling = true)]
            internal extern static void BlendEquationSeparatei(UInt32 buf, OpenMobile.Graphics.OpenGL.BlendEquationMode modeRGB, OpenMobile.Graphics.OpenGL.BlendEquationMode modeAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendEquationSeparateIndexedAMD", ExactSpelling = true)]
            internal extern static void BlendEquationSeparateIndexedAMD(UInt32 buf, OpenMobile.Graphics.OpenGL.AmdDrawBuffersBlend modeRGB, OpenMobile.Graphics.OpenGL.AmdDrawBuffersBlend modeAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendFunc", ExactSpelling = true)]
            internal extern static void BlendFunc(OpenMobile.Graphics.OpenGL.BlendingFactorSrc sfactor, OpenMobile.Graphics.OpenGL.BlendingFactorDest dfactor);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendFunci", ExactSpelling = true)]
            internal extern static void BlendFunci(UInt32 buf, OpenMobile.Graphics.OpenGL.ArbDrawBuffersBlend src, OpenMobile.Graphics.OpenGL.ArbDrawBuffersBlend dst);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendFuncIndexedAMD", ExactSpelling = true)]
            internal extern static void BlendFuncIndexedAMD(UInt32 buf, OpenMobile.Graphics.OpenGL.AmdDrawBuffersBlend src, OpenMobile.Graphics.OpenGL.AmdDrawBuffersBlend dst);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendFuncSeparate", ExactSpelling = true)]
            internal extern static void BlendFuncSeparate(OpenMobile.Graphics.OpenGL.BlendingFactorSrc sfactorRGB, OpenMobile.Graphics.OpenGL.BlendingFactorDest dfactorRGB, OpenMobile.Graphics.OpenGL.BlendingFactorSrc sfactorAlpha, OpenMobile.Graphics.OpenGL.BlendingFactorDest dfactorAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendFuncSeparateEXT", ExactSpelling = true)]
            internal extern static void BlendFuncSeparateEXT(OpenMobile.Graphics.OpenGL.ExtBlendFuncSeparate sfactorRGB, OpenMobile.Graphics.OpenGL.ExtBlendFuncSeparate dfactorRGB, OpenMobile.Graphics.OpenGL.ExtBlendFuncSeparate sfactorAlpha, OpenMobile.Graphics.OpenGL.ExtBlendFuncSeparate dfactorAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendFuncSeparatei", ExactSpelling = true)]
            internal extern static void BlendFuncSeparatei(UInt32 buf, OpenMobile.Graphics.OpenGL.ArbDrawBuffersBlend srcRGB, OpenMobile.Graphics.OpenGL.ArbDrawBuffersBlend dstRGB, OpenMobile.Graphics.OpenGL.ArbDrawBuffersBlend srcAlpha, OpenMobile.Graphics.OpenGL.ArbDrawBuffersBlend dstAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendFuncSeparateIndexedAMD", ExactSpelling = true)]
            internal extern static void BlendFuncSeparateIndexedAMD(UInt32 buf, OpenMobile.Graphics.OpenGL.AmdDrawBuffersBlend srcRGB, OpenMobile.Graphics.OpenGL.AmdDrawBuffersBlend dstRGB, OpenMobile.Graphics.OpenGL.AmdDrawBuffersBlend srcAlpha, OpenMobile.Graphics.OpenGL.AmdDrawBuffersBlend dstAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlendFuncSeparateINGR", ExactSpelling = true)]
            internal extern static void BlendFuncSeparateINGR(OpenMobile.Graphics.OpenGL.All sfactorRGB, OpenMobile.Graphics.OpenGL.All dfactorRGB, OpenMobile.Graphics.OpenGL.All sfactorAlpha, OpenMobile.Graphics.OpenGL.All dfactorAlpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlitFramebuffer", ExactSpelling = true)]
            internal extern static void BlitFramebuffer(Int32 srcX0, Int32 srcY0, Int32 srcX1, Int32 srcY1, Int32 dstX0, Int32 dstY0, Int32 dstX1, Int32 dstY1, OpenMobile.Graphics.OpenGL.ClearBufferMask mask, OpenMobile.Graphics.OpenGL.BlitFramebufferFilter filter);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBlitFramebufferEXT", ExactSpelling = true)]
            internal extern static void BlitFramebufferEXT(Int32 srcX0, Int32 srcY0, Int32 srcX1, Int32 srcY1, Int32 dstX0, Int32 dstY0, Int32 dstX1, Int32 dstY1, OpenMobile.Graphics.OpenGL.ClearBufferMask mask, OpenMobile.Graphics.OpenGL.ExtFramebufferBlit filter);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBufferData", ExactSpelling = true)]
            internal extern static void BufferData(OpenMobile.Graphics.OpenGL.BufferTarget target, IntPtr size, IntPtr data, OpenMobile.Graphics.OpenGL.BufferUsageHint usage);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBufferDataARB", ExactSpelling = true)]
            internal extern static void BufferDataARB(OpenMobile.Graphics.OpenGL.BufferTargetArb target, IntPtr size, IntPtr data, OpenMobile.Graphics.OpenGL.BufferUsageArb usage);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBufferSubData", ExactSpelling = true)]
            internal extern static void BufferSubData(OpenMobile.Graphics.OpenGL.BufferTarget target, IntPtr offset, IntPtr size, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glBufferSubDataARB", ExactSpelling = true)]
            internal extern static void BufferSubDataARB(OpenMobile.Graphics.OpenGL.BufferTargetArb target, IntPtr offset, IntPtr size, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCallList", ExactSpelling = true)]
            internal extern static void CallList(UInt32 list);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCallLists", ExactSpelling = true)]
            internal extern static void CallLists(Int32 n, OpenMobile.Graphics.OpenGL.ListNameType type, IntPtr lists);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCheckFramebufferStatus", ExactSpelling = true)]
            internal extern static OpenMobile.Graphics.OpenGL.FramebufferErrorCode CheckFramebufferStatus(OpenMobile.Graphics.OpenGL.FramebufferTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCheckFramebufferStatusEXT", ExactSpelling = true)]
            internal extern static OpenMobile.Graphics.OpenGL.FramebufferErrorCode CheckFramebufferStatusEXT(OpenMobile.Graphics.OpenGL.FramebufferTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCheckNamedFramebufferStatusEXT", ExactSpelling = true)]
            internal extern static OpenMobile.Graphics.OpenGL.ExtDirectStateAccess CheckNamedFramebufferStatusEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.FramebufferTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClampColor", ExactSpelling = true)]
            internal extern static void ClampColor(OpenMobile.Graphics.OpenGL.ClampColorTarget target, OpenMobile.Graphics.OpenGL.ClampColorMode clamp);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClampColorARB", ExactSpelling = true)]
            internal extern static void ClampColorARB(OpenMobile.Graphics.OpenGL.ArbColorBufferFloat target, OpenMobile.Graphics.OpenGL.ArbColorBufferFloat clamp);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClear", ExactSpelling = true)]
            internal extern static void Clear(OpenMobile.Graphics.OpenGL.ClearBufferMask mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearAccum", ExactSpelling = true)]
            internal extern static void ClearAccum(Single red, Single green, Single blue, Single alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearBufferfi", ExactSpelling = true)]
            internal extern static void ClearBufferfi(OpenMobile.Graphics.OpenGL.ClearBuffer buffer, Int32 drawbuffer, Single depth, Int32 stencil);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearBufferfv", ExactSpelling = true)]
            internal extern static unsafe void ClearBufferfv(OpenMobile.Graphics.OpenGL.ClearBuffer buffer, Int32 drawbuffer, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearBufferiv", ExactSpelling = true)]
            internal extern static unsafe void ClearBufferiv(OpenMobile.Graphics.OpenGL.ClearBuffer buffer, Int32 drawbuffer, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearBufferuiv", ExactSpelling = true)]
            internal extern static unsafe void ClearBufferuiv(OpenMobile.Graphics.OpenGL.ClearBuffer buffer, Int32 drawbuffer, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearColor", ExactSpelling = true)]
            internal extern static void ClearColor(Single red, Single green, Single blue, Single alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearColorIiEXT", ExactSpelling = true)]
            internal extern static void ClearColorIiEXT(Int32 red, Int32 green, Int32 blue, Int32 alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearColorIuiEXT", ExactSpelling = true)]
            internal extern static void ClearColorIuiEXT(UInt32 red, UInt32 green, UInt32 blue, UInt32 alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearDepth", ExactSpelling = true)]
            internal extern static void ClearDepth(Double depth);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearDepthdNV", ExactSpelling = true)]
            internal extern static void ClearDepthdNV(Double depth);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearIndex", ExactSpelling = true)]
            internal extern static void ClearIndex(Single c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClearStencil", ExactSpelling = true)]
            internal extern static void ClearStencil(Int32 s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClientActiveTexture", ExactSpelling = true)]
            internal extern static void ClientActiveTexture(OpenMobile.Graphics.OpenGL.TextureUnit texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClientActiveTextureARB", ExactSpelling = true)]
            internal extern static void ClientActiveTextureARB(OpenMobile.Graphics.OpenGL.TextureUnit texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClientActiveVertexStreamATI", ExactSpelling = true)]
            internal extern static void ClientActiveVertexStreamATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClientAttribDefaultEXT", ExactSpelling = true)]
            internal extern static void ClientAttribDefaultEXT(OpenMobile.Graphics.OpenGL.ClientAttribMask mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClientWaitSync", ExactSpelling = true)]
            internal extern static OpenMobile.Graphics.OpenGL.ArbSync ClientWaitSync(IntPtr sync, UInt32 flags, UInt64 timeout);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glClipPlane", ExactSpelling = true)]
            internal extern static unsafe void ClipPlane(OpenMobile.Graphics.OpenGL.ClipPlaneName plane, Double* equation);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3b", ExactSpelling = true)]
            internal extern static void Color3b(SByte red, SByte green, SByte blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3bv", ExactSpelling = true)]
            internal extern static unsafe void Color3bv(SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3d", ExactSpelling = true)]
            internal extern static void Color3d(Double red, Double green, Double blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3dv", ExactSpelling = true)]
            internal extern static unsafe void Color3dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3f", ExactSpelling = true)]
            internal extern static void Color3f(Single red, Single green, Single blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3fv", ExactSpelling = true)]
            internal extern static unsafe void Color3fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3hNV", ExactSpelling = true)]
            internal extern static void Color3hNV(OpenMobile.Half red, OpenMobile.Half green, OpenMobile.Half blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3hvNV", ExactSpelling = true)]
            internal extern static unsafe void Color3hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3i", ExactSpelling = true)]
            internal extern static void Color3i(Int32 red, Int32 green, Int32 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3iv", ExactSpelling = true)]
            internal extern static unsafe void Color3iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3s", ExactSpelling = true)]
            internal extern static void Color3s(Int16 red, Int16 green, Int16 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3sv", ExactSpelling = true)]
            internal extern static unsafe void Color3sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3ub", ExactSpelling = true)]
            internal extern static void Color3ub(Byte red, Byte green, Byte blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3ubv", ExactSpelling = true)]
            internal extern static unsafe void Color3ubv(Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3ui", ExactSpelling = true)]
            internal extern static void Color3ui(UInt32 red, UInt32 green, UInt32 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3uiv", ExactSpelling = true)]
            internal extern static unsafe void Color3uiv(UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3us", ExactSpelling = true)]
            internal extern static void Color3us(UInt16 red, UInt16 green, UInt16 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor3usv", ExactSpelling = true)]
            internal extern static unsafe void Color3usv(UInt16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4b", ExactSpelling = true)]
            internal extern static void Color4b(SByte red, SByte green, SByte blue, SByte alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4bv", ExactSpelling = true)]
            internal extern static unsafe void Color4bv(SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4d", ExactSpelling = true)]
            internal extern static void Color4d(Double red, Double green, Double blue, Double alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4dv", ExactSpelling = true)]
            internal extern static unsafe void Color4dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4f", ExactSpelling = true)]
            internal extern static void Color4f(Single red, Single green, Single blue, Single alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4fv", ExactSpelling = true)]
            internal extern static unsafe void Color4fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4hNV", ExactSpelling = true)]
            internal extern static void Color4hNV(OpenMobile.Half red, OpenMobile.Half green, OpenMobile.Half blue, OpenMobile.Half alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4hvNV", ExactSpelling = true)]
            internal extern static unsafe void Color4hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4i", ExactSpelling = true)]
            internal extern static void Color4i(Int32 red, Int32 green, Int32 blue, Int32 alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4iv", ExactSpelling = true)]
            internal extern static unsafe void Color4iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4s", ExactSpelling = true)]
            internal extern static void Color4s(Int16 red, Int16 green, Int16 blue, Int16 alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4sv", ExactSpelling = true)]
            internal extern static unsafe void Color4sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4ub", ExactSpelling = true)]
            internal extern static void Color4ub(Byte red, Byte green, Byte blue, Byte alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4ubv", ExactSpelling = true)]
            internal extern static unsafe void Color4ubv(Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4ui", ExactSpelling = true)]
            internal extern static void Color4ui(UInt32 red, UInt32 green, UInt32 blue, UInt32 alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4uiv", ExactSpelling = true)]
            internal extern static unsafe void Color4uiv(UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4us", ExactSpelling = true)]
            internal extern static void Color4us(UInt16 red, UInt16 green, UInt16 blue, UInt16 alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColor4usv", ExactSpelling = true)]
            internal extern static unsafe void Color4usv(UInt16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorFragmentOp1ATI", ExactSpelling = true)]
            internal extern static void ColorFragmentOp1ATI(OpenMobile.Graphics.OpenGL.AtiFragmentShader op, UInt32 dst, UInt32 dstMask, UInt32 dstMod, UInt32 arg1, UInt32 arg1Rep, UInt32 arg1Mod);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorFragmentOp2ATI", ExactSpelling = true)]
            internal extern static void ColorFragmentOp2ATI(OpenMobile.Graphics.OpenGL.AtiFragmentShader op, UInt32 dst, UInt32 dstMask, UInt32 dstMod, UInt32 arg1, UInt32 arg1Rep, UInt32 arg1Mod, UInt32 arg2, UInt32 arg2Rep, UInt32 arg2Mod);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorFragmentOp3ATI", ExactSpelling = true)]
            internal extern static void ColorFragmentOp3ATI(OpenMobile.Graphics.OpenGL.AtiFragmentShader op, UInt32 dst, UInt32 dstMask, UInt32 dstMod, UInt32 arg1, UInt32 arg1Rep, UInt32 arg1Mod, UInt32 arg2, UInt32 arg2Rep, UInt32 arg2Mod, UInt32 arg3, UInt32 arg3Rep, UInt32 arg3Mod);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorMask", ExactSpelling = true)]
            internal extern static void ColorMask(bool red, bool green, bool blue, bool alpha);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorMaski", ExactSpelling = true)]
            internal extern static void ColorMaski(UInt32 index, bool r, bool g, bool b, bool a);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorMaskIndexedEXT", ExactSpelling = true)]
            internal extern static void ColorMaskIndexedEXT(UInt32 index, bool r, bool g, bool b, bool a);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorMaterial", ExactSpelling = true)]
            internal extern static void ColorMaterial(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.ColorMaterialParameter mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorPointer", ExactSpelling = true)]
            internal extern static void ColorPointer(Int32 size, OpenMobile.Graphics.OpenGL.ColorPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorPointerEXT", ExactSpelling = true)]
            internal extern static void ColorPointerEXT(Int32 size, OpenMobile.Graphics.OpenGL.ColorPointerType type, Int32 stride, Int32 count, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorPointervINTEL", ExactSpelling = true)]
            internal extern static void ColorPointervINTEL(Int32 size, OpenMobile.Graphics.OpenGL.VertexPointerType type, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorSubTable", ExactSpelling = true)]
            internal extern static void ColorSubTable(OpenMobile.Graphics.OpenGL.ColorTableTarget target, Int32 start, Int32 count, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorSubTableEXT", ExactSpelling = true)]
            internal extern static void ColorSubTableEXT(OpenMobile.Graphics.OpenGL.ColorTableTarget target, Int32 start, Int32 count, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorTable", ExactSpelling = true)]
            internal extern static void ColorTable(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr table);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorTableEXT", ExactSpelling = true)]
            internal extern static void ColorTableEXT(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalFormat, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr table);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorTableParameterfv", ExactSpelling = true)]
            internal extern static unsafe void ColorTableParameterfv(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.ColorTableParameterPName pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glColorTableParameteriv", ExactSpelling = true)]
            internal extern static unsafe void ColorTableParameteriv(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.ColorTableParameterPName pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCombinerInputNV", ExactSpelling = true)]
            internal extern static void CombinerInputNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners stage, OpenMobile.Graphics.OpenGL.NvRegisterCombiners portion, OpenMobile.Graphics.OpenGL.NvRegisterCombiners variable, OpenMobile.Graphics.OpenGL.NvRegisterCombiners input, OpenMobile.Graphics.OpenGL.NvRegisterCombiners mapping, OpenMobile.Graphics.OpenGL.NvRegisterCombiners componentUsage);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCombinerOutputNV", ExactSpelling = true)]
            internal extern static void CombinerOutputNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners stage, OpenMobile.Graphics.OpenGL.NvRegisterCombiners portion, OpenMobile.Graphics.OpenGL.NvRegisterCombiners abOutput, OpenMobile.Graphics.OpenGL.NvRegisterCombiners cdOutput, OpenMobile.Graphics.OpenGL.NvRegisterCombiners sumOutput, OpenMobile.Graphics.OpenGL.NvRegisterCombiners scale, OpenMobile.Graphics.OpenGL.NvRegisterCombiners bias, bool abDotProduct, bool cdDotProduct, bool muxSum);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCombinerParameterfNV", ExactSpelling = true)]
            internal extern static void CombinerParameterfNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCombinerParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void CombinerParameterfvNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCombinerParameteriNV", ExactSpelling = true)]
            internal extern static void CombinerParameteriNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCombinerParameterivNV", ExactSpelling = true)]
            internal extern static unsafe void CombinerParameterivNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCombinerStageParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void CombinerStageParameterfvNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners2 stage, OpenMobile.Graphics.OpenGL.NvRegisterCombiners2 pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompileShader", ExactSpelling = true)]
            internal extern static void CompileShader(UInt32 shader);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompileShaderARB", ExactSpelling = true)]
            internal extern static void CompileShaderARB(UInt32 shaderObj);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedMultiTexImage1DEXT", ExactSpelling = true)]
            internal extern static void CompressedMultiTexImage1DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 border, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedMultiTexImage2DEXT", ExactSpelling = true)]
            internal extern static void CompressedMultiTexImage2DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 height, Int32 border, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedMultiTexImage3DEXT", ExactSpelling = true)]
            internal extern static void CompressedMultiTexImage3DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 height, Int32 depth, Int32 border, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedMultiTexSubImage1DEXT", ExactSpelling = true)]
            internal extern static void CompressedMultiTexSubImage1DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedMultiTexSubImage2DEXT", ExactSpelling = true)]
            internal extern static void CompressedMultiTexSubImage2DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedMultiTexSubImage3DEXT", ExactSpelling = true)]
            internal extern static void CompressedMultiTexSubImage3DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexImage1D", ExactSpelling = true)]
            internal extern static void CompressedTexImage1D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 border, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexImage1DARB", ExactSpelling = true)]
            internal extern static void CompressedTexImage1DARB(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 border, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexImage2D", ExactSpelling = true)]
            internal extern static void CompressedTexImage2D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, Int32 border, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexImage2DARB", ExactSpelling = true)]
            internal extern static void CompressedTexImage2DARB(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, Int32 border, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexImage3D", ExactSpelling = true)]
            internal extern static void CompressedTexImage3D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, Int32 depth, Int32 border, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexImage3DARB", ExactSpelling = true)]
            internal extern static void CompressedTexImage3DARB(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, Int32 depth, Int32 border, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexSubImage1D", ExactSpelling = true)]
            internal extern static void CompressedTexSubImage1D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexSubImage1DARB", ExactSpelling = true)]
            internal extern static void CompressedTexSubImage1DARB(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexSubImage2D", ExactSpelling = true)]
            internal extern static void CompressedTexSubImage2D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexSubImage2DARB", ExactSpelling = true)]
            internal extern static void CompressedTexSubImage2DARB(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexSubImage3D", ExactSpelling = true)]
            internal extern static void CompressedTexSubImage3D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTexSubImage3DARB", ExactSpelling = true)]
            internal extern static void CompressedTexSubImage3DARB(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTextureImage1DEXT", ExactSpelling = true)]
            internal extern static void CompressedTextureImage1DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 border, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTextureImage2DEXT", ExactSpelling = true)]
            internal extern static void CompressedTextureImage2DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 height, Int32 border, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTextureImage3DEXT", ExactSpelling = true)]
            internal extern static void CompressedTextureImage3DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 height, Int32 depth, Int32 border, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTextureSubImage1DEXT", ExactSpelling = true)]
            internal extern static void CompressedTextureSubImage1DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTextureSubImage2DEXT", ExactSpelling = true)]
            internal extern static void CompressedTextureSubImage2DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCompressedTextureSubImage3DEXT", ExactSpelling = true)]
            internal extern static void CompressedTextureSubImage3DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenMobile.Graphics.OpenGL.PixelFormat format, Int32 imageSize, IntPtr bits);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionFilter1D", ExactSpelling = true)]
            internal extern static void ConvolutionFilter1D(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr image);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionFilter1DEXT", ExactSpelling = true)]
            internal extern static void ConvolutionFilter1DEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr image);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionFilter2D", ExactSpelling = true)]
            internal extern static void ConvolutionFilter2D(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr image);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionFilter2DEXT", ExactSpelling = true)]
            internal extern static void ConvolutionFilter2DEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr image);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionParameterf", ExactSpelling = true)]
            internal extern static void ConvolutionParameterf(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.ConvolutionParameter pname, Single @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionParameterfEXT", ExactSpelling = true)]
            internal extern static void ConvolutionParameterfEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.ExtConvolution pname, Single @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionParameterfv", ExactSpelling = true)]
            internal extern static unsafe void ConvolutionParameterfv(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.ConvolutionParameter pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void ConvolutionParameterfvEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.ExtConvolution pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionParameteri", ExactSpelling = true)]
            internal extern static void ConvolutionParameteri(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.ConvolutionParameter pname, Int32 @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionParameteriEXT", ExactSpelling = true)]
            internal extern static void ConvolutionParameteriEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.ExtConvolution pname, Int32 @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionParameteriv", ExactSpelling = true)]
            internal extern static unsafe void ConvolutionParameteriv(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.ConvolutionParameter pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glConvolutionParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void ConvolutionParameterivEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.ExtConvolution pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyBufferSubData", ExactSpelling = true)]
            internal extern static void CopyBufferSubData(OpenMobile.Graphics.OpenGL.BufferTarget readTarget, OpenMobile.Graphics.OpenGL.BufferTarget writeTarget, IntPtr readOffset, IntPtr writeOffset, IntPtr size);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyColorSubTable", ExactSpelling = true)]
            internal extern static void CopyColorSubTable(OpenMobile.Graphics.OpenGL.ColorTableTarget target, Int32 start, Int32 x, Int32 y, Int32 width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyColorSubTableEXT", ExactSpelling = true)]
            internal extern static void CopyColorSubTableEXT(OpenMobile.Graphics.OpenGL.ColorTableTarget target, Int32 start, Int32 x, Int32 y, Int32 width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyColorTable", ExactSpelling = true)]
            internal extern static void CopyColorTable(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 x, Int32 y, Int32 width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyConvolutionFilter1D", ExactSpelling = true)]
            internal extern static void CopyConvolutionFilter1D(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 x, Int32 y, Int32 width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyConvolutionFilter1DEXT", ExactSpelling = true)]
            internal extern static void CopyConvolutionFilter1DEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 x, Int32 y, Int32 width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyConvolutionFilter2D", ExactSpelling = true)]
            internal extern static void CopyConvolutionFilter2D(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyConvolutionFilter2DEXT", ExactSpelling = true)]
            internal extern static void CopyConvolutionFilter2DEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyMultiTexImage1DEXT", ExactSpelling = true)]
            internal extern static void CopyMultiTexImage1DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 x, Int32 y, Int32 width, Int32 border);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyMultiTexImage2DEXT", ExactSpelling = true)]
            internal extern static void CopyMultiTexImage2DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 x, Int32 y, Int32 width, Int32 height, Int32 border);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyMultiTexSubImage1DEXT", ExactSpelling = true)]
            internal extern static void CopyMultiTexSubImage1DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 x, Int32 y, Int32 width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyMultiTexSubImage2DEXT", ExactSpelling = true)]
            internal extern static void CopyMultiTexSubImage2DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyMultiTexSubImage3DEXT", ExactSpelling = true)]
            internal extern static void CopyMultiTexSubImage3DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyPixels", ExactSpelling = true)]
            internal extern static void CopyPixels(Int32 x, Int32 y, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelCopyType type);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexImage1D", ExactSpelling = true)]
            internal extern static void CopyTexImage1D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 x, Int32 y, Int32 width, Int32 border);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexImage1DEXT", ExactSpelling = true)]
            internal extern static void CopyTexImage1DEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 x, Int32 y, Int32 width, Int32 border);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexImage2D", ExactSpelling = true)]
            internal extern static void CopyTexImage2D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 x, Int32 y, Int32 width, Int32 height, Int32 border);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexImage2DEXT", ExactSpelling = true)]
            internal extern static void CopyTexImage2DEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 x, Int32 y, Int32 width, Int32 height, Int32 border);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexSubImage1D", ExactSpelling = true)]
            internal extern static void CopyTexSubImage1D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 x, Int32 y, Int32 width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexSubImage1DEXT", ExactSpelling = true)]
            internal extern static void CopyTexSubImage1DEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 x, Int32 y, Int32 width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexSubImage2D", ExactSpelling = true)]
            internal extern static void CopyTexSubImage2D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexSubImage2DEXT", ExactSpelling = true)]
            internal extern static void CopyTexSubImage2DEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexSubImage3D", ExactSpelling = true)]
            internal extern static void CopyTexSubImage3D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTexSubImage3DEXT", ExactSpelling = true)]
            internal extern static void CopyTexSubImage3DEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTextureImage1DEXT", ExactSpelling = true)]
            internal extern static void CopyTextureImage1DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 x, Int32 y, Int32 width, Int32 border);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTextureImage2DEXT", ExactSpelling = true)]
            internal extern static void CopyTextureImage2DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 x, Int32 y, Int32 width, Int32 height, Int32 border);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTextureSubImage1DEXT", ExactSpelling = true)]
            internal extern static void CopyTextureSubImage1DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 x, Int32 y, Int32 width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTextureSubImage2DEXT", ExactSpelling = true)]
            internal extern static void CopyTextureSubImage2DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCopyTextureSubImage3DEXT", ExactSpelling = true)]
            internal extern static void CopyTextureSubImage3DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCreateProgram", ExactSpelling = true)]
            internal extern static Int32 CreateProgram();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCreateProgramObjectARB", ExactSpelling = true)]
            internal extern static Int32 CreateProgramObjectARB();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCreateShader", ExactSpelling = true)]
            internal extern static Int32 CreateShader(OpenMobile.Graphics.OpenGL.ShaderType type);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCreateShaderObjectARB", ExactSpelling = true)]
            internal extern static Int32 CreateShaderObjectARB(OpenMobile.Graphics.OpenGL.ArbShaderObjects shaderType);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCullFace", ExactSpelling = true)]
            internal extern static void CullFace(OpenMobile.Graphics.OpenGL.CullFaceMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCullParameterdvEXT", ExactSpelling = true)]
            internal extern static unsafe void CullParameterdvEXT(OpenMobile.Graphics.OpenGL.ExtCullVertex pname, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCullParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void CullParameterfvEXT(OpenMobile.Graphics.OpenGL.ExtCullVertex pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glCurrentPaletteMatrixARB", ExactSpelling = true)]
            internal extern static void CurrentPaletteMatrixARB(Int32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteBuffers", ExactSpelling = true)]
            internal extern static unsafe void DeleteBuffers(Int32 n, UInt32* buffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteBuffersARB", ExactSpelling = true)]
            internal extern static unsafe void DeleteBuffersARB(Int32 n, UInt32* buffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteFencesNV", ExactSpelling = true)]
            internal extern static unsafe void DeleteFencesNV(Int32 n, UInt32* fences);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteFragmentShaderATI", ExactSpelling = true)]
            internal extern static void DeleteFragmentShaderATI(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteFramebuffers", ExactSpelling = true)]
            internal extern static unsafe void DeleteFramebuffers(Int32 n, UInt32* framebuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteFramebuffersEXT", ExactSpelling = true)]
            internal extern static unsafe void DeleteFramebuffersEXT(Int32 n, UInt32* framebuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteLists", ExactSpelling = true)]
            internal extern static void DeleteLists(UInt32 list, Int32 range);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteObjectARB", ExactSpelling = true)]
            internal extern static void DeleteObjectARB(UInt32 obj);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteOcclusionQueriesNV", ExactSpelling = true)]
            internal extern static unsafe void DeleteOcclusionQueriesNV(Int32 n, UInt32* ids);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeletePerfMonitorsAMD", ExactSpelling = true)]
            internal extern static unsafe void DeletePerfMonitorsAMD(Int32 n, [OutAttribute] UInt32* monitors);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteProgram", ExactSpelling = true)]
            internal extern static void DeleteProgram(UInt32 program);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteProgramsARB", ExactSpelling = true)]
            internal extern static unsafe void DeleteProgramsARB(Int32 n, UInt32* programs);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteProgramsNV", ExactSpelling = true)]
            internal extern static unsafe void DeleteProgramsNV(Int32 n, UInt32* programs);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteQueries", ExactSpelling = true)]
            internal extern static unsafe void DeleteQueries(Int32 n, UInt32* ids);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteQueriesARB", ExactSpelling = true)]
            internal extern static unsafe void DeleteQueriesARB(Int32 n, UInt32* ids);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteRenderbuffers", ExactSpelling = true)]
            internal extern static unsafe void DeleteRenderbuffers(Int32 n, UInt32* renderbuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteRenderbuffersEXT", ExactSpelling = true)]
            internal extern static unsafe void DeleteRenderbuffersEXT(Int32 n, UInt32* renderbuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteShader", ExactSpelling = true)]
            internal extern static void DeleteShader(UInt32 shader);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteSync", ExactSpelling = true)]
            internal extern static void DeleteSync(IntPtr sync);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteTextures", ExactSpelling = true)]
            internal extern static unsafe void DeleteTextures(Int32 n, UInt32* textures);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteTexturesEXT", ExactSpelling = true)]
            internal extern static unsafe void DeleteTexturesEXT(Int32 n, UInt32* textures);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteTransformFeedbacksNV", ExactSpelling = true)]
            internal extern static unsafe void DeleteTransformFeedbacksNV(Int32 n, UInt32* ids);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteVertexArrays", ExactSpelling = true)]
            internal extern static unsafe void DeleteVertexArrays(Int32 n, UInt32* arrays);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDeleteVertexShaderEXT", ExactSpelling = true)]
            internal extern static void DeleteVertexShaderEXT(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthBoundsdNV", ExactSpelling = true)]
            internal extern static void DepthBoundsdNV(Double zmin, Double zmax);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthBoundsEXT", ExactSpelling = true)]
            internal extern static void DepthBoundsEXT(Double zmin, Double zmax);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthFunc", ExactSpelling = true)]
            internal extern static void DepthFunc(OpenMobile.Graphics.OpenGL.DepthFunction func);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthMask", ExactSpelling = true)]
            internal extern static void DepthMask(bool flag);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthRange", ExactSpelling = true)]
            internal extern static void DepthRange(Double near, Double far);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDepthRangedNV", ExactSpelling = true)]
            internal extern static void DepthRangedNV(Double zNear, Double zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDetachObjectARB", ExactSpelling = true)]
            internal extern static void DetachObjectARB(UInt32 containerObj, UInt32 attachedObj);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDetachShader", ExactSpelling = true)]
            internal extern static void DetachShader(UInt32 program, UInt32 shader);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisable", ExactSpelling = true)]
            internal extern static void Disable(OpenMobile.Graphics.OpenGL.EnableCap cap);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisableClientState", ExactSpelling = true)]
            internal extern static void DisableClientState(OpenMobile.Graphics.OpenGL.ArrayCap array);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisableClientStateIndexedEXT", ExactSpelling = true)]
            internal extern static void DisableClientStateIndexedEXT(OpenMobile.Graphics.OpenGL.EnableCap array, UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisablei", ExactSpelling = true)]
            internal extern static void Disablei(OpenMobile.Graphics.OpenGL.IndexedEnableCap target, UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisableIndexedEXT", ExactSpelling = true)]
            internal extern static void DisableIndexedEXT(OpenMobile.Graphics.OpenGL.ExtDrawBuffers2 target, UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisableVariantClientStateEXT", ExactSpelling = true)]
            internal extern static void DisableVariantClientStateEXT(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisableVertexAttribArray", ExactSpelling = true)]
            internal extern static void DisableVertexAttribArray(UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDisableVertexAttribArrayARB", ExactSpelling = true)]
            internal extern static void DisableVertexAttribArrayARB(UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawArrays", ExactSpelling = true)]
            internal extern static void DrawArrays(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 first, Int32 count);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawArraysEXT", ExactSpelling = true)]
            internal extern static void DrawArraysEXT(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 first, Int32 count);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawArraysInstanced", ExactSpelling = true)]
            internal extern static void DrawArraysInstanced(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 first, Int32 count, Int32 primcount);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawArraysInstancedARB", ExactSpelling = true)]
            internal extern static void DrawArraysInstancedARB(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 first, Int32 count, Int32 primcount);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawArraysInstancedEXT", ExactSpelling = true)]
            internal extern static void DrawArraysInstancedEXT(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 start, Int32 count, Int32 primcount);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawBuffer", ExactSpelling = true)]
            internal extern static void DrawBuffer(OpenMobile.Graphics.OpenGL.DrawBufferMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawBuffers", ExactSpelling = true)]
            internal extern static unsafe void DrawBuffers(Int32 n, OpenMobile.Graphics.OpenGL.DrawBuffersEnum* bufs);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawBuffersARB", ExactSpelling = true)]
            internal extern static unsafe void DrawBuffersARB(Int32 n, OpenMobile.Graphics.OpenGL.ArbDrawBuffers* bufs);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawBuffersATI", ExactSpelling = true)]
            internal extern static unsafe void DrawBuffersATI(Int32 n, OpenMobile.Graphics.OpenGL.AtiDrawBuffers* bufs);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawElementArrayATI", ExactSpelling = true)]
            internal extern static void DrawElementArrayATI(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 count);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawElements", ExactSpelling = true)]
            internal extern static void DrawElements(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawElementsBaseVertex", ExactSpelling = true)]
            internal extern static void DrawElementsBaseVertex(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices, Int32 basevertex);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawElementsInstanced", ExactSpelling = true)]
            internal extern static void DrawElementsInstanced(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawElementsInstancedARB", ExactSpelling = true)]
            internal extern static void DrawElementsInstancedARB(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawElementsInstancedBaseVertex", ExactSpelling = true)]
            internal extern static void DrawElementsInstancedBaseVertex(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount, Int32 basevertex);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawElementsInstancedEXT", ExactSpelling = true)]
            internal extern static void DrawElementsInstancedEXT(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32 count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawPixels", ExactSpelling = true)]
            internal extern static void DrawPixels(Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawRangeElementArrayATI", ExactSpelling = true)]
            internal extern static void DrawRangeElementArrayATI(OpenMobile.Graphics.OpenGL.BeginMode mode, UInt32 start, UInt32 end, Int32 count);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawRangeElements", ExactSpelling = true)]
            internal extern static void DrawRangeElements(OpenMobile.Graphics.OpenGL.BeginMode mode, UInt32 start, UInt32 end, Int32 count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawRangeElementsBaseVertex", ExactSpelling = true)]
            internal extern static void DrawRangeElementsBaseVertex(OpenMobile.Graphics.OpenGL.BeginMode mode, UInt32 start, UInt32 end, Int32 count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices, Int32 basevertex);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawRangeElementsEXT", ExactSpelling = true)]
            internal extern static void DrawRangeElementsEXT(OpenMobile.Graphics.OpenGL.BeginMode mode, UInt32 start, UInt32 end, Int32 count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glDrawTransformFeedbackNV", ExactSpelling = true)]
            internal extern static void DrawTransformFeedbackNV(OpenMobile.Graphics.OpenGL.NvTransformFeedback2 mode, UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEdgeFlag", ExactSpelling = true)]
            internal extern static void EdgeFlag(bool flag);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEdgeFlagPointer", ExactSpelling = true)]
            internal extern static void EdgeFlagPointer(Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEdgeFlagPointerEXT", ExactSpelling = true)]
            internal extern static unsafe void EdgeFlagPointerEXT(Int32 stride, Int32 count, bool* pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEdgeFlagv", ExactSpelling = true)]
            internal extern static unsafe void EdgeFlagv(bool* flag);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glElementPointerATI", ExactSpelling = true)]
            internal extern static void ElementPointerATI(OpenMobile.Graphics.OpenGL.AtiElementArray type, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnable", ExactSpelling = true)]
            internal extern static void Enable(OpenMobile.Graphics.OpenGL.EnableCap cap);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnableClientState", ExactSpelling = true)]
            internal extern static void EnableClientState(OpenMobile.Graphics.OpenGL.ArrayCap array);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnableClientStateIndexedEXT", ExactSpelling = true)]
            internal extern static void EnableClientStateIndexedEXT(OpenMobile.Graphics.OpenGL.EnableCap array, UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnablei", ExactSpelling = true)]
            internal extern static void Enablei(OpenMobile.Graphics.OpenGL.IndexedEnableCap target, UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnableIndexedEXT", ExactSpelling = true)]
            internal extern static void EnableIndexedEXT(OpenMobile.Graphics.OpenGL.ExtDrawBuffers2 target, UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnableVariantClientStateEXT", ExactSpelling = true)]
            internal extern static void EnableVariantClientStateEXT(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnableVertexAttribArray", ExactSpelling = true)]
            internal extern static void EnableVertexAttribArray(UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnableVertexAttribArrayARB", ExactSpelling = true)]
            internal extern static void EnableVertexAttribArrayARB(UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEnd", ExactSpelling = true)]
            internal extern static void End();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndConditionalRender", ExactSpelling = true)]
            internal extern static void EndConditionalRender();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndConditionalRenderNV", ExactSpelling = true)]
            internal extern static void EndConditionalRenderNV();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndFragmentShaderATI", ExactSpelling = true)]
            internal extern static void EndFragmentShaderATI();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndList", ExactSpelling = true)]
            internal extern static void EndList();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndOcclusionQueryNV", ExactSpelling = true)]
            internal extern static void EndOcclusionQueryNV();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndPerfMonitorAMD", ExactSpelling = true)]
            internal extern static void EndPerfMonitorAMD(UInt32 monitor);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndQuery", ExactSpelling = true)]
            internal extern static void EndQuery(OpenMobile.Graphics.OpenGL.QueryTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndQueryARB", ExactSpelling = true)]
            internal extern static void EndQueryARB(OpenMobile.Graphics.OpenGL.ArbOcclusionQuery target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndTransformFeedback", ExactSpelling = true)]
            internal extern static void EndTransformFeedback();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndTransformFeedbackEXT", ExactSpelling = true)]
            internal extern static void EndTransformFeedbackEXT();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndTransformFeedbackNV", ExactSpelling = true)]
            internal extern static void EndTransformFeedbackNV();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEndVertexShaderEXT", ExactSpelling = true)]
            internal extern static void EndVertexShaderEXT();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalCoord1d", ExactSpelling = true)]
            internal extern static void EvalCoord1d(Double u);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalCoord1dv", ExactSpelling = true)]
            internal extern static unsafe void EvalCoord1dv(Double* u);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalCoord1f", ExactSpelling = true)]
            internal extern static void EvalCoord1f(Single u);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalCoord1fv", ExactSpelling = true)]
            internal extern static unsafe void EvalCoord1fv(Single* u);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalCoord2d", ExactSpelling = true)]
            internal extern static void EvalCoord2d(Double u, Double v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalCoord2dv", ExactSpelling = true)]
            internal extern static unsafe void EvalCoord2dv(Double* u);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalCoord2f", ExactSpelling = true)]
            internal extern static void EvalCoord2f(Single u, Single v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalCoord2fv", ExactSpelling = true)]
            internal extern static unsafe void EvalCoord2fv(Single* u);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalMapsNV", ExactSpelling = true)]
            internal extern static void EvalMapsNV(OpenMobile.Graphics.OpenGL.NvEvaluators target, OpenMobile.Graphics.OpenGL.NvEvaluators mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalMesh1", ExactSpelling = true)]
            internal extern static void EvalMesh1(OpenMobile.Graphics.OpenGL.MeshMode1 mode, Int32 i1, Int32 i2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalMesh2", ExactSpelling = true)]
            internal extern static void EvalMesh2(OpenMobile.Graphics.OpenGL.MeshMode2 mode, Int32 i1, Int32 i2, Int32 j1, Int32 j2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalPoint1", ExactSpelling = true)]
            internal extern static void EvalPoint1(Int32 i);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glEvalPoint2", ExactSpelling = true)]
            internal extern static void EvalPoint2(Int32 i, Int32 j);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glExecuteProgramNV", ExactSpelling = true)]
            internal extern static unsafe void ExecuteProgramNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 id, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glExtractComponentEXT", ExactSpelling = true)]
            internal extern static void ExtractComponentEXT(UInt32 res, UInt32 src, UInt32 num);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFeedbackBuffer", ExactSpelling = true)]
            internal extern static unsafe void FeedbackBuffer(Int32 size, OpenMobile.Graphics.OpenGL.FeedbackType type, [OutAttribute] Single* buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFenceSync", ExactSpelling = true)]
            internal extern static IntPtr FenceSync(OpenMobile.Graphics.OpenGL.ArbSync condition, UInt32 flags);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFinalCombinerInputNV", ExactSpelling = true)]
            internal extern static void FinalCombinerInputNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners variable, OpenMobile.Graphics.OpenGL.NvRegisterCombiners input, OpenMobile.Graphics.OpenGL.NvRegisterCombiners mapping, OpenMobile.Graphics.OpenGL.NvRegisterCombiners componentUsage);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFinish", ExactSpelling = true)]
            internal extern static void Finish();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFinishFenceNV", ExactSpelling = true)]
            internal extern static void FinishFenceNV(UInt32 fence);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFlush", ExactSpelling = true)]
            internal extern static void Flush();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFlushMappedBufferRange", ExactSpelling = true)]
            internal extern static void FlushMappedBufferRange(OpenMobile.Graphics.OpenGL.BufferTarget target, IntPtr offset, IntPtr length);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFlushPixelDataRangeNV", ExactSpelling = true)]
            internal extern static void FlushPixelDataRangeNV(OpenMobile.Graphics.OpenGL.NvPixelDataRange target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFlushVertexArrayRangeNV", ExactSpelling = true)]
            internal extern static void FlushVertexArrayRangeNV();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoordd", ExactSpelling = true)]
            internal extern static void FogCoordd(Double coord);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoorddEXT", ExactSpelling = true)]
            internal extern static void FogCoorddEXT(Double coord);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoorddv", ExactSpelling = true)]
            internal extern static unsafe void FogCoorddv(Double* coord);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoorddvEXT", ExactSpelling = true)]
            internal extern static unsafe void FogCoorddvEXT(Double* coord);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoordf", ExactSpelling = true)]
            internal extern static void FogCoordf(Single coord);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoordfEXT", ExactSpelling = true)]
            internal extern static void FogCoordfEXT(Single coord);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoordfv", ExactSpelling = true)]
            internal extern static unsafe void FogCoordfv(Single* coord);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoordfvEXT", ExactSpelling = true)]
            internal extern static unsafe void FogCoordfvEXT(Single* coord);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoordhNV", ExactSpelling = true)]
            internal extern static void FogCoordhNV(OpenMobile.Half fog);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoordhvNV", ExactSpelling = true)]
            internal extern static unsafe void FogCoordhvNV(OpenMobile.Half* fog);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoordPointer", ExactSpelling = true)]
            internal extern static void FogCoordPointer(OpenMobile.Graphics.OpenGL.FogPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogCoordPointerEXT", ExactSpelling = true)]
            internal extern static void FogCoordPointerEXT(OpenMobile.Graphics.OpenGL.ExtFogCoord type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogf", ExactSpelling = true)]
            internal extern static void Fogf(OpenMobile.Graphics.OpenGL.FogParameter pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogfv", ExactSpelling = true)]
            internal extern static unsafe void Fogfv(OpenMobile.Graphics.OpenGL.FogParameter pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogi", ExactSpelling = true)]
            internal extern static void Fogi(OpenMobile.Graphics.OpenGL.FogParameter pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFogiv", ExactSpelling = true)]
            internal extern static unsafe void Fogiv(OpenMobile.Graphics.OpenGL.FogParameter pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferDrawBufferEXT", ExactSpelling = true)]
            internal extern static void FramebufferDrawBufferEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.DrawBufferMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferDrawBuffersEXT", ExactSpelling = true)]
            internal extern static unsafe void FramebufferDrawBuffersEXT(UInt32 framebuffer, Int32 n, OpenMobile.Graphics.OpenGL.DrawBufferMode* bufs);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferReadBufferEXT", ExactSpelling = true)]
            internal extern static void FramebufferReadBufferEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.ReadBufferMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferRenderbuffer", ExactSpelling = true)]
            internal extern static void FramebufferRenderbuffer(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.RenderbufferTarget renderbuffertarget, UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferRenderbufferEXT", ExactSpelling = true)]
            internal extern static void FramebufferRenderbufferEXT(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.RenderbufferTarget renderbuffertarget, UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTexture", ExactSpelling = true)]
            internal extern static void FramebufferTexture(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTexture1D", ExactSpelling = true)]
            internal extern static void FramebufferTexture1D(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.TextureTarget textarget, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTexture1DEXT", ExactSpelling = true)]
            internal extern static void FramebufferTexture1DEXT(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.TextureTarget textarget, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTexture2D", ExactSpelling = true)]
            internal extern static void FramebufferTexture2D(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.TextureTarget textarget, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTexture2DEXT", ExactSpelling = true)]
            internal extern static void FramebufferTexture2DEXT(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.TextureTarget textarget, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTexture3D", ExactSpelling = true)]
            internal extern static void FramebufferTexture3D(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.TextureTarget textarget, UInt32 texture, Int32 level, Int32 zoffset);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTexture3DEXT", ExactSpelling = true)]
            internal extern static void FramebufferTexture3DEXT(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.TextureTarget textarget, UInt32 texture, Int32 level, Int32 zoffset);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTextureARB", ExactSpelling = true)]
            internal extern static void FramebufferTextureARB(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTextureEXT", ExactSpelling = true)]
            internal extern static void FramebufferTextureEXT(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTextureFaceARB", ExactSpelling = true)]
            internal extern static void FramebufferTextureFaceARB(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level, OpenMobile.Graphics.OpenGL.TextureTarget face);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTextureFaceEXT", ExactSpelling = true)]
            internal extern static void FramebufferTextureFaceEXT(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level, OpenMobile.Graphics.OpenGL.TextureTarget face);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTextureLayer", ExactSpelling = true)]
            internal extern static void FramebufferTextureLayer(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level, Int32 layer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTextureLayerARB", ExactSpelling = true)]
            internal extern static void FramebufferTextureLayerARB(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level, Int32 layer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFramebufferTextureLayerEXT", ExactSpelling = true)]
            internal extern static void FramebufferTextureLayerEXT(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level, Int32 layer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFrameTerminatorGREMEDY", ExactSpelling = true)]
            internal extern static void FrameTerminatorGREMEDY();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFreeObjectBufferATI", ExactSpelling = true)]
            internal extern static void FreeObjectBufferATI(UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFrontFace", ExactSpelling = true)]
            internal extern static void FrontFace(OpenMobile.Graphics.OpenGL.FrontFaceDirection mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glFrustum", ExactSpelling = true)]
            internal extern static void Frustum(Double left, Double right, Double bottom, Double top, Double zNear, Double zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenBuffers", ExactSpelling = true)]
            internal extern static unsafe void GenBuffers(Int32 n, [OutAttribute] UInt32* buffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenBuffersARB", ExactSpelling = true)]
            internal extern static unsafe void GenBuffersARB(Int32 n, [OutAttribute] UInt32* buffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenerateMipmap", ExactSpelling = true)]
            internal extern static void GenerateMipmap(OpenMobile.Graphics.OpenGL.GenerateMipmapTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenerateMipmapEXT", ExactSpelling = true)]
            internal extern static void GenerateMipmapEXT(OpenMobile.Graphics.OpenGL.GenerateMipmapTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenerateMultiTexMipmapEXT", ExactSpelling = true)]
            internal extern static void GenerateMultiTexMipmapEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenerateTextureMipmapEXT", ExactSpelling = true)]
            internal extern static void GenerateTextureMipmapEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenFencesNV", ExactSpelling = true)]
            internal extern static unsafe void GenFencesNV(Int32 n, [OutAttribute] UInt32* fences);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenFragmentShadersATI", ExactSpelling = true)]
            internal extern static Int32 GenFragmentShadersATI(UInt32 range);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenFramebuffers", ExactSpelling = true)]
            internal extern static unsafe void GenFramebuffers(Int32 n, [OutAttribute] UInt32* framebuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenFramebuffersEXT", ExactSpelling = true)]
            internal extern static unsafe void GenFramebuffersEXT(Int32 n, [OutAttribute] UInt32* framebuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenLists", ExactSpelling = true)]
            internal extern static Int32 GenLists(Int32 range);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenOcclusionQueriesNV", ExactSpelling = true)]
            internal extern static unsafe void GenOcclusionQueriesNV(Int32 n, [OutAttribute] UInt32* ids);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenPerfMonitorsAMD", ExactSpelling = true)]
            internal extern static unsafe void GenPerfMonitorsAMD(Int32 n, [OutAttribute] UInt32* monitors);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenProgramsARB", ExactSpelling = true)]
            internal extern static unsafe void GenProgramsARB(Int32 n, [OutAttribute] UInt32* programs);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenProgramsNV", ExactSpelling = true)]
            internal extern static unsafe void GenProgramsNV(Int32 n, [OutAttribute] UInt32* programs);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenQueries", ExactSpelling = true)]
            internal extern static unsafe void GenQueries(Int32 n, [OutAttribute] UInt32* ids);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenQueriesARB", ExactSpelling = true)]
            internal extern static unsafe void GenQueriesARB(Int32 n, [OutAttribute] UInt32* ids);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenRenderbuffers", ExactSpelling = true)]
            internal extern static unsafe void GenRenderbuffers(Int32 n, [OutAttribute] UInt32* renderbuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenRenderbuffersEXT", ExactSpelling = true)]
            internal extern static unsafe void GenRenderbuffersEXT(Int32 n, [OutAttribute] UInt32* renderbuffers);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenSymbolsEXT", ExactSpelling = true)]
            internal extern static Int32 GenSymbolsEXT(OpenMobile.Graphics.OpenGL.ExtVertexShader datatype, OpenMobile.Graphics.OpenGL.ExtVertexShader storagetype, OpenMobile.Graphics.OpenGL.ExtVertexShader range, UInt32 components);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenTextures", ExactSpelling = true)]
            internal extern static unsafe void GenTextures(Int32 n, [OutAttribute] UInt32* textures);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenTexturesEXT", ExactSpelling = true)]
            internal extern static unsafe void GenTexturesEXT(Int32 n, [OutAttribute] UInt32* textures);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenTransformFeedbacksNV", ExactSpelling = true)]
            internal extern static unsafe void GenTransformFeedbacksNV(Int32 n, [OutAttribute] UInt32* ids);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenVertexArrays", ExactSpelling = true)]
            internal extern static unsafe void GenVertexArrays(Int32 n, [OutAttribute] UInt32* arrays);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGenVertexShadersEXT", ExactSpelling = true)]
            internal extern static Int32 GenVertexShadersEXT(UInt32 range);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetActiveAttrib", ExactSpelling = true)]
            internal extern static unsafe void GetActiveAttrib(UInt32 program, UInt32 index, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] Int32* size, [OutAttribute] OpenMobile.Graphics.OpenGL.ActiveAttribType* type, [OutAttribute] StringBuilder name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetActiveAttribARB", ExactSpelling = true)]
            internal extern static unsafe void GetActiveAttribARB(UInt32 programObj, UInt32 index, Int32 maxLength, [OutAttribute] Int32* length, [OutAttribute] Int32* size, [OutAttribute] OpenMobile.Graphics.OpenGL.ArbVertexShader* type, [OutAttribute] StringBuilder name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetActiveUniform", ExactSpelling = true)]
            internal extern static unsafe void GetActiveUniform(UInt32 program, UInt32 index, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] Int32* size, [OutAttribute] OpenMobile.Graphics.OpenGL.ActiveUniformType* type, [OutAttribute] StringBuilder name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetActiveUniformARB", ExactSpelling = true)]
            internal extern static unsafe void GetActiveUniformARB(UInt32 programObj, UInt32 index, Int32 maxLength, [OutAttribute] Int32* length, [OutAttribute] Int32* size, [OutAttribute] OpenMobile.Graphics.OpenGL.ArbShaderObjects* type, [OutAttribute] StringBuilder name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetActiveUniformBlockiv", ExactSpelling = true)]
            internal extern static unsafe void GetActiveUniformBlockiv(UInt32 program, UInt32 uniformBlockIndex, OpenMobile.Graphics.OpenGL.ActiveUniformBlockParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetActiveUniformBlockName", ExactSpelling = true)]
            internal extern static unsafe void GetActiveUniformBlockName(UInt32 program, UInt32 uniformBlockIndex, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] StringBuilder uniformBlockName);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetActiveUniformName", ExactSpelling = true)]
            internal extern static unsafe void GetActiveUniformName(UInt32 program, UInt32 uniformIndex, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] StringBuilder uniformName);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetActiveUniformsiv", ExactSpelling = true)]
            internal extern static unsafe void GetActiveUniformsiv(UInt32 program, Int32 uniformCount, UInt32* uniformIndices, OpenMobile.Graphics.OpenGL.ActiveUniformParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetActiveVaryingNV", ExactSpelling = true)]
            internal extern static unsafe void GetActiveVaryingNV(UInt32 program, UInt32 index, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] Int32* size, [OutAttribute] OpenMobile.Graphics.OpenGL.NvTransformFeedback* type, [OutAttribute] StringBuilder name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetArrayObjectfvATI", ExactSpelling = true)]
            internal extern static unsafe void GetArrayObjectfvATI(OpenMobile.Graphics.OpenGL.EnableCap array, OpenMobile.Graphics.OpenGL.AtiVertexArrayObject pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetArrayObjectivATI", ExactSpelling = true)]
            internal extern static unsafe void GetArrayObjectivATI(OpenMobile.Graphics.OpenGL.EnableCap array, OpenMobile.Graphics.OpenGL.AtiVertexArrayObject pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetAttachedObjectsARB", ExactSpelling = true)]
            internal extern static unsafe void GetAttachedObjectsARB(UInt32 containerObj, Int32 maxCount, [OutAttribute] Int32* count, [OutAttribute] UInt32* obj);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetAttachedShaders", ExactSpelling = true)]
            internal extern static unsafe void GetAttachedShaders(UInt32 program, Int32 maxCount, [OutAttribute] Int32* count, [OutAttribute] UInt32* obj);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetAttribLocation", ExactSpelling = true)]
            internal extern static Int32 GetAttribLocation(UInt32 program, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetAttribLocationARB", ExactSpelling = true)]
            internal extern static Int32 GetAttribLocationARB(UInt32 programObj, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBooleani_v", ExactSpelling = true)]
            internal extern static unsafe void GetBooleani_v(OpenMobile.Graphics.OpenGL.GetIndexedPName target, UInt32 index, [OutAttribute] bool* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBooleanIndexedvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetBooleanIndexedvEXT(OpenMobile.Graphics.OpenGL.ExtDrawBuffers2 target, UInt32 index, [OutAttribute] bool* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBooleanv", ExactSpelling = true)]
            internal extern static unsafe void GetBooleanv(OpenMobile.Graphics.OpenGL.GetPName pname, [OutAttribute] bool* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBufferParameteriv", ExactSpelling = true)]
            internal extern static unsafe void GetBufferParameteriv(OpenMobile.Graphics.OpenGL.BufferTarget target, OpenMobile.Graphics.OpenGL.BufferParameterName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBufferParameterivARB", ExactSpelling = true)]
            internal extern static unsafe void GetBufferParameterivARB(OpenMobile.Graphics.OpenGL.ArbVertexBufferObject target, OpenMobile.Graphics.OpenGL.BufferParameterNameArb pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBufferPointerv", ExactSpelling = true)]
            internal extern static void GetBufferPointerv(OpenMobile.Graphics.OpenGL.BufferTarget target, OpenMobile.Graphics.OpenGL.BufferPointer pname, [OutAttribute] IntPtr @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBufferPointervARB", ExactSpelling = true)]
            internal extern static void GetBufferPointervARB(OpenMobile.Graphics.OpenGL.ArbVertexBufferObject target, OpenMobile.Graphics.OpenGL.BufferPointerNameArb pname, [OutAttribute] IntPtr @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBufferSubData", ExactSpelling = true)]
            internal extern static void GetBufferSubData(OpenMobile.Graphics.OpenGL.BufferTarget target, IntPtr offset, IntPtr size, [OutAttribute] IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetBufferSubDataARB", ExactSpelling = true)]
            internal extern static void GetBufferSubDataARB(OpenMobile.Graphics.OpenGL.BufferTargetArb target, IntPtr offset, IntPtr size, [OutAttribute] IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetClipPlane", ExactSpelling = true)]
            internal extern static unsafe void GetClipPlane(OpenMobile.Graphics.OpenGL.ClipPlaneName plane, [OutAttribute] Double* equation);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetColorTable", ExactSpelling = true)]
            internal extern static void GetColorTable(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr table);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetColorTableEXT", ExactSpelling = true)]
            internal extern static void GetColorTableEXT(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetColorTableParameterfv", ExactSpelling = true)]
            internal extern static unsafe void GetColorTableParameterfv(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.GetColorTableParameterPName pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetColorTableParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetColorTableParameterfvEXT(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.GetColorTableParameterPName pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetColorTableParameteriv", ExactSpelling = true)]
            internal extern static unsafe void GetColorTableParameteriv(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.GetColorTableParameterPName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetColorTableParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetColorTableParameterivEXT(OpenMobile.Graphics.OpenGL.ColorTableTarget target, OpenMobile.Graphics.OpenGL.GetColorTableParameterPName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetCombinerInputParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void GetCombinerInputParameterfvNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners stage, OpenMobile.Graphics.OpenGL.NvRegisterCombiners portion, OpenMobile.Graphics.OpenGL.NvRegisterCombiners variable, OpenMobile.Graphics.OpenGL.NvRegisterCombiners pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetCombinerInputParameterivNV", ExactSpelling = true)]
            internal extern static unsafe void GetCombinerInputParameterivNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners stage, OpenMobile.Graphics.OpenGL.NvRegisterCombiners portion, OpenMobile.Graphics.OpenGL.NvRegisterCombiners variable, OpenMobile.Graphics.OpenGL.NvRegisterCombiners pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetCombinerOutputParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void GetCombinerOutputParameterfvNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners stage, OpenMobile.Graphics.OpenGL.NvRegisterCombiners portion, OpenMobile.Graphics.OpenGL.NvRegisterCombiners pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetCombinerOutputParameterivNV", ExactSpelling = true)]
            internal extern static unsafe void GetCombinerOutputParameterivNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners stage, OpenMobile.Graphics.OpenGL.NvRegisterCombiners portion, OpenMobile.Graphics.OpenGL.NvRegisterCombiners pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetCombinerStageParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void GetCombinerStageParameterfvNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners2 stage, OpenMobile.Graphics.OpenGL.NvRegisterCombiners2 pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetCompressedMultiTexImageEXT", ExactSpelling = true)]
            internal extern static void GetCompressedMultiTexImageEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 lod, [OutAttribute] IntPtr img);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetCompressedTexImage", ExactSpelling = true)]
            internal extern static void GetCompressedTexImage(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, [OutAttribute] IntPtr img);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetCompressedTexImageARB", ExactSpelling = true)]
            internal extern static void GetCompressedTexImageARB(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, [OutAttribute] IntPtr img);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetCompressedTextureImageEXT", ExactSpelling = true)]
            internal extern static void GetCompressedTextureImageEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 lod, [OutAttribute] IntPtr img);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetConvolutionFilter", ExactSpelling = true)]
            internal extern static void GetConvolutionFilter(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr image);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetConvolutionFilterEXT", ExactSpelling = true)]
            internal extern static void GetConvolutionFilterEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr image);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetConvolutionParameterfv", ExactSpelling = true)]
            internal extern static unsafe void GetConvolutionParameterfv(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.GetConvolutionParameterPName pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetConvolutionParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetConvolutionParameterfvEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.ExtConvolution pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetConvolutionParameteriv", ExactSpelling = true)]
            internal extern static unsafe void GetConvolutionParameteriv(OpenMobile.Graphics.OpenGL.ConvolutionTarget target, OpenMobile.Graphics.OpenGL.GetConvolutionParameterPName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetConvolutionParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetConvolutionParameterivEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.ExtConvolution pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetDoubleIndexedvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetDoubleIndexedvEXT(OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, [OutAttribute] Double* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetDoublev", ExactSpelling = true)]
            internal extern static unsafe void GetDoublev(OpenMobile.Graphics.OpenGL.GetPName pname, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetError", ExactSpelling = true)]
            internal extern static OpenMobile.Graphics.OpenGL.ErrorCode GetError();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFenceivNV", ExactSpelling = true)]
            internal extern static unsafe void GetFenceivNV(UInt32 fence, OpenMobile.Graphics.OpenGL.NvFence pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFinalCombinerInputParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void GetFinalCombinerInputParameterfvNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners variable, OpenMobile.Graphics.OpenGL.NvRegisterCombiners pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFinalCombinerInputParameterivNV", ExactSpelling = true)]
            internal extern static unsafe void GetFinalCombinerInputParameterivNV(OpenMobile.Graphics.OpenGL.NvRegisterCombiners variable, OpenMobile.Graphics.OpenGL.NvRegisterCombiners pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFloatIndexedvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetFloatIndexedvEXT(OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, [OutAttribute] Single* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFloatv", ExactSpelling = true)]
            internal extern static unsafe void GetFloatv(OpenMobile.Graphics.OpenGL.GetPName pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFragDataLocation", ExactSpelling = true)]
            internal extern static Int32 GetFragDataLocation(UInt32 program, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFragDataLocationEXT", ExactSpelling = true)]
            internal extern static Int32 GetFragDataLocationEXT(UInt32 program, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFramebufferAttachmentParameteriv", ExactSpelling = true)]
            internal extern static unsafe void GetFramebufferAttachmentParameteriv(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.FramebufferParameterName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFramebufferAttachmentParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetFramebufferAttachmentParameterivEXT(OpenMobile.Graphics.OpenGL.FramebufferTarget target, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.FramebufferParameterName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetFramebufferParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetFramebufferParameterivEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetHandleARB", ExactSpelling = true)]
            internal extern static Int32 GetHandleARB(OpenMobile.Graphics.OpenGL.ArbShaderObjects pname);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetHistogram", ExactSpelling = true)]
            internal extern static void GetHistogram(OpenMobile.Graphics.OpenGL.HistogramTarget target, bool reset, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetHistogramEXT", ExactSpelling = true)]
            internal extern static void GetHistogramEXT(OpenMobile.Graphics.OpenGL.ExtHistogram target, bool reset, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetHistogramParameterfv", ExactSpelling = true)]
            internal extern static unsafe void GetHistogramParameterfv(OpenMobile.Graphics.OpenGL.HistogramTarget target, OpenMobile.Graphics.OpenGL.GetHistogramParameterPName pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetHistogramParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetHistogramParameterfvEXT(OpenMobile.Graphics.OpenGL.ExtHistogram target, OpenMobile.Graphics.OpenGL.ExtHistogram pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetHistogramParameteriv", ExactSpelling = true)]
            internal extern static unsafe void GetHistogramParameteriv(OpenMobile.Graphics.OpenGL.HistogramTarget target, OpenMobile.Graphics.OpenGL.GetHistogramParameterPName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetHistogramParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetHistogramParameterivEXT(OpenMobile.Graphics.OpenGL.ExtHistogram target, OpenMobile.Graphics.OpenGL.ExtHistogram pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetInfoLogARB", ExactSpelling = true)]
            internal extern static unsafe void GetInfoLogARB(UInt32 obj, Int32 maxLength, [OutAttribute] Int32* length, [OutAttribute] StringBuilder infoLog);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetInteger64v", ExactSpelling = true)]
            internal extern static unsafe void GetInteger64v(OpenMobile.Graphics.OpenGL.ArbSync pname, [OutAttribute] Int64* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetIntegeri_v", ExactSpelling = true)]
            internal extern static unsafe void GetIntegeri_v(OpenMobile.Graphics.OpenGL.GetIndexedPName target, UInt32 index, [OutAttribute] Int32* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetIntegerIndexedvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetIntegerIndexedvEXT(OpenMobile.Graphics.OpenGL.ExtDrawBuffers2 target, UInt32 index, [OutAttribute] Int32* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetIntegerv", ExactSpelling = true)]
            internal extern static unsafe void GetIntegerv(OpenMobile.Graphics.OpenGL.GetPName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetInvariantBooleanvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetInvariantBooleanvEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader value, [OutAttribute] bool* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetInvariantFloatvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetInvariantFloatvEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader value, [OutAttribute] Single* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetInvariantIntegervEXT", ExactSpelling = true)]
            internal extern static unsafe void GetInvariantIntegervEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader value, [OutAttribute] Int32* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetLightfv", ExactSpelling = true)]
            internal extern static unsafe void GetLightfv(OpenMobile.Graphics.OpenGL.LightName light, OpenMobile.Graphics.OpenGL.LightParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetLightiv", ExactSpelling = true)]
            internal extern static unsafe void GetLightiv(OpenMobile.Graphics.OpenGL.LightName light, OpenMobile.Graphics.OpenGL.LightParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetLocalConstantBooleanvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetLocalConstantBooleanvEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader value, [OutAttribute] bool* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetLocalConstantFloatvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetLocalConstantFloatvEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader value, [OutAttribute] Single* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetLocalConstantIntegervEXT", ExactSpelling = true)]
            internal extern static unsafe void GetLocalConstantIntegervEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader value, [OutAttribute] Int32* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMapAttribParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void GetMapAttribParameterfvNV(OpenMobile.Graphics.OpenGL.NvEvaluators target, UInt32 index, OpenMobile.Graphics.OpenGL.NvEvaluators pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMapAttribParameterivNV", ExactSpelling = true)]
            internal extern static unsafe void GetMapAttribParameterivNV(OpenMobile.Graphics.OpenGL.NvEvaluators target, UInt32 index, OpenMobile.Graphics.OpenGL.NvEvaluators pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMapControlPointsNV", ExactSpelling = true)]
            internal extern static void GetMapControlPointsNV(OpenMobile.Graphics.OpenGL.NvEvaluators target, UInt32 index, OpenMobile.Graphics.OpenGL.NvEvaluators type, Int32 ustride, Int32 vstride, bool packed, [OutAttribute] IntPtr points);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMapdv", ExactSpelling = true)]
            internal extern static unsafe void GetMapdv(OpenMobile.Graphics.OpenGL.MapTarget target, OpenMobile.Graphics.OpenGL.GetMapQuery query, [OutAttribute] Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMapfv", ExactSpelling = true)]
            internal extern static unsafe void GetMapfv(OpenMobile.Graphics.OpenGL.MapTarget target, OpenMobile.Graphics.OpenGL.GetMapQuery query, [OutAttribute] Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMapiv", ExactSpelling = true)]
            internal extern static unsafe void GetMapiv(OpenMobile.Graphics.OpenGL.MapTarget target, OpenMobile.Graphics.OpenGL.GetMapQuery query, [OutAttribute] Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMapParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void GetMapParameterfvNV(OpenMobile.Graphics.OpenGL.NvEvaluators target, OpenMobile.Graphics.OpenGL.NvEvaluators pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMapParameterivNV", ExactSpelling = true)]
            internal extern static unsafe void GetMapParameterivNV(OpenMobile.Graphics.OpenGL.NvEvaluators target, OpenMobile.Graphics.OpenGL.NvEvaluators pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMaterialfv", ExactSpelling = true)]
            internal extern static unsafe void GetMaterialfv(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.MaterialParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMaterialiv", ExactSpelling = true)]
            internal extern static unsafe void GetMaterialiv(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.MaterialParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMinmax", ExactSpelling = true)]
            internal extern static void GetMinmax(OpenMobile.Graphics.OpenGL.MinmaxTarget target, bool reset, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMinmaxEXT", ExactSpelling = true)]
            internal extern static void GetMinmaxEXT(OpenMobile.Graphics.OpenGL.ExtHistogram target, bool reset, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMinmaxParameterfv", ExactSpelling = true)]
            internal extern static unsafe void GetMinmaxParameterfv(OpenMobile.Graphics.OpenGL.MinmaxTarget target, OpenMobile.Graphics.OpenGL.GetMinmaxParameterPName pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMinmaxParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMinmaxParameterfvEXT(OpenMobile.Graphics.OpenGL.ExtHistogram target, OpenMobile.Graphics.OpenGL.ExtHistogram pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMinmaxParameteriv", ExactSpelling = true)]
            internal extern static unsafe void GetMinmaxParameteriv(OpenMobile.Graphics.OpenGL.MinmaxTarget target, OpenMobile.Graphics.OpenGL.GetMinmaxParameterPName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMinmaxParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMinmaxParameterivEXT(OpenMobile.Graphics.OpenGL.ExtHistogram target, OpenMobile.Graphics.OpenGL.ExtHistogram pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultisamplefv", ExactSpelling = true)]
            internal extern static unsafe void GetMultisamplefv(OpenMobile.Graphics.OpenGL.GetMultisamplePName pname, UInt32 index, [OutAttribute] Single* val);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultisamplefvNV", ExactSpelling = true)]
            internal extern static unsafe void GetMultisamplefvNV(OpenMobile.Graphics.OpenGL.NvExplicitMultisample pname, UInt32 index, [OutAttribute] Single* val);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexEnvfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexEnvfvEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexEnvivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexEnvivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexGendvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexGendvEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexGenfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexGenfvEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexGenivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexGenivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexImageEXT", ExactSpelling = true)]
            internal extern static void GetMultiTexImageEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexLevelParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexLevelParameterfvEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexLevelParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexLevelParameterivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexParameterfvEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexParameterIivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexParameterIivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexParameterIuivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexParameterIuivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetMultiTexParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetMultiTexParameterivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedBufferParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetNamedBufferParameterivEXT(UInt32 buffer, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedBufferPointervEXT", ExactSpelling = true)]
            internal extern static void GetNamedBufferPointervEXT(UInt32 buffer, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess pname, [OutAttribute] IntPtr @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedBufferSubDataEXT", ExactSpelling = true)]
            internal extern static void GetNamedBufferSubDataEXT(UInt32 buffer, IntPtr offset, IntPtr size, [OutAttribute] IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedFramebufferAttachmentParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetNamedFramebufferAttachmentParameterivEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedProgramivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetNamedProgramivEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedProgramLocalParameterdvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetNamedProgramLocalParameterdvEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedProgramLocalParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetNamedProgramLocalParameterfvEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedProgramLocalParameterIivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetNamedProgramLocalParameterIivEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedProgramLocalParameterIuivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetNamedProgramLocalParameterIuivEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedProgramStringEXT", ExactSpelling = true)]
            internal extern static void GetNamedProgramStringEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess pname, [OutAttribute] IntPtr @string);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetNamedRenderbufferParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetNamedRenderbufferParameterivEXT(UInt32 renderbuffer, OpenMobile.Graphics.OpenGL.RenderbufferParameterName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetObjectBufferfvATI", ExactSpelling = true)]
            internal extern static unsafe void GetObjectBufferfvATI(UInt32 buffer, OpenMobile.Graphics.OpenGL.AtiVertexArrayObject pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetObjectBufferivATI", ExactSpelling = true)]
            internal extern static unsafe void GetObjectBufferivATI(UInt32 buffer, OpenMobile.Graphics.OpenGL.AtiVertexArrayObject pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetObjectParameterfvARB", ExactSpelling = true)]
            internal extern static unsafe void GetObjectParameterfvARB(UInt32 obj, OpenMobile.Graphics.OpenGL.ArbShaderObjects pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetObjectParameterivARB", ExactSpelling = true)]
            internal extern static unsafe void GetObjectParameterivARB(UInt32 obj, OpenMobile.Graphics.OpenGL.ArbShaderObjects pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetOcclusionQueryivNV", ExactSpelling = true)]
            internal extern static unsafe void GetOcclusionQueryivNV(UInt32 id, OpenMobile.Graphics.OpenGL.NvOcclusionQuery pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetOcclusionQueryuivNV", ExactSpelling = true)]
            internal extern static unsafe void GetOcclusionQueryuivNV(UInt32 id, OpenMobile.Graphics.OpenGL.NvOcclusionQuery pname, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPerfMonitorCounterDataAMD", ExactSpelling = true)]
            internal extern static unsafe void GetPerfMonitorCounterDataAMD(UInt32 monitor, OpenMobile.Graphics.OpenGL.AmdPerformanceMonitor pname, Int32 dataSize, [OutAttribute] UInt32* data, [OutAttribute] Int32* bytesWritten);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPerfMonitorCounterInfoAMD", ExactSpelling = true)]
            internal extern static void GetPerfMonitorCounterInfoAMD(UInt32 group, UInt32 counter, OpenMobile.Graphics.OpenGL.AmdPerformanceMonitor pname, [OutAttribute] IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPerfMonitorCountersAMD", ExactSpelling = true)]
            internal extern static unsafe void GetPerfMonitorCountersAMD(UInt32 group, [OutAttribute] Int32* numCounters, [OutAttribute] Int32* maxActiveCounters, Int32 counterSize, [OutAttribute] UInt32* counters);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPerfMonitorCounterStringAMD", ExactSpelling = true)]
            internal extern static unsafe void GetPerfMonitorCounterStringAMD(UInt32 group, UInt32 counter, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] StringBuilder counterString);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPerfMonitorGroupsAMD", ExactSpelling = true)]
            internal extern static unsafe void GetPerfMonitorGroupsAMD([OutAttribute] Int32* numGroups, Int32 groupsSize, [OutAttribute] UInt32* groups);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPerfMonitorGroupStringAMD", ExactSpelling = true)]
            internal extern static unsafe void GetPerfMonitorGroupStringAMD(UInt32 group, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] StringBuilder groupString);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPixelMapfv", ExactSpelling = true)]
            internal extern static unsafe void GetPixelMapfv(OpenMobile.Graphics.OpenGL.PixelMap map, [OutAttribute] Single* values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPixelMapuiv", ExactSpelling = true)]
            internal extern static unsafe void GetPixelMapuiv(OpenMobile.Graphics.OpenGL.PixelMap map, [OutAttribute] UInt32* values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPixelMapusv", ExactSpelling = true)]
            internal extern static unsafe void GetPixelMapusv(OpenMobile.Graphics.OpenGL.PixelMap map, [OutAttribute] UInt16* values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPointerIndexedvEXT", ExactSpelling = true)]
            internal extern static void GetPointerIndexedvEXT(OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, [OutAttribute] IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPointerv", ExactSpelling = true)]
            internal extern static void GetPointerv(OpenMobile.Graphics.OpenGL.GetPointervPName pname, [OutAttribute] IntPtr @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPointervEXT", ExactSpelling = true)]
            internal extern static void GetPointervEXT(OpenMobile.Graphics.OpenGL.GetPointervPName pname, [OutAttribute] IntPtr @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetPolygonStipple", ExactSpelling = true)]
            internal extern static unsafe void GetPolygonStipple([OutAttribute] Byte* mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramEnvParameterdvARB", ExactSpelling = true)]
            internal extern static unsafe void GetProgramEnvParameterdvARB(OpenMobile.Graphics.OpenGL.ArbVertexProgram target, UInt32 index, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramEnvParameterfvARB", ExactSpelling = true)]
            internal extern static unsafe void GetProgramEnvParameterfvARB(OpenMobile.Graphics.OpenGL.ArbVertexProgram target, UInt32 index, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramEnvParameterIivNV", ExactSpelling = true)]
            internal extern static unsafe void GetProgramEnvParameterIivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramEnvParameterIuivNV", ExactSpelling = true)]
            internal extern static unsafe void GetProgramEnvParameterIuivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramInfoLog", ExactSpelling = true)]
            internal extern static unsafe void GetProgramInfoLog(UInt32 program, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] StringBuilder infoLog);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramiv", ExactSpelling = true)]
            internal extern static unsafe void GetProgramiv(UInt32 program, OpenMobile.Graphics.OpenGL.ProgramParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramivARB", ExactSpelling = true)]
            internal extern static unsafe void GetProgramivARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, OpenMobile.Graphics.OpenGL.AssemblyProgramParameterArb pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramivNV", ExactSpelling = true)]
            internal extern static unsafe void GetProgramivNV(UInt32 id, OpenMobile.Graphics.OpenGL.NvVertexProgram pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramLocalParameterdvARB", ExactSpelling = true)]
            internal extern static unsafe void GetProgramLocalParameterdvARB(OpenMobile.Graphics.OpenGL.ArbVertexProgram target, UInt32 index, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramLocalParameterfvARB", ExactSpelling = true)]
            internal extern static unsafe void GetProgramLocalParameterfvARB(OpenMobile.Graphics.OpenGL.ArbVertexProgram target, UInt32 index, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramLocalParameterIivNV", ExactSpelling = true)]
            internal extern static unsafe void GetProgramLocalParameterIivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramLocalParameterIuivNV", ExactSpelling = true)]
            internal extern static unsafe void GetProgramLocalParameterIuivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramNamedParameterdvNV", ExactSpelling = true)]
            internal extern static unsafe void GetProgramNamedParameterdvNV(UInt32 id, Int32 len, Byte* name, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramNamedParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void GetProgramNamedParameterfvNV(UInt32 id, Int32 len, Byte* name, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramParameterdvNV", ExactSpelling = true)]
            internal extern static unsafe void GetProgramParameterdvNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, OpenMobile.Graphics.OpenGL.AssemblyProgramParameterArb pname, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void GetProgramParameterfvNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, OpenMobile.Graphics.OpenGL.AssemblyProgramParameterArb pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramStringARB", ExactSpelling = true)]
            internal extern static void GetProgramStringARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, OpenMobile.Graphics.OpenGL.AssemblyProgramParameterArb pname, [OutAttribute] IntPtr @string);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetProgramStringNV", ExactSpelling = true)]
            internal extern static unsafe void GetProgramStringNV(UInt32 id, OpenMobile.Graphics.OpenGL.NvVertexProgram pname, [OutAttribute] Byte* program);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetQueryiv", ExactSpelling = true)]
            internal extern static unsafe void GetQueryiv(OpenMobile.Graphics.OpenGL.QueryTarget target, OpenMobile.Graphics.OpenGL.GetQueryParam pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetQueryivARB", ExactSpelling = true)]
            internal extern static unsafe void GetQueryivARB(OpenMobile.Graphics.OpenGL.ArbOcclusionQuery target, OpenMobile.Graphics.OpenGL.ArbOcclusionQuery pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetQueryObjecti64vEXT", ExactSpelling = true)]
            internal extern static unsafe void GetQueryObjecti64vEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtTimerQuery pname, [OutAttribute] Int64* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetQueryObjectiv", ExactSpelling = true)]
            internal extern static unsafe void GetQueryObjectiv(UInt32 id, OpenMobile.Graphics.OpenGL.GetQueryObjectParam pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetQueryObjectivARB", ExactSpelling = true)]
            internal extern static unsafe void GetQueryObjectivARB(UInt32 id, OpenMobile.Graphics.OpenGL.ArbOcclusionQuery pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetQueryObjectui64vEXT", ExactSpelling = true)]
            internal extern static unsafe void GetQueryObjectui64vEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtTimerQuery pname, [OutAttribute] UInt64* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetQueryObjectuiv", ExactSpelling = true)]
            internal extern static unsafe void GetQueryObjectuiv(UInt32 id, OpenMobile.Graphics.OpenGL.GetQueryObjectParam pname, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetQueryObjectuivARB", ExactSpelling = true)]
            internal extern static unsafe void GetQueryObjectuivARB(UInt32 id, OpenMobile.Graphics.OpenGL.ArbOcclusionQuery pname, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetRenderbufferParameteriv", ExactSpelling = true)]
            internal extern static unsafe void GetRenderbufferParameteriv(OpenMobile.Graphics.OpenGL.RenderbufferTarget target, OpenMobile.Graphics.OpenGL.RenderbufferParameterName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetRenderbufferParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetRenderbufferParameterivEXT(OpenMobile.Graphics.OpenGL.RenderbufferTarget target, OpenMobile.Graphics.OpenGL.RenderbufferParameterName pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetSeparableFilter", ExactSpelling = true)]
            internal extern static void GetSeparableFilter(OpenMobile.Graphics.OpenGL.SeparableTarget target, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr row, [OutAttribute] IntPtr column, [OutAttribute] IntPtr span);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetSeparableFilterEXT", ExactSpelling = true)]
            internal extern static void GetSeparableFilterEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr row, [OutAttribute] IntPtr column, [OutAttribute] IntPtr span);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetShaderInfoLog", ExactSpelling = true)]
            internal extern static unsafe void GetShaderInfoLog(UInt32 shader, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] StringBuilder infoLog);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetShaderiv", ExactSpelling = true)]
            internal extern static unsafe void GetShaderiv(UInt32 shader, OpenMobile.Graphics.OpenGL.ShaderParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetShaderSource", ExactSpelling = true)]
            internal extern static unsafe void GetShaderSource(UInt32 shader, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] StringBuilder source);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetShaderSourceARB", ExactSpelling = true)]
            internal extern static unsafe void GetShaderSourceARB(UInt32 obj, Int32 maxLength, [OutAttribute] Int32* length, [OutAttribute] StringBuilder source);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetString", ExactSpelling = true)]
            internal extern static System.IntPtr GetString(OpenMobile.Graphics.OpenGL.StringName name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetStringi", ExactSpelling = true)]
            internal extern static System.IntPtr GetStringi(OpenMobile.Graphics.OpenGL.StringName name, UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetSynciv", ExactSpelling = true)]
            internal extern static unsafe void GetSynciv(IntPtr sync, OpenMobile.Graphics.OpenGL.ArbSync pname, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] Int32* values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexBumpParameterfvATI", ExactSpelling = true)]
            internal extern static unsafe void GetTexBumpParameterfvATI(OpenMobile.Graphics.OpenGL.AtiEnvmapBumpmap pname, [OutAttribute] Single* param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexBumpParameterivATI", ExactSpelling = true)]
            internal extern static unsafe void GetTexBumpParameterivATI(OpenMobile.Graphics.OpenGL.AtiEnvmapBumpmap pname, [OutAttribute] Int32* param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexEnvfv", ExactSpelling = true)]
            internal extern static unsafe void GetTexEnvfv(OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexEnviv", ExactSpelling = true)]
            internal extern static unsafe void GetTexEnviv(OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexGendv", ExactSpelling = true)]
            internal extern static unsafe void GetTexGendv(OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexGenfv", ExactSpelling = true)]
            internal extern static unsafe void GetTexGenfv(OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexGeniv", ExactSpelling = true)]
            internal extern static unsafe void GetTexGeniv(OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexImage", ExactSpelling = true)]
            internal extern static void GetTexImage(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexLevelParameterfv", ExactSpelling = true)]
            internal extern static unsafe void GetTexLevelParameterfv(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexLevelParameteriv", ExactSpelling = true)]
            internal extern static unsafe void GetTexLevelParameteriv(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexParameterfv", ExactSpelling = true)]
            internal extern static unsafe void GetTexParameterfv(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexParameterIiv", ExactSpelling = true)]
            internal extern static unsafe void GetTexParameterIiv(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexParameterIivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetTexParameterIivEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexParameterIuiv", ExactSpelling = true)]
            internal extern static unsafe void GetTexParameterIuiv(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexParameterIuivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetTexParameterIuivEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTexParameteriv", ExactSpelling = true)]
            internal extern static unsafe void GetTexParameteriv(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTextureImageEXT", ExactSpelling = true)]
            internal extern static void GetTextureImageEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTextureLevelParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetTextureLevelParameterfvEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTextureLevelParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetTextureLevelParameterivEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTextureParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetTextureParameterfvEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTextureParameterIivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetTextureParameterIivEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTextureParameterIuivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetTextureParameterIuivEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTextureParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetTextureParameterivEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.GetTextureParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTrackMatrixivNV", ExactSpelling = true)]
            internal extern static unsafe void GetTrackMatrixivNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 address, OpenMobile.Graphics.OpenGL.AssemblyProgramParameterArb pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTransformFeedbackVarying", ExactSpelling = true)]
            internal extern static unsafe void GetTransformFeedbackVarying(UInt32 program, UInt32 index, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] Int32* size, [OutAttribute] OpenMobile.Graphics.OpenGL.ActiveAttribType* type, [OutAttribute] StringBuilder name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTransformFeedbackVaryingEXT", ExactSpelling = true)]
            internal extern static unsafe void GetTransformFeedbackVaryingEXT(UInt32 program, UInt32 index, Int32 bufSize, [OutAttribute] Int32* length, [OutAttribute] Int32* size, [OutAttribute] OpenMobile.Graphics.OpenGL.ExtTransformFeedback* type, [OutAttribute] StringBuilder name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetTransformFeedbackVaryingNV", ExactSpelling = true)]
            internal extern static unsafe void GetTransformFeedbackVaryingNV(UInt32 program, UInt32 index, [OutAttribute] Int32* location);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformBlockIndex", ExactSpelling = true)]
            internal extern static Int32 GetUniformBlockIndex(UInt32 program, String uniformBlockName);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformBufferSizeEXT", ExactSpelling = true)]
            internal extern static Int32 GetUniformBufferSizeEXT(UInt32 program, Int32 location);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformfv", ExactSpelling = true)]
            internal extern static unsafe void GetUniformfv(UInt32 program, Int32 location, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformfvARB", ExactSpelling = true)]
            internal extern static unsafe void GetUniformfvARB(UInt32 programObj, Int32 location, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformIndices", ExactSpelling = true)]
            internal extern static unsafe void GetUniformIndices(UInt32 program, Int32 uniformCount, String[] uniformNames, [OutAttribute] UInt32* uniformIndices);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformiv", ExactSpelling = true)]
            internal extern static unsafe void GetUniformiv(UInt32 program, Int32 location, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformivARB", ExactSpelling = true)]
            internal extern static unsafe void GetUniformivARB(UInt32 programObj, Int32 location, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformLocation", ExactSpelling = true)]
            internal extern static Int32 GetUniformLocation(UInt32 program, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformLocationARB", ExactSpelling = true)]
            internal extern static Int32 GetUniformLocationARB(UInt32 programObj, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformOffsetEXT", ExactSpelling = true)]
            internal extern static IntPtr GetUniformOffsetEXT(UInt32 program, Int32 location);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformuiv", ExactSpelling = true)]
            internal extern static unsafe void GetUniformuiv(UInt32 program, Int32 location, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetUniformuivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetUniformuivEXT(UInt32 program, Int32 location, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVariantArrayObjectfvATI", ExactSpelling = true)]
            internal extern static unsafe void GetVariantArrayObjectfvATI(UInt32 id, OpenMobile.Graphics.OpenGL.AtiVertexArrayObject pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVariantArrayObjectivATI", ExactSpelling = true)]
            internal extern static unsafe void GetVariantArrayObjectivATI(UInt32 id, OpenMobile.Graphics.OpenGL.AtiVertexArrayObject pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVariantBooleanvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetVariantBooleanvEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader value, [OutAttribute] bool* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVariantFloatvEXT", ExactSpelling = true)]
            internal extern static unsafe void GetVariantFloatvEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader value, [OutAttribute] Single* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVariantIntegervEXT", ExactSpelling = true)]
            internal extern static unsafe void GetVariantIntegervEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader value, [OutAttribute] Int32* data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVariantPointervEXT", ExactSpelling = true)]
            internal extern static void GetVariantPointervEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader value, [OutAttribute] IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVaryingLocationNV", ExactSpelling = true)]
            internal extern static Int32 GetVaryingLocationNV(UInt32 program, String name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribArrayObjectfvATI", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribArrayObjectfvATI(UInt32 index, OpenMobile.Graphics.OpenGL.AtiVertexAttribArrayObject pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribArrayObjectivATI", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribArrayObjectivATI(UInt32 index, OpenMobile.Graphics.OpenGL.AtiVertexAttribArrayObject pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribdv", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribdv(UInt32 index, OpenMobile.Graphics.OpenGL.VertexAttribParameter pname, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribdvARB", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribdvARB(UInt32 index, OpenMobile.Graphics.OpenGL.VertexAttribParameterArb pname, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribdvNV", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribdvNV(UInt32 index, OpenMobile.Graphics.OpenGL.NvVertexProgram pname, [OutAttribute] Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribfv", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribfv(UInt32 index, OpenMobile.Graphics.OpenGL.VertexAttribParameter pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribfvARB", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribfvARB(UInt32 index, OpenMobile.Graphics.OpenGL.VertexAttribParameterArb pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribfvNV", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribfvNV(UInt32 index, OpenMobile.Graphics.OpenGL.NvVertexProgram pname, [OutAttribute] Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribIiv", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribIiv(UInt32 index, OpenMobile.Graphics.OpenGL.VertexAttribParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribIivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribIivEXT(UInt32 index, OpenMobile.Graphics.OpenGL.NvVertexProgram4 pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribIuiv", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribIuiv(UInt32 index, OpenMobile.Graphics.OpenGL.VertexAttribParameter pname, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribIuivEXT", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribIuivEXT(UInt32 index, OpenMobile.Graphics.OpenGL.NvVertexProgram4 pname, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribiv", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribiv(UInt32 index, OpenMobile.Graphics.OpenGL.VertexAttribParameter pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribivARB", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribivARB(UInt32 index, OpenMobile.Graphics.OpenGL.VertexAttribParameterArb pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribivNV", ExactSpelling = true)]
            internal extern static unsafe void GetVertexAttribivNV(UInt32 index, OpenMobile.Graphics.OpenGL.NvVertexProgram pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribPointerv", ExactSpelling = true)]
            internal extern static void GetVertexAttribPointerv(UInt32 index, OpenMobile.Graphics.OpenGL.VertexAttribPointerParameter pname, [OutAttribute] IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribPointervARB", ExactSpelling = true)]
            internal extern static void GetVertexAttribPointervARB(UInt32 index, OpenMobile.Graphics.OpenGL.VertexAttribPointerParameterArb pname, [OutAttribute] IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVertexAttribPointervNV", ExactSpelling = true)]
            internal extern static void GetVertexAttribPointervNV(UInt32 index, OpenMobile.Graphics.OpenGL.NvVertexProgram pname, [OutAttribute] IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVideoi64vNV", ExactSpelling = true)]
            internal extern static unsafe void GetVideoi64vNV(UInt32 video_slot, OpenMobile.Graphics.OpenGL.NvPresentVideo pname, [OutAttribute] Int64* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVideoivNV", ExactSpelling = true)]
            internal extern static unsafe void GetVideoivNV(UInt32 video_slot, OpenMobile.Graphics.OpenGL.NvPresentVideo pname, [OutAttribute] Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVideoui64vNV", ExactSpelling = true)]
            internal extern static unsafe void GetVideoui64vNV(UInt32 video_slot, OpenMobile.Graphics.OpenGL.NvPresentVideo pname, [OutAttribute] UInt64* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glGetVideouivNV", ExactSpelling = true)]
            internal extern static unsafe void GetVideouivNV(UInt32 video_slot, OpenMobile.Graphics.OpenGL.NvPresentVideo pname, [OutAttribute] UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glHint", ExactSpelling = true)]
            internal extern static void Hint(OpenMobile.Graphics.OpenGL.HintTarget target, OpenMobile.Graphics.OpenGL.HintMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glHistogram", ExactSpelling = true)]
            internal extern static void Histogram(OpenMobile.Graphics.OpenGL.HistogramTarget target, Int32 width, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, bool sink);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glHistogramEXT", ExactSpelling = true)]
            internal extern static void HistogramEXT(OpenMobile.Graphics.OpenGL.ExtHistogram target, Int32 width, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, bool sink);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexd", ExactSpelling = true)]
            internal extern static void Indexd(Double c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexdv", ExactSpelling = true)]
            internal extern static unsafe void Indexdv(Double* c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexf", ExactSpelling = true)]
            internal extern static void Indexf(Single c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexFuncEXT", ExactSpelling = true)]
            internal extern static void IndexFuncEXT(OpenMobile.Graphics.OpenGL.ExtIndexFunc func, Single @ref);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexfv", ExactSpelling = true)]
            internal extern static unsafe void Indexfv(Single* c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexi", ExactSpelling = true)]
            internal extern static void Indexi(Int32 c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexiv", ExactSpelling = true)]
            internal extern static unsafe void Indexiv(Int32* c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexMask", ExactSpelling = true)]
            internal extern static void IndexMask(UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexMaterialEXT", ExactSpelling = true)]
            internal extern static void IndexMaterialEXT(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.ExtIndexMaterial mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexPointer", ExactSpelling = true)]
            internal extern static void IndexPointer(OpenMobile.Graphics.OpenGL.IndexPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexPointerEXT", ExactSpelling = true)]
            internal extern static void IndexPointerEXT(OpenMobile.Graphics.OpenGL.IndexPointerType type, Int32 stride, Int32 count, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexs", ExactSpelling = true)]
            internal extern static void Indexs(Int16 c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexsv", ExactSpelling = true)]
            internal extern static unsafe void Indexsv(Int16* c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexub", ExactSpelling = true)]
            internal extern static void Indexub(Byte c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIndexubv", ExactSpelling = true)]
            internal extern static unsafe void Indexubv(Byte* c);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glInitNames", ExactSpelling = true)]
            internal extern static void InitNames();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glInsertComponentEXT", ExactSpelling = true)]
            internal extern static void InsertComponentEXT(UInt32 res, UInt32 src, UInt32 num);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glInterleavedArrays", ExactSpelling = true)]
            internal extern static void InterleavedArrays(OpenMobile.Graphics.OpenGL.InterleavedArrayFormat format, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsBuffer", ExactSpelling = true)]
            internal extern static bool IsBuffer(UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsBufferARB", ExactSpelling = true)]
            internal extern static bool IsBufferARB(UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsEnabled", ExactSpelling = true)]
            internal extern static bool IsEnabled(OpenMobile.Graphics.OpenGL.EnableCap cap);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsEnabledi", ExactSpelling = true)]
            internal extern static bool IsEnabledi(OpenMobile.Graphics.OpenGL.IndexedEnableCap target, UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsEnabledIndexedEXT", ExactSpelling = true)]
            internal extern static bool IsEnabledIndexedEXT(OpenMobile.Graphics.OpenGL.ExtDrawBuffers2 target, UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsFenceNV", ExactSpelling = true)]
            internal extern static bool IsFenceNV(UInt32 fence);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsFramebuffer", ExactSpelling = true)]
            internal extern static bool IsFramebuffer(UInt32 framebuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsFramebufferEXT", ExactSpelling = true)]
            internal extern static bool IsFramebufferEXT(UInt32 framebuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsList", ExactSpelling = true)]
            internal extern static bool IsList(UInt32 list);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsObjectBufferATI", ExactSpelling = true)]
            internal extern static bool IsObjectBufferATI(UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsOcclusionQueryNV", ExactSpelling = true)]
            internal extern static bool IsOcclusionQueryNV(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsProgram", ExactSpelling = true)]
            internal extern static bool IsProgram(UInt32 program);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsProgramARB", ExactSpelling = true)]
            internal extern static bool IsProgramARB(UInt32 program);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsProgramNV", ExactSpelling = true)]
            internal extern static bool IsProgramNV(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsQuery", ExactSpelling = true)]
            internal extern static bool IsQuery(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsQueryARB", ExactSpelling = true)]
            internal extern static bool IsQueryARB(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsRenderbuffer", ExactSpelling = true)]
            internal extern static bool IsRenderbuffer(UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsRenderbufferEXT", ExactSpelling = true)]
            internal extern static bool IsRenderbufferEXT(UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsShader", ExactSpelling = true)]
            internal extern static bool IsShader(UInt32 shader);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsSync", ExactSpelling = true)]
            internal extern static bool IsSync(IntPtr sync);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsTexture", ExactSpelling = true)]
            internal extern static bool IsTexture(UInt32 texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsTextureEXT", ExactSpelling = true)]
            internal extern static bool IsTextureEXT(UInt32 texture);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsTransformFeedbackNV", ExactSpelling = true)]
            internal extern static bool IsTransformFeedbackNV(UInt32 id);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsVariantEnabledEXT", ExactSpelling = true)]
            internal extern static bool IsVariantEnabledEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader cap);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glIsVertexArray", ExactSpelling = true)]
            internal extern static bool IsVertexArray(UInt32 array);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightf", ExactSpelling = true)]
            internal extern static void Lightf(OpenMobile.Graphics.OpenGL.LightName light, OpenMobile.Graphics.OpenGL.LightParameter pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightfv", ExactSpelling = true)]
            internal extern static unsafe void Lightfv(OpenMobile.Graphics.OpenGL.LightName light, OpenMobile.Graphics.OpenGL.LightParameter pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLighti", ExactSpelling = true)]
            internal extern static void Lighti(OpenMobile.Graphics.OpenGL.LightName light, OpenMobile.Graphics.OpenGL.LightParameter pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightiv", ExactSpelling = true)]
            internal extern static unsafe void Lightiv(OpenMobile.Graphics.OpenGL.LightName light, OpenMobile.Graphics.OpenGL.LightParameter pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightModelf", ExactSpelling = true)]
            internal extern static void LightModelf(OpenMobile.Graphics.OpenGL.LightModelParameter pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightModelfv", ExactSpelling = true)]
            internal extern static unsafe void LightModelfv(OpenMobile.Graphics.OpenGL.LightModelParameter pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightModeli", ExactSpelling = true)]
            internal extern static void LightModeli(OpenMobile.Graphics.OpenGL.LightModelParameter pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLightModeliv", ExactSpelling = true)]
            internal extern static unsafe void LightModeliv(OpenMobile.Graphics.OpenGL.LightModelParameter pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLineStipple", ExactSpelling = true)]
            internal extern static void LineStipple(Int32 factor, UInt16 pattern);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLineWidth", ExactSpelling = true)]
            internal extern static void LineWidth(Single width);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLinkProgram", ExactSpelling = true)]
            internal extern static void LinkProgram(UInt32 program);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLinkProgramARB", ExactSpelling = true)]
            internal extern static void LinkProgramARB(UInt32 programObj);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glListBase", ExactSpelling = true)]
            internal extern static void ListBase(UInt32 @base);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadIdentity", ExactSpelling = true)]
            internal extern static void LoadIdentity();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadMatrixd", ExactSpelling = true)]
            internal extern static unsafe void LoadMatrixd(Double* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadMatrixf", ExactSpelling = true)]
            internal extern static unsafe void LoadMatrixf(Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadName", ExactSpelling = true)]
            internal extern static void LoadName(UInt32 name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadProgramNV", ExactSpelling = true)]
            internal extern static unsafe void LoadProgramNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 id, Int32 len, Byte* program);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadTransposeMatrixd", ExactSpelling = true)]
            internal extern static unsafe void LoadTransposeMatrixd(Double* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadTransposeMatrixdARB", ExactSpelling = true)]
            internal extern static unsafe void LoadTransposeMatrixdARB(Double* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadTransposeMatrixf", ExactSpelling = true)]
            internal extern static unsafe void LoadTransposeMatrixf(Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLoadTransposeMatrixfARB", ExactSpelling = true)]
            internal extern static unsafe void LoadTransposeMatrixfARB(Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLockArraysEXT", ExactSpelling = true)]
            internal extern static void LockArraysEXT(Int32 first, Int32 count);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glLogicOp", ExactSpelling = true)]
            internal extern static void LogicOp(OpenMobile.Graphics.OpenGL.LogicOp opcode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMap1d", ExactSpelling = true)]
            internal extern static unsafe void Map1d(OpenMobile.Graphics.OpenGL.MapTarget target, Double u1, Double u2, Int32 stride, Int32 order, Double* points);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMap1f", ExactSpelling = true)]
            internal extern static unsafe void Map1f(OpenMobile.Graphics.OpenGL.MapTarget target, Single u1, Single u2, Int32 stride, Int32 order, Single* points);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMap2d", ExactSpelling = true)]
            internal extern static unsafe void Map2d(OpenMobile.Graphics.OpenGL.MapTarget target, Double u1, Double u2, Int32 ustride, Int32 uorder, Double v1, Double v2, Int32 vstride, Int32 vorder, Double* points);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMap2f", ExactSpelling = true)]
            internal extern static unsafe void Map2f(OpenMobile.Graphics.OpenGL.MapTarget target, Single u1, Single u2, Int32 ustride, Int32 uorder, Single v1, Single v2, Int32 vstride, Int32 vorder, Single* points);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapBuffer", ExactSpelling = true)]
            internal extern static unsafe System.IntPtr MapBuffer(OpenMobile.Graphics.OpenGL.BufferTarget target, OpenMobile.Graphics.OpenGL.BufferAccess access);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapBufferARB", ExactSpelling = true)]
            internal extern static unsafe System.IntPtr MapBufferARB(OpenMobile.Graphics.OpenGL.BufferTargetArb target, OpenMobile.Graphics.OpenGL.ArbVertexBufferObject access);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapBufferRange", ExactSpelling = true)]
            internal extern static unsafe System.IntPtr MapBufferRange(OpenMobile.Graphics.OpenGL.BufferTarget target, IntPtr offset, IntPtr length, OpenMobile.Graphics.OpenGL.BufferAccessMask access);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapControlPointsNV", ExactSpelling = true)]
            internal extern static void MapControlPointsNV(OpenMobile.Graphics.OpenGL.NvEvaluators target, UInt32 index, OpenMobile.Graphics.OpenGL.NvEvaluators type, Int32 ustride, Int32 vstride, Int32 uorder, Int32 vorder, bool packed, IntPtr points);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapGrid1d", ExactSpelling = true)]
            internal extern static void MapGrid1d(Int32 un, Double u1, Double u2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapGrid1f", ExactSpelling = true)]
            internal extern static void MapGrid1f(Int32 un, Single u1, Single u2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapGrid2d", ExactSpelling = true)]
            internal extern static void MapGrid2d(Int32 un, Double u1, Double u2, Int32 vn, Double v1, Double v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapGrid2f", ExactSpelling = true)]
            internal extern static void MapGrid2f(Int32 un, Single u1, Single u2, Int32 vn, Single v1, Single v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapNamedBufferEXT", ExactSpelling = true)]
            internal extern static unsafe System.IntPtr MapNamedBufferEXT(UInt32 buffer, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess access);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapObjectBufferATI", ExactSpelling = true)]
            internal extern static unsafe System.IntPtr MapObjectBufferATI(UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapParameterfvNV", ExactSpelling = true)]
            internal extern static unsafe void MapParameterfvNV(OpenMobile.Graphics.OpenGL.NvEvaluators target, OpenMobile.Graphics.OpenGL.NvEvaluators pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMapParameterivNV", ExactSpelling = true)]
            internal extern static unsafe void MapParameterivNV(OpenMobile.Graphics.OpenGL.NvEvaluators target, OpenMobile.Graphics.OpenGL.NvEvaluators pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMaterialf", ExactSpelling = true)]
            internal extern static void Materialf(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.MaterialParameter pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMaterialfv", ExactSpelling = true)]
            internal extern static unsafe void Materialfv(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.MaterialParameter pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMateriali", ExactSpelling = true)]
            internal extern static void Materiali(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.MaterialParameter pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMaterialiv", ExactSpelling = true)]
            internal extern static unsafe void Materialiv(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.MaterialParameter pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixFrustumEXT", ExactSpelling = true)]
            internal extern static void MatrixFrustumEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Double left, Double right, Double bottom, Double top, Double zNear, Double zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixIndexPointerARB", ExactSpelling = true)]
            internal extern static void MatrixIndexPointerARB(Int32 size, OpenMobile.Graphics.OpenGL.ArbMatrixPalette type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixIndexubvARB", ExactSpelling = true)]
            internal extern static unsafe void MatrixIndexubvARB(Int32 size, Byte* indices);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixIndexuivARB", ExactSpelling = true)]
            internal extern static unsafe void MatrixIndexuivARB(Int32 size, UInt32* indices);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixIndexusvARB", ExactSpelling = true)]
            internal extern static unsafe void MatrixIndexusvARB(Int32 size, UInt16* indices);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixLoaddEXT", ExactSpelling = true)]
            internal extern static unsafe void MatrixLoaddEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Double* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixLoadfEXT", ExactSpelling = true)]
            internal extern static unsafe void MatrixLoadfEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixLoadIdentityEXT", ExactSpelling = true)]
            internal extern static void MatrixLoadIdentityEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixLoadTransposedEXT", ExactSpelling = true)]
            internal extern static unsafe void MatrixLoadTransposedEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Double* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixLoadTransposefEXT", ExactSpelling = true)]
            internal extern static unsafe void MatrixLoadTransposefEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixMode", ExactSpelling = true)]
            internal extern static void MatrixMode(OpenMobile.Graphics.OpenGL.MatrixMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixMultdEXT", ExactSpelling = true)]
            internal extern static unsafe void MatrixMultdEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Double* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixMultfEXT", ExactSpelling = true)]
            internal extern static unsafe void MatrixMultfEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixMultTransposedEXT", ExactSpelling = true)]
            internal extern static unsafe void MatrixMultTransposedEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Double* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixMultTransposefEXT", ExactSpelling = true)]
            internal extern static unsafe void MatrixMultTransposefEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixOrthoEXT", ExactSpelling = true)]
            internal extern static void MatrixOrthoEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Double left, Double right, Double bottom, Double top, Double zNear, Double zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixPopEXT", ExactSpelling = true)]
            internal extern static void MatrixPopEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixPushEXT", ExactSpelling = true)]
            internal extern static void MatrixPushEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixRotatedEXT", ExactSpelling = true)]
            internal extern static void MatrixRotatedEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Double angle, Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixRotatefEXT", ExactSpelling = true)]
            internal extern static void MatrixRotatefEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Single angle, Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixScaledEXT", ExactSpelling = true)]
            internal extern static void MatrixScaledEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixScalefEXT", ExactSpelling = true)]
            internal extern static void MatrixScalefEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixTranslatedEXT", ExactSpelling = true)]
            internal extern static void MatrixTranslatedEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMatrixTranslatefEXT", ExactSpelling = true)]
            internal extern static void MatrixTranslatefEXT(OpenMobile.Graphics.OpenGL.MatrixMode mode, Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMinmax", ExactSpelling = true)]
            internal extern static void Minmax(OpenMobile.Graphics.OpenGL.MinmaxTarget target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, bool sink);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMinmaxEXT", ExactSpelling = true)]
            internal extern static void MinmaxEXT(OpenMobile.Graphics.OpenGL.ExtHistogram target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, bool sink);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMinSampleShading", ExactSpelling = true)]
            internal extern static void MinSampleShading(Single value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiDrawArrays", ExactSpelling = true)]
            internal extern static unsafe void MultiDrawArrays(OpenMobile.Graphics.OpenGL.BeginMode mode, [OutAttribute] Int32* first, [OutAttribute] Int32* count, Int32 primcount);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiDrawArraysEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiDrawArraysEXT(OpenMobile.Graphics.OpenGL.BeginMode mode, [OutAttribute] Int32* first, [OutAttribute] Int32* count, Int32 primcount);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiDrawElements", ExactSpelling = true)]
            internal extern static unsafe void MultiDrawElements(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32* count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiDrawElementsBaseVertex", ExactSpelling = true)]
            internal extern static unsafe void MultiDrawElementsBaseVertex(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32* count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount, Int32* basevertex);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiDrawElementsEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiDrawElementsEXT(OpenMobile.Graphics.OpenGL.BeginMode mode, Int32* count, OpenMobile.Graphics.OpenGL.DrawElementsType type, IntPtr indices, Int32 primcount);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexBufferEXT", ExactSpelling = true)]
            internal extern static void MultiTexBufferEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1d", ExactSpelling = true)]
            internal extern static void MultiTexCoord1d(OpenMobile.Graphics.OpenGL.TextureUnit target, Double s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1dARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord1dARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Double s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1dv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord1dv(OpenMobile.Graphics.OpenGL.TextureUnit target, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1dvARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord1dvARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1f", ExactSpelling = true)]
            internal extern static void MultiTexCoord1f(OpenMobile.Graphics.OpenGL.TextureUnit target, Single s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1fARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord1fARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Single s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1fv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord1fv(OpenMobile.Graphics.OpenGL.TextureUnit target, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1fvARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord1fvARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1hNV", ExactSpelling = true)]
            internal extern static void MultiTexCoord1hNV(OpenMobile.Graphics.OpenGL.TextureUnit target, OpenMobile.Half s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1hvNV", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord1hvNV(OpenMobile.Graphics.OpenGL.TextureUnit target, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1i", ExactSpelling = true)]
            internal extern static void MultiTexCoord1i(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32 s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1iARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord1iARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32 s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1iv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord1iv(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1ivARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord1ivARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1s", ExactSpelling = true)]
            internal extern static void MultiTexCoord1s(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16 s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1sARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord1sARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16 s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1sv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord1sv(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord1svARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord1svARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2d", ExactSpelling = true)]
            internal extern static void MultiTexCoord2d(OpenMobile.Graphics.OpenGL.TextureUnit target, Double s, Double t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2dARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord2dARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Double s, Double t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2dv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord2dv(OpenMobile.Graphics.OpenGL.TextureUnit target, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2dvARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord2dvARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2f", ExactSpelling = true)]
            internal extern static void MultiTexCoord2f(OpenMobile.Graphics.OpenGL.TextureUnit target, Single s, Single t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2fARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord2fARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Single s, Single t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2fv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord2fv(OpenMobile.Graphics.OpenGL.TextureUnit target, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2fvARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord2fvARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2hNV", ExactSpelling = true)]
            internal extern static void MultiTexCoord2hNV(OpenMobile.Graphics.OpenGL.TextureUnit target, OpenMobile.Half s, OpenMobile.Half t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2hvNV", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord2hvNV(OpenMobile.Graphics.OpenGL.TextureUnit target, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2i", ExactSpelling = true)]
            internal extern static void MultiTexCoord2i(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32 s, Int32 t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2iARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord2iARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32 s, Int32 t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2iv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord2iv(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2ivARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord2ivARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2s", ExactSpelling = true)]
            internal extern static void MultiTexCoord2s(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16 s, Int16 t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2sARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord2sARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16 s, Int16 t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2sv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord2sv(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord2svARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord2svARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3d", ExactSpelling = true)]
            internal extern static void MultiTexCoord3d(OpenMobile.Graphics.OpenGL.TextureUnit target, Double s, Double t, Double r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3dARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord3dARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Double s, Double t, Double r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3dv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord3dv(OpenMobile.Graphics.OpenGL.TextureUnit target, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3dvARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord3dvARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3f", ExactSpelling = true)]
            internal extern static void MultiTexCoord3f(OpenMobile.Graphics.OpenGL.TextureUnit target, Single s, Single t, Single r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3fARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord3fARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Single s, Single t, Single r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3fv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord3fv(OpenMobile.Graphics.OpenGL.TextureUnit target, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3fvARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord3fvARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3hNV", ExactSpelling = true)]
            internal extern static void MultiTexCoord3hNV(OpenMobile.Graphics.OpenGL.TextureUnit target, OpenMobile.Half s, OpenMobile.Half t, OpenMobile.Half r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3hvNV", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord3hvNV(OpenMobile.Graphics.OpenGL.TextureUnit target, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3i", ExactSpelling = true)]
            internal extern static void MultiTexCoord3i(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32 s, Int32 t, Int32 r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3iARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord3iARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32 s, Int32 t, Int32 r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3iv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord3iv(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3ivARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord3ivARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3s", ExactSpelling = true)]
            internal extern static void MultiTexCoord3s(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16 s, Int16 t, Int16 r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3sARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord3sARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16 s, Int16 t, Int16 r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3sv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord3sv(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord3svARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord3svARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4d", ExactSpelling = true)]
            internal extern static void MultiTexCoord4d(OpenMobile.Graphics.OpenGL.TextureUnit target, Double s, Double t, Double r, Double q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4dARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord4dARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Double s, Double t, Double r, Double q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4dv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord4dv(OpenMobile.Graphics.OpenGL.TextureUnit target, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4dvARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord4dvARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4f", ExactSpelling = true)]
            internal extern static void MultiTexCoord4f(OpenMobile.Graphics.OpenGL.TextureUnit target, Single s, Single t, Single r, Single q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4fARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord4fARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Single s, Single t, Single r, Single q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4fv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord4fv(OpenMobile.Graphics.OpenGL.TextureUnit target, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4fvARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord4fvARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4hNV", ExactSpelling = true)]
            internal extern static void MultiTexCoord4hNV(OpenMobile.Graphics.OpenGL.TextureUnit target, OpenMobile.Half s, OpenMobile.Half t, OpenMobile.Half r, OpenMobile.Half q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4hvNV", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord4hvNV(OpenMobile.Graphics.OpenGL.TextureUnit target, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4i", ExactSpelling = true)]
            internal extern static void MultiTexCoord4i(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32 s, Int32 t, Int32 r, Int32 q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4iARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord4iARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32 s, Int32 t, Int32 r, Int32 q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4iv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord4iv(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4ivARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord4ivARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4s", ExactSpelling = true)]
            internal extern static void MultiTexCoord4s(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16 s, Int16 t, Int16 r, Int16 q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4sARB", ExactSpelling = true)]
            internal extern static void MultiTexCoord4sARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16 s, Int16 t, Int16 r, Int16 q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4sv", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord4sv(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoord4svARB", ExactSpelling = true)]
            internal extern static unsafe void MultiTexCoord4svARB(OpenMobile.Graphics.OpenGL.TextureUnit target, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexCoordPointerEXT", ExactSpelling = true)]
            internal extern static void MultiTexCoordPointerEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, Int32 size, OpenMobile.Graphics.OpenGL.TexCoordPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexEnvfEXT", ExactSpelling = true)]
            internal extern static void MultiTexEnvfEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexEnvfvEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiTexEnvfvEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexEnviEXT", ExactSpelling = true)]
            internal extern static void MultiTexEnviEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexEnvivEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiTexEnvivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexGendEXT", ExactSpelling = true)]
            internal extern static void MultiTexGendEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Double param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexGendvEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiTexGendvEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexGenfEXT", ExactSpelling = true)]
            internal extern static void MultiTexGenfEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexGenfvEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiTexGenfvEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexGeniEXT", ExactSpelling = true)]
            internal extern static void MultiTexGeniEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexGenivEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiTexGenivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexImage1DEXT", ExactSpelling = true)]
            internal extern static void MultiTexImage1DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 border, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexImage2DEXT", ExactSpelling = true)]
            internal extern static void MultiTexImage2DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 height, Int32 border, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexImage3DEXT", ExactSpelling = true)]
            internal extern static void MultiTexImage3DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 height, Int32 depth, Int32 border, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexParameterfEXT", ExactSpelling = true)]
            internal extern static void MultiTexParameterfEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiTexParameterfvEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexParameteriEXT", ExactSpelling = true)]
            internal extern static void MultiTexParameteriEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexParameterIivEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiTexParameterIivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexParameterIuivEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiTexParameterIuivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void MultiTexParameterivEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexRenderbufferEXT", ExactSpelling = true)]
            internal extern static void MultiTexRenderbufferEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexSubImage1DEXT", ExactSpelling = true)]
            internal extern static void MultiTexSubImage1DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexSubImage2DEXT", ExactSpelling = true)]
            internal extern static void MultiTexSubImage2DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultiTexSubImage3DEXT", ExactSpelling = true)]
            internal extern static void MultiTexSubImage3DEXT(OpenMobile.Graphics.OpenGL.TextureUnit texunit, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultMatrixd", ExactSpelling = true)]
            internal extern static unsafe void MultMatrixd(Double* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultMatrixf", ExactSpelling = true)]
            internal extern static unsafe void MultMatrixf(Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultTransposeMatrixd", ExactSpelling = true)]
            internal extern static unsafe void MultTransposeMatrixd(Double* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultTransposeMatrixdARB", ExactSpelling = true)]
            internal extern static unsafe void MultTransposeMatrixdARB(Double* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultTransposeMatrixf", ExactSpelling = true)]
            internal extern static unsafe void MultTransposeMatrixf(Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glMultTransposeMatrixfARB", ExactSpelling = true)]
            internal extern static unsafe void MultTransposeMatrixfARB(Single* m);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedBufferDataEXT", ExactSpelling = true)]
            internal extern static void NamedBufferDataEXT(UInt32 buffer, IntPtr size, IntPtr data, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess usage);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedBufferSubDataEXT", ExactSpelling = true)]
            internal extern static void NamedBufferSubDataEXT(UInt32 buffer, IntPtr offset, IntPtr size, IntPtr data);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedFramebufferRenderbufferEXT", ExactSpelling = true)]
            internal extern static void NamedFramebufferRenderbufferEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.RenderbufferTarget renderbuffertarget, UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedFramebufferTexture1DEXT", ExactSpelling = true)]
            internal extern static void NamedFramebufferTexture1DEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.TextureTarget textarget, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedFramebufferTexture2DEXT", ExactSpelling = true)]
            internal extern static void NamedFramebufferTexture2DEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.TextureTarget textarget, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedFramebufferTexture3DEXT", ExactSpelling = true)]
            internal extern static void NamedFramebufferTexture3DEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, OpenMobile.Graphics.OpenGL.TextureTarget textarget, UInt32 texture, Int32 level, Int32 zoffset);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedFramebufferTextureEXT", ExactSpelling = true)]
            internal extern static void NamedFramebufferTextureEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedFramebufferTextureFaceEXT", ExactSpelling = true)]
            internal extern static void NamedFramebufferTextureFaceEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level, OpenMobile.Graphics.OpenGL.TextureTarget face);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedFramebufferTextureLayerEXT", ExactSpelling = true)]
            internal extern static void NamedFramebufferTextureLayerEXT(UInt32 framebuffer, OpenMobile.Graphics.OpenGL.FramebufferAttachment attachment, UInt32 texture, Int32 level, Int32 layer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParameter4dEXT", ExactSpelling = true)]
            internal extern static void NamedProgramLocalParameter4dEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParameter4dvEXT", ExactSpelling = true)]
            internal extern static unsafe void NamedProgramLocalParameter4dvEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParameter4fEXT", ExactSpelling = true)]
            internal extern static void NamedProgramLocalParameter4fEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParameter4fvEXT", ExactSpelling = true)]
            internal extern static unsafe void NamedProgramLocalParameter4fvEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParameterI4iEXT", ExactSpelling = true)]
            internal extern static void NamedProgramLocalParameterI4iEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, Int32 x, Int32 y, Int32 z, Int32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParameterI4ivEXT", ExactSpelling = true)]
            internal extern static unsafe void NamedProgramLocalParameterI4ivEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParameterI4uiEXT", ExactSpelling = true)]
            internal extern static void NamedProgramLocalParameterI4uiEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, UInt32 x, UInt32 y, UInt32 z, UInt32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParameterI4uivEXT", ExactSpelling = true)]
            internal extern static unsafe void NamedProgramLocalParameterI4uivEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParameters4fvEXT", ExactSpelling = true)]
            internal extern static unsafe void NamedProgramLocalParameters4fvEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, Int32 count, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParametersI4ivEXT", ExactSpelling = true)]
            internal extern static unsafe void NamedProgramLocalParametersI4ivEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, Int32 count, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramLocalParametersI4uivEXT", ExactSpelling = true)]
            internal extern static unsafe void NamedProgramLocalParametersI4uivEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, UInt32 index, Int32 count, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedProgramStringEXT", ExactSpelling = true)]
            internal extern static void NamedProgramStringEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess target, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess format, Int32 len, IntPtr @string);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedRenderbufferStorageEXT", ExactSpelling = true)]
            internal extern static void NamedRenderbufferStorageEXT(UInt32 renderbuffer, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedRenderbufferStorageMultisampleCoverageEXT", ExactSpelling = true)]
            internal extern static void NamedRenderbufferStorageMultisampleCoverageEXT(UInt32 renderbuffer, Int32 coverageSamples, Int32 colorSamples, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNamedRenderbufferStorageMultisampleEXT", ExactSpelling = true)]
            internal extern static void NamedRenderbufferStorageMultisampleEXT(UInt32 renderbuffer, Int32 samples, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNewList", ExactSpelling = true)]
            internal extern static void NewList(UInt32 list, OpenMobile.Graphics.OpenGL.ListMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNewObjectBufferATI", ExactSpelling = true)]
            internal extern static Int32 NewObjectBufferATI(Int32 size, IntPtr pointer, OpenMobile.Graphics.OpenGL.AtiVertexArrayObject usage);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3b", ExactSpelling = true)]
            internal extern static void Normal3b(SByte nx, SByte ny, SByte nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3bv", ExactSpelling = true)]
            internal extern static unsafe void Normal3bv(SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3d", ExactSpelling = true)]
            internal extern static void Normal3d(Double nx, Double ny, Double nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3dv", ExactSpelling = true)]
            internal extern static unsafe void Normal3dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3f", ExactSpelling = true)]
            internal extern static void Normal3f(Single nx, Single ny, Single nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3fv", ExactSpelling = true)]
            internal extern static unsafe void Normal3fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3hNV", ExactSpelling = true)]
            internal extern static void Normal3hNV(OpenMobile.Half nx, OpenMobile.Half ny, OpenMobile.Half nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3hvNV", ExactSpelling = true)]
            internal extern static unsafe void Normal3hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3i", ExactSpelling = true)]
            internal extern static void Normal3i(Int32 nx, Int32 ny, Int32 nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3iv", ExactSpelling = true)]
            internal extern static unsafe void Normal3iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3s", ExactSpelling = true)]
            internal extern static void Normal3s(Int16 nx, Int16 ny, Int16 nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormal3sv", ExactSpelling = true)]
            internal extern static unsafe void Normal3sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalPointer", ExactSpelling = true)]
            internal extern static void NormalPointer(OpenMobile.Graphics.OpenGL.NormalPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalPointerEXT", ExactSpelling = true)]
            internal extern static void NormalPointerEXT(OpenMobile.Graphics.OpenGL.NormalPointerType type, Int32 stride, Int32 count, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalPointervINTEL", ExactSpelling = true)]
            internal extern static void NormalPointervINTEL(OpenMobile.Graphics.OpenGL.NormalPointerType type, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalStream3bATI", ExactSpelling = true)]
            internal extern static void NormalStream3bATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, SByte nx, SByte ny, SByte nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalStream3bvATI", ExactSpelling = true)]
            internal extern static unsafe void NormalStream3bvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, SByte* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalStream3dATI", ExactSpelling = true)]
            internal extern static void NormalStream3dATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Double nx, Double ny, Double nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalStream3dvATI", ExactSpelling = true)]
            internal extern static unsafe void NormalStream3dvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Double* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalStream3fATI", ExactSpelling = true)]
            internal extern static void NormalStream3fATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Single nx, Single ny, Single nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalStream3fvATI", ExactSpelling = true)]
            internal extern static unsafe void NormalStream3fvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Single* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalStream3iATI", ExactSpelling = true)]
            internal extern static void NormalStream3iATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int32 nx, Int32 ny, Int32 nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalStream3ivATI", ExactSpelling = true)]
            internal extern static unsafe void NormalStream3ivATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int32* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalStream3sATI", ExactSpelling = true)]
            internal extern static void NormalStream3sATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int16 nx, Int16 ny, Int16 nz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glNormalStream3svATI", ExactSpelling = true)]
            internal extern static unsafe void NormalStream3svATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int16* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glOrtho", ExactSpelling = true)]
            internal extern static void Ortho(Double left, Double right, Double bottom, Double top, Double zNear, Double zFar);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPassTexCoordATI", ExactSpelling = true)]
            internal extern static void PassTexCoordATI(UInt32 dst, UInt32 coord, OpenMobile.Graphics.OpenGL.AtiFragmentShader swizzle);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPassThrough", ExactSpelling = true)]
            internal extern static void PassThrough(Single token);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPauseTransformFeedbackNV", ExactSpelling = true)]
            internal extern static void PauseTransformFeedbackNV();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelDataRangeNV", ExactSpelling = true)]
            internal extern static void PixelDataRangeNV(OpenMobile.Graphics.OpenGL.NvPixelDataRange target, Int32 length, [OutAttribute] IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelMapfv", ExactSpelling = true)]
            internal extern static unsafe void PixelMapfv(OpenMobile.Graphics.OpenGL.PixelMap map, Int32 mapsize, Single* values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelMapuiv", ExactSpelling = true)]
            internal extern static unsafe void PixelMapuiv(OpenMobile.Graphics.OpenGL.PixelMap map, Int32 mapsize, UInt32* values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelMapusv", ExactSpelling = true)]
            internal extern static unsafe void PixelMapusv(OpenMobile.Graphics.OpenGL.PixelMap map, Int32 mapsize, UInt16* values);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelStoref", ExactSpelling = true)]
            internal extern static void PixelStoref(OpenMobile.Graphics.OpenGL.PixelStoreParameter pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelStorei", ExactSpelling = true)]
            internal extern static void PixelStorei(OpenMobile.Graphics.OpenGL.PixelStoreParameter pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelTransferf", ExactSpelling = true)]
            internal extern static void PixelTransferf(OpenMobile.Graphics.OpenGL.PixelTransferParameter pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelTransferi", ExactSpelling = true)]
            internal extern static void PixelTransferi(OpenMobile.Graphics.OpenGL.PixelTransferParameter pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelTransformParameterfEXT", ExactSpelling = true)]
            internal extern static void PixelTransformParameterfEXT(OpenMobile.Graphics.OpenGL.ExtPixelTransform target, OpenMobile.Graphics.OpenGL.ExtPixelTransform pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelTransformParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void PixelTransformParameterfvEXT(OpenMobile.Graphics.OpenGL.ExtPixelTransform target, OpenMobile.Graphics.OpenGL.ExtPixelTransform pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelTransformParameteriEXT", ExactSpelling = true)]
            internal extern static void PixelTransformParameteriEXT(OpenMobile.Graphics.OpenGL.ExtPixelTransform target, OpenMobile.Graphics.OpenGL.ExtPixelTransform pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelTransformParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void PixelTransformParameterivEXT(OpenMobile.Graphics.OpenGL.ExtPixelTransform target, OpenMobile.Graphics.OpenGL.ExtPixelTransform pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPixelZoom", ExactSpelling = true)]
            internal extern static void PixelZoom(Single xfactor, Single yfactor);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPNTrianglesfATI", ExactSpelling = true)]
            internal extern static void PNTrianglesfATI(OpenMobile.Graphics.OpenGL.AtiPnTriangles pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPNTrianglesiATI", ExactSpelling = true)]
            internal extern static void PNTrianglesiATI(OpenMobile.Graphics.OpenGL.AtiPnTriangles pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterf", ExactSpelling = true)]
            internal extern static void PointParameterf(OpenMobile.Graphics.OpenGL.PointParameterName pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterfARB", ExactSpelling = true)]
            internal extern static void PointParameterfARB(OpenMobile.Graphics.OpenGL.ArbPointParameters pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterfEXT", ExactSpelling = true)]
            internal extern static void PointParameterfEXT(OpenMobile.Graphics.OpenGL.ExtPointParameters pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterfv", ExactSpelling = true)]
            internal extern static unsafe void PointParameterfv(OpenMobile.Graphics.OpenGL.PointParameterName pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterfvARB", ExactSpelling = true)]
            internal extern static unsafe void PointParameterfvARB(OpenMobile.Graphics.OpenGL.ArbPointParameters pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void PointParameterfvEXT(OpenMobile.Graphics.OpenGL.ExtPointParameters pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameteri", ExactSpelling = true)]
            internal extern static void PointParameteri(OpenMobile.Graphics.OpenGL.PointParameterName pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameteriNV", ExactSpelling = true)]
            internal extern static void PointParameteriNV(OpenMobile.Graphics.OpenGL.NvPointSprite pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameteriv", ExactSpelling = true)]
            internal extern static unsafe void PointParameteriv(OpenMobile.Graphics.OpenGL.PointParameterName pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointParameterivNV", ExactSpelling = true)]
            internal extern static unsafe void PointParameterivNV(OpenMobile.Graphics.OpenGL.NvPointSprite pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPointSize", ExactSpelling = true)]
            internal extern static void PointSize(Single size);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPolygonMode", ExactSpelling = true)]
            internal extern static void PolygonMode(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.PolygonMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPolygonOffset", ExactSpelling = true)]
            internal extern static void PolygonOffset(Single factor, Single units);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPolygonOffsetEXT", ExactSpelling = true)]
            internal extern static void PolygonOffsetEXT(Single factor, Single bias);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPolygonStipple", ExactSpelling = true)]
            internal extern static unsafe void PolygonStipple(Byte* mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPopAttrib", ExactSpelling = true)]
            internal extern static void PopAttrib();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPopClientAttrib", ExactSpelling = true)]
            internal extern static void PopClientAttrib();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPopMatrix", ExactSpelling = true)]
            internal extern static void PopMatrix();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPopName", ExactSpelling = true)]
            internal extern static void PopName();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPresentFrameDualFillNV", ExactSpelling = true)]
            internal extern static void PresentFrameDualFillNV(UInt32 video_slot, UInt64 minPresentTime, UInt32 beginPresentTimeId, UInt32 presentDurationId, OpenMobile.Graphics.OpenGL.NvPresentVideo type, OpenMobile.Graphics.OpenGL.NvPresentVideo target0, UInt32 fill0, OpenMobile.Graphics.OpenGL.NvPresentVideo target1, UInt32 fill1, OpenMobile.Graphics.OpenGL.NvPresentVideo target2, UInt32 fill2, OpenMobile.Graphics.OpenGL.NvPresentVideo target3, UInt32 fill3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPresentFrameKeyedNV", ExactSpelling = true)]
            internal extern static void PresentFrameKeyedNV(UInt32 video_slot, UInt64 minPresentTime, UInt32 beginPresentTimeId, UInt32 presentDurationId, OpenMobile.Graphics.OpenGL.NvPresentVideo type, OpenMobile.Graphics.OpenGL.NvPresentVideo target0, UInt32 fill0, UInt32 key0, OpenMobile.Graphics.OpenGL.NvPresentVideo target1, UInt32 fill1, UInt32 key1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPrimitiveRestartIndex", ExactSpelling = true)]
            internal extern static void PrimitiveRestartIndex(UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPrimitiveRestartIndexNV", ExactSpelling = true)]
            internal extern static void PrimitiveRestartIndexNV(UInt32 index);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPrimitiveRestartNV", ExactSpelling = true)]
            internal extern static void PrimitiveRestartNV();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPrioritizeTextures", ExactSpelling = true)]
            internal extern static unsafe void PrioritizeTextures(Int32 n, UInt32* textures, Single* priorities);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPrioritizeTexturesEXT", ExactSpelling = true)]
            internal extern static unsafe void PrioritizeTexturesEXT(Int32 n, UInt32* textures, Single* priorities);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramBufferParametersfvNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramBufferParametersfvNV(OpenMobile.Graphics.OpenGL.NvParameterBufferObject target, UInt32 buffer, UInt32 index, Int32 count, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramBufferParametersIivNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramBufferParametersIivNV(OpenMobile.Graphics.OpenGL.NvParameterBufferObject target, UInt32 buffer, UInt32 index, Int32 count, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramBufferParametersIuivNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramBufferParametersIuivNV(OpenMobile.Graphics.OpenGL.NvParameterBufferObject target, UInt32 buffer, UInt32 index, Int32 count, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParameter4dARB", ExactSpelling = true)]
            internal extern static void ProgramEnvParameter4dARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParameter4dvARB", ExactSpelling = true)]
            internal extern static unsafe void ProgramEnvParameter4dvARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParameter4fARB", ExactSpelling = true)]
            internal extern static void ProgramEnvParameter4fARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParameter4fvARB", ExactSpelling = true)]
            internal extern static unsafe void ProgramEnvParameter4fvARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParameterI4iNV", ExactSpelling = true)]
            internal extern static void ProgramEnvParameterI4iNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, Int32 x, Int32 y, Int32 z, Int32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParameterI4ivNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramEnvParameterI4ivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParameterI4uiNV", ExactSpelling = true)]
            internal extern static void ProgramEnvParameterI4uiNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, UInt32 x, UInt32 y, UInt32 z, UInt32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParameterI4uivNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramEnvParameterI4uivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParameters4fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramEnvParameters4fvEXT(OpenMobile.Graphics.OpenGL.ExtGpuProgramParameters target, UInt32 index, Int32 count, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParametersI4ivNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramEnvParametersI4ivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, Int32 count, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramEnvParametersI4uivNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramEnvParametersI4uivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, Int32 count, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParameter4dARB", ExactSpelling = true)]
            internal extern static void ProgramLocalParameter4dARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParameter4dvARB", ExactSpelling = true)]
            internal extern static unsafe void ProgramLocalParameter4dvARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParameter4fARB", ExactSpelling = true)]
            internal extern static void ProgramLocalParameter4fARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParameter4fvARB", ExactSpelling = true)]
            internal extern static unsafe void ProgramLocalParameter4fvARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParameterI4iNV", ExactSpelling = true)]
            internal extern static void ProgramLocalParameterI4iNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, Int32 x, Int32 y, Int32 z, Int32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParameterI4ivNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramLocalParameterI4ivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParameterI4uiNV", ExactSpelling = true)]
            internal extern static void ProgramLocalParameterI4uiNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, UInt32 x, UInt32 y, UInt32 z, UInt32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParameterI4uivNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramLocalParameterI4uivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParameters4fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramLocalParameters4fvEXT(OpenMobile.Graphics.OpenGL.ExtGpuProgramParameters target, UInt32 index, Int32 count, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParametersI4ivNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramLocalParametersI4ivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, Int32 count, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramLocalParametersI4uivNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramLocalParametersI4uivNV(OpenMobile.Graphics.OpenGL.NvGpuProgram4 target, UInt32 index, Int32 count, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramNamedParameter4dNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramNamedParameter4dNV(UInt32 id, Int32 len, Byte* name, Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramNamedParameter4dvNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramNamedParameter4dvNV(UInt32 id, Int32 len, Byte* name, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramNamedParameter4fNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramNamedParameter4fNV(UInt32 id, Int32 len, Byte* name, Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramNamedParameter4fvNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramNamedParameter4fvNV(UInt32 id, Int32 len, Byte* name, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramParameter4dNV", ExactSpelling = true)]
            internal extern static void ProgramParameter4dNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramParameter4dvNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramParameter4dvNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramParameter4fNV", ExactSpelling = true)]
            internal extern static void ProgramParameter4fNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramParameter4fvNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramParameter4fvNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramParameteriARB", ExactSpelling = true)]
            internal extern static void ProgramParameteriARB(UInt32 program, OpenMobile.Graphics.OpenGL.ArbGeometryShader4 pname, Int32 value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramParameteriEXT", ExactSpelling = true)]
            internal extern static void ProgramParameteriEXT(UInt32 program, OpenMobile.Graphics.OpenGL.ExtGeometryShader4 pname, Int32 value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramParameters4dvNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramParameters4dvNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, UInt32 count, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramParameters4fvNV", ExactSpelling = true)]
            internal extern static unsafe void ProgramParameters4fvNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 index, UInt32 count, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramStringARB", ExactSpelling = true)]
            internal extern static void ProgramStringARB(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, OpenMobile.Graphics.OpenGL.ArbVertexProgram format, Int32 len, IntPtr @string);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform1fEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform1fEXT(UInt32 program, Int32 location, Single v0);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform1fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform1fvEXT(UInt32 program, Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform1iEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform1iEXT(UInt32 program, Int32 location, Int32 v0);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform1ivEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform1ivEXT(UInt32 program, Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform1uiEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform1uiEXT(UInt32 program, Int32 location, UInt32 v0);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform1uivEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform1uivEXT(UInt32 program, Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform2fEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform2fEXT(UInt32 program, Int32 location, Single v0, Single v1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform2fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform2fvEXT(UInt32 program, Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform2iEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform2iEXT(UInt32 program, Int32 location, Int32 v0, Int32 v1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform2ivEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform2ivEXT(UInt32 program, Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform2uiEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform2uiEXT(UInt32 program, Int32 location, UInt32 v0, UInt32 v1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform2uivEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform2uivEXT(UInt32 program, Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform3fEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform3fEXT(UInt32 program, Int32 location, Single v0, Single v1, Single v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform3fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform3fvEXT(UInt32 program, Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform3iEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform3iEXT(UInt32 program, Int32 location, Int32 v0, Int32 v1, Int32 v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform3ivEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform3ivEXT(UInt32 program, Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform3uiEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform3uiEXT(UInt32 program, Int32 location, UInt32 v0, UInt32 v1, UInt32 v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform3uivEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform3uivEXT(UInt32 program, Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform4fEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform4fEXT(UInt32 program, Int32 location, Single v0, Single v1, Single v2, Single v3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform4fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform4fvEXT(UInt32 program, Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform4iEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform4iEXT(UInt32 program, Int32 location, Int32 v0, Int32 v1, Int32 v2, Int32 v3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform4ivEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform4ivEXT(UInt32 program, Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform4uiEXT", ExactSpelling = true)]
            internal extern static void ProgramUniform4uiEXT(UInt32 program, Int32 location, UInt32 v0, UInt32 v1, UInt32 v2, UInt32 v3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniform4uivEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniform4uivEXT(UInt32 program, Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniformMatrix2fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniformMatrix2fvEXT(UInt32 program, Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniformMatrix2x3fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniformMatrix2x3fvEXT(UInt32 program, Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniformMatrix2x4fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniformMatrix2x4fvEXT(UInt32 program, Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniformMatrix3fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniformMatrix3fvEXT(UInt32 program, Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniformMatrix3x2fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniformMatrix3x2fvEXT(UInt32 program, Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniformMatrix3x4fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniformMatrix3x4fvEXT(UInt32 program, Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniformMatrix4fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniformMatrix4fvEXT(UInt32 program, Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniformMatrix4x2fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniformMatrix4x2fvEXT(UInt32 program, Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramUniformMatrix4x3fvEXT", ExactSpelling = true)]
            internal extern static unsafe void ProgramUniformMatrix4x3fvEXT(UInt32 program, Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProgramVertexLimitNV", ExactSpelling = true)]
            internal extern static void ProgramVertexLimitNV(OpenMobile.Graphics.OpenGL.NvGeometryProgram4 target, Int32 limit);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProvokingVertex", ExactSpelling = true)]
            internal extern static void ProvokingVertex(OpenMobile.Graphics.OpenGL.ProvokingVertexMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glProvokingVertexEXT", ExactSpelling = true)]
            internal extern static void ProvokingVertexEXT(OpenMobile.Graphics.OpenGL.ExtProvokingVertex mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPushClientAttrib", ExactSpelling = true)]
            internal extern static void PushClientAttrib(OpenMobile.Graphics.OpenGL.ClientAttribMask mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPushClientAttribDefaultEXT", ExactSpelling = true)]
            internal extern static void PushClientAttribDefaultEXT(OpenMobile.Graphics.OpenGL.ClientAttribMask mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPushMatrix", ExactSpelling = true)]
            internal extern static void PushMatrix();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glPushName", ExactSpelling = true)]
            internal extern static void PushName(UInt32 name);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos2d", ExactSpelling = true)]
            internal extern static void RasterPos2d(Double x, Double y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos2dv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos2dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos2f", ExactSpelling = true)]
            internal extern static void RasterPos2f(Single x, Single y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos2fv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos2fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos2i", ExactSpelling = true)]
            internal extern static void RasterPos2i(Int32 x, Int32 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos2iv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos2iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos2s", ExactSpelling = true)]
            internal extern static void RasterPos2s(Int16 x, Int16 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos2sv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos2sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos3d", ExactSpelling = true)]
            internal extern static void RasterPos3d(Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos3dv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos3dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos3f", ExactSpelling = true)]
            internal extern static void RasterPos3f(Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos3fv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos3fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos3i", ExactSpelling = true)]
            internal extern static void RasterPos3i(Int32 x, Int32 y, Int32 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos3iv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos3iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos3s", ExactSpelling = true)]
            internal extern static void RasterPos3s(Int16 x, Int16 y, Int16 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos3sv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos3sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos4d", ExactSpelling = true)]
            internal extern static void RasterPos4d(Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos4dv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos4dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos4f", ExactSpelling = true)]
            internal extern static void RasterPos4f(Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos4fv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos4fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos4i", ExactSpelling = true)]
            internal extern static void RasterPos4i(Int32 x, Int32 y, Int32 z, Int32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos4iv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos4iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos4s", ExactSpelling = true)]
            internal extern static void RasterPos4s(Int16 x, Int16 y, Int16 z, Int16 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRasterPos4sv", ExactSpelling = true)]
            internal extern static unsafe void RasterPos4sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glReadBuffer", ExactSpelling = true)]
            internal extern static void ReadBuffer(OpenMobile.Graphics.OpenGL.ReadBufferMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glReadPixels", ExactSpelling = true)]
            internal extern static void ReadPixels(Int32 x, Int32 y, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, [OutAttribute] IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRectd", ExactSpelling = true)]
            internal extern static void Rectd(Double x1, Double y1, Double x2, Double y2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRectdv", ExactSpelling = true)]
            internal extern static unsafe void Rectdv(Double* v1, Double* v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRectf", ExactSpelling = true)]
            internal extern static void Rectf(Single x1, Single y1, Single x2, Single y2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRectfv", ExactSpelling = true)]
            internal extern static unsafe void Rectfv(Single* v1, Single* v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRecti", ExactSpelling = true)]
            internal extern static void Recti(Int32 x1, Int32 y1, Int32 x2, Int32 y2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRectiv", ExactSpelling = true)]
            internal extern static unsafe void Rectiv(Int32* v1, Int32* v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRects", ExactSpelling = true)]
            internal extern static void Rects(Int16 x1, Int16 y1, Int16 x2, Int16 y2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRectsv", ExactSpelling = true)]
            internal extern static unsafe void Rectsv(Int16* v1, Int16* v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRenderbufferStorage", ExactSpelling = true)]
            internal extern static void RenderbufferStorage(OpenMobile.Graphics.OpenGL.RenderbufferTarget target, OpenMobile.Graphics.OpenGL.RenderbufferStorage internalformat, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRenderbufferStorageEXT", ExactSpelling = true)]
            internal extern static void RenderbufferStorageEXT(OpenMobile.Graphics.OpenGL.RenderbufferTarget target, OpenMobile.Graphics.OpenGL.RenderbufferStorage internalformat, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRenderbufferStorageMultisample", ExactSpelling = true)]
            internal extern static void RenderbufferStorageMultisample(OpenMobile.Graphics.OpenGL.RenderbufferTarget target, Int32 samples, OpenMobile.Graphics.OpenGL.RenderbufferStorage internalformat, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRenderbufferStorageMultisampleCoverageNV", ExactSpelling = true)]
            internal extern static void RenderbufferStorageMultisampleCoverageNV(OpenMobile.Graphics.OpenGL.RenderbufferTarget target, Int32 coverageSamples, Int32 colorSamples, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRenderbufferStorageMultisampleEXT", ExactSpelling = true)]
            internal extern static void RenderbufferStorageMultisampleEXT(OpenMobile.Graphics.OpenGL.ExtFramebufferMultisample target, Int32 samples, OpenMobile.Graphics.OpenGL.ExtFramebufferMultisample internalformat, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRenderMode", ExactSpelling = true)]
            internal extern static Int32 RenderMode(OpenMobile.Graphics.OpenGL.RenderingMode mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRequestResidentProgramsNV", ExactSpelling = true)]
            internal extern static unsafe void RequestResidentProgramsNV(Int32 n, UInt32* programs);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glResetHistogram", ExactSpelling = true)]
            internal extern static void ResetHistogram(OpenMobile.Graphics.OpenGL.HistogramTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glResetHistogramEXT", ExactSpelling = true)]
            internal extern static void ResetHistogramEXT(OpenMobile.Graphics.OpenGL.ExtHistogram target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glResetMinmax", ExactSpelling = true)]
            internal extern static void ResetMinmax(OpenMobile.Graphics.OpenGL.MinmaxTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glResetMinmaxEXT", ExactSpelling = true)]
            internal extern static void ResetMinmaxEXT(OpenMobile.Graphics.OpenGL.ExtHistogram target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glResumeTransformFeedbackNV", ExactSpelling = true)]
            internal extern static void ResumeTransformFeedbackNV();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRotated", ExactSpelling = true)]
            internal extern static void Rotated(Double angle, Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glRotatef", ExactSpelling = true)]
            internal extern static void Rotatef(Single angle, Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSampleCoverage", ExactSpelling = true)]
            internal extern static void SampleCoverage(Single value, bool invert);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSampleCoverageARB", ExactSpelling = true)]
            internal extern static void SampleCoverageARB(Single value, bool invert);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSampleMapATI", ExactSpelling = true)]
            internal extern static void SampleMapATI(UInt32 dst, UInt32 interp, OpenMobile.Graphics.OpenGL.AtiFragmentShader swizzle);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSampleMaskEXT", ExactSpelling = true)]
            internal extern static void SampleMaskEXT(Single value, bool invert);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSampleMaski", ExactSpelling = true)]
            internal extern static void SampleMaski(UInt32 index, UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSampleMaskIndexedNV", ExactSpelling = true)]
            internal extern static void SampleMaskIndexedNV(UInt32 index, UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSamplePatternEXT", ExactSpelling = true)]
            internal extern static void SamplePatternEXT(OpenMobile.Graphics.OpenGL.ExtMultisample pattern);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glScaled", ExactSpelling = true)]
            internal extern static void Scaled(Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glScalef", ExactSpelling = true)]
            internal extern static void Scalef(Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glScissor", ExactSpelling = true)]
            internal extern static void Scissor(Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3b", ExactSpelling = true)]
            internal extern static void SecondaryColor3b(SByte red, SByte green, SByte blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3bEXT", ExactSpelling = true)]
            internal extern static void SecondaryColor3bEXT(SByte red, SByte green, SByte blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3bv", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3bv(SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3bvEXT", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3bvEXT(SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3d", ExactSpelling = true)]
            internal extern static void SecondaryColor3d(Double red, Double green, Double blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3dEXT", ExactSpelling = true)]
            internal extern static void SecondaryColor3dEXT(Double red, Double green, Double blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3dv", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3dvEXT", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3dvEXT(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3f", ExactSpelling = true)]
            internal extern static void SecondaryColor3f(Single red, Single green, Single blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3fEXT", ExactSpelling = true)]
            internal extern static void SecondaryColor3fEXT(Single red, Single green, Single blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3fv", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3fvEXT", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3fvEXT(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3hNV", ExactSpelling = true)]
            internal extern static void SecondaryColor3hNV(OpenMobile.Half red, OpenMobile.Half green, OpenMobile.Half blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3hvNV", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3i", ExactSpelling = true)]
            internal extern static void SecondaryColor3i(Int32 red, Int32 green, Int32 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3iEXT", ExactSpelling = true)]
            internal extern static void SecondaryColor3iEXT(Int32 red, Int32 green, Int32 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3iv", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3ivEXT", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3ivEXT(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3s", ExactSpelling = true)]
            internal extern static void SecondaryColor3s(Int16 red, Int16 green, Int16 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3sEXT", ExactSpelling = true)]
            internal extern static void SecondaryColor3sEXT(Int16 red, Int16 green, Int16 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3sv", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3svEXT", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3svEXT(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3ub", ExactSpelling = true)]
            internal extern static void SecondaryColor3ub(Byte red, Byte green, Byte blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3ubEXT", ExactSpelling = true)]
            internal extern static void SecondaryColor3ubEXT(Byte red, Byte green, Byte blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3ubv", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3ubv(Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3ubvEXT", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3ubvEXT(Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3ui", ExactSpelling = true)]
            internal extern static void SecondaryColor3ui(UInt32 red, UInt32 green, UInt32 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3uiEXT", ExactSpelling = true)]
            internal extern static void SecondaryColor3uiEXT(UInt32 red, UInt32 green, UInt32 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3uiv", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3uiv(UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3uivEXT", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3uivEXT(UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3us", ExactSpelling = true)]
            internal extern static void SecondaryColor3us(UInt16 red, UInt16 green, UInt16 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3usEXT", ExactSpelling = true)]
            internal extern static void SecondaryColor3usEXT(UInt16 red, UInt16 green, UInt16 blue);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3usv", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3usv(UInt16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColor3usvEXT", ExactSpelling = true)]
            internal extern static unsafe void SecondaryColor3usvEXT(UInt16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColorPointer", ExactSpelling = true)]
            internal extern static void SecondaryColorPointer(Int32 size, OpenMobile.Graphics.OpenGL.ColorPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSecondaryColorPointerEXT", ExactSpelling = true)]
            internal extern static void SecondaryColorPointerEXT(Int32 size, OpenMobile.Graphics.OpenGL.ColorPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSelectBuffer", ExactSpelling = true)]
            internal extern static unsafe void SelectBuffer(Int32 size, [OutAttribute] UInt32* buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSelectPerfMonitorCountersAMD", ExactSpelling = true)]
            internal extern static unsafe void SelectPerfMonitorCountersAMD(UInt32 monitor, bool enable, UInt32 group, Int32 numCounters, [OutAttribute] UInt32* counterList);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSeparableFilter2D", ExactSpelling = true)]
            internal extern static void SeparableFilter2D(OpenMobile.Graphics.OpenGL.SeparableTarget target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr row, IntPtr column);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSeparableFilter2DEXT", ExactSpelling = true)]
            internal extern static void SeparableFilter2DEXT(OpenMobile.Graphics.OpenGL.ExtConvolution target, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr row, IntPtr column);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSetFenceNV", ExactSpelling = true)]
            internal extern static void SetFenceNV(UInt32 fence, OpenMobile.Graphics.OpenGL.NvFence condition);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSetFragmentShaderConstantATI", ExactSpelling = true)]
            internal extern static unsafe void SetFragmentShaderConstantATI(UInt32 dst, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSetInvariantEXT", ExactSpelling = true)]
            internal extern static void SetInvariantEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader type, IntPtr addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSetLocalConstantEXT", ExactSpelling = true)]
            internal extern static void SetLocalConstantEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader type, IntPtr addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glShadeModel", ExactSpelling = true)]
            internal extern static void ShadeModel(OpenMobile.Graphics.OpenGL.ShadingModel mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glShaderOp1EXT", ExactSpelling = true)]
            internal extern static void ShaderOp1EXT(OpenMobile.Graphics.OpenGL.ExtVertexShader op, UInt32 res, UInt32 arg1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glShaderOp2EXT", ExactSpelling = true)]
            internal extern static void ShaderOp2EXT(OpenMobile.Graphics.OpenGL.ExtVertexShader op, UInt32 res, UInt32 arg1, UInt32 arg2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glShaderOp3EXT", ExactSpelling = true)]
            internal extern static void ShaderOp3EXT(OpenMobile.Graphics.OpenGL.ExtVertexShader op, UInt32 res, UInt32 arg1, UInt32 arg2, UInt32 arg3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glShaderSource", ExactSpelling = true)]
            internal extern static unsafe void ShaderSource(UInt32 shader, Int32 count, String[] @string, Int32* length);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glShaderSourceARB", ExactSpelling = true)]
            internal extern static unsafe void ShaderSourceARB(UInt32 shaderObj, Int32 count, String[] @string, Int32* length);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilClearTagEXT", ExactSpelling = true)]
            internal extern static void StencilClearTagEXT(Int32 stencilTagBits, UInt32 stencilClearTag);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilFunc", ExactSpelling = true)]
            internal extern static void StencilFunc(OpenMobile.Graphics.OpenGL.StencilFunction func, Int32 @ref, UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilFuncSeparate", ExactSpelling = true)]
            internal extern static void StencilFuncSeparate(OpenMobile.Graphics.OpenGL.StencilFace face, OpenMobile.Graphics.OpenGL.StencilFunction func, Int32 @ref, UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilFuncSeparateATI", ExactSpelling = true)]
            internal extern static void StencilFuncSeparateATI(OpenMobile.Graphics.OpenGL.StencilFunction frontfunc, OpenMobile.Graphics.OpenGL.StencilFunction backfunc, Int32 @ref, UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilMask", ExactSpelling = true)]
            internal extern static void StencilMask(UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilMaskSeparate", ExactSpelling = true)]
            internal extern static void StencilMaskSeparate(OpenMobile.Graphics.OpenGL.StencilFace face, UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilOp", ExactSpelling = true)]
            internal extern static void StencilOp(OpenMobile.Graphics.OpenGL.StencilOp fail, OpenMobile.Graphics.OpenGL.StencilOp zfail, OpenMobile.Graphics.OpenGL.StencilOp zpass);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilOpSeparate", ExactSpelling = true)]
            internal extern static void StencilOpSeparate(OpenMobile.Graphics.OpenGL.StencilFace face, OpenMobile.Graphics.OpenGL.StencilOp sfail, OpenMobile.Graphics.OpenGL.StencilOp dpfail, OpenMobile.Graphics.OpenGL.StencilOp dppass);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStencilOpSeparateATI", ExactSpelling = true)]
            internal extern static void StencilOpSeparateATI(OpenMobile.Graphics.OpenGL.AtiSeparateStencil face, OpenMobile.Graphics.OpenGL.StencilOp sfail, OpenMobile.Graphics.OpenGL.StencilOp dpfail, OpenMobile.Graphics.OpenGL.StencilOp dppass);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glStringMarkerGREMEDY", ExactSpelling = true)]
            internal extern static void StringMarkerGREMEDY(Int32 len, IntPtr @string);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glSwizzleEXT", ExactSpelling = true)]
            internal extern static void SwizzleEXT(UInt32 res, UInt32 @in, OpenMobile.Graphics.OpenGL.ExtVertexShader outX, OpenMobile.Graphics.OpenGL.ExtVertexShader outY, OpenMobile.Graphics.OpenGL.ExtVertexShader outZ, OpenMobile.Graphics.OpenGL.ExtVertexShader outW);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangent3bEXT", ExactSpelling = true)]
            internal extern static void Tangent3bEXT(SByte tx, SByte ty, SByte tz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangent3bvEXT", ExactSpelling = true)]
            internal extern static unsafe void Tangent3bvEXT(SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangent3dEXT", ExactSpelling = true)]
            internal extern static void Tangent3dEXT(Double tx, Double ty, Double tz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangent3dvEXT", ExactSpelling = true)]
            internal extern static unsafe void Tangent3dvEXT(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangent3fEXT", ExactSpelling = true)]
            internal extern static void Tangent3fEXT(Single tx, Single ty, Single tz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangent3fvEXT", ExactSpelling = true)]
            internal extern static unsafe void Tangent3fvEXT(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangent3iEXT", ExactSpelling = true)]
            internal extern static void Tangent3iEXT(Int32 tx, Int32 ty, Int32 tz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangent3ivEXT", ExactSpelling = true)]
            internal extern static unsafe void Tangent3ivEXT(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangent3sEXT", ExactSpelling = true)]
            internal extern static void Tangent3sEXT(Int16 tx, Int16 ty, Int16 tz);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangent3svEXT", ExactSpelling = true)]
            internal extern static unsafe void Tangent3svEXT(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTangentPointerEXT", ExactSpelling = true)]
            internal extern static void TangentPointerEXT(OpenMobile.Graphics.OpenGL.NormalPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTbufferMask3DFX", ExactSpelling = true)]
            internal extern static void TbufferMask3DFX(UInt32 mask);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTessellationFactorAMD", ExactSpelling = true)]
            internal extern static void TessellationFactorAMD(Single factor);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTessellationModeAMD", ExactSpelling = true)]
            internal extern static void TessellationModeAMD(OpenMobile.Graphics.OpenGL.AmdVertexShaderTesselator mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTestFenceNV", ExactSpelling = true)]
            internal extern static bool TestFenceNV(UInt32 fence);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexBuffer", ExactSpelling = true)]
            internal extern static void TexBuffer(OpenMobile.Graphics.OpenGL.TextureBufferTarget target, OpenMobile.Graphics.OpenGL.SizedInternalFormat internalformat, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexBufferARB", ExactSpelling = true)]
            internal extern static void TexBufferARB(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.ArbTextureBufferObject internalformat, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexBufferEXT", ExactSpelling = true)]
            internal extern static void TexBufferEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.ExtTextureBufferObject internalformat, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexBumpParameterfvATI", ExactSpelling = true)]
            internal extern static unsafe void TexBumpParameterfvATI(OpenMobile.Graphics.OpenGL.AtiEnvmapBumpmap pname, Single* param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexBumpParameterivATI", ExactSpelling = true)]
            internal extern static unsafe void TexBumpParameterivATI(OpenMobile.Graphics.OpenGL.AtiEnvmapBumpmap pname, Int32* param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord1d", ExactSpelling = true)]
            internal extern static void TexCoord1d(Double s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord1dv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord1dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord1f", ExactSpelling = true)]
            internal extern static void TexCoord1f(Single s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord1fv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord1fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord1hNV", ExactSpelling = true)]
            internal extern static void TexCoord1hNV(OpenMobile.Half s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord1hvNV", ExactSpelling = true)]
            internal extern static unsafe void TexCoord1hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord1i", ExactSpelling = true)]
            internal extern static void TexCoord1i(Int32 s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord1iv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord1iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord1s", ExactSpelling = true)]
            internal extern static void TexCoord1s(Int16 s);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord1sv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord1sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord2d", ExactSpelling = true)]
            internal extern static void TexCoord2d(Double s, Double t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord2dv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord2dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord2f", ExactSpelling = true)]
            internal extern static void TexCoord2f(Single s, Single t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord2fv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord2fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord2hNV", ExactSpelling = true)]
            internal extern static void TexCoord2hNV(OpenMobile.Half s, OpenMobile.Half t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord2hvNV", ExactSpelling = true)]
            internal extern static unsafe void TexCoord2hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord2i", ExactSpelling = true)]
            internal extern static void TexCoord2i(Int32 s, Int32 t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord2iv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord2iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord2s", ExactSpelling = true)]
            internal extern static void TexCoord2s(Int16 s, Int16 t);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord2sv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord2sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord3d", ExactSpelling = true)]
            internal extern static void TexCoord3d(Double s, Double t, Double r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord3dv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord3dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord3f", ExactSpelling = true)]
            internal extern static void TexCoord3f(Single s, Single t, Single r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord3fv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord3fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord3hNV", ExactSpelling = true)]
            internal extern static void TexCoord3hNV(OpenMobile.Half s, OpenMobile.Half t, OpenMobile.Half r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord3hvNV", ExactSpelling = true)]
            internal extern static unsafe void TexCoord3hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord3i", ExactSpelling = true)]
            internal extern static void TexCoord3i(Int32 s, Int32 t, Int32 r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord3iv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord3iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord3s", ExactSpelling = true)]
            internal extern static void TexCoord3s(Int16 s, Int16 t, Int16 r);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord3sv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord3sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord4d", ExactSpelling = true)]
            internal extern static void TexCoord4d(Double s, Double t, Double r, Double q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord4dv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord4dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord4f", ExactSpelling = true)]
            internal extern static void TexCoord4f(Single s, Single t, Single r, Single q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord4fv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord4fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord4hNV", ExactSpelling = true)]
            internal extern static void TexCoord4hNV(OpenMobile.Half s, OpenMobile.Half t, OpenMobile.Half r, OpenMobile.Half q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord4hvNV", ExactSpelling = true)]
            internal extern static unsafe void TexCoord4hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord4i", ExactSpelling = true)]
            internal extern static void TexCoord4i(Int32 s, Int32 t, Int32 r, Int32 q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord4iv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord4iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord4s", ExactSpelling = true)]
            internal extern static void TexCoord4s(Int16 s, Int16 t, Int16 r, Int16 q);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoord4sv", ExactSpelling = true)]
            internal extern static unsafe void TexCoord4sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoordPointer", ExactSpelling = true)]
            internal extern static void TexCoordPointer(Int32 size, OpenMobile.Graphics.OpenGL.TexCoordPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoordPointerEXT", ExactSpelling = true)]
            internal extern static void TexCoordPointerEXT(Int32 size, OpenMobile.Graphics.OpenGL.TexCoordPointerType type, Int32 stride, Int32 count, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexCoordPointervINTEL", ExactSpelling = true)]
            internal extern static void TexCoordPointervINTEL(Int32 size, OpenMobile.Graphics.OpenGL.VertexPointerType type, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnvf", ExactSpelling = true)]
            internal extern static void TexEnvf(OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnvfv", ExactSpelling = true)]
            internal extern static unsafe void TexEnvfv(OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnvi", ExactSpelling = true)]
            internal extern static void TexEnvi(OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexEnviv", ExactSpelling = true)]
            internal extern static unsafe void TexEnviv(OpenMobile.Graphics.OpenGL.TextureEnvTarget target, OpenMobile.Graphics.OpenGL.TextureEnvParameter pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGend", ExactSpelling = true)]
            internal extern static void TexGend(OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Double param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGendv", ExactSpelling = true)]
            internal extern static unsafe void TexGendv(OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Double* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGenf", ExactSpelling = true)]
            internal extern static void TexGenf(OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGenfv", ExactSpelling = true)]
            internal extern static unsafe void TexGenfv(OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGeni", ExactSpelling = true)]
            internal extern static void TexGeni(OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexGeniv", ExactSpelling = true)]
            internal extern static unsafe void TexGeniv(OpenMobile.Graphics.OpenGL.TextureCoordName coord, OpenMobile.Graphics.OpenGL.TextureGenParameter pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexImage1D", ExactSpelling = true)]
            internal extern static void TexImage1D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 border, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexImage2D", ExactSpelling = true)]
            internal extern static void TexImage2D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, Int32 border, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexImage2DMultisample", ExactSpelling = true)]
            internal extern static void TexImage2DMultisample(OpenMobile.Graphics.OpenGL.TextureTargetMultisample target, Int32 samples, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, bool fixedsamplelocations);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexImage3D", ExactSpelling = true)]
            internal extern static void TexImage3D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, Int32 depth, Int32 border, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexImage3DEXT", ExactSpelling = true)]
            internal extern static void TexImage3DEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, Int32 depth, Int32 border, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexImage3DMultisample", ExactSpelling = true)]
            internal extern static void TexImage3DMultisample(OpenMobile.Graphics.OpenGL.TextureTargetMultisample target, Int32 samples, OpenMobile.Graphics.OpenGL.PixelInternalFormat internalformat, Int32 width, Int32 height, Int32 depth, bool fixedsamplelocations);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterf", ExactSpelling = true)]
            internal extern static void TexParameterf(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterfv", ExactSpelling = true)]
            internal extern static unsafe void TexParameterfv(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameteri", ExactSpelling = true)]
            internal extern static void TexParameteri(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterIiv", ExactSpelling = true)]
            internal extern static unsafe void TexParameterIiv(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterIivEXT", ExactSpelling = true)]
            internal extern static unsafe void TexParameterIivEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterIuiv", ExactSpelling = true)]
            internal extern static unsafe void TexParameterIuiv(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameterIuivEXT", ExactSpelling = true)]
            internal extern static unsafe void TexParameterIuivEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexParameteriv", ExactSpelling = true)]
            internal extern static unsafe void TexParameteriv(OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexRenderbufferNV", ExactSpelling = true)]
            internal extern static void TexRenderbufferNV(OpenMobile.Graphics.OpenGL.TextureTarget target, UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexSubImage1D", ExactSpelling = true)]
            internal extern static void TexSubImage1D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexSubImage1DEXT", ExactSpelling = true)]
            internal extern static void TexSubImage1DEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexSubImage2D", ExactSpelling = true)]
            internal extern static void TexSubImage2D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexSubImage2DEXT", ExactSpelling = true)]
            internal extern static void TexSubImage2DEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexSubImage3D", ExactSpelling = true)]
            internal extern static void TexSubImage3D(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTexSubImage3DEXT", ExactSpelling = true)]
            internal extern static void TexSubImage3DEXT(OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureBufferEXT", ExactSpelling = true)]
            internal extern static void TextureBufferEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureImage1DEXT", ExactSpelling = true)]
            internal extern static void TextureImage1DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 border, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureImage2DEXT", ExactSpelling = true)]
            internal extern static void TextureImage2DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 height, Int32 border, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureImage3DEXT", ExactSpelling = true)]
            internal extern static void TextureImage3DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, OpenMobile.Graphics.OpenGL.ExtDirectStateAccess internalformat, Int32 width, Int32 height, Int32 depth, Int32 border, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureLightEXT", ExactSpelling = true)]
            internal extern static void TextureLightEXT(OpenMobile.Graphics.OpenGL.ExtLightTexture pname);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureMaterialEXT", ExactSpelling = true)]
            internal extern static void TextureMaterialEXT(OpenMobile.Graphics.OpenGL.MaterialFace face, OpenMobile.Graphics.OpenGL.MaterialParameter mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureNormalEXT", ExactSpelling = true)]
            internal extern static void TextureNormalEXT(OpenMobile.Graphics.OpenGL.ExtTexturePerturbNormal mode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureParameterfEXT", ExactSpelling = true)]
            internal extern static void TextureParameterfEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureParameterfvEXT", ExactSpelling = true)]
            internal extern static unsafe void TextureParameterfvEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Single* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureParameteriEXT", ExactSpelling = true)]
            internal extern static void TextureParameteriEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureParameterIivEXT", ExactSpelling = true)]
            internal extern static unsafe void TextureParameterIivEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureParameterIuivEXT", ExactSpelling = true)]
            internal extern static unsafe void TextureParameterIuivEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, UInt32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureParameterivEXT", ExactSpelling = true)]
            internal extern static unsafe void TextureParameterivEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, OpenMobile.Graphics.OpenGL.TextureParameterName pname, Int32* @params);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureRenderbufferEXT", ExactSpelling = true)]
            internal extern static void TextureRenderbufferEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, UInt32 renderbuffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureSubImage1DEXT", ExactSpelling = true)]
            internal extern static void TextureSubImage1DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 width, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureSubImage2DEXT", ExactSpelling = true)]
            internal extern static void TextureSubImage2DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTextureSubImage3DEXT", ExactSpelling = true)]
            internal extern static void TextureSubImage3DEXT(UInt32 texture, OpenMobile.Graphics.OpenGL.TextureTarget target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 zoffset, Int32 width, Int32 height, Int32 depth, OpenMobile.Graphics.OpenGL.PixelFormat format, OpenMobile.Graphics.OpenGL.PixelType type, IntPtr pixels);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTrackMatrixNV", ExactSpelling = true)]
            internal extern static void TrackMatrixNV(OpenMobile.Graphics.OpenGL.AssemblyProgramTargetArb target, UInt32 address, OpenMobile.Graphics.OpenGL.NvVertexProgram matrix, OpenMobile.Graphics.OpenGL.NvVertexProgram transform);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTransformFeedbackAttribsNV", ExactSpelling = true)]
            internal extern static unsafe void TransformFeedbackAttribsNV(UInt32 count, Int32* attribs, OpenMobile.Graphics.OpenGL.NvTransformFeedback bufferMode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTransformFeedbackVaryings", ExactSpelling = true)]
            internal extern static void TransformFeedbackVaryings(UInt32 program, Int32 count, String[] varyings, OpenMobile.Graphics.OpenGL.TransformFeedbackMode bufferMode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTransformFeedbackVaryingsEXT", ExactSpelling = true)]
            internal extern static void TransformFeedbackVaryingsEXT(UInt32 program, Int32 count, String[] varyings, OpenMobile.Graphics.OpenGL.ExtTransformFeedback bufferMode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTransformFeedbackVaryingsNV", ExactSpelling = true)]
            internal extern static void TransformFeedbackVaryingsNV(UInt32 program, Int32 count, String[] varyings, OpenMobile.Graphics.OpenGL.NvTransformFeedback bufferMode);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTranslated", ExactSpelling = true)]
            internal extern static void Translated(Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glTranslatef", ExactSpelling = true)]
            internal extern static void Translatef(Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1f", ExactSpelling = true)]
            internal extern static void Uniform1f(Int32 location, Single v0);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1fARB", ExactSpelling = true)]
            internal extern static void Uniform1fARB(Int32 location, Single v0);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1fv", ExactSpelling = true)]
            internal extern static unsafe void Uniform1fv(Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1fvARB", ExactSpelling = true)]
            internal extern static unsafe void Uniform1fvARB(Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1i", ExactSpelling = true)]
            internal extern static void Uniform1i(Int32 location, Int32 v0);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1iARB", ExactSpelling = true)]
            internal extern static void Uniform1iARB(Int32 location, Int32 v0);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1iv", ExactSpelling = true)]
            internal extern static unsafe void Uniform1iv(Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1ivARB", ExactSpelling = true)]
            internal extern static unsafe void Uniform1ivARB(Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1ui", ExactSpelling = true)]
            internal extern static void Uniform1ui(Int32 location, UInt32 v0);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1uiEXT", ExactSpelling = true)]
            internal extern static void Uniform1uiEXT(Int32 location, UInt32 v0);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1uiv", ExactSpelling = true)]
            internal extern static unsafe void Uniform1uiv(Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform1uivEXT", ExactSpelling = true)]
            internal extern static unsafe void Uniform1uivEXT(Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2f", ExactSpelling = true)]
            internal extern static void Uniform2f(Int32 location, Single v0, Single v1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2fARB", ExactSpelling = true)]
            internal extern static void Uniform2fARB(Int32 location, Single v0, Single v1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2fv", ExactSpelling = true)]
            internal extern static unsafe void Uniform2fv(Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2fvARB", ExactSpelling = true)]
            internal extern static unsafe void Uniform2fvARB(Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2i", ExactSpelling = true)]
            internal extern static void Uniform2i(Int32 location, Int32 v0, Int32 v1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2iARB", ExactSpelling = true)]
            internal extern static void Uniform2iARB(Int32 location, Int32 v0, Int32 v1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2iv", ExactSpelling = true)]
            internal extern static unsafe void Uniform2iv(Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2ivARB", ExactSpelling = true)]
            internal extern static unsafe void Uniform2ivARB(Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2ui", ExactSpelling = true)]
            internal extern static void Uniform2ui(Int32 location, UInt32 v0, UInt32 v1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2uiEXT", ExactSpelling = true)]
            internal extern static void Uniform2uiEXT(Int32 location, UInt32 v0, UInt32 v1);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2uiv", ExactSpelling = true)]
            internal extern static unsafe void Uniform2uiv(Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform2uivEXT", ExactSpelling = true)]
            internal extern static unsafe void Uniform2uivEXT(Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3f", ExactSpelling = true)]
            internal extern static void Uniform3f(Int32 location, Single v0, Single v1, Single v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3fARB", ExactSpelling = true)]
            internal extern static void Uniform3fARB(Int32 location, Single v0, Single v1, Single v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3fv", ExactSpelling = true)]
            internal extern static unsafe void Uniform3fv(Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3fvARB", ExactSpelling = true)]
            internal extern static unsafe void Uniform3fvARB(Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3i", ExactSpelling = true)]
            internal extern static void Uniform3i(Int32 location, Int32 v0, Int32 v1, Int32 v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3iARB", ExactSpelling = true)]
            internal extern static void Uniform3iARB(Int32 location, Int32 v0, Int32 v1, Int32 v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3iv", ExactSpelling = true)]
            internal extern static unsafe void Uniform3iv(Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3ivARB", ExactSpelling = true)]
            internal extern static unsafe void Uniform3ivARB(Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3ui", ExactSpelling = true)]
            internal extern static void Uniform3ui(Int32 location, UInt32 v0, UInt32 v1, UInt32 v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3uiEXT", ExactSpelling = true)]
            internal extern static void Uniform3uiEXT(Int32 location, UInt32 v0, UInt32 v1, UInt32 v2);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3uiv", ExactSpelling = true)]
            internal extern static unsafe void Uniform3uiv(Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform3uivEXT", ExactSpelling = true)]
            internal extern static unsafe void Uniform3uivEXT(Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4f", ExactSpelling = true)]
            internal extern static void Uniform4f(Int32 location, Single v0, Single v1, Single v2, Single v3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4fARB", ExactSpelling = true)]
            internal extern static void Uniform4fARB(Int32 location, Single v0, Single v1, Single v2, Single v3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4fv", ExactSpelling = true)]
            internal extern static unsafe void Uniform4fv(Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4fvARB", ExactSpelling = true)]
            internal extern static unsafe void Uniform4fvARB(Int32 location, Int32 count, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4i", ExactSpelling = true)]
            internal extern static void Uniform4i(Int32 location, Int32 v0, Int32 v1, Int32 v2, Int32 v3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4iARB", ExactSpelling = true)]
            internal extern static void Uniform4iARB(Int32 location, Int32 v0, Int32 v1, Int32 v2, Int32 v3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4iv", ExactSpelling = true)]
            internal extern static unsafe void Uniform4iv(Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4ivARB", ExactSpelling = true)]
            internal extern static unsafe void Uniform4ivARB(Int32 location, Int32 count, Int32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4ui", ExactSpelling = true)]
            internal extern static void Uniform4ui(Int32 location, UInt32 v0, UInt32 v1, UInt32 v2, UInt32 v3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4uiEXT", ExactSpelling = true)]
            internal extern static void Uniform4uiEXT(Int32 location, UInt32 v0, UInt32 v1, UInt32 v2, UInt32 v3);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4uiv", ExactSpelling = true)]
            internal extern static unsafe void Uniform4uiv(Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniform4uivEXT", ExactSpelling = true)]
            internal extern static unsafe void Uniform4uivEXT(Int32 location, Int32 count, UInt32* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformBlockBinding", ExactSpelling = true)]
            internal extern static void UniformBlockBinding(UInt32 program, UInt32 uniformBlockIndex, UInt32 uniformBlockBinding);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformBufferEXT", ExactSpelling = true)]
            internal extern static void UniformBufferEXT(UInt32 program, Int32 location, UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix2fv", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix2fv(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix2fvARB", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix2fvARB(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix2x3fv", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix2x3fv(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix2x4fv", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix2x4fv(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix3fv", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix3fv(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix3fvARB", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix3fvARB(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix3x2fv", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix3x2fv(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix3x4fv", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix3x4fv(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix4fv", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix4fv(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix4fvARB", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix4fvARB(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix4x2fv", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix4x2fv(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUniformMatrix4x3fv", ExactSpelling = true)]
            internal extern static unsafe void UniformMatrix4x3fv(Int32 location, Int32 count, bool transpose, Single* value);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUnlockArraysEXT", ExactSpelling = true)]
            internal extern static void UnlockArraysEXT();
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUnmapBuffer", ExactSpelling = true)]
            internal extern static bool UnmapBuffer(OpenMobile.Graphics.OpenGL.BufferTarget target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUnmapBufferARB", ExactSpelling = true)]
            internal extern static bool UnmapBufferARB(OpenMobile.Graphics.OpenGL.BufferTargetArb target);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUnmapNamedBufferEXT", ExactSpelling = true)]
            internal extern static bool UnmapNamedBufferEXT(UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUnmapObjectBufferATI", ExactSpelling = true)]
            internal extern static void UnmapObjectBufferATI(UInt32 buffer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUpdateObjectBufferATI", ExactSpelling = true)]
            internal extern static void UpdateObjectBufferATI(UInt32 buffer, UInt32 offset, Int32 size, IntPtr pointer, OpenMobile.Graphics.OpenGL.AtiVertexArrayObject preserve);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUseProgram", ExactSpelling = true)]
            internal extern static void UseProgram(UInt32 program);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glUseProgramObjectARB", ExactSpelling = true)]
            internal extern static void UseProgramObjectARB(UInt32 programObj);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glValidateProgram", ExactSpelling = true)]
            internal extern static void ValidateProgram(UInt32 program);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glValidateProgramARB", ExactSpelling = true)]
            internal extern static void ValidateProgramARB(UInt32 programObj);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVariantArrayObjectATI", ExactSpelling = true)]
            internal extern static void VariantArrayObjectATI(UInt32 id, OpenMobile.Graphics.OpenGL.AtiVertexArrayObject type, Int32 stride, UInt32 buffer, UInt32 offset);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVariantbvEXT", ExactSpelling = true)]
            internal extern static unsafe void VariantbvEXT(UInt32 id, SByte* addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVariantdvEXT", ExactSpelling = true)]
            internal extern static unsafe void VariantdvEXT(UInt32 id, Double* addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVariantfvEXT", ExactSpelling = true)]
            internal extern static unsafe void VariantfvEXT(UInt32 id, Single* addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVariantivEXT", ExactSpelling = true)]
            internal extern static unsafe void VariantivEXT(UInt32 id, Int32* addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVariantPointerEXT", ExactSpelling = true)]
            internal extern static void VariantPointerEXT(UInt32 id, OpenMobile.Graphics.OpenGL.ExtVertexShader type, UInt32 stride, IntPtr addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVariantsvEXT", ExactSpelling = true)]
            internal extern static unsafe void VariantsvEXT(UInt32 id, Int16* addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVariantubvEXT", ExactSpelling = true)]
            internal extern static unsafe void VariantubvEXT(UInt32 id, Byte* addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVariantuivEXT", ExactSpelling = true)]
            internal extern static unsafe void VariantuivEXT(UInt32 id, UInt32* addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVariantusvEXT", ExactSpelling = true)]
            internal extern static unsafe void VariantusvEXT(UInt32 id, UInt16* addr);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex2d", ExactSpelling = true)]
            internal extern static void Vertex2d(Double x, Double y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex2dv", ExactSpelling = true)]
            internal extern static unsafe void Vertex2dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex2f", ExactSpelling = true)]
            internal extern static void Vertex2f(Single x, Single y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex2fv", ExactSpelling = true)]
            internal extern static unsafe void Vertex2fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex2hNV", ExactSpelling = true)]
            internal extern static void Vertex2hNV(OpenMobile.Half x, OpenMobile.Half y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex2hvNV", ExactSpelling = true)]
            internal extern static unsafe void Vertex2hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex2i", ExactSpelling = true)]
            internal extern static void Vertex2i(Int32 x, Int32 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex2iv", ExactSpelling = true)]
            internal extern static unsafe void Vertex2iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex2s", ExactSpelling = true)]
            internal extern static void Vertex2s(Int16 x, Int16 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex2sv", ExactSpelling = true)]
            internal extern static unsafe void Vertex2sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex3d", ExactSpelling = true)]
            internal extern static void Vertex3d(Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex3dv", ExactSpelling = true)]
            internal extern static unsafe void Vertex3dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex3f", ExactSpelling = true)]
            internal extern static void Vertex3f(Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex3fv", ExactSpelling = true)]
            internal extern static unsafe void Vertex3fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex3hNV", ExactSpelling = true)]
            internal extern static void Vertex3hNV(OpenMobile.Half x, OpenMobile.Half y, OpenMobile.Half z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex3hvNV", ExactSpelling = true)]
            internal extern static unsafe void Vertex3hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex3i", ExactSpelling = true)]
            internal extern static void Vertex3i(Int32 x, Int32 y, Int32 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex3iv", ExactSpelling = true)]
            internal extern static unsafe void Vertex3iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex3s", ExactSpelling = true)]
            internal extern static void Vertex3s(Int16 x, Int16 y, Int16 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex3sv", ExactSpelling = true)]
            internal extern static unsafe void Vertex3sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex4d", ExactSpelling = true)]
            internal extern static void Vertex4d(Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex4dv", ExactSpelling = true)]
            internal extern static unsafe void Vertex4dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex4f", ExactSpelling = true)]
            internal extern static void Vertex4f(Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex4fv", ExactSpelling = true)]
            internal extern static unsafe void Vertex4fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex4hNV", ExactSpelling = true)]
            internal extern static void Vertex4hNV(OpenMobile.Half x, OpenMobile.Half y, OpenMobile.Half z, OpenMobile.Half w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex4hvNV", ExactSpelling = true)]
            internal extern static unsafe void Vertex4hvNV(OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex4i", ExactSpelling = true)]
            internal extern static void Vertex4i(Int32 x, Int32 y, Int32 z, Int32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex4iv", ExactSpelling = true)]
            internal extern static unsafe void Vertex4iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex4s", ExactSpelling = true)]
            internal extern static void Vertex4s(Int16 x, Int16 y, Int16 z, Int16 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertex4sv", ExactSpelling = true)]
            internal extern static unsafe void Vertex4sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexArrayRangeNV", ExactSpelling = true)]
            internal extern static void VertexArrayRangeNV(Int32 length, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1d", ExactSpelling = true)]
            internal extern static void VertexAttrib1d(UInt32 index, Double x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1dARB", ExactSpelling = true)]
            internal extern static void VertexAttrib1dARB(UInt32 index, Double x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1dNV", ExactSpelling = true)]
            internal extern static void VertexAttrib1dNV(UInt32 index, Double x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1dv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib1dv(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1dvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib1dvARB(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1dvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib1dvNV(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1f", ExactSpelling = true)]
            internal extern static void VertexAttrib1f(UInt32 index, Single x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1fARB", ExactSpelling = true)]
            internal extern static void VertexAttrib1fARB(UInt32 index, Single x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1fNV", ExactSpelling = true)]
            internal extern static void VertexAttrib1fNV(UInt32 index, Single x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1fv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib1fv(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1fvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib1fvARB(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1fvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib1fvNV(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1hNV", ExactSpelling = true)]
            internal extern static void VertexAttrib1hNV(UInt32 index, OpenMobile.Half x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1hvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib1hvNV(UInt32 index, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1s", ExactSpelling = true)]
            internal extern static void VertexAttrib1s(UInt32 index, Int16 x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1sARB", ExactSpelling = true)]
            internal extern static void VertexAttrib1sARB(UInt32 index, Int16 x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1sNV", ExactSpelling = true)]
            internal extern static void VertexAttrib1sNV(UInt32 index, Int16 x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1sv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib1sv(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1svARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib1svARB(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib1svNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib1svNV(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2d", ExactSpelling = true)]
            internal extern static void VertexAttrib2d(UInt32 index, Double x, Double y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2dARB", ExactSpelling = true)]
            internal extern static void VertexAttrib2dARB(UInt32 index, Double x, Double y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2dNV", ExactSpelling = true)]
            internal extern static void VertexAttrib2dNV(UInt32 index, Double x, Double y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2dv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib2dv(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2dvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib2dvARB(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2dvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib2dvNV(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2f", ExactSpelling = true)]
            internal extern static void VertexAttrib2f(UInt32 index, Single x, Single y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2fARB", ExactSpelling = true)]
            internal extern static void VertexAttrib2fARB(UInt32 index, Single x, Single y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2fNV", ExactSpelling = true)]
            internal extern static void VertexAttrib2fNV(UInt32 index, Single x, Single y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2fv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib2fv(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2fvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib2fvARB(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2fvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib2fvNV(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2hNV", ExactSpelling = true)]
            internal extern static void VertexAttrib2hNV(UInt32 index, OpenMobile.Half x, OpenMobile.Half y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2hvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib2hvNV(UInt32 index, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2s", ExactSpelling = true)]
            internal extern static void VertexAttrib2s(UInt32 index, Int16 x, Int16 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2sARB", ExactSpelling = true)]
            internal extern static void VertexAttrib2sARB(UInt32 index, Int16 x, Int16 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2sNV", ExactSpelling = true)]
            internal extern static void VertexAttrib2sNV(UInt32 index, Int16 x, Int16 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2sv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib2sv(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2svARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib2svARB(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib2svNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib2svNV(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3d", ExactSpelling = true)]
            internal extern static void VertexAttrib3d(UInt32 index, Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3dARB", ExactSpelling = true)]
            internal extern static void VertexAttrib3dARB(UInt32 index, Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3dNV", ExactSpelling = true)]
            internal extern static void VertexAttrib3dNV(UInt32 index, Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3dv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib3dv(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3dvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib3dvARB(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3dvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib3dvNV(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3f", ExactSpelling = true)]
            internal extern static void VertexAttrib3f(UInt32 index, Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3fARB", ExactSpelling = true)]
            internal extern static void VertexAttrib3fARB(UInt32 index, Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3fNV", ExactSpelling = true)]
            internal extern static void VertexAttrib3fNV(UInt32 index, Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3fv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib3fv(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3fvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib3fvARB(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3fvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib3fvNV(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3hNV", ExactSpelling = true)]
            internal extern static void VertexAttrib3hNV(UInt32 index, OpenMobile.Half x, OpenMobile.Half y, OpenMobile.Half z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3hvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib3hvNV(UInt32 index, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3s", ExactSpelling = true)]
            internal extern static void VertexAttrib3s(UInt32 index, Int16 x, Int16 y, Int16 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3sARB", ExactSpelling = true)]
            internal extern static void VertexAttrib3sARB(UInt32 index, Int16 x, Int16 y, Int16 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3sNV", ExactSpelling = true)]
            internal extern static void VertexAttrib3sNV(UInt32 index, Int16 x, Int16 y, Int16 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3sv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib3sv(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3svARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib3svARB(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib3svNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib3svNV(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4bv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4bv(UInt32 index, SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4bvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4bvARB(UInt32 index, SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4d", ExactSpelling = true)]
            internal extern static void VertexAttrib4d(UInt32 index, Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4dARB", ExactSpelling = true)]
            internal extern static void VertexAttrib4dARB(UInt32 index, Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4dNV", ExactSpelling = true)]
            internal extern static void VertexAttrib4dNV(UInt32 index, Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4dv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4dv(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4dvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4dvARB(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4dvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4dvNV(UInt32 index, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4f", ExactSpelling = true)]
            internal extern static void VertexAttrib4f(UInt32 index, Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4fARB", ExactSpelling = true)]
            internal extern static void VertexAttrib4fARB(UInt32 index, Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4fNV", ExactSpelling = true)]
            internal extern static void VertexAttrib4fNV(UInt32 index, Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4fv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4fv(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4fvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4fvARB(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4fvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4fvNV(UInt32 index, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4hNV", ExactSpelling = true)]
            internal extern static void VertexAttrib4hNV(UInt32 index, OpenMobile.Half x, OpenMobile.Half y, OpenMobile.Half z, OpenMobile.Half w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4hvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4hvNV(UInt32 index, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4iv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4iv(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4ivARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4ivARB(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4Nbv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4Nbv(UInt32 index, SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4NbvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4NbvARB(UInt32 index, SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4Niv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4Niv(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4NivARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4NivARB(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4Nsv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4Nsv(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4NsvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4NsvARB(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4Nub", ExactSpelling = true)]
            internal extern static void VertexAttrib4Nub(UInt32 index, Byte x, Byte y, Byte z, Byte w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4NubARB", ExactSpelling = true)]
            internal extern static void VertexAttrib4NubARB(UInt32 index, Byte x, Byte y, Byte z, Byte w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4Nubv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4Nubv(UInt32 index, Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4NubvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4NubvARB(UInt32 index, Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4Nuiv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4Nuiv(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4NuivARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4NuivARB(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4Nusv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4Nusv(UInt32 index, UInt16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4NusvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4NusvARB(UInt32 index, UInt16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4s", ExactSpelling = true)]
            internal extern static void VertexAttrib4s(UInt32 index, Int16 x, Int16 y, Int16 z, Int16 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4sARB", ExactSpelling = true)]
            internal extern static void VertexAttrib4sARB(UInt32 index, Int16 x, Int16 y, Int16 z, Int16 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4sNV", ExactSpelling = true)]
            internal extern static void VertexAttrib4sNV(UInt32 index, Int16 x, Int16 y, Int16 z, Int16 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4sv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4sv(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4svARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4svARB(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4svNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4svNV(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4ubNV", ExactSpelling = true)]
            internal extern static void VertexAttrib4ubNV(UInt32 index, Byte x, Byte y, Byte z, Byte w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4ubv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4ubv(UInt32 index, Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4ubvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4ubvARB(UInt32 index, Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4ubvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4ubvNV(UInt32 index, Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4uiv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4uiv(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4uivARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4uivARB(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4usv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4usv(UInt32 index, UInt16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttrib4usvARB", ExactSpelling = true)]
            internal extern static unsafe void VertexAttrib4usvARB(UInt32 index, UInt16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribArrayObjectATI", ExactSpelling = true)]
            internal extern static void VertexAttribArrayObjectATI(UInt32 index, Int32 size, OpenMobile.Graphics.OpenGL.AtiVertexAttribArrayObject type, bool normalized, Int32 stride, UInt32 buffer, UInt32 offset);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribDivisorARB", ExactSpelling = true)]
            internal extern static void VertexAttribDivisorARB(UInt32 index, UInt32 divisor);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI1i", ExactSpelling = true)]
            internal extern static void VertexAttribI1i(UInt32 index, Int32 x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI1iEXT", ExactSpelling = true)]
            internal extern static void VertexAttribI1iEXT(UInt32 index, Int32 x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI1iv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI1iv(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI1ivEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI1ivEXT(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI1ui", ExactSpelling = true)]
            internal extern static void VertexAttribI1ui(UInt32 index, UInt32 x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI1uiEXT", ExactSpelling = true)]
            internal extern static void VertexAttribI1uiEXT(UInt32 index, UInt32 x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI1uiv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI1uiv(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI1uivEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI1uivEXT(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI2i", ExactSpelling = true)]
            internal extern static void VertexAttribI2i(UInt32 index, Int32 x, Int32 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI2iEXT", ExactSpelling = true)]
            internal extern static void VertexAttribI2iEXT(UInt32 index, Int32 x, Int32 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI2iv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI2iv(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI2ivEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI2ivEXT(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI2ui", ExactSpelling = true)]
            internal extern static void VertexAttribI2ui(UInt32 index, UInt32 x, UInt32 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI2uiEXT", ExactSpelling = true)]
            internal extern static void VertexAttribI2uiEXT(UInt32 index, UInt32 x, UInt32 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI2uiv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI2uiv(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI2uivEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI2uivEXT(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI3i", ExactSpelling = true)]
            internal extern static void VertexAttribI3i(UInt32 index, Int32 x, Int32 y, Int32 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI3iEXT", ExactSpelling = true)]
            internal extern static void VertexAttribI3iEXT(UInt32 index, Int32 x, Int32 y, Int32 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI3iv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI3iv(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI3ivEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI3ivEXT(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI3ui", ExactSpelling = true)]
            internal extern static void VertexAttribI3ui(UInt32 index, UInt32 x, UInt32 y, UInt32 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI3uiEXT", ExactSpelling = true)]
            internal extern static void VertexAttribI3uiEXT(UInt32 index, UInt32 x, UInt32 y, UInt32 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI3uiv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI3uiv(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI3uivEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI3uivEXT(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4bv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4bv(UInt32 index, SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4bvEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4bvEXT(UInt32 index, SByte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4i", ExactSpelling = true)]
            internal extern static void VertexAttribI4i(UInt32 index, Int32 x, Int32 y, Int32 z, Int32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4iEXT", ExactSpelling = true)]
            internal extern static void VertexAttribI4iEXT(UInt32 index, Int32 x, Int32 y, Int32 z, Int32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4iv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4iv(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4ivEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4ivEXT(UInt32 index, Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4sv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4sv(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4svEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4svEXT(UInt32 index, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4ubv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4ubv(UInt32 index, Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4ubvEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4ubvEXT(UInt32 index, Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4ui", ExactSpelling = true)]
            internal extern static void VertexAttribI4ui(UInt32 index, UInt32 x, UInt32 y, UInt32 z, UInt32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4uiEXT", ExactSpelling = true)]
            internal extern static void VertexAttribI4uiEXT(UInt32 index, UInt32 x, UInt32 y, UInt32 z, UInt32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4uiv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4uiv(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4uivEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4uivEXT(UInt32 index, UInt32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4usv", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4usv(UInt32 index, UInt16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribI4usvEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribI4usvEXT(UInt32 index, UInt16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribIPointer", ExactSpelling = true)]
            internal extern static void VertexAttribIPointer(UInt32 index, Int32 size, OpenMobile.Graphics.OpenGL.VertexAttribIPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribIPointerEXT", ExactSpelling = true)]
            internal extern static void VertexAttribIPointerEXT(UInt32 index, Int32 size, OpenMobile.Graphics.OpenGL.NvVertexProgram4 type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribPointer", ExactSpelling = true)]
            internal extern static void VertexAttribPointer(UInt32 index, Int32 size, OpenMobile.Graphics.OpenGL.VertexAttribPointerType type, bool normalized, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribPointerARB", ExactSpelling = true)]
            internal extern static void VertexAttribPointerARB(UInt32 index, Int32 size, OpenMobile.Graphics.OpenGL.VertexAttribPointerTypeArb type, bool normalized, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribPointerNV", ExactSpelling = true)]
            internal extern static void VertexAttribPointerNV(UInt32 index, Int32 fsize, OpenMobile.Graphics.OpenGL.VertexAttribParameterArb type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs1dvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs1dvNV(UInt32 index, Int32 count, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs1fvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs1fvNV(UInt32 index, Int32 count, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs1hvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs1hvNV(UInt32 index, Int32 n, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs1svNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs1svNV(UInt32 index, Int32 count, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs2dvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs2dvNV(UInt32 index, Int32 count, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs2fvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs2fvNV(UInt32 index, Int32 count, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs2hvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs2hvNV(UInt32 index, Int32 n, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs2svNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs2svNV(UInt32 index, Int32 count, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs3dvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs3dvNV(UInt32 index, Int32 count, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs3fvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs3fvNV(UInt32 index, Int32 count, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs3hvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs3hvNV(UInt32 index, Int32 n, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs3svNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs3svNV(UInt32 index, Int32 count, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs4dvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs4dvNV(UInt32 index, Int32 count, Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs4fvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs4fvNV(UInt32 index, Int32 count, Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs4hvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs4hvNV(UInt32 index, Int32 n, OpenMobile.Half* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs4svNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs4svNV(UInt32 index, Int32 count, Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexAttribs4ubvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexAttribs4ubvNV(UInt32 index, Int32 count, Byte* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexBlendARB", ExactSpelling = true)]
            internal extern static void VertexBlendARB(Int32 count);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexBlendEnvfATI", ExactSpelling = true)]
            internal extern static void VertexBlendEnvfATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams pname, Single param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexBlendEnviATI", ExactSpelling = true)]
            internal extern static void VertexBlendEnviATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams pname, Int32 param);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexPointer", ExactSpelling = true)]
            internal extern static void VertexPointer(Int32 size, OpenMobile.Graphics.OpenGL.VertexPointerType type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexPointerEXT", ExactSpelling = true)]
            internal extern static void VertexPointerEXT(Int32 size, OpenMobile.Graphics.OpenGL.VertexPointerType type, Int32 stride, Int32 count, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexPointervINTEL", ExactSpelling = true)]
            internal extern static void VertexPointervINTEL(Int32 size, OpenMobile.Graphics.OpenGL.VertexPointerType type, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream1dATI", ExactSpelling = true)]
            internal extern static void VertexStream1dATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Double x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream1dvATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream1dvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Double* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream1fATI", ExactSpelling = true)]
            internal extern static void VertexStream1fATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Single x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream1fvATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream1fvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Single* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream1iATI", ExactSpelling = true)]
            internal extern static void VertexStream1iATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int32 x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream1ivATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream1ivATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int32* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream1sATI", ExactSpelling = true)]
            internal extern static void VertexStream1sATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int16 x);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream1svATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream1svATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int16* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream2dATI", ExactSpelling = true)]
            internal extern static void VertexStream2dATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Double x, Double y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream2dvATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream2dvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Double* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream2fATI", ExactSpelling = true)]
            internal extern static void VertexStream2fATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Single x, Single y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream2fvATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream2fvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Single* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream2iATI", ExactSpelling = true)]
            internal extern static void VertexStream2iATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int32 x, Int32 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream2ivATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream2ivATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int32* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream2sATI", ExactSpelling = true)]
            internal extern static void VertexStream2sATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int16 x, Int16 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream2svATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream2svATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int16* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream3dATI", ExactSpelling = true)]
            internal extern static void VertexStream3dATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream3dvATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream3dvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Double* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream3fATI", ExactSpelling = true)]
            internal extern static void VertexStream3fATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream3fvATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream3fvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Single* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream3iATI", ExactSpelling = true)]
            internal extern static void VertexStream3iATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int32 x, Int32 y, Int32 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream3ivATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream3ivATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int32* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream3sATI", ExactSpelling = true)]
            internal extern static void VertexStream3sATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int16 x, Int16 y, Int16 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream3svATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream3svATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int16* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream4dATI", ExactSpelling = true)]
            internal extern static void VertexStream4dATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Double x, Double y, Double z, Double w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream4dvATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream4dvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Double* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream4fATI", ExactSpelling = true)]
            internal extern static void VertexStream4fATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Single x, Single y, Single z, Single w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream4fvATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream4fvATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Single* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream4iATI", ExactSpelling = true)]
            internal extern static void VertexStream4iATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int32 x, Int32 y, Int32 z, Int32 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream4ivATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream4ivATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int32* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream4sATI", ExactSpelling = true)]
            internal extern static void VertexStream4sATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int16 x, Int16 y, Int16 z, Int16 w);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexStream4svATI", ExactSpelling = true)]
            internal extern static unsafe void VertexStream4svATI(OpenMobile.Graphics.OpenGL.AtiVertexStreams stream, Int16* coords);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexWeightfEXT", ExactSpelling = true)]
            internal extern static void VertexWeightfEXT(Single weight);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexWeightfvEXT", ExactSpelling = true)]
            internal extern static unsafe void VertexWeightfvEXT(Single* weight);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexWeighthNV", ExactSpelling = true)]
            internal extern static void VertexWeighthNV(OpenMobile.Half weight);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexWeighthvNV", ExactSpelling = true)]
            internal extern static unsafe void VertexWeighthvNV(OpenMobile.Half* weight);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glVertexWeightPointerEXT", ExactSpelling = true)]
            internal extern static void VertexWeightPointerEXT(Int32 size, OpenMobile.Graphics.OpenGL.ExtVertexWeighting type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glViewport", ExactSpelling = true)]
            internal extern static void Viewport(Int32 x, Int32 y, Int32 width, Int32 height);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWaitSync", ExactSpelling = true)]
            internal extern static void WaitSync(IntPtr sync, UInt32 flags, UInt64 timeout);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWeightbvARB", ExactSpelling = true)]
            internal extern static unsafe void WeightbvARB(Int32 size, SByte* weights);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWeightdvARB", ExactSpelling = true)]
            internal extern static unsafe void WeightdvARB(Int32 size, Double* weights);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWeightfvARB", ExactSpelling = true)]
            internal extern static unsafe void WeightfvARB(Int32 size, Single* weights);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWeightivARB", ExactSpelling = true)]
            internal extern static unsafe void WeightivARB(Int32 size, Int32* weights);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWeightPointerARB", ExactSpelling = true)]
            internal extern static void WeightPointerARB(Int32 size, OpenMobile.Graphics.OpenGL.ArbVertexBlend type, Int32 stride, IntPtr pointer);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWeightsvARB", ExactSpelling = true)]
            internal extern static unsafe void WeightsvARB(Int32 size, Int16* weights);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWeightubvARB", ExactSpelling = true)]
            internal extern static unsafe void WeightubvARB(Int32 size, Byte* weights);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWeightuivARB", ExactSpelling = true)]
            internal extern static unsafe void WeightuivARB(Int32 size, UInt32* weights);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWeightusvARB", ExactSpelling = true)]
            internal extern static unsafe void WeightusvARB(Int32 size, UInt16* weights);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2d", ExactSpelling = true)]
            internal extern static void WindowPos2d(Double x, Double y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2dARB", ExactSpelling = true)]
            internal extern static void WindowPos2dARB(Double x, Double y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2dv", ExactSpelling = true)]
            internal extern static unsafe void WindowPos2dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2dvARB", ExactSpelling = true)]
            internal extern static unsafe void WindowPos2dvARB(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2f", ExactSpelling = true)]
            internal extern static void WindowPos2f(Single x, Single y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2fARB", ExactSpelling = true)]
            internal extern static void WindowPos2fARB(Single x, Single y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2fv", ExactSpelling = true)]
            internal extern static unsafe void WindowPos2fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2fvARB", ExactSpelling = true)]
            internal extern static unsafe void WindowPos2fvARB(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2i", ExactSpelling = true)]
            internal extern static void WindowPos2i(Int32 x, Int32 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2iARB", ExactSpelling = true)]
            internal extern static void WindowPos2iARB(Int32 x, Int32 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2iv", ExactSpelling = true)]
            internal extern static unsafe void WindowPos2iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2ivARB", ExactSpelling = true)]
            internal extern static unsafe void WindowPos2ivARB(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2s", ExactSpelling = true)]
            internal extern static void WindowPos2s(Int16 x, Int16 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2sARB", ExactSpelling = true)]
            internal extern static void WindowPos2sARB(Int16 x, Int16 y);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2sv", ExactSpelling = true)]
            internal extern static unsafe void WindowPos2sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos2svARB", ExactSpelling = true)]
            internal extern static unsafe void WindowPos2svARB(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3d", ExactSpelling = true)]
            internal extern static void WindowPos3d(Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3dARB", ExactSpelling = true)]
            internal extern static void WindowPos3dARB(Double x, Double y, Double z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3dv", ExactSpelling = true)]
            internal extern static unsafe void WindowPos3dv(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3dvARB", ExactSpelling = true)]
            internal extern static unsafe void WindowPos3dvARB(Double* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3f", ExactSpelling = true)]
            internal extern static void WindowPos3f(Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3fARB", ExactSpelling = true)]
            internal extern static void WindowPos3fARB(Single x, Single y, Single z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3fv", ExactSpelling = true)]
            internal extern static unsafe void WindowPos3fv(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3fvARB", ExactSpelling = true)]
            internal extern static unsafe void WindowPos3fvARB(Single* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3i", ExactSpelling = true)]
            internal extern static void WindowPos3i(Int32 x, Int32 y, Int32 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3iARB", ExactSpelling = true)]
            internal extern static void WindowPos3iARB(Int32 x, Int32 y, Int32 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3iv", ExactSpelling = true)]
            internal extern static unsafe void WindowPos3iv(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3ivARB", ExactSpelling = true)]
            internal extern static unsafe void WindowPos3ivARB(Int32* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3s", ExactSpelling = true)]
            internal extern static void WindowPos3s(Int16 x, Int16 y, Int16 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3sARB", ExactSpelling = true)]
            internal extern static void WindowPos3sARB(Int16 x, Int16 y, Int16 z);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3sv", ExactSpelling = true)]
            internal extern static unsafe void WindowPos3sv(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWindowPos3svARB", ExactSpelling = true)]
            internal extern static unsafe void WindowPos3svARB(Int16* v);
            [System.Security.SuppressUnmanagedCodeSecurity()]
            [System.Runtime.InteropServices.DllImport(Raw.Library, EntryPoint = "glWriteMaskEXT", ExactSpelling = true)]
            internal extern static void WriteMaskEXT(UInt32 res, UInt32 @in, OpenMobile.Graphics.OpenGL.ExtVertexShader outX, OpenMobile.Graphics.OpenGL.ExtVertexShader outY, OpenMobile.Graphics.OpenGL.ExtVertexShader outZ, OpenMobile.Graphics.OpenGL.ExtVertexShader outW);
        }
    }
}
