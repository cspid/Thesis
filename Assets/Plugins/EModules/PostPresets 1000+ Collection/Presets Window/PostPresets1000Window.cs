#if UNITY_EDITOR
//#define P128

using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;


namespace EModules.PostPresets {

//If you want to add your own component/shader
//Then inherit ISupportedPostComponent interface
//And do not forget to add the constructor to the array Params.AllPostComponents
public interface ISupportedPostComponent {

    string GET_TITLE { get; }
    string GET_DOWNLOAD_MESSAGE { get; }
    string GET_DOWNLOAD_LINK { get; }
    
    void InitializeTypes();
    
    bool LeftSideGUI(EditorWindow window, float leftwidth); // postrocessing compnent gui
    void TopFastButtonsGUI(EditorWindow window, Rect leftwidth); // buttons to enable/disable effects
    
    
    string GetHashString(); // hash depends settings
    bool IsAllowToDraw { get; } // is a component available for use
    // - main component
    Type MonoComponentType { get; }
    MonoBehaviour MonoComponent { get; set; } // cached component for the current frame
    void SetMonoComponentDefaultParametrs(); // default parametrs for added new component
    // - additionals components
    Type SecondMonoComponentType { get; }
    MonoBehaviour SecondMonoComponent { get; set; } // cached component for the current frame
    void SetSecondMonoComponentDefaultParametrs(); // default parametrs for added new component
    
    bool AntiAliasEnable { get; set; } //
    bool LutEffectExist { get; set; } //
    int LutEnable { get; set; } //
    Texture2D LutTexture { get; set; } //
    float LutAmount { get; set; } // LUT opacity value
    
    void CREATE_UNDO(string undoName); // create undo
    void SET_DIRTY(); // save after change
    void CameraPredrawAction(); // Called before the preview is drawed
    void CameraPostDrawAction(); // Called after the preview is drawed
    
}

public partial class Params {
    //Supported components list
    public static List<ISupportedPostComponent> AllPostComponents = new List<ISupportedPostComponent>()
    {   new PostPresets_UnityPostGUI_v2(), new PostPresets_DefaultFastMobile(),
        new PostPresets_UnityPostGUI_v1(), new PostPresets_CameraLensEffects(), new PostPresets_AmplifyPostGUI()
    };
}


public class GradiendDataArray {
    public List<GradiendData> list;
}
public struct GradiendData
{   public string name;
    public double bright;
    public double hue;
    public double compareToFirst;
    public Texture2D gradientTexture;
}

public partial class Params {
    #if P128
    public static Type WindowType = typeof(PostPresets99Window);
    public const string TITLE = "Post 128+";
    #else
    public static Type WindowType = typeof(PostPresets1000Window);
    public const string TITLE = "Post 1000+";
    #endif
    
    public const string STANDARD_FOLDER =  "LUTs Standard";
    public const string USER_FOLDER =  "LUTs User";
    
    static readonly Version  m_uModern  = new Version(2018, 1);
    public static bool OLD_VERSION { get { return UnityEditorInternal.InternalEditorUtility.GetUnityVersion() < m_uModern; } }
    
    public static Camera camera;
    
    
    //MENU
    [MenuItem( Params.TITLE + "/" + PostPresets_UnityPostGUI_v2.DOWNLOAD_MESSAGE, false, 100 )]
    public static void OpenURL_UNI2()
    {   Application.OpenURL( PostPresets_UnityPostGUI_v2.DOWNLOAD_LINK );
    }
    [MenuItem( Params.TITLE + "/" + PostPresets_UnityPostGUI_v1.DOWNLOAD_MESSAGE, false, 101 )]
    public static void OpenURL_UNI1()
    {   Application.OpenURL( PostPresets_UnityPostGUI_v1.DOWNLOAD_LINK );
    }
    #if P128
    [MenuItem( Params.TITLE + "/" + PostPresets_DefaultFastMobile.DOWNLOAD_MESSAGE, false, 102 )]
    public static void OpenURL_FMB()
    {   Application.OpenURL( PostPresets_DefaultFastMobile.DOWNLOAD_LINK );
    }
    #endif
    [MenuItem( Params.TITLE + "/" + PostPresets_CameraLensEffects.DOWNLOAD_MESSAGE, false, 103 )]
    public static void OpenURL_FL()
    {   Application.OpenURL( PostPresets_CameraLensEffects.DOWNLOAD_LINK );
    }
    [MenuItem( Params.TITLE + "/" + PostPresets_AmplifyPostGUI.DOWNLOAD_MESSAGE, false, 104 )]
    public static void OpenURL_AMP()
    {   Application.OpenURL( PostPresets_AmplifyPostGUI.DOWNLOAD_LINK );
    }
    //MENU
    
    
    
    public static GUIStyle Label;
    public static GUIStyle Button;
    
    public static CachedInt overrideCamera = new CachedInt("overrideCamera", -1);
    
    public static CachedString selectedComponent = new CachedString("selectedComponent", "-1");
    public static CachedInt useAnotherComponentsToPreview = new CachedInt("useAnotherComponentsToPreview", 0);
    // public static CachedString anotherComponentsToPreview = new CachedString("anotherComponentsToPreview", "-1");
    public static CachedInt anotherComponentsToPreviewInt = new CachedInt("anotherComponentsToPreviewInt", 0);
    
    public static CachedFloat autoRefresh = new CachedFloat("AutoRefresh", 1);
    public static CachedFloat scrollX = new CachedFloat("ScrollX");
    public static CachedFloat scrollY = new CachedFloat("ScrollY");
    
    public static CachedFloat presetScrollX = new CachedFloat("presetScrollX");
    public static CachedFloat presetScrollY = new CachedFloat("presetScrollY");
    public static CachedFloat favScrollX = new CachedFloat("favScrollX");
    public static CachedFloat favScrollY = new CachedFloat("favScrollY");
    public static CachedFloat customScrollX = new CachedFloat("favScrollX");
    public static CachedFloat customScrollY = new CachedFloat("favScrollY");
    
    public static CachedString filtres = new CachedString("filtres", "");
    public static CachedInt sortMode = new CachedInt("sortMode", 0);
    public static CachedFloat sortInverse = new CachedFloat("sortInverse", 0);
    public static CachedFloat zoomFactor = new CachedFloat("zoomFactor", 0);
    public static CachedInt showFav = new CachedInt("showFav", 0);
    
    public static Vector2 scroll;
    
    //static System.Reflection.Assembly[] _mAssemblyList;
    static Dictionary<string, Type> _mAssemblyTypesListNames;
    // static Dictionary<string,Type> _mAssemblyTypesListFullNames;
    static void ChechAssembly()
    {   if (_mAssemblyTypesListNames == null)
        {   /*var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() ).ToArray();*/
            var mono = typeof(UnityEngine.Object);
            //var scr = typeof(UnityEngine.ScriptableObject);
            
            _mAssemblyTypesListNames = AppDomain.CurrentDomain.GetAssemblies()
                                       .SelectMany( a => a.GetTypes() )
                                       .Where( t => mono.IsAssignableFrom( t )/* || scr.IsAssignableFrom( t )*/ || t.Namespace == PostPresets_UnityPostGUI_v2.NAMESPACE )
                                       .GroupBy( t => t.Name )
                                       .ToDictionary( g => g.Key, g => g.First() );
                                       
            /*  _mAssemblyTypesListFullNames = types
               .GroupBy( t => t.FullName )
               .ToDictionary( g => g.Key, g => g.First() );*/
        }
    }
    public static Dictionary<string, Type> AssemblyTypesListNames
    {   get
        {   ChechAssembly();
            return _mAssemblyTypesListNames;
        }
    }
    /*public static Dictionary<string, Type> AssemblyTypesListFullNames {
      get {
        ChechAssembly();
        return _mAssemblyTypesListFullNames;
      }
    }*/
    
    static Type _mResult;
    public static Type GetTypeFromStringName(string str)
    {   if (!AssemblyTypesListNames.TryGetValue( str, out _mResult )) return null;
        return _mResult;
    }
    /*public static Type GetTypeFromStringFullName(string str)
    {
      if (!AssemblyTypesListFullNames.TryGetValue( str, out _mResult )) return null;
      return _mResult;
    }*/
    
    public static Rect Shrink(Rect r, int value)
    {   r.x += value;
        r.y += value;
        r.width -= value * 2;
        r.height -= value * 2;
        return r;
    }
    
    static  GUIContent  _mGUIContent = new GUIContent();
    public static GUIContent CONTENT(string text, string tooltip)
    {   _mGUIContent.text = text;
        _mGUIContent.tooltip = tooltip;
        return _mGUIContent;
    }
    
    public static string ScriptPath;
    static string _mEditorResourcesPath;
    public static string EditorResourcesPath
    {   get
        {   if (string.IsNullOrEmpty( _mEditorResourcesPath ))
            {   string path;
            
                if (searchForEditorResourcesPath( out path ))
                    _mEditorResourcesPath = path;
                else
                {   EditorUtility.DisplayDialog( "PostPresetsWindow Instalation Error", "Unable to locate editor resources. Make sure the PostPresetsWindow package has been installed correctly.", "Ok" );
                    return null;
                }
            }
            return _mEditorResourcesPath;
        }
    }
    
    static bool searchForEditorResourcesPath(out string path)
    {   var allPathes = AssetDatabase.GetAllAssetPaths();
        path = allPathes.FirstOrDefault( p => p.EndsWith( WindowType.Name + ".cs" ) );
        if (string.IsNullOrEmpty( path )) return false;
        var tempPath = ScriptPath = path.Remove( path.LastIndexOf( '/' ) );
        var candidates = allPathes.Where( p => p.StartsWith( tempPath ) );
        path = tempPath;
        if (candidates.Any( c => c.Contains( STANDARD_FOLDER ) )) return true;
        tempPath = path.Remove( path.LastIndexOf( '/' ) );
        candidates = allPathes.Where( p => p.StartsWith( tempPath ) );
        path = tempPath;
        if (candidates.Any( c => c.Contains( STANDARD_FOLDER ) )) return true;
        return false;
    }
    
    
    
    
    
    
    
