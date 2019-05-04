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
public partial class PostPresets1000Window : EditorWindow {
    void ExportLut()
    {
    
    }
    
    void RewriteHue()
    {   var gradients = GetGradientsPathsList( Params.USER_FOLDER );
        CreateCompareFile( ".LUTs User.data", gradients );
        
        ResetWindowAndCLearCache();
    }
    
    
    
    
    
    
    void InitializeFavorites()
    {   favorites.Clear();
        var path = Params.EditorResourcesPath + "/.Favorites.data";
        if (File.Exists( path ))
        {   using (StreamReader sr = new StreamReader( path ))
            {   while (!sr.EndOfStream)
                {   var f = sr.ReadLine();
                    if (!favorites.ContainsKey( f )) favorites.Add( f, 0 );
                }
            }
        }
    }
    
    
    
    static Texture2D texCopy;
    
    static public void CreateCompareFile(string fileName, Texture2D[] gradients)
    {   if (gradients.Length == 0) return;
    
        var first = GetReadableTexture(gradients[0]);
        if (!first) return;
        StringBuilder result = new StringBuilder();
        EditorUtility.DisplayProgressBar( "CreateCompareFile", "Converted:", 0 );
        int i = 0;
        
        foreach (var _gr in gradients)
        {   var gr = GetReadableTexture(_gr);
        
            if (!gr) return;
            
            result.Append( _gr.name );
            result.Append( ":" );
            result.Append( CALC_BRIGHT( gr ) );
            result.Append( ":" );
            result.Append( CALC_HUE( gr ) );
            result.Append( ":" );
            result.Append( CALC_FIRSTDIFF( gr, first ) );
            result.Append( "\n" );
            
            EditorUtility.DisplayProgressBar( "CreateCompareFile", "Converted: " + (i + 1) + " / " + gradients.Length, ++i / (float)gradients.Length );
        }
        EditorUtility.ClearProgressBar();
        
        File.WriteAllText( EModules.PostPresets.Params.EditorResourcesPath + "/" + fileName, result.ToString().TrimEnd( '\n' ) );
    }
    
    
    static Texture2D GetReadableTexture(Texture2D _gr)
    {   var path = AssetDatabase.GetAssetPath(_gr);
        if (string.IsNullOrEmpty( path )) return _gr;
        if (!((TextureImporter)TextureImporter.GetAtPath( path )).isReadable)
        {
        
            /*if (((TextureImporter)TextureImporter.GetAtPath( path )).textureCompression != TextureImporterCompression.Uncompressed) {
              throw new Exception( "'" + path + "' compressed texture can't be read, turn off compression or enable isReadable options" );
              return null;
            }*/
            
            if (texCopy == null) texCopy = new Texture2D( _gr.width, _gr.height, _gr.format, _gr.mipmapCount > 1, Params.OLD_VERSION );
            texCopy.LoadRawTextureData( _gr.GetRawTextureData() );
            texCopy.Apply();
            return texCopy;
        }
        return _gr;
    }
    
    static double CALC_BRIGHT(Texture2D t)
    {   var data = t. GetPixels();
        List<double>list_b = new List<double>();
        
        for (int i = 0 ; i < data.Length ; i += 4)
        {   var  r = data[i].r;
            var  g = data[i].g;
            var  b = data[i].b;
            
            list_b.Add( GET_SATURATE( r, g, b ) );
        }
        
        return list_b.Sum();
    }
    
    static double GET_SATURATE(double r, double g, double b)
    {   var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var d = max - min;
        if (d == 0) return 0;
        var l  = (max + min) / 2;
        return l > 0.5 ? d / (2 - max - min) : d / (max + min);
    }
    
    static double CALC_HUE(Texture2D t)
    {   var data = t. GetPixels();
        List<double>list_r = new List<double>();
        
        for (int i = 0 ; i < data.Length ; i += 4)
        {   var r = data[i].r;
            var g = data[i].g;
            var b = data[i].b;
            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));
            float dif = max - min;
            
            if (dif == 0) continue;
            
            list_r.Add( r - g + g - b );
        }
        
        if (list_r.Count == 0 && !t.name.Contains( "B&W" )) Debug.LogError( "hue == 0 " + t.name );
        
        return list_r.Count == 0 ? 0 : list_r.Sum();
    }
    
    static double GET_HUE(double r, double g, double b)
    {   var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var d = max - min;
        
        if (d == 0) return 0;
        
        if (r == max) return (g - b) / d + (g < b ? 6 : 0);
        if (g == max) return (b - r) / d + 2;
        if (b == max) return (r - g) / d + 4;
        
        return 0;
    }
    
    static double CALC_FIRSTDIFF(Texture2D t, Texture2D first)
    {   var sourceData = t. GetPixels();
        var firstData = first. GetPixels();
        
        List<double>dif = new List<double>();
        for (int i = 0 ; i < sourceData.Length ; i++)
        {   dif.Add( GetLum( sourceData[i] ) - GetLum( firstData[i] ) );
        }
        
        return dif.Sum();
    }
    
    static double GetLum(Color c)
    {   return (c.r + c.g + c.b) / 3;
    }
}
}
#endif
