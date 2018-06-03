using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Supaplex
{
    class ScrollViewerBehaviour
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.RegisterAttached("VerticalOffsetProperty", typeof(double),
        typeof(ScrollViewerBehaviour), new PropertyMetadata(OnVerticalOffsetProprtyChanged));

        //private static System.Timers.Timer _scrollTimer = new Timer(200);
        //private static ScrollViewer _scrollViewer;
        //private static double _newOffset;
        //private static double _currentOffset;
        private static void OnVerticalOffsetProprtyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            if(scrollViewer != null)
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            //_scrollViewer = d as ScrollViewer;
            //_newOffset = (double) e.NewValue;
            //_currentOffset = _scrollViewer.VerticalOffset;        
            //_scrollTimer.Elapsed += VerticalScrollingReplace;
            //_scrollTimer.Start();
        }

        //private static void VerticalScrollingReplace(object sender, ElapsedEventArgs e)
        //{
        //    if (_currentOffset == _newOffset)
        //    {
        //        _scrollTimer.Stop();
        //        return;
        //    }
        //    _currentOffset += 1;
        //    if(_scrollViewer != null)
        //        _scrollViewer.ScrollToVerticalOffset(_currentOffset);
        //}



        private static double GetVerticalOffsetProperty(UIElement element)
        {
            return (double)element.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffsetProperty(UIElement element, double value)
        {
            element.SetValue(VerticalOffsetProperty, value);
        }

        public static readonly DependencyProperty HorisontalOffsetProperty =
        DependencyProperty.RegisterAttached("HorisontalOffsetProperty", typeof(double),
        typeof(ScrollViewerBehaviour), new PropertyMetadata(OnHorisontalOffsetProprtyChanged));

        private static void OnHorisontalOffsetProprtyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            if(scrollViewer != null)
                scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
        }

        private static double GetHorisontalOffsetProperty(UIElement element)
        {
            return (double)element.GetValue(VerticalOffsetProperty);
        }

        public static void SetHorisontalOffsetProperty(UIElement element, double value)
        {
            element.SetValue(VerticalOffsetProperty, value);
        }

    }
}
