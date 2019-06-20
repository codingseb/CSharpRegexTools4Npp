/********************************** Module Header **********************************\
* Module Name:  SearchableTextControl.cs
* Project:      CSWPFSearchAndHighlightTextBlockControl
* Copyright (c) Microsoft Corporation.
*
* The SearchableTextControl.cs file defines a User Control Class in order to search for
* keyword and highlight it when the operation gets the result.
*
* This source is subject to the Microsoft Public License.
* See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
* All other rights reserved.
*
* THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
* EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
* MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
* 
* Modified By Coding Seb for the purpose of this project
\***********************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RegexDialog
{
    public class SearchableTextControl : Control
    {
        private TextBlock displayTextBlock;
        private TextPointer StartSelectPosition;
        private TextPointer EndSelectPosition;
        private bool isSelecting;

        public string SelectedText = "";

        public delegate void TextSelectedHandler(string SelectedText);
        public event TextSelectedHandler TextSelected;

        static SearchableTextControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableTextControl),
                new FrameworkPropertyMetadata(typeof(SearchableTextControl)));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (displayTextBlock != null && IsSelectable)
            {
                base.OnMouseLeftButtonDown(e);
                displayTextBlock.Focusable = false;
                displayTextBlock.IsHitTestVisible = false;
                ResetSelectionTextRange();
                Point mouseDownPoint = e.GetPosition(this);
                StartSelectPosition = displayTextBlock.GetPositionFromPoint(mouseDownPoint, true);
                isSelecting = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isSelecting && displayTextBlock != null && IsSelectable)
            {
                base.OnMouseMove(e);
                SelectTextFromMouseAction(e);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (displayTextBlock != null && IsSelectable && isSelecting)
            {
                base.OnMouseUp(e);
                SelectTextFromMouseAction(e);
                isSelecting = false;
            }
        }

        private void SelectTextFromMouseAction(MouseEventArgs e)
        {
            Point mouseUpPoint = e.GetPosition(this);
            EndSelectPosition = displayTextBlock.GetPositionFromPoint(mouseUpPoint, true);
            ResetSelectionTextRange();
            TextRange textRange = new TextRange(StartSelectPosition, EndSelectPosition);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, SystemColors.HighlightTextBrush);
            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, SystemColors.HighlightBrush);

            SelectedText = textRange.Text;
            TextSelected?.Invoke(SelectedText);
        }

        private void ResetSelectionTextRange()
        {
            if (displayTextBlock != null)
            {
                TextRange textRange = new TextRange(displayTextBlock.ContentStart, displayTextBlock.ContentEnd);
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Foreground);
                textRange.ApplyPropertyValue(TextElement.BackgroundProperty, Background);
            }
        }

        public bool IsSelectable
        {
            get { return (bool)GetValue(IsSelectableProperty); }
            set { SetValue(IsSelectableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelectable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectableProperty =
            DependencyProperty.Register("IsSelectable", typeof(bool), typeof(SearchableTextControl), new UIPropertyMetadata(false, OnIsSelectableChanged));

        private static void OnIsSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SearchableTextControl obj = d as SearchableTextControl;
            obj.ResetSelectionTextRange();
            obj.Cursor = obj.IsSelectable ? Cursors.IBeam : Cursors.Arrow;
        }

        /// <summary>
        /// Text sandbox which is used to get or set the value from a dependency property.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Real implementation about TextProperty which  registers a dependency property with 
        // the specified property name, property type, owner type, and property metadata. 
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SearchableTextControl),
            new UIPropertyMetadata(string.Empty,
              UpdateControlCallBack));

        /// <summary>
        /// HighlightBackground sandbox which is used to get or set the value from a dependency property,
        /// if it gets a value,it should be forced to bind to a Brushes type.
        /// </summary>
        public Brush HighlightBackground
        {
            get { return (Brush)GetValue(HighlightBackgroundProperty); }
            set { SetValue(HighlightBackgroundProperty, value); }
        }

        // Real implementation about HighlightBackgroundProperty which registers a dependency property 
        // with the specified property name, property type, owner type, and property metadata. 
        public static readonly DependencyProperty HighlightBackgroundProperty =
            DependencyProperty.Register("HighlightBackground", typeof(Brush), typeof(SearchableTextControl),
            new UIPropertyMetadata(Brushes.Yellow, UpdateControlCallBack));

        /// <summary>
        /// HighlightForeground sandbox which is used to get or set the value from a dependency property,
        /// if it gets a value,it should be forced to bind to a Brushes type.
        /// </summary>
        public Brush HighlightForeground
        {
            get { return (Brush)GetValue(HighlightForegroundProperty); }
            set { SetValue(HighlightForegroundProperty, value); }
        }

        // Real implementation about HighlightForegroundProperty which registers a dependency property with
        // the specified property name, property type, owner type, and property metadata. 
        public static readonly DependencyProperty HighlightForegroundProperty =
            DependencyProperty.Register("HighlightForeground", typeof(Brush), typeof(SearchableTextControl),
            new UIPropertyMetadata(Brushes.Black, UpdateControlCallBack));

        /// <summary>
        /// IsMatchCase sandbox which is used to get or set the value from a dependency property,
        /// if it gets a value,it should be forced to bind to a bool type.
        /// </summary>
        public bool IsMatchCase
        {
            get { return (bool)GetValue(IsMatchCaseProperty); }
            set { SetValue(IsMatchCaseProperty, value); }
        }

        // Real implementation about IsMatchCaseProperty which  registers a dependency property with
        // the specified property name, property type, owner type, and property metadata. 
        public static readonly DependencyProperty IsMatchCaseProperty =
            DependencyProperty.Register("IsMatchCase", typeof(bool), typeof(SearchableTextControl),
            new UIPropertyMetadata(true, UpdateControlCallBack));

        /// <summary>
        /// IsHighlight sandbox which is used to get or set the value from a dependency property,
        /// if it gets a value,it should be forced to bind to a bool type.
        /// </summary>
        public bool IsHighlight
        {
            get { return (bool)GetValue(IsHighlightProperty); }
            set { SetValue(IsHighlightProperty, value); }
        }

        // Real implementation about IsHighlightProperty which  registers a dependency property with
        // the specified property name, property type, owner type, and property metadata. 
        public static readonly DependencyProperty IsHighlightProperty =
            DependencyProperty.Register("IsHighlight", typeof(bool), typeof(SearchableTextControl),
            new UIPropertyMetadata(false, UpdateControlCallBack));

        /// <summary>
        /// FontStyle for Highlighting
        /// </summary>
        public FontStyle HighlightFontStyle
        {
            get { return (FontStyle)GetValue(HighlightFontStyleProperty); }
            set { SetValue(HighlightFontStyleProperty, value); }
        }

        public static readonly DependencyProperty HighlightFontStyleProperty =
            DependencyProperty.Register("HighlightFontStyle", typeof(FontStyle), typeof(SearchableTextControl),
            new UIPropertyMetadata(FontStyles.Normal, UpdateControlCallBack));

        /// <summary>
        /// FontWeight for Highlighting
        /// </summary>
        public FontWeight HighlightFontWeight
        {
            get { return (FontWeight)GetValue(HighlightFontWeightProperty); }
            set { SetValue(HighlightFontWeightProperty, value); }
        }

        public static readonly DependencyProperty HighlightFontWeightProperty =
            DependencyProperty.Register("HighlightFontWeight", typeof(FontWeight), typeof(SearchableTextControl),
            new UIPropertyMetadata(FontWeights.Bold, UpdateControlCallBack));

        public bool IsMatchAccents
        {
            get { return (bool)GetValue(IsMatchAccentsProperty); }
            set { SetValue(IsMatchAccentsProperty, value); }
        }

        public static readonly DependencyProperty IsMatchAccentsProperty =
            DependencyProperty.Register("IsMatchAccents", typeof(bool), typeof(SearchableTextControl),
            new UIPropertyMetadata(true, UpdateControlCallBack));

        /// <summary>
        /// SearchText sandbox which is used to get or set the value from a dependency property,
        /// if it gets a value,it should be forced to bind to a string type.
        /// </summary>
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        /// <summary>
        /// Real implementation about SearchTextProperty which registers a dependency property with
        /// the specified property name, property type, owner type, and property metadata. 
        /// </summary>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(SearchableTextControl),
            new UIPropertyMetadata(string.Empty, UpdateControlCallBack));

        /// <summary>
        /// Create a call back function which is used to invalidate the rendering of the element, 
        /// and force a complete new layout pass.
        /// One such advanced scenario is if you are creating a PropertyChangedCallback for a 
        /// dependency property that is not  on a Freezable or FrameworkElement derived class that 
        /// still influences the layout when it changes.
        /// </summary>
        private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SearchableTextControl obj = d as SearchableTextControl;
            obj.InvalidateVisual();
        }

        /// <summary>
        /// override the OnRender method which is used to search for the keyword and highlight
        /// it when the operation gets the result.
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Define a TextBlock to hold the search result.
            displayTextBlock = Template.FindName("PART_TEXT", this) as TextBlock;

            if (string.IsNullOrEmpty(Text))
            {
                base.OnRender(drawingContext);

                return;
            }
            if (!IsHighlight)
            {
                displayTextBlock.Text = Text;
                base.OnRender(drawingContext);

                return;
            }

            displayTextBlock.Inlines.Clear();
            string searchstring = IsMatchCase ? SearchText : SearchText.ToUpper();

            string compareText = IsMatchCase ? Text : Text.ToUpper();

            searchstring = IsMatchAccents ? searchstring.RemoveAccents() : searchstring;
            compareText = IsMatchAccents ? compareText.RemoveAccents() : compareText;

            string displayText = Text;

            Run run;
            while (!string.IsNullOrEmpty(searchstring) && compareText.IndexOf(searchstring) >= 0)
            {
                int position = compareText.IndexOf(searchstring);
                run = GenerateRun(displayText.Substring(0, position), false);

                if (run != null)
                {
                    displayTextBlock.Inlines.Add(run);
                }

                run = GenerateRun(displayText.Substring(position, searchstring.Length), true);

                if (run != null)
                {
                    displayTextBlock.Inlines.Add(run);
                }

                compareText = compareText.Substring(position + searchstring.Length);
                displayText = displayText.Substring(position + searchstring.Length);
            }

            run = GenerateRun(displayText, false);

            if (run != null)
            {
                displayTextBlock.Inlines.Add(run);
            }

            base.OnRender(drawingContext);
        }

        /// <summary>
        /// Set inline-level flow content element intended to contain a run of formatted or unformatted 
        /// text into your background and foreground setting.
        /// </summary>
        private Run GenerateRun(string searchedString, bool isHighlight)
        {
            if (!string.IsNullOrEmpty(searchedString))
            {
                return new Run(searchedString)
                {
                    Background = isHighlight ? HighlightBackground : Background,
                    Foreground = isHighlight ? HighlightForeground : Foreground,

                    FontStyle = isHighlight ? HighlightFontStyle : FontStyle,

                    // Set the source text with the style which is Bold.
                    FontWeight = isHighlight ? HighlightFontWeight : FontWeight,
                };
            }
            return null;
        }
    }
}