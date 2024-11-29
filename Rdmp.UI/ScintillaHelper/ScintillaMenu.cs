// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode.Settings;
using ScintillaNET;
using WeCantSpell.Hunspell;

namespace Rdmp.UI.ScintillaHelper;

[DesignerCategory("")]
internal class ScintillaMenu : ContextMenuStrip
{
    private readonly Scintilla _scintilla;
    private ToolStripMenuItem _miUndo;
    private ToolStripMenuItem _miRedo;
    private ToolStripMenuItem _miCut;
    private ToolStripMenuItem _miCopy;
    private ToolStripMenuItem _miDelete;
    private ToolStripMenuItem _miSelectAll;
    private ToolStripMenuItem _miWordwrap;
    private ToolStripMenuItem _miCheckSpelling;
    private ToolStripMenuItem _miSpelling;

    /// <summary>
    /// Spell checker for the hosted control.  If set then right clicks will spell check the word
    /// under the caret and show suggestions
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public WordList Hunspell { get; set; }

    public ScintillaMenu(Scintilla scintilla, bool spellCheck) : base()
    {
        _scintilla = scintilla;
        InitContextMenu(spellCheck);
    }

    private void InitContextMenu(bool spellCheck)
    {
        _miUndo = new ToolStripMenuItem("Undo", null, (s, ea) => _scintilla.Undo());
        Items.Add(_miUndo);
        _miRedo = new ToolStripMenuItem("Redo", null, (s, ea) => _scintilla.Redo());

        Items.Add(_miRedo);

        if (spellCheck)
        {
            _miCheckSpelling = new ToolStripMenuItem("Check Spelling", null,
                (s, ea) => ScintillaTextEditorFactory.CheckSpelling(_scintilla, Hunspell))
            {
                ShortcutKeys = Keys.F7
            };

            Items.Add(_miCheckSpelling);
        }

        Items.Add(new ToolStripSeparator());

        _miWordwrap = new ToolStripMenuItem("Word Wrap");
        foreach (WrapMode mode in Enum.GetValues(typeof(WrapMode)))
        {
            var mi = new ToolStripMenuItem(mode.ToString(), null, SetWordWrapMode)
            {
                Tag = mode,
                Checked = _scintilla.WrapMode == mode
            };
            _miWordwrap.DropDownItems.Add(mi);
        }

        _miSpelling = new ToolStripMenuItem("Spelling");
        Items.Add(_miSpelling);

        Items.Add(_miWordwrap);

        Items.Add(new ToolStripSeparator());

        _miCut = new ToolStripMenuItem("Cut", null, (s, ea) => _scintilla.Cut());
        Items.Add(_miCut);
        _miCopy = new ToolStripMenuItem("Copy", null, (s, ea) => _scintilla.Copy());
        Items.Add(_miCopy);
        Items.Add(new ToolStripMenuItem("Paste", null, (s, ea) => _scintilla.Paste()));
        _miDelete = new ToolStripMenuItem("Delete", null, (s, ea) => _scintilla.ReplaceSelection(""));
        Items.Add(_miDelete);
        Items.Add(new ToolStripSeparator());

        _miSelectAll = new ToolStripMenuItem("Select All", null, (s, ea) => _scintilla.SelectAll());
        Items.Add(_miSelectAll);
    }

    private void SetWordWrapMode(object sender, EventArgs e)
    {
        var mode = (WrapMode)((ToolStripMenuItem)sender).Tag;
        UserSettings.WrapMode = (int)mode;
        _scintilla.WrapMode = mode;
    }

    protected override void OnOpening(CancelEventArgs e)
    {
        base.OnOpening(e);

        var textIsSelected = !string.IsNullOrWhiteSpace(_scintilla.SelectedText);

        _miUndo.Enabled = _scintilla.CanUndo;
        _miRedo.Enabled = _scintilla.CanRedo;
        _miCut.Enabled = textIsSelected;
        _miCopy.Enabled = textIsSelected;
        _miDelete.Enabled = textIsSelected;
        _miSelectAll.Enabled = _scintilla.TextLength > 0;

        //check the current wrap mode and uncheck the rest
        foreach (ToolStripMenuItem item in _miWordwrap.DropDownItems)
            item.Checked = (WrapMode)item.Tag == _scintilla.WrapMode;

        _miSpelling.DropDown.Items.Clear();
        _miSpelling.Enabled = false;

        // Only proceed if we are checking spelling
        if (Hunspell == null) return;
        //get current word
        var word = GetCurrentWord();

        if (string.IsNullOrWhiteSpace(word) || Hunspell.Check(word)) return;
        foreach (var suggested in Hunspell.Suggest(word))
        {
            var mi = new ToolStripMenuItem(suggested, null, (s, ev) => { SetWord(word, suggested); });
            _miSpelling.DropDownItems.Add(mi);
            _miSpelling.Enabled = true;
        }
    }

    private string GetCurrentWord()
    {
        var pos = _scintilla.CurrentPosition;

        var wordStart = _scintilla.WordStartPosition(pos, true);
        var wordEnd = _scintilla.WordEndPosition(pos, true);
        return _scintilla.GetTextRange(wordStart, wordEnd - wordStart);
    }

    private void SetWord(string oldWord, string newWord)
    {
        //make sure the current word matches the old word we are replacing
        //(I guess somehow an async something could have changed the text while the menu was open)
        if (!string.Equals(GetCurrentWord(), oldWord))
            return;

        var pos = _scintilla.CurrentPosition;
        var wordStart = _scintilla.WordStartPosition(pos, true);
        var wordEnd = _scintilla.WordEndPosition(pos, true);

        _scintilla.DeleteRange(wordStart, wordEnd - wordStart);
        _scintilla.InsertText(wordStart, newWord);
    }
}