    static Dictionary<Type, Dictionary<string, FieldInfo>> _mGetFieldValue = new Dictionary<Type, Dictionary<string, FieldInfo>>();
    public static FieldInfo GetField(string name, object o)
    {   var type = o.GetType();
        return GetField( name, o, type );
    }
    public static FieldInfo GetField(string name, object o, Type type)
    {   //Debug.Log( name + " - " +o + " - " + type );
        if (!_mGetFieldValue.ContainsKey( type )) _mGetFieldValue.Add( type, new Dictionary<string, FieldInfo>() );
        if (!_mGetFieldValue[type].ContainsKey( name ))
            _mGetFieldValue[type].Add( name, type.GetField( name, (BindingFlags)int.MaxValue ) );
        return _mGetFieldValue[type][name];
    }
    
    public static object GetFieldValue(string name, object o)
    {   return GetField( name, o ).GetValue( o );
    }
    public static object GetFieldValue(string name, object o, Type type)
    {   return GetField( name, o, type ).GetValue( o );
    }
    public static object SetFieldValue(string name, object o, object value)
    {   GetField( name, o ).SetValue( o, value );
        return value;
    }
    public static object SetFieldValue(string name, object o, object value, Type type)
    {   GetField( name, o, type ).SetValue( o, value );
        return value;
    }
    
    
    
    static Dictionary<Type, Dictionary<string, PropertyInfo>> _mGetPropertyValue = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
    public static PropertyInfo GetProperty(string name, object o)
    {   var type = o.GetType();
        if (!_mGetPropertyValue.ContainsKey( type )) _mGetPropertyValue.Add( type, new Dictionary<string, PropertyInfo>() );
        if (!_mGetPropertyValue[type].ContainsKey( name ))
            _mGetPropertyValue[type].Add( name, type.GetProperty( name, (BindingFlags)int.MaxValue ) );
        return _mGetPropertyValue[type][name];
    }
    public static object GetPropertyValue(string name, object o)
    {   return GetProperty( name, o ).GetValue( o, null );
    }
    public static object SetPropertyValue(string name, object o, object value)
    {   GetProperty( name, o ).SetValue( o, value, null );
        return value;
    }
    
    
    
    
    
