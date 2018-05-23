using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CarouselView.CustomControls;
using CarouselView.Droid.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
[assembly: ExportRenderer(typeof(CustomScrollView), typeof(CarouselRenderer))]
namespace CarouselView.Droid.CustomRenderers
{
    public class CarouselRenderer : ScrollViewRenderer
    {
        public CarouselRenderer() : base(Android.App.Application.Context)
        {

        }
        private static readonly float SlowDownThreshold = 20f;
        HorizontalScrollView _scrollView;
        bool isCurrentlyTouched = false;
        bool hasSnapped = true;
        int scrollDirection = 0;
        CustomScrollView currentScrollView;
        private int currenScrollPositionX;


        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            if (e.NewElement == null) return;
            currentScrollView = (CustomScrollView)e.NewElement;
            e.NewElement.PropertyChanged += ElementPropertyChanged;
        }
        
        void ElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Renderer")
            {
                _scrollView = (HorizontalScrollView)typeof(ScrollViewRenderer)
                    .GetField("_hScrollView", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(this);
                _scrollView.HorizontalScrollBarEnabled = false;
                _scrollView.ScrollChange += _scrollView_ScrollChange;
                _scrollView.SmoothScrollingEnabled = true;
            }

        }

        private async void _scrollView_ScrollChange(object sender, ScrollChangeEventArgs e)
        {
            var scrollValue = e.ScrollX - e.OldScrollX;
            currenScrollPositionX = e.ScrollX;
            if(isCurrentlyTouched == false && !hasSnapped)
            {
                if (currentScrollView.carouselParent.SnapMode == Carousel.SnappingMode.Instant)
                {
                    _scrollView.SmoothScrollBy(0, 0);
                    hasSnapped = true;
                    await ((CustomScrollView)Element).carouselParent.Snap(scrollDirection);
                }
                else if(currentScrollView.carouselParent.SnapMode == Carousel.SnappingMode.RollOut)
                {
                    if (Math.Abs(scrollValue) < SlowDownThreshold)
                    {
                        {
                            _scrollView.SmoothScrollBy(0, 0);
                            hasSnapped = true;
                            await ((CustomScrollView)Element).carouselParent.Snap();
                        }
                    }
                }
            }           
        }
        public override bool OnTouchEvent(MotionEvent ev)
        {
            switch (ev.Action)
            {
                case MotionEventActions.Move:
                    hasSnapped = false;
                    isCurrentlyTouched = true;
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    isCurrentlyTouched = false;
                    break;
            }
            return base.OnTouchEvent(ev);
        }
    }
}    
