using System;
using System.IO;
using Terminal.Gui;
using YamlDotNet.Serialization;
using Attribute = Terminal.Gui.Attribute;

namespace Rdmp.Core.CommandLine;

/// <summary>
/// Stores console colors to use in the RDMP command line gui.
/// Primarily a singleton with implicit loading of settings from the ColorSettings.yaml
/// file (if present).  Allows end users to change the colors used if they do not work
/// well with their terminal or accessiblity needs etc.
/// </summary>
public class ColorSettings
{
    private static object oLockInstance = new object();
    private static ColorSettings _instance;
    public static ColorSettings Instance
    {
        get
        {
            lock (oLockInstance)
            {
                // if first time
                if (_instance == null)
                {
                    if (File.Exists("ColorSettings.yaml"))
                    {
                        try
                        {
                            var d = new Deserializer();
                            _instance = d.Deserialize<ColorSettings>(File.ReadAllText("ColorSettings.yaml"));
                            return _instance;
                        }
                        catch (Exception)
                        {
                            // could not load the yaml color settings, just use the default
                        }
                    }

                    _instance = new ColorSettings();
                }
                
                return _instance;
            }
        }
    }

    [YamlIgnore]
    public ColorScheme Red => new ColorScheme
    {
        Normal = new Attribute(RedForegroundNormal, RedBackgroundNormal),
        Focus = new Attribute(RedForegroundFocus, RedBackgroundFocus),
        Disabled = new Attribute(RedForegroundDisabled, RedBackgroundDisabled),
        HotFocus = new Attribute(RedForegroundHotFocus, RedBackgroundHotFocus),
    };     
    public Color RedForegroundNormal { get; set; }
    public Color RedBackgroundNormal { get; set; }
    public Color RedForegroundFocus { get; set; }
    public Color RedBackgroundFocus { get; set; }
    public Color RedForegroundDisabled { get; set; }
    public Color RedBackgroundDisabled { get; set; }
    public Color RedForegroundHotFocus { get; set; }
    public Color RedBackgroundHotFocus { get; set; }


    [YamlIgnore]
    public ColorScheme Yellow => new ColorScheme
    {
        Normal = new Attribute(YellowForegroundNormal, YellowBackgroundNormal),
        Focus = new Attribute(YellowForegroundFocus, YellowBackgroundFocus),
        Disabled = new Attribute(YellowForegroundDisabled, YellowBackgroundDisabled),
        HotFocus = new Attribute(YellowForegroundHotFocus, YellowBackgroundHotFocus),
    };
    public Color YellowForegroundNormal { get; set; }
    public Color YellowBackgroundNormal { get; set; }
    public Color YellowForegroundFocus { get; set; }
    public Color YellowBackgroundFocus { get; set; }
    public Color YellowForegroundDisabled { get; set; }
    public Color YellowBackgroundDisabled { get; set; }
    public Color YellowForegroundHotFocus { get; set; }
    public Color YellowBackgroundHotFocus { get; set; }


    [YamlIgnore]
    public ColorScheme White => new ColorScheme
    {
        Normal = new Attribute(WhiteForegroundNormal, WhiteBackgroundNormal),
        Focus = new Attribute(WhiteForegroundFocus, WhiteBackgroundFocus),
        Disabled = new Attribute(WhiteForegroundDisabled, WhiteBackgroundDisabled),
        HotFocus = new Attribute(WhiteForegroundHotFocus, WhiteBackgroundHotFocus),
    };
    public Color WhiteForegroundNormal { get; set; }
    public Color WhiteBackgroundNormal { get; set; }
    public Color WhiteForegroundFocus { get; set; }
    public Color WhiteBackgroundFocus { get; set; }
    public Color WhiteForegroundDisabled { get; set; }
    public Color WhiteBackgroundDisabled { get; set; }
    public Color WhiteForegroundHotFocus { get; set; }
    public Color WhiteBackgroundHotFocus { get; set; }

    public ColorSettings()
    {
        RedForegroundNormal = Color.Red;
        RedBackgroundNormal = Color.Black;
        RedForegroundFocus = Color.Red;
        RedBackgroundFocus = Color.DarkGray;
        RedForegroundDisabled = Color.Red;
        RedBackgroundDisabled = Color.Black;
        RedForegroundHotFocus = Color.Red;
        RedBackgroundHotFocus = Color.Black;

        YellowForegroundNormal = Color.BrightYellow;
        YellowBackgroundNormal = Color.Black;
        YellowForegroundFocus = Color.BrightYellow;
        YellowBackgroundFocus = Color.DarkGray;
        YellowForegroundDisabled = Color.BrightYellow;
        YellowBackgroundDisabled = Color.Black;
        YellowForegroundHotFocus = Color.BrightYellow;
        YellowBackgroundHotFocus = Color.Black;

        WhiteForegroundNormal = Color.White;
        WhiteBackgroundNormal = Color.Black;
        WhiteForegroundFocus = Color.White;
        WhiteBackgroundFocus = Color.DarkGray;
        WhiteForegroundDisabled = Color.White;
        WhiteBackgroundDisabled = Color.Black;
        WhiteForegroundHotFocus = Color.White;
        WhiteBackgroundHotFocus = Color.Black;
    }
}
