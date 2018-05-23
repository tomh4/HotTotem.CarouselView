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
        bool isScrolling = false;
        int scrollDirection = 0;
        CustomScrollView currentScrollView;
        private GestureDetector _detector;
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
                var listener = new GestureListener();
                //listener.OnTouchFlinged += Listener_OnTouchFlinged;
                listener.OnTouchScrolled += Listener_OnTouchScrolled;
                _detector = new GestureDetector(this.Context, listener);
                _scrollView.HorizontalScrollBarEnabled = false;
                //_scrollView.Touch += _scrollView_Touch;
                _scrollView.ScrollChange += _scrollView_ScrollChange;
                _scrollView.SmoothScrollingEnabled = true;
            }

        }
        private void Listener_OnTouchScrolled(object source, ScrollEventArgs e)
        {
            hasSnapped = false;
            isScrolling = true;
            if(e.distX < 0)
            {
                scrollDirection = -1;
            }
            else
            {
                scrollDirection = 1;
            }
        }
        
        private async void Listener_OnTouchFlinged(object source, FlingEventArgs e)
        {
            // Not always triggered, play around with scrolling speed
            //if (currentScrollView.carouselParent.SnapMode == Carousel.SnappingMode.Instant && !hasSnapped)
            //{
            //    hasSnapped = true;
            //    if (e.velX < 0)
            //    {
            //        await ((CustomScrollView)Element).carouselParent.Snap(1);
            //    }
            //    else if (e.velX > 0)
            //    {
            //        await ((CustomScrollView)Element).carouselParent.Snap(-1);
            //    }
            //    else
            //    {
            //        await ((CustomScrollView)Element).carouselParent.Snap(0);
            //    }
            //}
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
            _detector.OnTouchEvent(ev);
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
        private async void _scrollView_Touch(object sender, TouchEventArgs e)
        {
            //e.Handled = false;
            //switch (e.Event.Action)
            //{
            //    case MotionEventActions.Move:
            //        hasSnapped = false;
            //        isCurrentlyTouched = true;
            //        break;
            //    case MotionEventActions.Cancel:
            //    case MotionEventActions.Up:
            //        isCurrentlyTouched = false;
            //        // Not always triggered, play around with scrolling speed
            //        if (currentScrollView.carouselParent.SnapMode == Carousel.SnappingMode.RollOut)
            //        {
            //            if (!isScrolling && !hasSnapped)
            //            {
            //                hasSnapped = true;
            //                await ((CustomScrollView)Element).carouselParent.Snap();
            //            }
            //        }
            //        else
            //        {
            //            if(currentScrollView.carouselParent.SnapMode == Carousel.SnappingMode.Instant && !hasSnapped)
            //            {
            //                hasSnapped = true;
            //                _scrollView.SmoothScrollTo(currenScrollPositionX, 0);
            //                await ((CustomScrollView)Element).carouselParent.Snap(scrollDirection);
            //            }
            //        }
            //        break;
            //}
        }
    }
    delegate void FlingEventHandler(object source, FlingEventArgs e);
    class FlingEventArgs : EventArgs
    {
        public MotionEvent ev1;
        public MotionEvent ev2;
        public float velX;
        public float velY;
    }
    delegate void ScrollEventHandler(object source, ScrollEventArgs e);
    class ScrollEventArgs : EventArgs
    {
        public MotionEvent ev1;
        public MotionEvent ev2;
        public float distX;
        public float distY;
    }
    class GestureListener : GestureDetector.SimpleOnGestureListener
    {
        public event FlingEventHandler OnTouchFlinged;
        public event ScrollEventHandler OnTouchScrolled;
        public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            OnTouchFlinged?.Invoke(this, new FlingEventArgs()
            {
                ev1 = e1,
                ev2 = e2,
                velX = velocityX,
                velY = velocityY

            });
            return base.OnFling(e1, e2, velocityX, velocityY);
        }
        public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            OnTouchScrolled?.Invoke(this, new ScrollEventArgs()
            {
                ev1 = e1,
                ev2 = e2,
                distX = distanceX,
                distY = distanceY

            });
            return base.OnScroll(e1, e2, distanceX, distanceY);
        }
    }
}    
