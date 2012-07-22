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
using System.Text;
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.Graphics;

namespace OpenMobile.Controls
{
    /// <summary>
    /// Transition effects interface
    /// </summary>
    public interface iPanelTransitionEffect
    {
        string Name { get; }
        void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier);
    }

    /// <summary>
    /// The transition effect handler
    /// </summary>
    public static class PanelTransitionEffectHandler
    {
        static List<iPanelTransitionEffect> _TransitionEffecs = new List<iPanelTransitionEffect>();
        static iPanelTransitionEffect Effect_None = new PanelTransitionEffect_None();

        /// <summary>
        /// Initialize handler and load default data
        /// </summary>
        static public void Init()
        {
            AddEffect(Effect_None);
            AddEffect(new PanelTransitionEffect_SlideLeft());
            AddEffect(new PanelTransitionEffect_SlideRight());
            AddEffect(new PanelTransitionEffect_SlideUp());
            AddEffect(new PanelTransitionEffect_SlideDown());
            AddEffect(new PanelTransitionEffect_Crossfade());
            AddEffect(new PanelTransitionEffect_CrossfadeFast());
            AddEffect(new PanelTransitionEffect_CollapseGrowCrossUL());
            AddEffect(new PanelTransitionEffect_CollapseGrowCrossCenter());
            AddEffect(new PanelTransitionEffect_CollapseGrowCenter());
        }

        /// <summary>
        /// Registers a new transition effect
        /// </summary>
        /// <param name="Effect"></param>
        static public void AddEffect(iPanelTransitionEffect Effect)
        {
            iPanelTransitionEffect Exist = _TransitionEffecs.Find(x => x.Name.ToLower() == Effect.Name.ToLower());
            if (Exist == null)
            {   // Add new effect
                _TransitionEffecs.Add(Effect);
            }
        }

        /// <summary>
        /// Get's the transition effect that corresponds to the given name
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        static public iPanelTransitionEffect GetEffect(string Name)
        {
            if (Name.ToLower() == "random")
            {
                Random rnd = new Random();
                return _TransitionEffecs[rnd.Next(1, _TransitionEffecs.Count)];
            }
            iPanelTransitionEffect effect = _TransitionEffecs.Find(x => x.Name.ToLower() == Name.ToLower());
            if (effect == null)
                return Effect_None;
            return effect;
        }

    }


    public class PanelTransitionEffect_SlideLeft : iPanelTransitionEffect
    {
        #region iPanelTransitionEffect Members

        public string Name
        {
            get { return "SlideLeft"; }
        }

        public void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier)
        {
            // Animator
            SmoothAnimator Animation = new SmoothAnimator(6f * SpeedMultiplier);

            // Start values for effect
            TransitionEffectParam_In.Offset = new Rectangle(1000, 0, 0, 0);
            TransitionEffectParam_Out.Offset = new Rectangle(0, 0, 0, 0);

            // Execute animation
            Animation.Animate(delegate(int AnimationStep)
            {
                TransitionEffectParam_In.Offset.X -= AnimationStep;
                TransitionEffectParam_Out.Offset.X -= AnimationStep;

                // End animation?
                if (TransitionEffectParam_In.Offset.X <= 0)
                {
                    // Set end transition effects
                    TransitionEffectParam_In.Offset.X = 0;
                    TransitionEffectParam_Out.Offset.X = -1000;

                    // Redraw
                    Refresh();
                    return false;
                }

                // Redraw
                Refresh();

                // Continue animation loop
                return true;
            });
        }

        #endregion
    }

    public class PanelTransitionEffect_SlideRight : iPanelTransitionEffect
    {
        #region iPanelTransitionEffect Members

        public string Name
        {
            get { return "SlideRight"; }
        }

        public void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier)
        {
            // Animator
            SmoothAnimator Animation = new SmoothAnimator(6f * SpeedMultiplier);

            // Start values for effect
            TransitionEffectParam_In.Offset = new Rectangle(-1000, 0, 0, 0);
            TransitionEffectParam_Out.Offset = new Rectangle(0, 0, 0, 0);

            // Execute animation
            Animation.Animate(delegate(int AnimationStep)
            {
                TransitionEffectParam_In.Offset.X += AnimationStep;
                TransitionEffectParam_Out.Offset.X += AnimationStep;

                // End animation?
                if (TransitionEffectParam_In.Offset.X >= 0)
                {
                    // Set end transition effects
                    TransitionEffectParam_In.Offset.X = 0;
                    TransitionEffectParam_Out.Offset.X = 1000;

                    // Redraw
                    Refresh();
                    return false;
                }

                // Redraw
                Refresh();

                // Continue animation loop
                return true;
            });
        }

        #endregion
    }

    public class PanelTransitionEffect_SlideUp : iPanelTransitionEffect
    {
        #region iPanelTransitionEffect Members

        public string Name
        {
            get { return "SlideUp"; }
        }

        public void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier)
        {
            // Animator
            SmoothAnimator Animation = new SmoothAnimator(4.5f * SpeedMultiplier);

            // Start values for effect
            TransitionEffectParam_In.Offset = new Rectangle(0, 600, 0, 0);
            TransitionEffectParam_Out.Offset = new Rectangle(0, 0, 0, 0);

            // Execute animation
            Animation.Animate(delegate(int AnimationStep)
            {
                TransitionEffectParam_In.Offset.Y -= AnimationStep;
                TransitionEffectParam_Out.Offset.Y -= AnimationStep;

                // End animation?
                if (TransitionEffectParam_In.Offset.Y <= 0)
                {
                    // Set end transition effects
                    TransitionEffectParam_In.Offset.Y = 0;
                    TransitionEffectParam_Out.Offset.Y = -600;

                    // Redraw
                    Refresh();
                    return false;
                }

                // Redraw
                Refresh();

                // Continue animation loop
                return true;
            });
        }

        #endregion
    }

    public class PanelTransitionEffect_SlideDown : iPanelTransitionEffect
    {
        #region iPanelTransitionEffect Members

        public string Name
        {
            get { return "SlideDown"; }
        }

        public void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier)
        {
            // Animator
            SmoothAnimator Animation = new SmoothAnimator(4.5f * SpeedMultiplier);

            // Start values for effect
            TransitionEffectParam_In.Offset = new Rectangle(0, -600, 0, 0);
            TransitionEffectParam_Out.Offset = new Rectangle(0, 0, 0, 0);

            // Execute animation
            Animation.Animate(delegate(int AnimationStep)
            {
                TransitionEffectParam_In.Offset.Y += AnimationStep;
                TransitionEffectParam_Out.Offset.Y += AnimationStep;

                // End animation?
                if (TransitionEffectParam_In.Offset.Y >= 0)
                {
                    // Set end transition effects
                    TransitionEffectParam_In.Offset.Y = 0;
                    TransitionEffectParam_Out.Offset.Y = 600;

                    // Redraw
                    Refresh();
                    return false;
                }

                // Redraw
                Refresh();

                // Continue animation loop
                return true;
            });
        }

        #endregion
    }

    public class PanelTransitionEffect_Crossfade : iPanelTransitionEffect
    {
        #region iPanelTransitionEffect Members

        public string Name
        {
            get { return "Crossfade"; }
        }

        public void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier)
        {
            // Animator
            SmoothAnimator Animation = new SmoothAnimator(1f * SpeedMultiplier);

            // Start values for effect
            TransitionEffectParam_In.Alpha = 0.01F;
            TransitionEffectParam_Out.Alpha = 1.0F;

            // Execute animation
            Animation.Animate(delegate(int AnimationStep)
            {
                if (AnimationStep == 0)
                    AnimationStep = 1;
                TransitionEffectParam_In.Alpha += (AnimationStep / 255F);
                TransitionEffectParam_Out.Alpha -= (AnimationStep / 255F);

                // Value limits
                if (TransitionEffectParam_In.Alpha >= 1.0F)
                    TransitionEffectParam_In.Alpha = 1.0F;
                if (TransitionEffectParam_Out.Alpha >= 1.0F)
                    TransitionEffectParam_Out.Alpha = 1.0F;

                // End animation?
                if ((TransitionEffectParam_In.Alpha >= 1.0F) && (TransitionEffectParam_Out.Alpha <= 0.0F))
                {
                    // Set end transition effects
                    TransitionEffectParam_In.Alpha = 1.0F;
                    TransitionEffectParam_Out.Alpha = 0.0F;

                    // Redraw
                    Refresh();
                    return false;
                }

                // Redraw
                Refresh();

                // Continue animation loop
                return true;
            });
        }

        #endregion
    }

    public class PanelTransitionEffect_CrossfadeFast : iPanelTransitionEffect
    {
        #region iPanelTransitionEffect Members

        public string Name
        {
            get { return "CrossfadeFast"; }
        }

        public void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier)
        {
            // Animator
            SmoothAnimator Animation = new SmoothAnimator(2f * SpeedMultiplier);

            // Start values for effect
            TransitionEffectParam_In.Alpha = 0.01F;
            TransitionEffectParam_Out.Alpha = 1.0F;

            // Execute animation
            Animation.Animate(delegate(int AnimationStep)
            {
                if (AnimationStep == 0)
                    AnimationStep = 1;
                TransitionEffectParam_In.Alpha += (AnimationStep / 255F);
                TransitionEffectParam_Out.Alpha -= (AnimationStep / 255F);

                // Value limits
                if (TransitionEffectParam_In.Alpha >= 1.0F)
                    TransitionEffectParam_In.Alpha = 1.0F;
                if (TransitionEffectParam_Out.Alpha >= 1.0F)
                    TransitionEffectParam_Out.Alpha = 1.0F;

                // End animation?
                if ((TransitionEffectParam_In.Alpha >= 1.0F) && (TransitionEffectParam_Out.Alpha <= 0.0F))
                {
                    // Set end transition effects
                    TransitionEffectParam_In.Alpha = 1.0F;
                    TransitionEffectParam_Out.Alpha = 0.0F;

                    // Redraw
                    Refresh();
                    return false;
                }

                // Redraw
                Refresh();

                // Continue animation loop
                return true;
            });
        }

        #endregion
    }

    public class PanelTransitionEffect_CollapseGrowCrossUL : iPanelTransitionEffect
    {
        #region iPanelTransitionEffect Members

        public string Name
        {
            get { return "CollapseGrowCrossUL"; }
        }

        public void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier)
        {
            // Animator
            SmoothAnimator Animation = new SmoothAnimator(0.3f * SpeedMultiplier);

            // Start values for effect
            TransitionEffectParam_In.Scale.X = 0.01F;
            TransitionEffectParam_In.Scale.Y = 0.01F;
            TransitionEffectParam_Out.Scale.X = 1F;
            TransitionEffectParam_Out.Scale.Y = 1F;

            // Execute animation
            Animation.Animate(delegate(int AnimationStep)
            {
                if (AnimationStep == 0)
                    AnimationStep = 1;

                TransitionEffectParam_In.Scale.X += (AnimationStep * 0.01F);
                TransitionEffectParam_In.Scale.Y = TransitionEffectParam_In.Scale.X;
                TransitionEffectParam_Out.Scale.X -= (AnimationStep * 0.01F);
                TransitionEffectParam_Out.Scale.Y = TransitionEffectParam_Out.Scale.X;

                // Value limits
                if (TransitionEffectParam_In.Scale.X >= 1.0F)
                    TransitionEffectParam_In.Scale.X = 1.0F;
                if (TransitionEffectParam_In.Scale.Y >= 1.0F)
                    TransitionEffectParam_In.Scale.Y = 1.0F;
                if (TransitionEffectParam_Out.Scale.X <= 0.01F)
                    TransitionEffectParam_Out.Scale.X = 0.01F;
                if (TransitionEffectParam_Out.Scale.Y <= 0.01F)
                    TransitionEffectParam_Out.Scale.Y = 0.01F;

                // End animation?
                if ((TransitionEffectParam_In.Scale.X >= 1.0F) && (TransitionEffectParam_Out.Scale.X <= 0.01F))
                {
                    // Set end transition effects
                    TransitionEffectParam_In.Scale.X = 1F;
                    TransitionEffectParam_In.Scale.Y = 1F;
                    TransitionEffectParam_Out.Scale.X = 0.01F;
                    TransitionEffectParam_Out.Scale.Y = 0.01F;

                    // Redraw
                    Refresh();
                    return false;
                }

                // Redraw
                Refresh();

                // Continue animation loop
                return true;
            });
        }

        #endregion
    }

    public class PanelTransitionEffect_CollapseGrowCrossCenter : iPanelTransitionEffect
    {
        #region iPanelTransitionEffect Members

        public string Name
        {
            get { return "CollapseGrowCrossCenter"; }
        }

        public void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier)
        {
            // Animator
            SmoothAnimator Animation = new SmoothAnimator(0.3f * SpeedMultiplier);

            // Start values for effect
            TransitionEffectParam_In.Scale.X = 0.01F;
            TransitionEffectParam_In.Scale.Y = 0.01F;
            TransitionEffectParam_Out.Scale.X = 1F;
            TransitionEffectParam_Out.Scale.Y = 1F;

            TransitionEffectParam_In.Offset = new Rectangle(500,300,0,0);
            TransitionEffectParam_Out.Offset = new Rectangle(0,0,0,0);

            // Execute animation
            Animation.Animate(delegate(int AnimationStep)
            {
                if (AnimationStep == 0)
                    AnimationStep = 1;

                // Calculate scale values
                TransitionEffectParam_In.Scale.X += (AnimationStep * 0.01F);
                TransitionEffectParam_In.Scale.Y = TransitionEffectParam_In.Scale.X;
                TransitionEffectParam_Out.Scale.X -= (AnimationStep * 0.01F);
                TransitionEffectParam_Out.Scale.Y = TransitionEffectParam_Out.Scale.X;

                // Calculate position offset to keep graphics centered
                TransitionEffectParam_In.Offset.X = 500 - (int)(500 * TransitionEffectParam_In.Scale.X);
                TransitionEffectParam_In.Offset.Y = 300 - (int)(300 * TransitionEffectParam_In.Scale.Y);
                TransitionEffectParam_Out.Offset.X = 500 - (int)(500 * TransitionEffectParam_Out.Scale.X);
                TransitionEffectParam_Out.Offset.Y = 300 - (int)(300 * TransitionEffectParam_Out.Scale.Y);

                // Value limits
                if (TransitionEffectParam_In.Scale.X >= 1.0F)
                    TransitionEffectParam_In.Scale.X = 1.0F;
                if (TransitionEffectParam_In.Scale.Y >= 1.0F)
                    TransitionEffectParam_In.Scale.Y = 1.0F;
                if (TransitionEffectParam_Out.Scale.X <= 0.01F)
                    TransitionEffectParam_Out.Scale.X = 0.01F;
                if (TransitionEffectParam_Out.Scale.Y <= 0.01F)
                    TransitionEffectParam_Out.Scale.Y = 0.01F;

                // End animation?
                if ((TransitionEffectParam_In.Scale.X >= 1.0F) && (TransitionEffectParam_Out.Scale.X <= 0.01F))
                {
                    // Set end transition effects
                    TransitionEffectParam_In.Scale.X = 1F;
                    TransitionEffectParam_In.Scale.Y = 1F;
                    TransitionEffectParam_Out.Scale.X = 0.01F;
                    TransitionEffectParam_Out.Scale.Y = 0.01F;
                    TransitionEffectParam_In.Offset = new Rectangle(0, 0, 0, 0);
                    TransitionEffectParam_Out.Offset = new Rectangle(500, 300, 0, 0);

                    // Redraw
                    Refresh();
                    return false;
                }

                // Redraw
                Refresh();

                // Continue animation loop
                return true;
            });
        }

        #endregion
    }

    public class PanelTransitionEffect_CollapseGrowCenter : iPanelTransitionEffect
    {
        #region iPanelTransitionEffect Members

        public string Name
        {
            get { return "CollapseGrowCenter"; }
        }

        public void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier)
        {
            // Animator
            SmoothAnimator Animation = new SmoothAnimator(0.5f * SpeedMultiplier);

            // Start values for effect
            TransitionEffectParam_In.Scale.X = 0.01F;
            TransitionEffectParam_In.Scale.Y = 0.01F;
            TransitionEffectParam_Out.Scale.X = 1F;
            TransitionEffectParam_Out.Scale.Y = 1F;

            TransitionEffectParam_In.Offset = new Rectangle(500, 300, 0, 0);
            TransitionEffectParam_Out.Offset = new Rectangle(0, 0, 0, 0);

            // Execute animation
            Animation.Animate(delegate(int AnimationStep)
            {
                if (AnimationStep == 0)
                    AnimationStep = 1;

                // Calculate scale values for object going out
                TransitionEffectParam_Out.Scale.X -= (AnimationStep * 0.01F);
                TransitionEffectParam_Out.Scale.Y = TransitionEffectParam_Out.Scale.X;

                // Value limits
                if (TransitionEffectParam_Out.Scale.X <= 0.01F)
                    TransitionEffectParam_Out.Scale.X = 0.01F;
                if (TransitionEffectParam_Out.Scale.Y <= 0.01F)
                    TransitionEffectParam_Out.Scale.Y = 0.01F;

                // Calculate scale values for object going in (after object going out has completed)
                if (TransitionEffectParam_Out.Scale.X <= 0.01F)
                {
                    TransitionEffectParam_In.Scale.X += (AnimationStep * 0.01F);
                    TransitionEffectParam_In.Scale.Y = TransitionEffectParam_In.Scale.X;
                }

                // Value limits
                if (TransitionEffectParam_In.Scale.X >= 1.0F)
                    TransitionEffectParam_In.Scale.X = 1.0F;
                if (TransitionEffectParam_In.Scale.Y >= 1.0F)
                    TransitionEffectParam_In.Scale.Y = 1.0F;

                // Calculate position offset to keep graphics centered
                TransitionEffectParam_In.Offset.X = 500 - (int)(500 * TransitionEffectParam_In.Scale.X);
                TransitionEffectParam_In.Offset.Y = 300 - (int)(300 * TransitionEffectParam_In.Scale.Y);
                TransitionEffectParam_Out.Offset.X = 500 - (int)(500 * TransitionEffectParam_Out.Scale.X);
                TransitionEffectParam_Out.Offset.Y = 300 - (int)(300 * TransitionEffectParam_Out.Scale.Y);


                // End animation?
                if ((TransitionEffectParam_In.Scale.X >= 1.0F) && (TransitionEffectParam_Out.Scale.X <= 0.01F))
                {
                    // Set end transition effects
                    TransitionEffectParam_In.Scale.X = 1F;
                    TransitionEffectParam_In.Scale.Y = 1F;
                    TransitionEffectParam_Out.Scale.X = 0.01F;
                    TransitionEffectParam_Out.Scale.Y = 0.01F;
                    TransitionEffectParam_In.Offset = new Rectangle(0, 0, 0, 0);
                    TransitionEffectParam_Out.Offset = new Rectangle(500, 300, 0, 0);

                    // Redraw
                    Refresh();
                    return false;
                }

                // Redraw
                Refresh();

                // Continue animation loop
                return true;
            });
        }

        #endregion
    }

    public class PanelTransitionEffect_None : iPanelTransitionEffect
    {
        #region iPanelTransitionEffect Members

        public string Name
        {
            get { return "None"; }
        }

        public void Run(renderingParams TransitionEffectParam_In, renderingParams TransitionEffectParam_Out, ReDrawTrigger Refresh, float SpeedMultiplier)
        {
            // Reset transition effects
            TransitionEffectParam_In = new renderingParams();
            TransitionEffectParam_Out = new renderingParams();
        }

        #endregion
    }

}
