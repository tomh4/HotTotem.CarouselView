using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

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
        private static readonly float SlowDownThreshold = 2f;
        HorizontalScrollView _scrollView;
        bool isCurrentlyTouched = false;
        bool hasSnapped = true;
        bool isScrolling = false;
        int scrollDirection = 0;
        CustomScrollView currentScrollView;

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
                _scrollView.Touch += _scrollView_Touch;
                _scrollView.ScrollChange += _scrollView_ScrollChange;
            }

        }

        private async void _scrollView_ScrollChange(object sender, ScrollChangeEventArgs e)
        {
            var scrollValue = e.ScrollX - e.OldScrollX;
            if(scrollValue > 0)
            {
                scrollDirection = 1;
            }
            else if (scrollValue < 0)
            {
                scrollDirection = -1;
            }
            else
            {
                scrollDirection = 0;
            }
            if (Math.Abs(scrollValue) < SlowDownThreshold)
            {
                isScrolling = false;
                if (!isCurrentlyTouched && !hasSnapped && currentScrollView.carouselParent.SnapMode == Carousel.SnappingMode.RollOut)
                {
                    hasSnapped = true;
                    await((CustomScrollView)Element).carouselParent.Snap();
                }
            }
            else
            {
                isScrolling = true;
            }

        }

        private async void _scrollView_Touch(object sender, TouchEventArgs e)
        {
            e.Handled = false;
            switch (e.Event.Action)
            {
                case MotionEventActions.Move:
                    hasSnapped = false;
                    isCurrentlyTouched = true;
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    isCurrentlyTouched = false;
                    // Not always triggered, play around with scrolling speed
                    if (currentScrollView.carouselParent.SnapMode == Carousel.SnappingMode.RollOut)
                    {
                        if (!isScrolling)
                        {
                            hasSnapped = true;
                            await ((CustomScrollView)Element).carouselParent.Snap();
                        }
                    }
                    else
                    {
                        if(currentScrollView.carouselParent.SnapMode == Carousel.SnappingMode.Instant)
                        {
                            hasSnapped = true;
                            await ((CustomScrollView)Element).carouselParent.Snap(scrollDirection);
                        }
                    }
                    break;
            }
        }
    }
}    
