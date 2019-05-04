using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  UnityEngine.Rendering;
#if UNITY_EDITOR
    using System.Linq;
    using UnityEditor;
#endif


namespace EModules.PostPresets {


[RequireComponent( typeof( Camera ) ), DisallowMultipleComponent, ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class LUTsFastMobileCameraScript : MonoBehaviour {
    [SerializeField]
    Shader shader;
    
    [SerializeField]
    public ShaderFloat[] FLOATS = new ShaderFloat[10];
    
    [SerializeField]
    public ShaderTexture[] LUTs = new ShaderTexture[10];
    
    public float Type { get { return FLOATS[4].Get(); } set { FLOATS[4].Set( value, mat ); } }
    public float Bright { get { return FLOATS[0].Get(); } set { FLOATS[0].Set( value, mat ); } }
    public float Saturate { get { return FLOATS[1].Get(); } set { FLOATS[1].Set( value, mat ); } }
    public float Contrast { get { return FLOATS[2].Get(); } set { FLOATS[2].Set( value, mat ); } }
    public float GlowAmount { get { return FLOATS[3].Get(); } set { FLOATS[3].Set( value, mat ); } }
    public float GlowRadius { get { return FLOATS[5].Get(); } set { FLOATS[5].Set( value, mat ); } }
    public float GlowTreshold { get { return FLOATS[8].Get(); } set { FLOATS[8].Set( value, mat ); } }
    public float LUT1_Amount { get { return FLOATS[6].Get(); } set { FLOATS[6].Set( value, mat ); } }
    public float LUT2_Amount { get { return FLOATS[7].Get(); } set { FLOATS[7].Set( value, mat ); } }
    
    public Texture2D LUT1
    {   get { return LUTs[0].Get(); }
        set
        {   if (LUTs == null || LUTs[0] == null )
            {   FLOATS = new ShaderFloat[10];
                LUTs = new ShaderTexture[10];
                init();
            }
            LUTs[0].Set( value, mat );
        }
    }
    public Texture2D LUT2 { get { return LUTs[1].Get(); } set { LUTs[1].Set( value, mat ); } }
    
    
    [System.NonSerialized]
    bool wasInit;
    [System.NonSerialized]
    public Material mat;
    
    public void init()
    {   if (FLOATS[0] == null)
            for (int i = 0 ; i < FLOATS.Length ; i++)
                FLOATS[i] = new ShaderFloat();
                
        if (LUTs[0] == null)
            for (int i = 0 ; i < LUTs.Length ; i++)
                LUTs[i] = new ShaderTexture();
                
        if (!mat)
        {   mat = new Material (shader);
            mat.hideFlags = HideFlags.HideAndDontSave;
        }
        
        if (!wasInit)
        {   wasInit = true;
            UpdateParams();
        }
        
        
        for (int i = 0 ; i < FLOATS.Length ; i++)
            FLOATS[i].Update( mat );
            
        for (int i = 0 ; i < LUTs.Length ; i++)
            LUTs[i].Update( mat );
            
            
    }
    
    void UpdateParams()
    {   if (FLOATS[0] != null)
        {   FLOATS[4].init( -1 /*type*/, 0, mat );
            FLOATS[0].init( Shader.PropertyToID( "_bright" ), 1, mat );
            FLOATS[1].init( Shader.PropertyToID( "_sat" ), 1, mat );
            FLOATS[2].init( Shader.PropertyToID( "_cont" ), 1, mat );
            FLOATS[3].init( Shader.PropertyToID( "_glowAmount" ), 1, mat );
            FLOATS[5].init( Shader.PropertyToID( "_glowRadius" ), 1, mat );
            FLOATS[6].init( Shader.PropertyToID( "_LUT1_Amount" ), 1, mat );
            FLOATS[7].init( Shader.PropertyToID( "_LUT2_Amount" ), 1, mat );
            FLOATS[8].init( Shader.PropertyToID( "_glowTreshold" ), 0.75f, mat );
        }
        
        if (LUTs[0] != null)
        {   LUTs[0].init( Shader.PropertyToID( "_LUT1" ), Shader.PropertyToID( "_LUT1_params" ), null, mat );
            LUTs[1].init( Shader.PropertyToID( "_LUT2" ), Shader.PropertyToID( "_LUT2_params" ), null, mat );
        }
        
    }
    
    
    
    #if UNITY_EDITOR
    
    private void EditorOnDisable()
    {
    }
    #endif
    // bool m_NowDrawingScene { get { return UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera == GetComponent<Camera>(); } }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {   /* if (!m_NowDrawingScene)
         {   Graphics.Blit (source, destination);
             return;
         }*/
        /*
         if (!mat)
         {   //mat = new Material (Shader.Find ("Legacy Shaders/LUTs Fast Mobile Shader 2.0"));
             //mat.hideFlags = HideFlags.HideAndDontSave;
             Graphics.Blit (source, destination);
             return;
         }*/
        #if UNITY_EDITOR
        init();
        #endif
        mat.SetTexture(_MainTex, source );
        Graphics.Blit (source, destination, mat);
    }
    
    int _MainTex = Shader.PropertyToID("_MainTex");
#pragma warning disable
    Camera this_camera;
#pragma warning restore
    private void OnEnable()
    {   this_camera = GetComponent<Camera>();
        if (!shader)
        {   shader = Shader.Find ("Legacy Shaders/LUTs Fast Mobile Shader 2.0");
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        init();
        
    }  private void OnDisable()
    {
        #if UNITY_EDITOR
        EditorOnDisable();
        #endif
        DESTROY(mat);
        wasInit = false;
    } void DESTROY(UnityEngine.Object o)
    {   if (Application.isPlaying) Destroy(o);
        else DestroyImmediate( o);
    }
    /*
    
    CameraEvent event_ = CameraEvent.BeforeImageEffects;
    Camera this_camera;
    private void OnEnable()
    {   this_camera = GetComponent<Camera>();
      init();
      if (__CCBuffer2 != null)  this_camera.RemoveCommandBuffer(event_, __CCBuffer2);
      __CCBuffer2 = null;
      this_camera.forceIntoRenderTexture = true;
      this_camera.AddCommandBuffer(event_, CCBuffer);
    
    }  private void OnDisable()
    {
      #if UNITY_EDITOR
      EditorOnDisable();
      #endif
      DESTROY(mat);
      DestroyTex2();
      if (this_camera & __CCBuffer2 != null)  this_camera.RemoveCommandBuffer(event_, __CCBuffer2);
      __CCBuffer2 = null;
      wasInit = false;
    } void DESTROY(UnityEngine.Object o)
    {   if (Application.isPlaying) Destroy(o);
      else DestroyImmediate( o);
    }
    
    
    
    
    
    [System.NonSerialized]
    UnityEngine.Rendering.CommandBuffer __CCBuffer2;
    UnityEngine.Rendering.CommandBuffer CCBuffer
    {   get
      {   if (__CCBuffer2 != null) return __CCBuffer2;
          __CCBuffer2 = new UnityEngine.Rendering.CommandBuffer() {name = "EMX 1000+ Buffer" };
          if (!mat)
          {   mat = new Material (Shader.Find ("Legacy Shaders/LUTs Fast Mobile Shader 2.0"));
              mat.hideFlags = HideFlags.HideAndDontSave;
          }
          //  int _MainTex = Shader.PropertyToID("_MainTex");
          // __CCBuffer2.GetTemporaryRT (_MainTex, -1, -1, 0, FilterMode.Bilinear);
          CheckLastTex();
          __CCBuffer2.Blit(BuiltinRenderTextureType.CurrentActive, LastTexture2);
          __CCBuffer2.Blit(LastTexture2, BuiltinRenderTextureType.CameraTarget, mat);
          //  __CCBuffer2.ReleaseTemporaryRT(_MainTex);
          return __CCBuffer2;
      }
    }
    
    private void OnPreRender()
    {
      #if UNITY_EDITOR
      if (m_NowDrawingScene) return;
      #endif
      if (LastTexture2) LastTexture2.DiscardContents();
    }
    
    float lastSizex, lastSizey;
    public RenderTexture LastTexture2 = null;
    void CheckLastTex()
    {   if (LastTexture2 == null || lastSizex != this_camera.pixelWidth || lastSizey != this_camera.pixelHeight)
      {   if (LastTexture2) DestroyTex2();
          lastSizex = this_camera.pixelWidth ; lastSizey = this_camera.pixelHeight;
          // LastTexture2 =   new RenderTexture(this_camera.pixelWidth, this_camera.pixelHeight, SystemInfo.supports32bitsIndexBuffer ? 32 : 16)
          LastTexture2 =   new RenderTexture(this_camera.pixelWidth, this_camera.pixelHeight,  32)
          {   hideFlags = HideFlags.HideAndDontSave,
                  name = "EModules/MobileWater/LastFrame-RenderTexture" + gameObject.name,
                  filterMode = FilterMode.Bilinear
          };
      }
    }
    
    void DestroyTex2()
    {   if (!LastTexture2) return;
      if (!Application.isPlaying) DestroyImmediate(LastTexture2, true);
      else Destroy(LastTexture2);
    }*/
    
}






[System.Serializable]
public class ShaderFloat {

    [System.NonSerialized]
    public int shaderHash;
    [SerializeField]
    bool assigned;
    [SerializeField]
    public float serializable_value;
    [System.NonSerialized]
    public float cached_value = -1;
    
    public void init(int shaderHash, float defaultValue, Material mat)
    {   this.shaderHash = shaderHash;
        if (!assigned)
        {   serializable_value = defaultValue;
            assigned = true;
        }
        Set( serializable_value, mat );
        cached_value = serializable_value;
    }
    
    public float Get() { return cached_value; }
    public void Update(Material mat)
    {   if (!assigned) return;
        //if (cached_value != serializable_value)
        {   Set( serializable_value, mat );
        }
    }
    
    
    public void Set(float value, Material mat)
    {   if (shaderHash != -1)
        {   // if (value.Equals( cached_value ) || !mat) return;
            if (mat == null) return;
            serializable_value = cached_value = value;
            mat.SetFloat( shaderHash, value );
        }
        else
        {   // if (value.Equals( cached_value ) || !mat) return;
            serializable_value = cached_value = value;
            
            var type = (int)value;
            var GLOW_BLEACH = (type & 16) != 0;
            var FIX_OVEREXPO = (type & 8) != 0;
            var USE_LUT2 = (type & 4) != 0;
            var USE_BRIGHT = (type & 1) != 0;
            var USE_GLOW =  (type & 2) != 0;
            
            SetKey( "USE_LUT2", USE_LUT2, mat );
            SetKey( "USE_BRIGHT", USE_BRIGHT, mat );
            SetKey( "USE_GLOW", USE_GLOW, mat );
            SetKey( "FIX_OVEREXPO", FIX_OVEREXPO, mat );
            SetKey( "GLOW_BLEACH", GLOW_BLEACH, mat );
        }
    }
    
    void SetKey(string key, bool value, Material mat)
    {   if (value) mat.EnableKeyword( key );
        else mat.DisableKeyword( key );
    }
}


[System.Serializable]
public class ShaderTexture {

    [System.NonSerialized]
    public int shaderHash;
    [System.NonSerialized]
    public int shaderHashParams;
    [SerializeField]
    bool assigned;
    [SerializeField]
    public Texture2D serializable_value;
    [System.NonSerialized]
    public Texture2D cached_value = null;
    
    public void init(int shaderHash, int shaderHashParams, Texture2D defaultValue, Material mat)
    {   this.shaderHash = shaderHash;
        this.shaderHashParams = shaderHashParams;
        if (!assigned)
        {   serializable_value = defaultValue;
            assigned = true;
        }
        Set( serializable_value, mat );
        cached_value = serializable_value;
    }
    
    public Texture2D Get() { return cached_value; }
    public void Update(Material mat)
    {   if (cached_value != serializable_value)
        {   Set( serializable_value, mat );
        }
    }
    
    public void Set(Texture2D value, Material mat)
    {   // if (value == cached_value || !mat) return;
        serializable_value = cached_value = value;
        mat.SetTexture( shaderHash, value );
        if (value) mat.SetVector( shaderHashParams, new Vector4( 1f / value.width, 1f / value.height, value.height - 1f, 0 ) );
    }
    
    
    
    
}



#if UNITY_EDITOR
[CustomEditor( typeof( LUTsFastMobileCameraScript ) )]
[DisallowMultipleComponent]
public class LUTsFastMobileCameraScriptEditor : Editor {

    public override void OnInspectorGUI()
    {   LUTsFastMobileCameraScript target = (LUTsFastMobileCameraScript)base.target;
    
        if (!target) return;
        target. init();
        if (target.FLOATS == null) return;
        if (target.mat == null) return;
        
        
        var type = (int)target.Type;
        var newType = 0;
        
        
        /*var USE_LUT2 = target.mat.IsKeywordEnabled( "USE_LUT2" );
        var USE_BRIGHT =  target.mat.IsKeywordEnabled( "USE_BRIGHT" );
        var USE_GLOW =  target.mat.IsKeywordEnabled( "USE_GLOW" );*/
        
        var oldC = GUI.color;
        if (!target.LUT1) GUI.color *= Color.red;
        var newLUT1 = EditorGUILayout.ObjectField( "LUT A", target.LUT1, typeof(Texture2D), allowSceneObjects: false  ) as Texture2D;
        GUI.color = oldC;
        var newLUT1_Amount = EditorGUILayout.Slider( "LUT A - amount", target.LUT1_Amount, 0, 1 );
        GUILayout.Space( 20 );
        
        var useSecondLut = EditorGUILayout.ToggleLeft( "Use Second LUT", (type & 4) != 0);
        if (useSecondLut) newType |= 4;
        GUI.enabled = useSecondLut;
        if (!target.LUT2 && useSecondLut) GUI.color *= Color.red;
        var newLUT2 = EditorGUILayout.ObjectField( "LUT B", target.LUT2, typeof(Texture2D), allowSceneObjects: false  ) as Texture2D;
        GUI.color = oldC;
        var newLUT2_Amount = EditorGUILayout.Slider( "LUT B - amount", target.LUT2_Amount, 0, 1 );
        GUI.enabled = true;
        GUILayout.Space( 20 );
        
        
        var over = EditorGUILayout.ToggleLeft( "Fix Overexposured Pixels (HDR)", (type & 8) != 0);
        if (over) newType |= 8;
        
        var drawBright = EditorGUILayout.ToggleLeft( "Use Bright/Saturate/Contrast", (type & 1) != 0);
        if (drawBright) newType |= 1;
        GUI.enabled = drawBright;
        var newBright = EditorGUILayout.FloatField( "Bright", target.Bright );
        var newSaturate = EditorGUILayout.FloatField( "Saturate", target.Saturate );
        var newContrast = EditorGUILayout.FloatField( "Contrast", target.Contrast );
        GUI.enabled = true;
        GUILayout.Space( 20 );
        
        
        var drawGlow = EditorGUILayout.ToggleLeft( "Use Glow", (type & 2) != 0);
        if (drawGlow) newType |= 2;
        GUI.enabled = drawGlow;
        var newGlowAmount = EditorGUILayout.FloatField( "GlowAmount", target.GlowAmount );
        var newGlowRadius = EditorGUILayout.FloatField( "GlowRadius", target.GlowRadius );
        var newGlowTreshold = EditorGUILayout.FloatField( "GlowTreshold", target.GlowTreshold );
        var glowBleach = EditorGUILayout.ToggleLeft( "Bleach Style", (type & 16) != 0);
        if (glowBleach) newType |= 16;
        GUI.enabled = true;
        
        
        if (target.Bright != newBright || target.Saturate != newSaturate || target.Contrast != newContrast || target.GlowAmount != newGlowAmount || target.GlowRadius != newGlowRadius
                || target.GlowTreshold != newGlowTreshold
                || target.Type != newType || newLUT1_Amount != target.LUT1_Amount || newLUT2_Amount != target.LUT2_Amount)
        {   Undo.RegisterCompleteObjectUndo( base.target, "Change LUTsFastMobileCameraScript" );
        
            target.Type = newType;
            target.Bright = newBright;
            target.Saturate = newSaturate;
            target.Contrast = newContrast;
            target.GlowAmount = newGlowAmount;
            target.GlowRadius = newGlowRadius;
            target.GlowTreshold = newGlowTreshold;
            
            target.LUT1_Amount = newLUT1_Amount;
            target.LUT2_Amount = newLUT2_Amount;
            
            EditorUtility.SetDirty( base.target );
        }
        
        if (newLUT1 != target.LUT1 || newLUT2 != target.LUT2)
        {   Undo.RegisterCompleteObjectUndo( base.target, "Change LUTsFastMobileCameraScript" );
            target.LUT1 = newLUT1;
            target.LUT2 = newLUT2;
            EditorUtility.SetDirty( base.target );
        }
        
        //EditorGUILayout.ObjectField( "Material", target.mat, typeof( Material ), allowSceneObjects: false );
        
    }
}
#endif
}
