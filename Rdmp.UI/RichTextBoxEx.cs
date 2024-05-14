// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Rdmp.UI;

/// <summary>
/// Text box with support for hyperlinks.
/// </summary>
public partial class RichTextBoxEx : RichTextBox
{
    #region Interop-Defines

    [StructLayout(LayoutKind.Sequential)]
    private struct CHARFORMAT2_STRUCT
    {
        public uint cbSize;
        public uint dwMask;
        public uint dwEffects;
        public int yHeight;
        public int yOffset;
        public int crTextColor;
        public byte bCharSet;
        public byte bPitchAndFamily;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szFaceName;

        public ushort wWeight;
        public ushort sSpacing;
        public int crBackColor; // Color.ToArgb() -> int
        public int lcid;
        public int dwReserved;
        public short sStyle;
        public short wKerning;
        public byte bUnderlineType;
        public byte bAnimation;
        public byte bRevAuthor;
        public byte bReserved1;
    }

    [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "SendMessageW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private const int WM_USER = 0x0400;
    private const int EM_GETCHARFORMAT = WM_USER + 58;
    private const int EM_SETCHARFORMAT = WM_USER + 68;
    
    private const int SCF_SELECTION = 0x0001;
    private const int SCF_WORD = 0x0002;
    private const int SCF_ALL = 0x0004;

    #region CHARFORMAT2 Flags

    private const uint CFE_BOLD = 0x0001;
    private const uint CFE_ITALIC = 0x0002;
    private const uint CFE_UNDERLINE = 0x0004;
    private const uint CFE_STRIKEOUT = 0x0008;
    private const uint CFE_PROTECTED = 0x0010;
    private const uint CFE_LINK = 0x0020;
    private const uint CFE_AUTOCOLOR = 0x40000000;
    private const uint CFE_SUBSCRIPT = 0x00010000; /* Superscript and subscript are */
    private const uint CFE_SUPERSCRIPT = 0x00020000; /*  mutually exclusive			 */

    private const int CFM_SMALLCAPS = 0x0040; /* (*)	*/
    private const int CFM_ALLCAPS = 0x0080; /* Displayed by 3.0	*/
    private const int CFM_HIDDEN = 0x0100; /* Hidden by 3.0 */
    private const int CFM_OUTLINE = 0x0200; /* (*)	*/
    private const int CFM_SHADOW = 0x0400; /* (*)	*/
    private const int CFM_EMBOSS = 0x0800; /* (*)	*/
    private const int CFM_IMPRINT = 0x1000; /* (*)	*/
    private const int CFM_DISABLED = 0x2000;
    private const int CFM_REVISED = 0x4000;

    private const int CFM_BACKCOLOR = 0x04000000;
    private const int CFM_LCID = 0x02000000;
    private const int CFM_UNDERLINETYPE = 0x00800000; /* Many displayed by 3.0 */
    private const int CFM_WEIGHT = 0x00400000;
    private const int CFM_SPACING = 0x00200000; /* Displayed by 3.0	*/
    private const int CFM_KERNING = 0x00100000; /* (*)	*/
    private const int CFM_STYLE = 0x00080000; /* (*)	*/
    private const int CFM_ANIMATION = 0x00040000; /* (*)	*/
    private const int CFM_REVAUTHOR = 0x00008000;


    private const uint CFM_BOLD = 0x00000001;
    private const uint CFM_ITALIC = 0x00000002;
    private const uint CFM_UNDERLINE = 0x00000004;
    private const uint CFM_STRIKEOUT = 0x00000008;
    private const uint CFM_PROTECTED = 0x00000010;
    private const uint CFM_LINK = 0x00000020;
    private const uint CFM_SIZE = 0x80000000;
    private const uint CFM_COLOR = 0x40000000;
    private const uint CFM_FACE = 0x20000000;
    private const uint CFM_OFFSET = 0x10000000;
    private const uint CFM_CHARSET = 0x08000000;
    private const uint CFM_SUBSCRIPT = CFE_SUBSCRIPT | CFE_SUPERSCRIPT;
    private const uint CFM_SUPERSCRIPT = CFM_SUBSCRIPT;

    private const byte CFU_UNDERLINENONE = 0x00000000;
    private const byte CFU_UNDERLINE = 0x00000001;
    private const byte CFU_UNDERLINEWORD = 0x00000002; /* (*) displayed as ordinary underline	*/
    private const byte CFU_UNDERLINEDOUBLE = 0x00000003; /* (*) displayed as ordinary underline	*/
    private const byte CFU_UNDERLINEDOTTED = 0x00000004;
    private const byte CFU_UNDERLINEDASH = 0x00000005;
    private const byte CFU_UNDERLINEDASHDOT = 0x00000006;
    private const byte CFU_UNDERLINEDASHDOTDOT = 0x00000007;
    private const byte CFU_UNDERLINEWAVE = 0x00000008;
    private const byte CFU_UNDERLINETHICK = 0x00000009;
    private const byte CFU_UNDERLINEHAIRLINE = 0x0000000A; /* (*) displayed as ordinary underline	*/

    #endregion

    #endregion

    public RichTextBoxEx()
    {
        // Otherwise, non-standard links get lost when user starts typing
        // next to a non-standard link
        DetectUrls = false;
    }

    [DefaultValue(false)]
    public new bool DetectUrls
    {
        get => base.DetectUrls;
        set => base.DetectUrls = value;
    }

    /// <summary>
    /// Insert a given text as a link into the RichTextBox at the current insert position.
    /// </summary>
    /// <param name="text">Text to be inserted</param>
    public void InsertLink(string text)
    {
        InsertLink(text, SelectionStart);
    }

    /// <summary>
    /// Insert a given text at a given position as a link.
    /// </summary>
    /// <param name="text">Text to be inserted</param>
    /// <param name="position">Insert position</param>
    public void InsertLink(string text, int position)
    {
        if (position < 0 || position > Text.Length)
            throw new ArgumentOutOfRangeException(nameof(position));

        SelectionStart = position;
        SelectedText = text;
        Select(position, text.Length);
        SetSelectionLink(true);
        Select(position + text.Length, 0);
    }

    /// <summary>
    /// Insert a given text at at the current input position as a link.
    /// The link text is followed by a hash (#) and the given hyperlink text, both of
    /// them invisible.
    /// When clicked on, the whole link text and hyperlink string are given in the
    /// LinkClickedEventArgs.
    /// </summary>
    /// <param name="text">Text to be inserted</param>
    /// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
    public void InsertLink(string text, string hyperlink)
    {
        InsertLink(text, hyperlink, SelectionStart);
    }

    /// <summary>
    /// Insert a given text at a given position as a link. The link text is followed by
    /// a hash (#) and the given hyperlink text, both of them invisible.
    /// When clicked on, the whole link text and hyperlink string are given in the
    /// LinkClickedEventArgs.
    /// </summary>
    /// <param name="text">Text to be inserted</param>
    /// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
    /// <param name="position">Insert position</param>
    public void InsertLink(string text, string hyperlink, int position)
    {
        if (position < 0 || position > Rtf.Length)
            throw new ArgumentOutOfRangeException(nameof(position));

        //if it ends with whitespace then we have to put that outside the RTF
        var suffix = string.Concat(text.Reverse().TakeWhile(c => c == '\r' || c == '\n' || c == ' ' || c == '\t')
            .Reverse());

        SelectionStart = position;
        SelectedRtf = $@"{{\rtf1\ansi {text.TrimEnd()}\v #{hyperlink}\v0}}";
        Select(position, text.Length + hyperlink.Length + 1);
        SetSelectionLink(true);
        Select(position + text.Length + hyperlink.Length + 1, 0);

        //avoids bong
        if (suffix != "")
            SelectedText = suffix;
    }

    /// <summary>
    /// Set the current selection's link style
    /// </summary>
    /// <param name="link">true: set link style, false: clear link style</param>
    public void SetSelectionLink(bool link)
    {
        SetSelectionStyle(CFM_LINK, link ? CFE_LINK : 0);
    }

    /// <summary>
    /// Get the link style for the current selection
    /// </summary>
    /// <returns>0: link style not set, 1: link style set, -1: mixed</returns>
    public int GetSelectionLink() => GetSelectionStyle(CFM_LINK, CFE_LINK);


    private void SetSelectionStyle(uint mask, uint effect)
    {
        var cf = new CHARFORMAT2_STRUCT();
        cf.cbSize = (uint)Marshal.SizeOf(cf);
        cf.dwMask = mask;
        cf.dwEffects = effect;

        var wpar = new IntPtr(SCF_SELECTION);
        var lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
        Marshal.StructureToPtr(cf, lpar, false);

        var res = SendMessage(Handle, EM_SETCHARFORMAT, wpar, lpar);

        Marshal.FreeCoTaskMem(lpar);
    }

    private int GetSelectionStyle(uint mask, uint effect)
    {
        var cf = new CHARFORMAT2_STRUCT();
        cf.cbSize = (uint)Marshal.SizeOf(cf);
        cf.szFaceName = new char[32];

        var wpar = new IntPtr(SCF_SELECTION);
        var lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
        Marshal.StructureToPtr(cf, lpar, false);

        var res = SendMessage(Handle, EM_GETCHARFORMAT, wpar, lpar);

        cf = (CHARFORMAT2_STRUCT)Marshal.PtrToStructure(lpar, typeof(CHARFORMAT2_STRUCT));

        int state;
        // dwMask holds the information which properties are consistent throughout the selection:
        if ((cf.dwMask & mask) == mask)
            state = (cf.dwEffects & effect) == effect ? 1 : 0;
        else
            state = -1;

        Marshal.FreeCoTaskMem(lpar);
        return state;
    }
}