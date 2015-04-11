using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace OpenMobile.Graphics
{
    public enum OMShaders
    {
        None,

        /// <summary>
        /// Gives a negative image
        /// </summary>
        Negative,

        /// <summary>
        /// Gives a "glowing" effect
        /// </summary>
        Bloom,

        /// <summary>
        /// Gives an blurred image (Gaussian blur)
        /// </summary>
        Blurr,

        /// <summary>
        /// Renders a "Radar" effect on top of the texture
        /// </summary>
        Radar,

        /// <summary>
        /// Renders a "dot" effect that follows the mouse on top of the texture
        /// </summary>
        MouseDot
    }

    public abstract class Shader
    {
        public abstract OMShaders ShaderID { get; } 

        /// <summary>
        /// The program handle for this shader
        /// </summary>
        public int Handle
        {
            get
            {
                return this._Handle;
            }
            set
            {
                if (this._Handle != value)
                {
                    this._Handle = value;
                }
            }
        }
        private int _Handle;

        /// <summary>
        /// True = This shader is alive and valid, false may indicate a failure while loading the program
        /// </summary>
        public bool Valid
        {
            get
            {
                return this._Valid;
            }
            set
            {
                if (this._Valid != value)
                {
                    this._Valid = value;
                }
            }
        }
        private bool _Valid = true;        

        public abstract string ShaderCode { get; } 

        public Shader()
        {
            if (String.IsNullOrEmpty(ShaderCode))
                return;

            int status_code;
            string info;

            Handle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(Handle, ShaderCode);
            GL.CompileShader(Handle);
            GL.GetShaderInfoLog(Handle, out info);
            GL.GetShader(Handle, ShaderParameter.CompileStatus, out status_code);

            if (status_code != 1)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error while compiling shader [{0}]\r\n:{1}", ShaderID, info));
                _Valid = false;
                //throw new ApplicationException(info);
            }
        }
    }

    #region Shader effects

    public class Shader_Negative : Shader
    {
        /// <summary>
        /// This is the ID (Name) of this shader
        /// </summary>
        public override OMShaders ShaderID
        {
            get
            {
                return OMShaders.Negative;
            }
        }

        #region ShaderCode

        public override string ShaderCode
        {
            get
            {
                return @"
#version 120
precision mediump float;

uniform sampler2D u_Texture;
varying vec2 v_TexCoordinate;

void main()
 {
 float T = 1.0;

 vec4 irgba = texture2D(u_Texture, gl_TexCoord[0].st).rgba;
 vec3 irgb = texture2D(u_Texture, gl_TexCoord[0].st).rgb;

  if (irgba.a < 1.) {
	gl_FragColor = irgba;
	return;
 }

 vec3 neg = vec3(1., 1., 1.) - irgb;
 gl_FragColor = vec4(mix(irgb,neg, T), 1.);
 }
";
            }
        }

        #endregion
    }

    public class Shader_Bloom : Shader
    {
        /// <summary>
        /// This is the ID (Name) of this shader
        /// </summary>
        public override OMShaders ShaderID
        {
            get
            {
                return OMShaders.Bloom;
            }
        }

        #region ShaderCode

        public override string ShaderCode
        {
            get
            {
                return @"
uniform sampler2D u_Texture;

void main()
{
   vec4 sum = vec4(0);
   vec2 texcoord = vec2(gl_TexCoord[0]);
   int j;
   int i;

   vec4 irgb = texture2D(u_Texture, gl_TexCoord[0].st).rgba;

   for( i= -4 ;i < 4; i++)
   {
        for (j = -3; j < 3; j++)
        {
            sum += texture2D(u_Texture, texcoord + vec2(j, i)*0.004) * 0.25;
        }
   }
       if (texture2D(u_Texture, texcoord).r < 0.3)
    {
       gl_FragColor = sum*sum*0.012 + texture2D(u_Texture, texcoord);
    }
    else
    {
        if (texture2D(u_Texture, texcoord).r < 0.5)
        {
            gl_FragColor = sum*sum*0.009 + texture2D(u_Texture, texcoord);
        }
        else
        {
            gl_FragColor = sum*sum*0.0075 + texture2D(u_Texture, texcoord);
        }
    }

 // Preserve alpha channel
 //gl_FragColor.a = irgb.a;
}
";
            }
        }

        #endregion
    }

    public class Shader_Blurr : Shader
    {
        /// <summary>
        /// This is the ID (Name) of this shader
        /// </summary>
        public override OMShaders ShaderID
        {
            get
            {
                return OMShaders.Blurr;
            }
        }

        #region ShaderCode

        public override string ShaderCode
        {
            get
            {
                return @"
precision mediump float;

uniform sampler2D u_Texture;
varying vec2 v_TexCoordinate;

void main()
 {

 v_TexCoordinate = gl_TexCoord[0].st;

 //vec3 irgb = texture2D(u_Texture, v_TexCoordinate).rgb;
 vec4 irgb = texture2D(u_Texture, v_TexCoordinate).rgba;
 float ResS = 720.;
 float ResT = 720.;

vec2 stp0 = vec2(1./ResS, 0.);
 vec2 st0p = vec2(0., 1./ResT);
 vec2 stpp = vec2(1./ResS, 1./ResT);
 vec2 stpm = vec2(1./ResS, -1./ResT);

vec3 i00 = texture2D(u_Texture, v_TexCoordinate).rgb;
 vec3 im1m1 = texture2D(u_Texture, v_TexCoordinate-stpp).rgb;
 vec3 ip1p1 = texture2D(u_Texture, v_TexCoordinate+stpp).rgb;
 vec3 im1p1 = texture2D(u_Texture, v_TexCoordinate-stpm).rgb;
 vec3 ip1m1 = texture2D(u_Texture, v_TexCoordinate+stpm).rgb;
 vec3 im10 = texture2D(u_Texture, v_TexCoordinate-stp0).rgb;
 vec3 ip10 = texture2D(u_Texture, v_TexCoordinate+stp0).rgb;
 vec3 i0m1 = texture2D(u_Texture, v_TexCoordinate-st0p).rgb;
 vec3 i0p1 = texture2D(u_Texture, v_TexCoordinate+st0p).rgb;

vec3 target = vec3(0., 0., 0.);
 target += 1.*(im1m1+ip1m1+ip1p1+im1p1);
 target += 2.*(im10+ip10+i0p1);
 target += 4.*(i00);
 target /= 16.;
 gl_FragColor = vec4(target, 1.);

// Preserve alpha channel
 gl_FragColor.a = irgb.a;

 }
";
            }
        }

        #endregion
    }

    public class Shader_Radar : Shader
    {
        /// <summary>
        /// This is the ID (Name) of this shader
        /// </summary>
        public override OMShaders ShaderID
        {
            get
            {
                return OMShaders.Radar;
            }
        }

        #region ShaderCode

        public override string ShaderCode
        {
            get
            {
                return @"
#version 120
#ifdef GL_ES
precision mediump float;
#endif

uniform vec2 windowpos;
uniform vec2 resolution;
uniform vec2 position;
uniform vec2 scale;
uniform float time;
uniform sampler2D u_Texture;

//layout(location = origin_upper_left) in vec4 gl_FragCoord;

#define PI 3.1416

vec3 color(float d) {
	return d * vec3(0, 1, 0);	
}

void main(void)
{

    //vec2 texcoord = vec2(gl_TexCoord[0]);
    //resolution = texcoord;
    //position = position;
    
    vec2 fragCoord = gl_FragCoord.xy;
    //fragCoord.y = -fragCoord.y + (fragCoord.y * 2);

    vec4 irgb = texture2D(u_Texture, gl_TexCoord[0].st).rgba;

	vec2 p = (1.0 + -1.0 * (fragCoord / position));
	//p.x *= (resolution.x / resolution.y);

	float a = (atan(p.y,p.x) + time);
	float r = dot(p,p);

	vec3 col = color(pow(fract(a/PI / -2.0), 20.0));
	if (r > 1.0) col = vec3(0,0,0);
	
	//gl_FragColor = vec4(col, 0.1);

	gl_FragColor = texture2D( u_Texture, gl_TexCoord[0].st ) + vec4(col, 0.0);

 // Preserve alpha channel
 gl_FragColor.a = irgb.a;
}
";
            }
        }

        #endregion
    }

    public class Shader_MouseDot : Shader
    {
        /// <summary>
        /// This is the ID (Name) of this shader
        /// </summary>
        public override OMShaders ShaderID
        {
            get
            {
                return OMShaders.MouseDot;
            }
        }

        #region ShaderCode

        public override string ShaderCode
        {
            get
            {
                return @"
#ifdef GL_ES
precision mediump float;
#endif

uniform vec2 windowpos;
uniform vec2 mouse;
uniform vec2 resolution;
uniform vec2 scale;
uniform sampler2D u_Texture;

/* Made by Krisztián Szabó */
void main(){
	/* The light's positions */
	vec2 light_pos = resolution * mouse;

	/* The radius of the light */
	float radius = 200.0;

	/* Intensity range: 0.0 - 1.0 */
	float intensity = 0.2;

    vec4 irgb = texture2D(u_Texture, gl_TexCoord[0].st).rgba;
	
	/* Distance between the fragment and the light */
	float dist = distance(gl_FragCoord.xy, light_pos);
    dist = dist / scale.x;
	
	/* Basic light color, change it to your likings */
	//vec3 light_color = vec3(0.2, 1.0, 1.0);
	vec3 light_color = vec3(1.0, 0.0, 0.0);

	/* Alpha value of the fragment calculated based on intensity and distance */
	float alpha = 1.0 / (dist*intensity);
	
	/* The final color, calculated by multiplying the light color with the alpha value */
	vec4 final_color = vec4(light_color, 1.0)*vec4(alpha, alpha, alpha, 1.0);
	
	/*gl_FragColor = final_color;*/
    vec4 textureColor = texture2D( u_Texture, gl_TexCoord[0].st );

    if (textureColor.a == 0)
        gl_FragColor = final_color;
    else
	    gl_FragColor = textureColor + final_color;
	
	/* If you want to apply radius to the effect comment out the gl_FragColor line and remove comments from below: */
	
	/*if(dist <= radius){
		gl_FragColor = final_color;
	}*/	

    // Preserve alpha channel
    gl_FragColor.a = irgb.a;
}
";
            }
        }

        #endregion
    }

    #endregion

    public class V2Shaders
    {
        public static int[] _ShaderProgramHandles;
        public static Shader[] _Shaders;
        public static System.Diagnostics.Stopwatch swTime = new System.Diagnostics.Stopwatch();

        private static void Init()
        {
            if (_ShaderProgramHandles == null)
            {
                _ShaderProgramHandles = new int[Enum.GetValues(typeof(OMShaders)).Length];
                for (int i = 0; i < _ShaderProgramHandles.Length; i++)
                    _ShaderProgramHandles[i] = -1;
            }
            if (_Shaders == null)
                _Shaders = new Shader[Enum.GetValues(typeof(OMShaders)).Length];

            swTime.Start();
        }

        public static void ActivateShader(GameWindow targetWindow, MouseData mouseData, OMShaders shader, int effectPosX, int effectPosY, int windowLeft, int windowTop, int windowWidth, int windowHeight, float widthScale, float heightScale)
        {
            Init();

            // Do we have to initialize the shader program?
            if (_ShaderProgramHandles[(int)shader] == -1)
            {   // Yes

                try
                {
                    if (_Shaders[(int)shader] == null)
                    {
                        // Initialize shader code
                        switch (shader)
                        {
                            case OMShaders.None:
                                break;
                            case OMShaders.Negative:
                                _Shaders[(int)shader] = new Shader_Negative();
                                break;
                            case OMShaders.Bloom:
                                _Shaders[(int)shader] = new Shader_Bloom();
                                break;
                            case OMShaders.Blurr:
                                _Shaders[(int)shader] = new Shader_Blurr();
                                break;
                            case OMShaders.Radar:
                                _Shaders[(int)shader] = new Shader_Radar();
                                break;
                            case OMShaders.MouseDot:
                                _Shaders[(int)shader] = new Shader_MouseDot();
                                break;
                            default:
                                break;
                        }

                        // Create program
                        _ShaderProgramHandles[(int)shader] = GL.CreateProgram();
                        GL.AttachShader(_ShaderProgramHandles[(int)shader], _Shaders[(int)shader].Handle);
                        System.Diagnostics.Debug.WriteLine(String.Format("Shader [{0}] compiled without problems", _Shaders[(int)shader].ShaderID));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Error while compiling and loading shader [{0}]\r\n:{1}", _Shaders[(int)shader].ShaderID, ex.Message));
                    _Shaders[(int)shader].Valid = false;
                }            
            }

            if (_Shaders[(int)shader] != null && _Shaders[(int)shader].Valid)
            {
                if (_ShaderProgramHandles[(int)shader] >= 0)
                {
                    try
                    {
                        GL.LinkProgram(_ShaderProgramHandles[(int)shader]);
                        GL.UseProgram(_ShaderProgramHandles[(int)shader]);

                        //// Calculate screen coordinates 
                        //System.Drawing.Point effectPos_Screen = targetWindow.PointToScreen(new System.Drawing.Point(effectPosX, effectPosY));

                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.Uniform1(GL.GetUniformLocation(_ShaderProgramHandles[(int)shader], "u_Texture"), TextureUnit.Texture0 - TextureUnit.Texture0);
                        GL.Uniform1(GL.GetUniformLocation(_ShaderProgramHandles[(int)shader], "time"), (float)swTime.Elapsed.TotalSeconds);
                        GL.Uniform2(GL.GetUniformLocation(_ShaderProgramHandles[(int)shader], "resolution"), new Vector2(windowWidth, windowHeight));
                        GL.Uniform2(GL.GetUniformLocation(_ShaderProgramHandles[(int)shader], "scale"), new Vector2(widthScale, heightScale));
                        GL.Uniform2(GL.GetUniformLocation(_ShaderProgramHandles[(int)shader], "windowpos"), new Vector2(windowLeft, windowTop));
                        GL.Uniform2(GL.GetUniformLocation(_ShaderProgramHandles[(int)shader], "position"), new Vector2(effectPosX * widthScale, effectPosY * heightScale));

                        Vector2 mouse = new Vector2();
                        mouse.X = ((((float)mouseData.CursorPosition.X) * widthScale) / (float)windowWidth);
                        mouse.Y = 1 - ((((float)mouseData.CursorPosition.Y) * heightScale) / (float)windowHeight);
                        GL.Uniform2(GL.GetUniformLocation(_ShaderProgramHandles[(int)shader], "mouse"), mouse);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("Error while activating shader program [{0}]\r\n:{1}", _Shaders[(int)shader].ShaderID, ex.Message));
                        _Shaders[(int)shader].Valid = false;
                    }
                }
            }
        }

        public static void DeactivateShader(OMShaders shader)
        {
            try
            {
                GL.UseProgram(0);
                //GL.DeleteProgram(_ShaderProgramHandles[(int)shader]);
            }
            catch
            { }
        }
    }
}
