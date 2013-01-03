/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using OpenMobile.Framework;

namespace OpenMobile.Graphics
{
    /// <summary>
    /// A smooth animator class
    /// </summary>
    public class SmoothAnimator
    {
        #region Predefined animations

        public class AnimatorControl
        {
            /// <summary>
            /// Cancels the animation
            /// </summary>
            public bool Cancel
            {
                get
                {
                    return this._Cancel;
                }
                set
                {
                    if (this._Cancel != value)
                    {
                        this._Cancel = value;
                    }
                }
            }
            private bool _Cancel;        
        }

        /// <summary>
        /// Animates a set of controls contained in a ControlLayout object
        /// <para>Controls is faded in via the opacity and visibility properties</para>
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="screen"></param>
        /// <param name="speed"></param>
        /// <param name="cancel">A controlclass for controlling the animation. Use Null if this is not required</param>
        public static void PresetAnimation_FadeIn(ControlLayout controls, int screen, float speed, AnimatorControl control)
        {
            if (control != null && control.Cancel)
                return;

            if (controls.Opacity < 255 || !controls.Visible)
            {
                // Ensure visibility
                controls.Visible = true;
                controls.Opacity = 0;

                // Animate
                SmoothAnimator Animation = new SmoothAnimator(speed * BuiltInComponents.SystemSettings.TransitionSpeed);
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                {
                    // Check for cancel
                    if (control != null && control.Cancel)
                        return false;

                    controls.Opacity += AnimationStep;

                    if (controls.Opacity >= 255)
                        return false;

                    // Request a screen redraw
                    controls.Refresh();

                    // Continue animation
                    return true;
                });
                controls.Opacity = 255;
                controls.Refresh();
            }
        }

        /// <summary>
        /// Animates a set of controls contained in a ControlLayout object
        /// <para>Controls is faded out via the opacity and visibility properties</para>
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="screen"></param>
        /// <param name="speed"></param>
        /// <param name="control">A controlclass for controlling the animation</param>
        public static void PresetAnimation_FadeOut(ControlLayout controls, int screen, float speed, AnimatorControl control)
        {
            // Check for cancel
            if (control != null && control.Cancel)
                return;

            if (controls.Visible && controls.Opacity > 0)
            {
                // Animate
                SmoothAnimator Animation = new SmoothAnimator(speed * BuiltInComponents.SystemSettings.TransitionSpeed);
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                {
                    // Check for cancel
                    if (control != null && control.Cancel)
                        return false;

                    controls.Opacity -= AnimationStep;

                    if (controls.Opacity <= 0)
                        return false;

                    // Request a screen redraw
                    controls.Refresh();

                    // Continue animation
                    return true;
                });
                controls.Visible = false;
                controls.Opacity = 0;
                controls.Refresh();
            }
        }

        #endregion


        /// <summary>
        /// Delegate for Animator class. Return true when the animation should run, returning false stops the animation.
        /// </summary>
        /// <param name="AnimationStep">Animationstep in int</param>
        /// <param name="AnimationStepF">Animationstep in float</param>
        /// <param name="AnimationDurationMS">Total duration of animation in MS</param>
        public delegate bool AnimatorDelegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS);

        /// <summary>
        /// Speed of animation. 
        /// <para>A value of 1.0F indicates that the AnimationStep will correspond to milliseconds since last call to the animation loop</para>
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Execute animation
        /// </summary>
        public bool Run { get; set; }

        /// <summary>
        /// Execute animation asynchronous
        /// </summary>
        public bool Asynchronous { get; set; }

        /// <summary>
        /// Animation thread delay (A higer delay is better for slower systems but may lead to "choppy" graphics)
        /// </summary>
        public int ThreadDelay { get; set; }

        /// <summary>
        /// Frames pr second to run animation at (default 30fps)
        /// </summary>
        public float FPS { get; set; }

        /// <summary>
        /// Initialize a new smooth animator
        /// </summary>
        /// <param name="Animation_Speed"></param>
        /// <param name="Asynchronous"></param>
        public SmoothAnimator(float Animation_Speed, bool Asynchronous)
            : this(Animation_Speed)
        {
            this.Asynchronous = Asynchronous;
        }
        /// <summary>
        /// Initialize a new smooth animator
        /// </summary>
        public SmoothAnimator()
            : this(1.0F)
        {
        }
        /// <summary>
        /// Initialize a new smooth animator
        /// <para>A speed of 1.0F indicates that the AnimationStep will correspond to milliseconds since last call to the animation loop</para>
        /// </summary>
        /// <param name="Animation_Speed"></param>
        public SmoothAnimator(float Animation_Speed)
        {
            this.Speed = Animation_Speed;
            this.FPS = 60.0F;
            this.ThreadDelay = 1;
            this.Asynchronous = false;
        }

        /// <summary>
        /// Execute animation. Sample code with anonymous delegate:
        /// <para>Animate(delegate(int AnimationStep, float AnimationStepF)</para>
        /// <para>{</para>
        /// <para>.... Code goes here ....</para>
        /// <para>]);</para>
        /// </summary>
        /// <param name="d"></param>
        public void Animate(AnimatorDelegate d)
        {
            if (this.Asynchronous)
            {
                OpenMobile.Threading.SafeThread.Asynchronous(delegate()
                {
                    RunAnimation(d);
                });
            }
            else
            {
                RunAnimation(d);
            }
        }

        private void RunAnimation(AnimatorDelegate d)
        {
            int Animation_Step;
            float Animation_StepF;
            double Interval = System.Diagnostics.Stopwatch.Frequency / FPS;
            double currentTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            double lastUpdateTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            double ticks = 0;
            double ticksMS = 0;
            double totalMS = 0;

            this.Run = true;
            while (Run)
            {
                currentTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                ticks = currentTicks - lastUpdateTicks;
                if (ticks >= Interval)
                {
                    lastUpdateTicks = currentTicks;
                    ticksMS = (ticks / System.Diagnostics.Stopwatch.Frequency) * 1000;
                    totalMS += ticksMS;
                    Animation_StepF = (float)(ticksMS * Speed);
                    Animation_Step = ((int)(ticksMS * Speed));
                    // Minimumsvalue for animation step
                    if (Animation_Step == 0)
                        Animation_Step = 1;

                    // Call animation 
                    Run = d(Animation_Step, Animation_StepF, totalMS);
                }
                Thread.Sleep(ThreadDelay);
            }
        }
    }
}