    public static void RepaintImages()
    {   var  w =
            #if P128
            PostPresets99Window
            #else
            PostPresets1000Window
            #endif
            .currentWindow as
            #if P128
            PostPresets99Window
            #else
            PostPresets1000Window
            #endif
            ;
        if (!w) return;
        w.ClearImages( false );
        w.Repaint();
    }
}




///////////////////////
///////////////////////
///////////////////////
//! EditorWindow *** //
///////////////////////
///////////////////////
///////////////////////

#if P128
public class PostPresets99Window : EditorWindow
#else
public partial class PostPresets1000Window : EditorWindow
#endif

{


    const int IMAGE_RENDER_PER_FRAME = 3;
    
    #if !P128
    public Texture2D _mfavicon_enable = null, _mfavicon_disable = null;
    public Texture2D favicon_enable
    {   get
        {   if (!_mfavicon_enable)
            {   _mfavicon_enable = new Texture2D( 1, 1, TextureFormat.ARGB32, false, Params.OLD_VERSION );
                _mfavicon_enable.LoadImage( Convert.FromBase64String( _enable_icon ), true );
                //_mfavicon_enable.Apply();
            }
            return _mfavicon_enable;
            // return _mfavicon_enable ?? (_mfavicon_enable = AssetDatabase.LoadAssetAtPath<Texture>( Params.ScriptPath + "/favicon_enable.png" ) as Texture2D);
        }
    }
    public Texture2D favicon_disable
    {   get
        {   if (!_mfavicon_disable)
            {   _mfavicon_disable = new Texture2D( 1, 1, TextureFormat.ARGB32, false, Params.OLD_VERSION );
                _mfavicon_disable.LoadImage( Convert.FromBase64String( _disable_icon ), true );
                // _mfavicon_disable.Apply();
            }
            return _mfavicon_disable;
            //  }
        }
    }
    #endif
    
    [InitializeOnLoad]
    class MyInitialize {
        static MyInitialize()
        {   if (!currentWindow) return;
            #if P128
            ((PostPresets99Window)currentWindow).ResetWindowAndCLearCache();
            #else
            ((PostPresets1000Window)currentWindow).ResetWindowAndCLearCache();
            #endif
        }
    }
    
    class MyAllPostprocessor : AssetPostprocessor {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {   if (!currentWindow) return;
            #if P128
            ((PostPresets99Window)currentWindow).ResetWindowAndCLearCache();
            #else
            ((PostPresets1000Window)currentWindow).ResetWindowAndCLearCache();
            #endif
        }
    }
    
    
    public static EditorWindow currentWindow;
    public Texture2D[] t;
    // static Texture2D[] _mStandardGradients;
    /* public static Texture2D[] StandardGradients {
       get {
         if (string.IsNullOrEmpty( Params.EditorResourcesPath )) return new Texture2D[0];
         if (_mStandardGradients == null) _mStandardGradients = GetGradientsPathsList( Params.EditorResourcesPath + "/" + Params.STANDARD_FOLDER );
         return _mStandardGradients;
       }
     }*/
    //static Texture2D[] _mUserGradients;
    /*public static Texture2D[] UserGradients {
      get {
        if (_mUserGradients == null) _mUserGradients = GetGradientsPathsList(  + Params.USER_FOLDER );
        return _mUserGradients;
      }
    }*/
    static Dictionary<string, Texture2D[]> _cachedDic = new Dictionary<string, Texture2D[]>();
    public static Texture2D[] GetGradientsPathsList(string path)
    {   if (string.IsNullOrEmpty( Params.EditorResourcesPath )) return new Texture2D[0];
        if (!_cachedDic.ContainsKey( path ))
        {   var fullPath = Params.EditorResourcesPath + "/" + path;
            _cachedDic.Add( path, AssetDatabase.GetAllAssetPaths()
                            .Where( p => p.StartsWith( fullPath ) )
                            .Select( p => AssetDatabase.LoadAssetAtPath( p, typeof( Texture2D ) ) as Texture2D )
                            .Where( t => t )
                            .OrderBy( t => t.name )
                            .ToArray() );
        }
        return _cachedDic[path];
    }
    
    
    [MenuItem( Params.TITLE + "/Presets Manager", false, 0 )]
    public static void Init()
    {   if (string.IsNullOrEmpty( Params.EditorResourcesPath )) return;
    
        _cachedDic.Clear();
        foreach (var window in Resources.FindObjectsOfTypeAll( Params.WindowType )) ((EditorWindow)window).Close();
        
        currentWindow = GetWindow( Params.WindowType, false, Params.TITLE, true );
        if (!EditorPrefs.GetBool( "EModules/" + Params.TITLE + "/Init", false ))
        {   EditorPrefs.GetBool( "EModules/" + Params.TITLE + "/Init", true );
            var p  = currentWindow.position;
            p.width = 1100;
            p.height = 650;
            p.x = Screen.currentResolution.width / 2 - p.width / 2;
            p.y = Screen.currentResolution.height / 2 - p.height / 2;
            currentWindow.position = p;
        }
        var k = "EModules/" + Params.TITLE + "/ScreenShot";
        foreach (var texture2D in Resources.FindObjectsOfTypeAll<Texture2D>().Where( t => t.name == k ))
            DestroyImmediate( texture2D, true );
            
        Params.WindowType.GetMethod( "ResetWindow", (System.Reflection.BindingFlags)int.MaxValue ).Invoke( currentWindow, null );
        
        Undo.undoRedoPerformed -= UndoPerform;
        Undo.undoRedoPerformed += UndoPerform;
    }
    
    
    
    
    
    bool[] AllPostComponentsInstalled;
    GradiendDataArray gradientDataArray = new GradiendDataArray() {list = new List<GradiendData>() };
    
    void ResetWindowAndCLearCache()
    {   _cachedDic.Clear();
        cacher_GetImportSetting.Clear();
        ResetWindow();
    }
    
    void ResetWindow()
    {   mayResetScroll = true;
        renderedScreen = new Texture2D[0];
        renderedDoubleCheck = new Texture2D[0];
        EditorPrefs.SetInt( "EModules/" + Params.TITLE + "/Scene", SceneManager.GetActiveScene().GetHashCode() );
        
        
        foreach (var item in Params.AllPostComponents) item.InitializeTypes();
        AllPostComponentsInstalled = Params.AllPostComponents.Select( c => c.MonoComponentType != null ).ToArray();
        
        
        InitializeCompareArray( (Params.showFav == 2 ? Params.USER_FOLDER : Params.STANDARD_FOLDER) );
        INVOKE( "InitializeFavorites" );
        
        Repaint();
    }
    
    // char[] trimChars = "1234567890+ ABCDEFGHJKLMNOPQRSTUWXYZ".ToCharArray();
    
    void InitializeCompareArray(string FileDataName)
    {
    
        var path = Params.EditorResourcesPath + "/" + "." + FileDataName + ".data";
        gradientDataArray = new GradiendDataArray() { list = new List<GradiendData>() };
        if (File.Exists( path ))
        {   using (StreamReader sr = new StreamReader( path ))
            {   while (!sr.EndOfStream)
                {   var data = sr.ReadLine().Split(':');
                    gradientDataArray.list.Add( new GradiendData()
                    {   name = data[0],
                            bright = double.Parse( data[1] ),
                            hue = double.Parse( data[2] ),
                            compareToFirst = double.Parse( data[3] ),
                    } );
                }
            }
        }
        
        name_to_index.Clear();
        default_gradients.Clear();
        hue_gradients.Clear();
        bright_gradients.Clear();
        compareToFirst_gradients.Clear();
        inverse_default_gradients.Clear();
        inverse_hue_gradients.Clear();
        inverse_bright_gradients.Clear();
        inverse_compareToFirst_gradients.Clear();
        
        
        var GRADIENTS =  GetGradientsPathsList( FileDataName );
        
        for (int i = 0 ; i < GRADIENTS.Length ; i++)
        {   name_to_index.Add( GRADIENTS[i].name, i );
            // var n = gradients[i].name.TrimEnd( trimChars );
            // name_to_filterstring.Add( gradients[i].name, n );
            // if (!filtername_to_filterindex.ContainsKey( n )) filtername_to_filterindex.Add( n, (filtername_to_filterindex.Count + 1) / 4 );
            
            default_gradients.Add( new GradiendData() { name = GRADIENTS[i].name, gradientTexture = GRADIENTS[i] } );
        }
        
        //read data for unplussed textures
        for (int i = 0 ; i < gradientDataArray.list.Count ; i++)
        {   var data = gradientDataArray.list[i];
            if (!name_to_index.ContainsKey( data.name ))
            {   continue;
            }
            data.gradientTexture = GRADIENTS[name_to_index[data.name]];
            
            hue_gradients.Add( data );
            bright_gradients.Add( data );
            compareToFirst_gradients.Add( data );
        }
        need_update_sorting = hue_gradients.Count != GRADIENTS.Length;
        
        //sort unplussed textures
        hue_gradients.Sort( (a, b) => Sign( b.hue, a.hue ) );
        bright_gradients.Sort( (a, b) => Sign( b.bright, a.bright ) );
        compareToFirst_gradients.Sort( (a, b) => Sign( b.compareToFirst, a.compareToFirst ) );
        
        inverse_default_gradients = default_gradients.Reverse<GradiendData>().ToList();
        inverse_hue_gradients = hue_gradients.Reverse<GradiendData>().ToList();
        inverse_bright_gradients = bright_gradients.Reverse<GradiendData>().ToList();
        inverse_compareToFirst_gradients = compareToFirst_gradients.Reverse<GradiendData>().ToList();
        
        //add plussed textures to sorted unplussed
        /*AddPlusesTextures( ref hue_gradients );
        AddPlusesTextures( ref bright_gradients );
        AddPlusesTextures( ref compareToFirst_gradients );*/
    }
    
#pragma warning disable
    bool need_update_sorting;
#pragma warning restore
    /*void AddPlusesTextures(ref List<GradiendData> array)
    {
      for (int i = 0 ; i < array.Count ; i++)
      {
        var target = array[i].name + " +";
        while (name_to_index.ContainsKey( target ))
        {
          var t = gradients[name_to_index[target]];
          array.Insert( i, array[i] );
          i++;
          var data = array[i];
          data.gradientTexture = t;
          array[i] = data;
          target += '+';
        }
      }
    }*/
    
    int Sign(double a, double b)
    {   if (a > b) return 1;
        if (a < b) return -1;
        return 0;
    }
    
    // Dictionary<string,int> filtername_to_filterindex = new Dictionary<string, int>();
    // Dictionary<string,string> name_to_filterstring = new Dictionary<string, string>();
    Dictionary<string, int> name_to_index = new Dictionary<string, int>();
    List<GradiendData> default_gradients = new List<GradiendData>();
    List<GradiendData> hue_gradients = new List<GradiendData>();
    List<GradiendData> bright_gradients = new List<GradiendData>();
    List<GradiendData> compareToFirst_gradients = new List<GradiendData>();
    List<GradiendData>inverse_default_gradients = new List<GradiendData>();
    List<GradiendData>inverse_hue_gradients = new List<GradiendData>();
    List<GradiendData>inverse_bright_gradients = new List<GradiendData>();
    List<GradiendData>inverse_compareToFirst_gradients = new List<GradiendData>();
    string[] sotrNames = new string[] {"Name", "Saturate", "Warming", "Content"};
    /* Dictionary<double,string> hue_to_name = new Dictionary<double,string>();
     Dictionary<double,string> bright_to_name = new Dictionary<double,string>();
     Dictionary<double,string> compareToFirst_to_name = new Dictionary<double,string>();*/
    
    
    static Scene activeScene;
    static void UndoPerform()
    {   //if (!currentWindow) currentWindow = Resources.FindObjectsOfTypeAll( Params.WindowType ).FirstOrDefault() as EditorWindow;
        if (!currentWindow) return;
        currentWindow.Repaint();
    }
    
    private void OnDestroy()
    {   EditorApplication.modifierKeysChanged -= KeysChanged;
        Undo.undoRedoPerformed -= UndoPerform;
    }
    
    private void OnDisable()
    {   EditorApplication.modifierKeysChanged += KeysChanged;
    
    }
    
    private void OnEnable()
    {   EditorApplication.modifierKeysChanged -= KeysChanged;
        EditorApplication.modifierKeysChanged += KeysChanged;
    }
    
    void KeysChanged()
    {   if (currentWindow != null) currentWindow.Repaint();
    }
    
    
    
    Dictionary<string, int> favorites = new Dictionary<string, int>();
    
    void CreateFavorite(string name)
    {   if (!favorites.ContainsKey( name )) favorites.Add( name, 0 );
        StringBuilder result = new StringBuilder();
        foreach (var f in favorites)
            result.AppendLine( f.Key );
        File.WriteAllText( Params.EditorResourcesPath + "/.Favorites.data", result.ToString().TrimEnd( '\n' ) );
    }
    void RemoveFavorite(string name)
    {   if (!favorites.ContainsKey( name )) return;
        favorites.Remove( name );
        StringBuilder result = new StringBuilder();
        foreach (var f in favorites)
            result.AppendLine( f.Key );
        File.WriteAllText( Params.EditorResourcesPath + "/.Favorites.data", result.ToString().TrimEnd( '\n' ) );
    }
    
    
    
    
    
    static bool ShowInspector = true;
    void CameraButton()
    {   GUILayout.FlexibleSpace();
        var oc = GUI.color;
        if (Params.overrideCamera != -1)  GUI.color *= Color.red;
        if (GUILayout.Button( "Camera ☰", GUILayout.Width( 70) ))
        {   CreateCameraMenu();
        }
        GUI.color = oc;
        GUILayout.Label( "█", GUILayout.Width( 10 ) );
    }
    
    void OnGUI()
    {   if (!SceneManager.GetActiveScene().IsValid()) return;
    
        //foreach (var item in Params.AllPostComponents) item.InitializeTypes();
        
        if (!currentWindow)
        {   currentWindow = Resources.FindObjectsOfTypeAll( Params.WindowType ).FirstOrDefault() as EditorWindow;
            if (currentWindow) ResetWindow();
        }
        if (!currentWindow) return;
        
        if (SceneManager.GetActiveScene().GetHashCode() != EditorPrefs.GetInt( "EModules/" + Params.TITLE + "/Scene", -1 )) ResetWindow();
        
        if (Params.Label == null)
        {   Params.Label = new GUIStyle( GUI.skin.label );
            Params.Label.fontSize = 14;
            Params.Label.fontStyle = FontStyle.Bold;
            
            Params.Button = new GUIStyle( GUI.skin.button );
            // Button.fontSize = 14;
            var t = new Texture2D( 1, 1, TextureFormat.ARGB32, false, Params.OLD_VERSION )
            {   hideFlags = HideFlags.DontSave
            };
            t.SetPixel( 0, 0, new Color( 0, 0.1f, 0.4f, 0.3f ) );
            t.Apply();
            Params.Button.normal.background = null;
            Params.Button.hover.background = null;
            Params.Button.focused.background = null;
            Params.Button.active.background = t;
        }
        
        var tc = Camera.allCameras;
        Camera rc = null;
        if (Params.overrideCamera != -1)
        {   if (Params.overrideCamera < tc.Length) rc = tc[Params.overrideCamera];
            else Params.overrideCamera.Set(-1);
        }
        if (Params.overrideCamera == -1)
        {   if (tc.Length != 0)
            {   var d = tc.Select(c => c.depth).Max();
                rc = tc.FirstOrDefault(c => c.depth == d);
            }
            else
            {   rc = null;
            }
        }
        
        if (rc != Params.camera) oldRefreshCheck = "#Camera Changed " + (rc ? ( UnityEngine.Random.Range(0, 1000).ToString() ) : "null");
        Params.camera = rc;
        var leftwidth = ShowInspector ? Mathf.Clamp( position.width / 3f, 200, 350 ) : 100;
        
        if (!Params.camera)
        {   GUILayout.BeginHorizontal(GUILayout.Width( leftwidth ));
            GUILayout.Label( "Scene's Camera not found...", Params.Label, GUILayout.Width(leftwidth - 70) );
            CameraButton();
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Create Camera", GUILayout.Width( leftwidth )))
            {   var o  = new GameObject("Camera");
                o.AddComponent<Camera>();
                o.AddComponent<FlareLayer>();
                o.AddComponent<AudioListener>();
                Selection.objects = new [] {o};
                Undo.RegisterCreatedObjectUndo(o, "Create Camera");
            }
            return;
        }
        //var cameraComponents = Params.camera.GetComponents<MonoBehaviour>().ToList();
        var assignedComponents = Params.AllPostComponents.Select((c, i) =>
        {   if (c.SecondMonoComponentType != null) c.SecondMonoComponent = Params.camera.GetComponents(c.SecondMonoComponentType).Select(c2 => c2 as MonoBehaviour).OrderBy(
                            c2 => c2.enabled ? 0 : 1).FirstOrDefault();
            return !AllPostComponentsInstalled[i] ? new MonoBehaviour[0] :  Params.camera.GetComponents(c.MonoComponentType).Select(f => f as MonoBehaviour).Where(f => f).ToArray() as MonoBehaviour[];
        } ).ToArray();
#pragma warning disable
        var assignedComponentsFirst = assignedComponents.Select((comps, i) =>
        {   return Params.AllPostComponents[i].MonoComponent = comps.FirstOrDefault(f => f.enabled) ?? comps.FirstOrDefault();
        }
#pragma warning restore
                                                               ).ToArray();
                                                               
                                                               
                                                               
        var selectedComponentIndex = Params.AllPostComponents.FindIndex(c => c.GET_TITLE == Params.selectedComponent);
        // var anotherCompoinentIndex = Params.AllPostComponents.FindIndex(c => c.GET_TITLE == Params.anotherComponentsToPreview);
        if (selectedComponentIndex == -1) AllPostComponentsInstalled.ToList().FindIndex( 1, i => i );
        selectedComponentIndex = Mathf.Clamp( selectedComponentIndex, 0, Params.AllPostComponents.Count - 1 );
        // anotherCompoinentIndex = Mathf.Clamp( anotherCompoinentIndex, 0, Params.AllPostComponents.Count - 1 );
        // if (selectedComponentIndex != anotherCompoinentIndex && !assignedComponentsFirst[anotherCompoinentIndex]) anotherCompoinentIndex = selectedComponentIndex;
        var anotherCompoinentIndex = Params.anotherComponentsToPreviewInt;
        
        GUILayout.BeginHorizontal();
        
        
        
        
        
        GUILayout.BeginVertical( GUILayout.Width( leftwidth ) );
        GUILayout.Space( 10 );
        GUILayout.BeginHorizontal( GUILayout.Width( leftwidth ) );
        var isp = EditorGUILayout.ToggleLeft( "Show Inspector", ShowInspector, GUILayout.Width(leftwidth - 70) );
        if (isp != ShowInspector)
        {   ShowInspector = isp;
            ClearImages( true );
            Repaint();
        }
        #if P128
        GUI.enabled = false;
        #endif
        GUILayout.FlexibleSpace();
        if (GUILayout.Button( "Menu ☰", GUILayout.Width(70)))
        {   CreateMenu();
        }
        GUILayout.Label( "█", GUILayout.Width( 10 ) );
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.Space( 10 );
        
        
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        
        
        
        if (ShowInspector)
        {   GUILayout.BeginHorizontal();
            GUILayout.Label( "Enable Component" );
            if (Params.AllPostComponents[selectedComponentIndex].MonoComponent != null)
            {   GUILayout.FlexibleSpace();
                var tg = EditorGUILayout.ToggleLeft("", Params.AllPostComponents[selectedComponentIndex].MonoComponent.enabled, GUILayout.Width(20));
                if (tg != Params.AllPostComponents[selectedComponentIndex].MonoComponent.enabled)
                {   Undo.RecordObject( Params.AllPostComponents[selectedComponentIndex].MonoComponent, "enable mono" );
                    Params.AllPostComponents[selectedComponentIndex].MonoComponent.enabled = tg;
                    if (Params.AllPostComponents[selectedComponentIndex].SecondMonoComponent != null) Params.AllPostComponents[selectedComponentIndex].SecondMonoComponent.enabled = tg;
                    EditorUtility.SetDirty( Params.AllPostComponents[selectedComponentIndex].MonoComponent );
                    EditorUtility.SetDirty( Params.AllPostComponents[selectedComponentIndex].MonoComponent.gameObject );
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            var oldC = GUI.color;
            GUI.color = Color.red;
            GUILayout.Label( "█", GUILayout.Width( 10 ) );
            GUI.color = oldC;
        }
        
        // var restoreEnables = assignedComponents.Select(c => c.Select(ar => ar.enabled).ToArray()).ToArray();
        var allCommps = Params.camera.GetComponents<MonoBehaviour>();
        var restoreEnables2 = allCommps.Select(c => c.enabled).ToArray();
        
        
        var NEW_POP_A = !ShowInspector ? selectedComponentIndex : EditorGUILayout.Popup(
                            selectedComponentIndex,
                            Params.AllPostComponents.Select( (c, i) => (c.GET_TITLE) + " " + (AllPostComponentsInstalled[i] ? "" : " (Not Installed)") ).ToArray());
        if (ShowInspector)
        {   GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        if (ShowInspector) EditorGUILayout.HelpBox( "Сomponent whose settings will be displayed", position.width < 800 ? MessageType.None : MessageType.Info );
        GUILayout.EndHorizontal();
        
        GUILayout.Space( 10 );
        
        var NEW_USE =  (int)Params.useAnotherComponentsToPreview;
        if (ShowInspector)
        {   GUILayout.BeginHorizontal();
            GUILayout.Label( "█", GUILayout.Width( 10 ) );
            NEW_USE = EditorGUILayout.ToggleLeft( "Use different Preview component", Params.useAnotherComponentsToPreview == 1 ) ? 1 : 0;
            GUILayout.EndHorizontal();
        }
        
        var enable = GUI.enabled;
        GUI.enabled = NEW_USE == 1;
        var drawDots = !GUI.enabled || !AllPostComponentsInstalled[anotherCompoinentIndex];
        /*
        var assignedNames = Params.AllPostComponents.Select( (c, i) => new { c, i } ).Where( p => assignedComponentsFirst[p.i] ).ToList();
        var assignedIndex = assignedNames.FindIndex(p => p.c.GET_TITLE == Params.AllPostComponents[anotherCompoinentIndex].GET_TITLE);
        
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        if (ShowInspector) GUILayout.Label( "Preview Component" );
        var temp_NEW_POP_O =  !ShowInspector ? assignedIndex : EditorGUILayout.Popup(
                                  !drawDots ? assignedIndex : 0,
                                  !drawDots ? assignedNames.Select( p => p.c.GET_TITLE ).ToArray() : new[] { "..." } );
        */
        int __ni = 0;
        var assignedNames = Params.AllPostComponents
                            .Select( (c, i) => assignedComponents[i]
        .Select((asd, ci) =>  new { c, i = __ni++, comp = asd, ci = assignedComponents[i].Length < 2 ? "" : ci.ToString() })
        .ToArray()).SelectMany(asd => asd).ToArray();
        var assignedIndex = Mathf.Clamp(anotherCompoinentIndex, 0, assignedNames.Length);
        drawDots |= assignedNames.Length == 0;
        
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        if (ShowInspector) GUILayout.Label( "Preview Component" );
        var temp_NEW_POP_O =  !ShowInspector ? assignedIndex : EditorGUILayout.Popup(
                                  !drawDots ? assignedIndex : 0,
                                  !drawDots ? assignedNames.Select( p => p.c.GET_TITLE + " " + p.comp.name  + " " + p.ci).ToArray() : new[] { "..." } );
                                  
        if (Params.useAnotherComponentsToPreview == 1 && assignedIndex >= 0 && assignedIndex < assignedNames.Length)
        {   assignedNames[assignedIndex].c.MonoComponent = assignedNames[assignedIndex].comp;
        }
        
        // var NEW_POP_O = !drawDots && assignedIndex != -1 ? assignedNames[temp_NEW_POP_O].i : anotherCompoinentIndex;
        var NEW_POP_O = !drawDots && temp_NEW_POP_O != -1 ? temp_NEW_POP_O : assignedIndex;
        GUILayout.EndVertical();
        if (ShowInspector) EditorGUILayout.HelpBox( "For preview in the current window will be used the selected component", position.width < 800 ? MessageType.None : MessageType.Info );
        GUILayout.EndHorizontal();
        GUI.enabled = enable;
        
        if (ShowInspector) EditorGUILayout.HelpBox( "Use a single different preview component, if not selected, then the current camera settings will be used for previewing", MessageType.None );
        
        
        if (Params.AllPostComponents[NEW_POP_A].GET_TITLE != Params.selectedComponent ||
                !drawDots && NEW_POP_O != Params.anotherComponentsToPreviewInt ||
                NEW_USE != Params.useAnotherComponentsToPreview)
        {
        
            Params.selectedComponent.Set( Params.AllPostComponents[NEW_POP_A].GET_TITLE );
            Params.anotherComponentsToPreviewInt.Set( NEW_POP_O );
            Params.useAnotherComponentsToPreview.Set( NEW_USE );
            
            ClearImages( true );
            if (Params.showFav == 0) mayResetScroll = WasElementsChanged;
            WasElementsChanged = false;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
        
        
        GUILayout.Space( 20 );
        
        ISupportedPostComponent currentComponent = /*!assignedComponentsFirst[NEW_POP_A] ? null :*/ Params.AllPostComponents[NEW_POP_A];
        // ISupportedPostComponent previewComponent = !assignedComponentsFirst[NEW_POP_O] ? null : Params.AllPostComponents[NEW_POP_O];
        ISupportedPostComponent previewComponent = assignedIndex < 0 || assignedIndex >= assignedNames.Length || !assignedNames[assignedIndex].comp ? null : assignedNames[assignedIndex].c;
        
        
        
        if (currentComponent == null || !AllPostComponentsInstalled[NEW_POP_A])
        {
        
        
            GUILayout.Label( Params.AllPostComponents[NEW_POP_A].GET_TITLE + " not imported", Params.Label );
            if (GUILayout.Button( Params.AllPostComponents[NEW_POP_A].GET_DOWNLOAD_MESSAGE, GUILayout.Height( 40 ) ))
            {   Application.OpenURL( Params.AllPostComponents[NEW_POP_A].GET_DOWNLOAD_LINK );
            }
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            return;
        }
        
        
        // Left column ////
        GUILayout.BeginHorizontal(GUILayout.Width( leftwidth ));
        GUILayout.Label( "Camera: " + Params.camera.name, Params.Label, GUILayout.Width(leftwidth - 70) );
        if (GUI.Button( GUILayoutUtility.GetLastRect(), "", Params.Button )) Selection.objects = new[] { Params.camera.gameObject };
        EditorGUIUtility.AddCursorRect( GUILayoutUtility.GetLastRect(), MouseCursor.Link );
        CameraButton();
        GUILayout.EndHorizontal();
        
        // if (Event.current.type == EventType.Repaint) Debug.Log( "update" );
        
        if (currentComponent.MonoComponent == null || currentComponent.SecondMonoComponentType != null && currentComponent.SecondMonoComponent == null)
        {
        
            var compName = (currentComponent.MonoComponent == null ? currentComponent.MonoComponentType.Name : currentComponent.SecondMonoComponentType.Name);
            
            var oc = GUI.color;
            GUI.color *= Color.red;
            if (GUILayout.Button( "Add " + compName + " Script", GUILayout.Height( 200 ) ))
            {   Undo.RecordObject( Params.camera.gameObject, "Add " + compName + " Script" );
                if (currentComponent.MonoComponent == null) currentComponent.MonoComponent = Params.camera.gameObject.AddComponent( currentComponent.MonoComponentType ) as MonoBehaviour;
                if (currentComponent.SecondMonoComponent == null
                        && currentComponent.SecondMonoComponentType != null) currentComponent.SecondMonoComponent = Params.camera.gameObject.AddComponent( currentComponent.SecondMonoComponentType ) as MonoBehaviour;
                if (currentComponent.MonoComponent) currentComponent.SetMonoComponentDefaultParametrs();
                if (currentComponent.SecondMonoComponent) currentComponent.SetSecondMonoComponentDefaultParametrs();
                EditorUtility.SetDirty( Params.camera.gameObject );
            }
            GUI.color = oc;
            return;
        }
        
        if (ShowInspector)
        {   GUILayout.BeginHorizontal(GUILayout.Width( leftwidth ));
            GUILayout.Label( "█", GUILayout.Width( 10 ) );
            Params.autoRefresh.Set( EditorGUILayout.ToggleLeft( "Automatic repaint when changing", Params.autoRefresh == 1, GUILayout.Width(leftwidth - 84)) ? 1 : 0 );
            GUILayout.FlexibleSpace();
            if (GUILayout.Button( "Repaint ☰", GUILayout.Width(70) ))
            {   oldRefreshCheck = "#" + UnityEngine.Random.Range(0, 1000) + " Repaint";
            }
            GUILayout.Label( "█", GUILayout.Width( 10 ) );
            
            GUILayout.EndHorizontal();
            GUILayout.Space( 10 );
        }
        
        
        if (!currentComponent.MonoComponent.enabled || currentComponent.SecondMonoComponent != null && !currentComponent.SecondMonoComponent.enabled)
        {
        
            var compName = !currentComponent.MonoComponent.enabled ?  currentComponent.MonoComponent.GetType().Name : currentComponent.SecondMonoComponent.GetType().Name;
            GUILayout.Label( compName + " Component Disabled", Params.Label );
            var oc = GUI.color;
            GUI.color *= Color.red;
            if (GUILayout.Button( "Enable " + compName + " Component", GUILayout.Height( 200 ) ))
            {   Undo.RecordObject( currentComponent.MonoComponent, "Enable " + compName + " Component" );
                currentComponent.MonoComponent.enabled = true;
                if (currentComponent.SecondMonoComponent != null) currentComponent.SecondMonoComponent.enabled = true;
                EditorUtility.SetDirty( currentComponent.MonoComponent );
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            GUI.color = oc;
            return;
        }
        
        
        
        
        if (position.width > 2)
        {   if (widthCheck == null) widthCheck = position.width;
            if (widthCheck.Value != position.width)
            {   widthCheck = position.width;
                NeedUp = true;
            }
        }
        
        if (Event.current.rawType == EventType.Repaint)
        {   if (NeedUp)
            {   NeedUp = false;
                ClearImages( false );
                currentWindow.Repaint();
            }
        }
        
        
        // var DRAW_DIF_PREV = !(Params.useAnotherComponentsToPreview == 0 || previewComponent == null || !previewComponent.IsAllowToDraw || !previewComponent.MonoComponent);
        var DRAW_DIF_PREV = Params.useAnotherComponentsToPreview == 1 && previewComponent != null && previewComponent.IsAllowToDraw && previewComponent.MonoComponent;
        //! *** POSTPROCESSING COMPONENT GUI *** //
        
        bool endGui = false;
        if (ShowInspector)
        {   endGui = !currentComponent.LeftSideGUI( this, leftwidth );
        
            // if (DRAW_DIF_PREV) previewComponent.LeftSideGUI( this, leftwidth );
        }
        //! *** POSTPROCESSING COMPONENT GUI *** //
        
        if (GUILayout.Button( "HELP: http://emem.store/wiki?=PostPresets 1000+", Params.Button, GUILayout.Width( leftwidth ) )) Application.OpenURL( "http://emem.store/wiki?=PostPresets%201000+" );
        EditorGUIUtility.AddCursorRect( GUILayoutUtility.GetLastRect(), MouseCursor.Link );
        
        GUILayout.EndVertical();
        // Left column
        
        if (endGui) return;
        
        
        GUILayout.Space( 10 );
        
        leftwidth += 20;
        
        //! PRESETS GRID GUI
        ////////////////////////
        ////////////////////////
        GUILayout.BeginVertical();
        
        if (currentComponent.IsAllowToDraw)
        {   //var rect = EditorGUILayout.GetControlRect( Params.GetR GUILayout.Width( Math.Min( (window.position.width - leftWidth - 10 - 40) / 7, 100 ) ), GUILayout.Height( 40 ) );
            currentComponent.TopFastButtonsGUI( this, GetRect( leftwidth + 10, 40, this, 1 ) );
            // + GetRect( leftwidth, 0, this, 3 ).width
            FiltresAndSorting( this, leftwidth + 10 );
            
            var rect = GUILayoutUtility.GetLastRect();
            rect.height = rect.y + rect.height;
            rect.y = 0;
            rect.width = RIGHTADD( this );
            rect.x = position.width - rect.width;
            
            #if P128
            GUI.enabled = false;
            #endif
            Zooming( this, rect );
            GUI.enabled = true;
        }//TopFastButtonsGUI
        
        
        
        
        if (!DRAW_DIF_PREV)
        {   if (!currentComponent.LutEffectExist)
            {   GUILayout.Label( "The component settings do not allow you to assign a LUT case 1", Params.Label );
                // GUILayout.Label( currentComponent.MonoComponent.name, Params.Label );
                return;
            }
            else
            {   DrawPresets( currentComponent );
            }
        }//DrawPresets
        else
        {   if (!previewComponent.LutEffectExist)
            {   GUILayout.Label( "The component settings do not allow you to assign a LUT case 2", Params.Label );
                // GUILayout.Label( currentComponent.MonoComponent.name, Params.Label );
                return;
            }
            else
            {   for (int i = 0 ; i < assignedComponents.Length ; i++)
                {   for (int x = 0 ; x < assignedComponents[i].Length ; x++)
                    {   assignedComponents[i][x].enabled = false;
                    }
                }
                
                currentComponent.MonoComponent.enabled = false;
                if (currentComponent.SecondMonoComponent) currentComponent.SecondMonoComponent.enabled = false;
                previewComponent.MonoComponent.enabled = true;
                if (previewComponent.SecondMonoComponent) previewComponent.SecondMonoComponent.enabled = true;
                
                DrawPresets( previewComponent );
                for (int x = 0 ; x < restoreEnables2.Length ; x++) allCommps[x].enabled =  restoreEnables2[x];
                /* for (int i = 0 ; i < assignedComponents.Length ; i++)
                 {   for (int x = 0 ; x < assignedComponents[i].Length ; x++)
                     {   assignedComponents[i][x].enabled = restoreEnables[i][x];
                     }
                 }*/
            }
        }//DrawPresets
        
        GUILayout.EndVertical();
        ////////////////////////
        ////////////////////////
        //! PRESETS GRID GUI
        
        GUILayout.EndHorizontal();
        
        
        
    }//!OnGUI
    
    
    bool NeedUp = false;
    
    
    void FiltresAndSorting(EditorWindow window, float leftWidth)
    {
        #if !P128
        GUI.enabled = true;
        #else
        GUI.enabled = false;
        #endif
        
        // FILTRES
        //var filtresCount = filtername_to_filterindex.Values.Max();
        /*for (int i = 0 ; i < filtresCount ; i++)
        {
          var filtMask = 1 << i;
          var enable = ((int)Params.filtres & filtMask) != 0;
          var captureI=i;
          var name = filtername_to_filterindex.Where(f=>f.Value == captureI).Select(f=>f.Key).Aggregate((a,b)=>a+'\n' +b);
          if (Button( GetRect( leftWidth, 40, window, 8 ), name, enable ))
          {
            if (enable) Params.filtres.Set( ((int)Params.filtres & ~filtMask) );
            else Params.filtres.Set( ((int)Params.filtres | filtMask) );
            renderedDoubleCheck = new Texture2D[gradients.Length];
            window.Repaint();
          }
        }*/
        GUILayout.BeginHorizontal();
        var lineR =  GetRect( leftWidth, 16, window, 1 ); //SPACE
        lineR.width -= 20;
        GUILayout.EndHorizontal();
        var seaR = lineR;
        // seaR.width *= 0.7f;
        lineR.x += seaR.width + 40;
        lineR.width -= seaR.width + 40;
        var newFilter = EditorGUI.TextField(seaR, (string)Params.filtres );
        if (string.IsNullOrEmpty( Params.filtres ))
        {   var oc = GUI.color;
            GUI.color *= new Color( 1, 1, 1, 0.3f );
            GUI.Label( seaR, "Search: " );
            GUI.color = oc;
        }
        if (newFilter != Params.filtres)
        {   Params.filtres.Set( newFilter );
            window.Repaint();
        }
        seaR.x += seaR.width;
        seaR.width = 20;
        if (GUI.Button( seaR, "X" ))
        {   Params.filtres.Set( "" );
            EditorGUI.FocusTextInControl( null );
            ClearImages( true );
            window.Repaint();
        }
        
        
        
        int D = 8;
        // SORTING
        GUILayout.BeginHorizontal();
        var r = GetRect( leftWidth, 20, window, D);
        // r.width -= 20;
        GUI.Label( r, "Sorting:" );
        for (int i = 0 ; i < 4 ; i++)
        {   var sortNotInverse = ((int)Params.sortInverse & (1 << i)) == 0;
            if (Button( GetRect( leftWidth, 20, window, D ), sotrNames[i] + (Params.sortMode == i ? (sortNotInverse ? "▼" : "▲") : ""), Params.sortMode == i, "Set Ordering Method" ))
            {   if (Params.sortMode == i)
                {   if (!sortNotInverse) Params.sortInverse.Set( ((int)Params.sortInverse & ~(1 << i)) );
                    else Params.sortInverse.Set( ((int)Params.sortInverse | (1 << i)) );
                }
                else
                {   Params.sortMode.Set( i );
                }
                ClearImages( true );
                window.Repaint();
            }
        }
        
        GetRect( leftWidth, 20, window, D );
        var hr = GetRect( leftWidth, 20, window, D );
        hr.width *= 2;
        if (GUI.Button( hr, "Goto active LUT" ))
        {   FRAME_SCROLL();
        }
        
        GUILayout.EndHorizontal();
        
        GUI.enabled = true;
        
    }//!FiltresAndSorting
    
    
    
    void Zooming(EditorWindow window, Rect rect)
    {   GUI.Box( rect, "" );
    
    
        var lineR = rect; //SPACE
        lineR.height = 16;
        GUI.Label( lineR, "Zoom:" );
        lineR.y += lineR.height;
        lineR.height = 24;
        
        var ZoomRect = lineR;
        ZoomRect.width /= 3;
        for (int i = 0 ; i < 3 ; i++)
        {   if (Button( ZoomRect, "x" + (i + 1) * 3, Params.zoomFactor == i, "Set Zooming Factor" ))
            {   Params.zoomFactor.Set( i );
                ClearImages( true );
                window.Repaint();
            }
            ZoomRect.x += ZoomRect.width;
        }
        
        
        
        // lineR=  rect; //SPACE
        lineR.y += lineR.height;
        lineR.height = 16;
        lineR.y += 22;
        
        
        var RR = lineR;
        #if !P128
        var ow = RR.width;
        RR.width = 16;
        
        GUI.DrawTexture( RR, favicon_enable );
        RR.x += RR.width;
        RR.width = ow - 32;
        // RR.width = ow - 16;
        #endif
        EditorGUIUtility.AddCursorRect( RR, MouseCursor.Link );
        
        var oc = GUI.color;
        if (Params.showFav == 1) GUI.color = new Color( 1f, 0.8f, 0.63f, 1f );
        if (Params.showFav == 2) GUI.color = new Color( 0.63f, 0.8f, 1f, 1f );
        
        var newShow =  EditorGUI.Popup(RR, Params.showFav, new[] { "Standard LUT's", "Favorites", "User LUT's", } );
        if (newShow != Params.showFav)
        {   Params.showFav.Set( newShow );
            ClearImages( true );
            if (Params.showFav == 0) mayResetScroll = WasElementsChanged;
            WasElementsChanged = false;
            window.Repaint();
            ResetWindow();
        }
        
        /*if (GUI.Button( RR, "Show Favorites", Params.Button )) {
          Params.showFav.Set( 1 - Params.showFav );
          ClearImages( true );
          if (Params.showFav == 0) mayResetScroll = WasElementsChanged;
          WasElementsChanged = false;
          window.Repaint();
        }*/
        
        
        //if (Params.showFav != 0 && Event.current.type == EventType.Repaint) EditorStyles.popup.Draw( RR, new GUIContent(), 0 );
        GUI.color = oc;
        RR.x += RR.width;
        RR.width = 16;
        #if !P128
        GUI.DrawTexture( RR, favicon_enable );
        
        lineR.y += lineR.height + 5;
        
        if (Params.showFav == 2 && need_update_sorting && Params.sortMode != 1)
        {   var oldC = GUI.color;
            GUI.color *= Color.red;
            GUI.Label( lineR, "need to update sorting" );
            GUI.color = oldC;
        }
        else
        {   lineR.width = 20;
            GUI.Label( lineR, "█" );
            lineR.x += lineR.width;
            lineR.width = ow - 20;
            GUI.Label( lineR, "../" + (Params.showFav == 2 ? Params.USER_FOLDER : Params.STANDARD_FOLDER) );
        }
        #endif
        
    }
    
    
    float RIGHTADD(EditorWindow window) { return window.position.width * 0.125f; }
    Rect GetRect(float leftWidth, float height, EditorWindow window, int divider)
    {   return EditorGUILayout.GetControlRect( GUILayout.Width( Math.Min( (Mathf.Max( 100, window.position.width - leftWidth - 10 - RIGHTADD( window ) )) / divider, 700 / divider ) ),
                                               GUILayout.Height( height ) );
    }
    
    bool Button(Rect rect, string name, bool enable, string helpTExt = null)
    {   if (enable) EditorGUI.DrawRect( rect, new Color( 0.33f, 0.6f, 0.8f, 0.4f ) );
        EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
        var content = Params.CONTENT( name, helpTExt ?? "enable/disable " + name.Replace( '\n', ' ' ) );
        var result = (GUI.Button( rect, content ));
        //if (enable) EditorGUI.DrawRect( rect, new Color( 0.33f, 0.6f, 0.8f, 0.4f ) );
        if (Event.current.type == EventType.Repaint && enable) GUI.skin.button.Draw( rect, content, true, true, true, true );
        if (enable) EditorGUI.DrawRect( rect, new Color( 0.33f, 0.6f, 0.8f, 0.4f ) );
        return result;
    } //! TOP FAST BUTTIONS
    
    
    float? widthCheck;
    
    Texture2D[] renderedDoubleCheck = new Texture2D[0];
    Texture2D[] renderedScreen = new Texture2D[0];
    static int renderedIndex;
    static bool mayResetScroll = true;
    static string oldRefreshCheck;
    
    bool SeartchValidate(string currentName)
    {   //if (((1 << filtername_to_filterindex[name_to_filterstring[targetArray[i].name]]) & (int)Params.filtres) == 0) continue;
        return string.IsNullOrEmpty( Params.filtres ) || currentName.ToLower().Contains( ((string)Params.filtres).ToLower() );
    }
    
    public void ClearImages(bool ClearCache)
    {   renderedIndex = 0;
        if (ClearCache)
        {   foreach (var t in renderedScreen) if (t) DestroyImmediate( t, true );
            mayResetScroll = true;
            renderedScreen = new Texture2D[0];
            renderedDoubleCheck = new Texture2D[0];
        }
        else
        {   renderedDoubleCheck = new Texture2D[renderedScreen.Length];
        }
    }
    
    
    void FRAME_SCROLL()
    {   if (filteredSelectIndex != -1)
        {   var fixedcellHeight = cellHeight;
            var scrollY = Params.presetScrollY;
            if (Params.showFav == 1)
            {   scrollY = Params.favScrollY;
            }
            else if (Params.showFav == 2)
            {   scrollY = Params.customScrollY;
            }
            //(float)Params.presetScrollY + " " + filteredSelectIndex / XXX * cellHeight + " " + WIN_HEIGHT + " " + (filteredSelectIndex / XXX * cellHeight +
            //Debug.Log( fixedcellHeight > Params.presetScrollY + WIN_HEIGHT );
            if (filteredSelectIndex / XXX * fixedcellHeight < scrollY) scrollY.Set( filteredSelectIndex / XXX * fixedcellHeight );
            else if (filteredSelectIndex / XXX * fixedcellHeight + fixedcellHeight > scrollY + WIN_HEIGHT) scrollY.Set( filteredSelectIndex / XXX * fixedcellHeight + fixedcellHeight - WIN_HEIGHT );
        }
    }
    bool WasElementsChanged = false;
    int filteredSelectIndex;
    int cellHeight;
    int XXX;
    float WIN_HEIGHT;
    
    void DrawPresets(ISupportedPostComponent component)
    {   var zoom = (int)Mathf.Clamp(Params.zoomFactor + 1, 1, 3);
        cellHeight = (int)((125 + 16) / ((zoom - 1) / 2f + 1));
        int CAPTUREHeight = (int)((125 ) / ((zoom - 1) / 2f + 1));
        WIN_HEIGHT = lastHeight ?? (position.height - (40 + 20 + 24 + 20));
        
        
        var sortNotInverse = ((int)Params.sortInverse & (1 << (int)Params.sortMode)) == 0;
        var targetArray = default_gradients;
        switch ((int)Params.sortMode)
        {   case 0: targetArray = sortNotInverse ? default_gradients : inverse_default_gradients; break;
            case 1: targetArray = sortNotInverse ? bright_gradients : inverse_bright_gradients; break;
            case 2: targetArray = sortNotInverse ? hue_gradients : inverse_hue_gradients; break;
            case 3: targetArray = sortNotInverse ? compareToFirst_gradients : inverse_compareToFirst_gradients; break;
        }//sort type
        
        var l = component.LutTexture;
        int selectIndex = -1;
        filteredSelectIndex = -1;
        var __i  = -1;
        var __M  = -1;
        for (__i = 0 ; __i < targetArray.Count ; __i++)
        {   if (!SeartchValidate( targetArray[__i].name )) continue;
            if (Params.showFav == 1 && !favorites.ContainsKey( targetArray[__i].name )) continue;
            __M++;
            if (selectIndex == -1 && l == targetArray[__i].gradientTexture)
            {   filteredSelectIndex = __M;
                selectIndex = __i;
            }
        }
        
        // var XXX = Math.Min(3 * zoom, Math.Max(1, M + 1));
        XXX = 3 * zoom;
        //int displaingCount = Mathf.CeilToInt(WIN_HEIGHT / cellHeight + 1 ) * XXX;
        
        var scrollX = Params.presetScrollX;
        var scrollY = Params.presetScrollY;
        if (Params.showFav == 1)
        {   scrollX = Params.favScrollX;
            scrollY = Params.favScrollY;
        }
        else if (Params.showFav == 2)
        {   scrollX = Params.customScrollX;
            scrollY = Params.customScrollY;
        }
        
        if (mayResetScroll)
        {   mayResetScroll = false;
            FRAME_SCROLL();
        }//frame scroll
        
        
        Params.scroll.x = scrollX;
        Params.scroll.y = scrollY;
        Params.scroll = GUILayout.BeginScrollView( Params.scroll );
        scrollX.Set( Params.scroll.x );
        scrollY.Set( Params.scroll.y );
        
        
        
        GUILayout.BeginVertical();
        var height = Mathf.Ceil( name_to_index.Count / (float)XXX);
        int wasRender = 0;
        int __FirstRenderInc = -1;
        bool needRepaint = false;
        
        var newRefreshCheck = component.GetHashString();
        if ((Params.autoRefresh == 1 || oldRefreshCheck.StartsWith("#")) && !string.Equals( newRefreshCheck, oldRefreshCheck, StringComparison.Ordinal ))
        {   oldRefreshCheck = newRefreshCheck;
            ClearImages( false );
            currentWindow.Repaint();
            
        }//refrest if changed
        
        
        
        __i = -1;
        Rect line = new Rect();
        bool renderTexture = false;
        bool newLine = false;
        
        if (__M == -1)
        {   if (Params.showFav == 1) GUILayout.Label( "No Favorite Elements", Params.Label );
            else if (Params.showFav == 2) GUILayout.Label( "No User's Elements. Place your png into " + Params.EditorResourcesPath + "/" + Params.USER_FOLDER, Params.Label );
            else GUILayout.Label( "No elements '" + (string)Params.filtres + "' found", Params.Label );
        }
        else
        {   line = EditorGUILayout.GetControlRect( GUILayout.Height( Mathf.RoundToInt( (__M / (float)XXX + 1) * cellHeight ) ) );
            line.height = cellHeight;
            line.y -= line.height;
            int __renderInc = -1;
            bool? oldAnti = null;
            
            
            for (int y = 0 ; y < height ; y++)
            {   newLine = true;
            
            
                for (int x = 0 ; x < XXX ; x++)
                {   __i++;
                    //var i = x + y * XXX;
                    if (__i >= targetArray.Count) break;
                    
                    if (!SeartchValidate( targetArray[__i].name ))
                    {   x--;
                        continue;
                    }
                    if (Params.showFav == 1 && !favorites.ContainsKey( targetArray[__i].name ))
                    {   x--;
                        continue;
                    }
                    
                    if (newLine)
                    {   newLine = false;
                        //line = EditorGUILayout.GetControlRect( GUILayout.Height( cellHeight ) );
                        line.y += line.height;
                        renderTexture = line.y + line.height * 1 >= Params.scroll.y && line.y - line.height * 1 <= Params.scroll.y + WIN_HEIGHT;
                        // if (renderTexture) Debug.Log( line.y - Params.scroll.y + " " + line.height );
                    }
                    
                    
                    
                    var rect = line;
                    rect.width = line.width / XXX;
                    rect.x = rect.width * x;
                    
                    if (!renderTexture) continue;
                    __renderInc++;
                    
                    if (__FirstRenderInc == -1) __FirstRenderInc = __renderInc;
                    
                    if (renderedScreen.Length != targetArray.Count)
                    {   System.Array.Resize( ref renderedScreen, targetArray.Count );
                        ClearImages( false );
                        currentWindow.Repaint();
                    }
                    
                    if (renderedIndex < __FirstRenderInc) renderedIndex = __FirstRenderInc;
                    
                    /* if (Params.showFav == 1)
                       Debug.Log( (wasRender < (Params.zoomFactor + 1) * IMAGE_RENDER_PER_FRAME) + " " + (renderedIndex % displaingCount) + " " + __renderInc + " " + __FirstRenderInc );*/
                    
                    if (wasRender < (Params.zoomFactor + 1) * IMAGE_RENDER_PER_FRAME /*&& renderedIndex % displaingCount == (__renderInc - __FirstRenderInc)*/)
                    {   if ((int)rect.width - 10 > 0 && !renderedDoubleCheck[__i])
                        {   renderedIndex++;
                        
                            if (!oldAnti.HasValue)
                            {   component.CameraPredrawAction(); // PRE
                                oldAnti = component.AntiAliasEnable;
                                component.AntiAliasEnable = false;
                            }
                            
                            //memory
                            var oldLutEnable = component.LutEnable;
                            var oldLutTexture = component.LutTexture;
                            var oldAmount = component.LutAmount;
                            //change
                            component.LutEnable = int.MaxValue;
                            component.LutTexture = (targetArray[__i]).gradientTexture;
                            component.LutAmount = (1);
                            
                            
                            //draw
                            if (renderedScreen[__i]) DestroyImmediate( renderedScreen[__i], true );
                            renderedDoubleCheck[__i] = renderedScreen[__i] = TakeScreen( Params.camera, (int)rect.width - 10, CAPTUREHeight);
                            
                            //restore
                            component.LutEnable = oldLutEnable;
                            component.LutTexture = (oldLutTexture);
                            component.LutAmount = (oldAmount);
                            
                            
                            wasRender++;
                        }
                    }
                    
                    var screen =  renderedScreen[__i];
                    
                    if (!renderedDoubleCheck[__i]) needRepaint = true;
                    
                    if (DrawCell( rect, screen, targetArray[__i].name, targetArray[__i].gradientTexture, selectIndex == __i, component ))
                    {   WasElementsChanged = true;
                        component.CREATE_UNDO( "set " + targetArray[__i].name );
                        component.LutTexture = (targetArray[__i].gradientTexture);
                        /*if (component.LutEnable == 0)*/
                        if (component.LutAmount == 0) component.LutAmount = 1;
                        if (!component.LutEffectExist) component.LutEffectExist = true;
                        component.LutEnable = int.MaxValue;
                        // component.LutEnable = true;
                        component.SET_DIRTY();
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                }
            }
            if (oldAnti.HasValue)
            {   component.AntiAliasEnable = oldAnti.Value;
                component.CameraPostDrawAction(); // POST
            }
        }
        
        
        GUILayout.EndVertical();
        
        
        if (wasRender < (Params.zoomFactor + 1) * IMAGE_RENDER_PER_FRAME)
        {   // renderedIndex = firstI;
            renderedIndex += (int)(Params.zoomFactor + 1) * IMAGE_RENDER_PER_FRAME - wasRender;
            //needRepaint = true;
        }
        if (needRepaint)
        {   Repaint();
        }
        GUILayout.EndScrollView();
        if (GUILayoutUtility.GetLastRect().height > 10) lastHeight = GUILayoutUtility.GetLastRect().height;
    }//!DrawPresets
    float? lastHeight = null;
    
    
    Dictionary<int, TextureImporter> cacher_GetImportSetting = new Dictionary<int, TextureImporter>();
    TextureImporter GetImportSetting(Texture2D gr)
    {   if (!gr) return null;
    
        if (!cacher_GetImportSetting.ContainsKey( gr.GetInstanceID() ))
        {   var path = AssetDatabase.GetAssetPath(gr);
            if (string.IsNullOrEmpty( path )) cacher_GetImportSetting.Add( gr.GetInstanceID(), null );
            cacher_GetImportSetting.Add( gr.GetInstanceID(), (TextureImporter)TextureImporter.GetAtPath( path ) );
        }
        
        return cacher_GetImportSetting[gr.GetInstanceID()];
    }
    
    void FixImportSetting(Texture2D gr)
    {   var set = GetImportSetting(gr);
        if (set == null) return;
        
        
        set.textureType = TextureImporterType.Default;
        set.mipmapEnabled = false;
        set.anisoLevel = 0;
        set.sRGBTexture = false;
        set.npotScale = TextureImporterNPOTScale.None;
        set.textureCompression = TextureImporterCompression.Uncompressed;
        set.alphaSource = TextureImporterAlphaSource.None;
        set.wrapMode = TextureWrapMode.Clamp;
        // set.SaveAndReimport();
        //AssetDatabase.Refresh();
        
        
        /* set.textureCompression = TextureImporterCompression.Uncompressed;
         set.sRGBTexture = false;
         set.mipmapEnabled = false;*/
        EditorUtility.SetDirty( set );
        set.SaveAndReimport();
    }
    
    
    
    
    bool DrawCell(Rect rect, Texture2D tex, string name, Texture2D refLut, bool selected, ISupportedPostComponent component)
    {   rect = Params.Shrink( rect, 2 );
        if (Event.current.type == EventType.Repaint) GUI.skin.window.Draw( rect, new GUIContent(), 0 );
        rect = Params.Shrink( rect, 2 );
        
        if (selected) EditorGUI.DrawRect( rect, Color.red );
        
        var tr = rect;
        var lr = tr;
        tr.height -= 16;
        tr = Params.Shrink( tr, 2 );
        if (tex) GUI.DrawTexture( tr, tex );
        else GUI.DrawTexture( tr, Texture2D.blackTexture );
        
        lr.y += lr.height -= 16;
        lr.height = 16;
        GUI.Label( lr, name );
        
        if (selected)
        {   lr.width /= 2;
            lr.x += lr.width;
            lr.width -= 10;
            var newAmount = GUI.HorizontalSlider( lr, component.LutAmount,  0, 1);
            if (newAmount != component.LutAmount)
            {   component.CREATE_UNDO( "set post blend" );
                component.LutAmount = newAmount;
                component.SET_DIRTY();
                SceneView.RepaintAll();
            }
        }
        
        var fav_r = rect;
        
        #if !P128
        var favcontains = favorites.ContainsKey( name );
        var SIZE = Math.Min(32, fav_r.height * 0.3f);
        fav_r.x += fav_r.width - SIZE - 5;
        fav_r.y += 5;
        fav_r.width = fav_r.height = SIZE;
        if (favcontains || selected || Event.current.control)
        {   GUI.DrawTexture( fav_r, favorites.ContainsKey( name ) ? favicon_enable : favicon_disable );
            EditorGUIUtility.AddCursorRect( fav_r, MouseCursor.Link );
            if (GUI.Button( fav_r, favContent, Params.Button ))
            {   if (favcontains) RemoveFavorite( name );
                else CreateFavorite( name );
            }
        }
        #endif
        //if (tex) Debug.Log( GetImportSetting( tex ) );
        if (refLut && GetImportSetting( refLut ) != null &&
                (GetImportSetting( refLut ).textureCompression != TextureImporterCompression.Uncompressed ||
                 GetImportSetting( refLut ).sRGBTexture ||
                 GetImportSetting( refLut ).mipmapEnabled || GetImportSetting( refLut ).anisoLevel != 0 || GetImportSetting( refLut ).wrapMode != TextureWrapMode.Clamp))
        {   fav_r.x = rect.x + 5;
            fav_r.width *= 2;
            if (GUI.Button( fav_r, new GUIContent( "Fix", "wrong texture import settings, click to fix" ) ))
            {   FixImportSetting( refLut );
            }
        }
        
        //  if (selected) GUI.DrawTexture( rect, Button.active.background );
        
        return GUI.Button( rect, "", Params.Button );
    }
    
    #if !P128
    GUIContent favContent = new GUIContent() {tooltip = "Add to favorites\nCONTROL+CLICK to add to favorites unselected LUTs"};
    #endif
    
    
    Texture2D TakeScreen(Camera camera, int resWidth, int resHeight)
    {   var rt = new RenderTexture(resWidth, resHeight, 24);
        rt.hideFlags = HideFlags.DontSave;
        var oldT = camera.targetTexture;
        camera.targetTexture = rt;
        var screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false, Params.OLD_VERSION );
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels( new Rect( 0, 0, resWidth, resHeight ), 0, 0 );
        camera.targetTexture = oldT;
        rt.DiscardContents();
        RenderTexture.active = null;
        DestroyImmediate( rt );
        screenShot.hideFlags = HideFlags.DontSave;
        screenShot.name = "EModules/" + Params.TITLE + "/ScreenShot";
        screenShot.Apply();
        return screenShot;
    }
    
    
    void INVOKE(string method)
    {   var m = this.GetType().GetMethod( method, (BindingFlags)int.MaxValue );
        if (m != null) m.Invoke( this, null );
    }
    
    void CreateMenu()
    {   var menu = new GenericMenu();
    
        menu.AddDisabledItem( new GUIContent( "Export LUT" ) );
        menu.AddDisabledItem( new GUIContent( "exports your current color settings to the one LUT file" ) );
        menu.AddSeparator( "" );
        menu.AddItem( new GUIContent( "Update HUE\\Bright\\Content sorting data for the '" + Params.USER_FOLDER + "' folder" ), false, () =>
        {   INVOKE( "RewriteHue" );
        } );
        
        menu.ShowAsContext();
        /*if (GUI.Button( lineR, Params.CONTENT( "Export LUT", "exports your current settings to the LUT file" ) )) {
          var m = this.GetType().GetMethod( "ExportLut", (BindingFlags)int.MaxValue );
          if (m != null) m.Invoke( this, null );
        }*/
    }
    
    
    void CreateCameraMenu()
    {   var menu = new GenericMenu();
        menu.AddItem( new GUIContent( "none" ), Params.overrideCamera == -1, () => Params.overrideCamera.Set(-1));
        menu.AddSeparator( "" );
        int i = 0;
        foreach (var c in Camera.allCameras)
        {   var cc = i;
            menu.AddItem( new GUIContent(c.name), Params.overrideCamera == i, () => Params.overrideCamera .Set(cc));
            i++;
        }
        menu.ShowAsContext();
    }
    
    
    
#pragma warning disable
    #if !P128
    string _disable_icon =
        "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";
    string _enable_icon =
        "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";
    string _neutrallut =
        "iVBORw0KGgoAAAANSUhEUgAABAAAAAAgCAIAAAADnJ3xAAACY0lEQVR4Ae3bXQsBURSG0TPlgv//Y7nznUQzuZnQsyQRM9lrv0O705nGcWzH5b67Pr4/X3jr+ZC5j01j+37OTw58OeHLy8cZ1j//7s+/P/+lhK+fn3/3X/v7u76+m0/+/O8Cc3+yC/+Afj9vOHN003B9ub5+9/rajMNwI0CAAAECBAgQIEAgImAAiDRamQQIECBAgAABAgQuApuxB0GAAAECBAgQIECAQEXACkCl0+okQIAAAQIECBAgcBYwAIgBAQIECBAgQIAAgZCAASDUbKUSIECAAAECBAgQsAdABggQIECAAAECBAiEBKwAhJqtVAIECBAgQIAAAQIGABkgQIAAAQIECBAgEBIwAISarVQCBAgQIECAAAECBgAZIECAAAECBAgQIBASsAk41GylEiBAgAABAgQIELACIAMECBAgQIAAAQIEQgIGgFCzlUqAAAECBAgQIEDAACADBAgQIECAAAECBEIC9gCEmq1UAgQIECBAgAABAlYAZIAAAQIECBAgQIBASMAAEGq2UgkQIECAAAECBAgYAGSAAAECBAgQIECAQEjAHoBQs5VKgAABAgQIECBAwAqADBAgQIAAAQIECBAICRgAQs1WKgECBAgQIECAAAEDgAwQIECAAAECBAgQCAkYAELNVioBAgQIECBAgAABm4BlgAABAgQIECBAgEBIwApAqNlKJUCAAAECBAgQIGAAkAECBAgQIECAAAECIQEDQKjZSiVAgAABAgQIECBgD4AMECBAgAABAgQIEAgJWAEINVupBAgQIECAAAECBAwAMkCAAAECBAgQIEAgJHACW5RAfHLgP0sAAAAASUVORK5CYII=";
    #endif
#pragma warning restore
        
        
}



public class CachedInt {
    string _key;
    int? _value;
    readonly int _defaultValue;
    
    public CachedInt(string key)
    {   this._key = "EModules/" + Params.TITLE + "/" + key;
        this._value = null;
        this._defaultValue = 0;
    }
    public CachedInt(string key, int value)
    {   this._key = "EModules/" + Params.TITLE + "/" + key;
        this._value = null;
        this._defaultValue = value;
    }
    
    public static implicit operator int(CachedInt cf) { return cf._value ?? (cf._value = EditorPrefs.GetInt( cf._key, cf._defaultValue )).Value; }
    public void Set(int value)
    {   if (this._value == value) return;
        this._value = value;
        EditorPrefs.SetInt( _key, value );
    }
}

public class CachedFloat {
    string _key;
    float? _value;
    readonly float _defaultValue;
    
    public CachedFloat(string key)
    {   this._key = "EModules/" + Params.TITLE + "/" + key;
        this._value = null;
        this._defaultValue = 0;
    }
    public CachedFloat(string key, float value)
    {   this._key = "EModules/" + Params.TITLE + "/" + key;
        this._value = null;
        this._defaultValue = value;
    }
    
    public static implicit operator float(CachedFloat cf) { return cf._value ?? (cf._value = EditorPrefs.GetFloat( cf._key, cf._defaultValue )).Value; }
    public void Set(float value)
    {   if (this._value == value) return;
        this._value = value;
        EditorPrefs.SetFloat( _key, value );
    }
}

public class CachedString {
    string _key;
    string _value;
    readonly string _defaultValue;
    
    public CachedString(string key)
    {   this._key = "EModules/" + Params.TITLE + "/" + key;
        this._value = null;
        this._defaultValue = "";
    }
    public CachedString(string key, string value)
    {   this._key = "EModules/" + Params.TITLE + "/" + key;
        this._value = null;
        this._defaultValue = value;
    }
    
    public static implicit operator string(CachedString cf) { return cf._value ?? (cf._value = EditorPrefs.GetString( cf._key, cf._defaultValue )); }
    public void Set(string value)
    {   if (this._value == value) return;
        this._value = value;
        EditorPrefs.SetString( _key, value );
    }
}
}

#endif
