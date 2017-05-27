using System;
using System.Collections.Generic;
using CoreFoundation;
using Foundation;
using UIKit;

namespace BWWalkthrough
{
    [Register("BWWalkthroughViewController")]
    public class BWWalkthroughViewController : UIViewController, IUIScrollViewDelegate
    {
        private const int NSEC_PER_SEC = 1000000000;
        private List<UIViewController> controllers = new List<UIViewController>();
        private NSLayoutConstraint[] lastViewConstraint;

        public IBWWalkthroughViewControllerDelegate walkDelegate { get; set; }

        [Outlet]
        public UIPageControl PageControl { get; set; }

        [Outlet]
        public UIPageControl NextButton { get; set; }

        [Outlet]
        public UIPageControl PreviousButton { get; set; }

        [Outlet]
        public UIPageControl CloseButton { get; set; }

        public UIScrollView Scrollview { get; set; }

        public int CurrentPage
        {
            get
            {
                var page = (int)Math.Ceiling(Scrollview.ContentOffset.X / View.Bounds.Size.Width);
                return page;
            }
        }

        public UIViewController CurrentViewController
        {
            get
            {
                var currentPage = this.CurrentPage;
                return controllers[currentPage];
            }
        }

        public BWWalkthroughViewController(IntPtr handle) : base(handle)
        {
            Scrollview = new UIScrollView();
            Scrollview.ShowsHorizontalScrollIndicator = false;
            Scrollview.ShowsVerticalScrollIndicator = false;
            Scrollview.PagingEnabled = true;
        }

        public BWWalkthroughViewController(NSCoder coder) : base(coder)
        {
            Scrollview = new UIScrollView();
            Scrollview.ShowsHorizontalScrollIndicator = false;
            Scrollview.ShowsVerticalScrollIndicator = false;
            Scrollview.PagingEnabled = true;
        }

        public BWWalkthroughViewController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {
            Scrollview = new UIScrollView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (PageControl != null)
            {
                PageControl.TouchUpInside += (sender, e) =>
                {
                    this.PageControlDidTouch();
                };
            }

            Scrollview.Delegate = this;
            Scrollview.TranslatesAutoresizingMaskIntoConstraints = false;

            View.InsertSubview(Scrollview, 0);

            View.AddConstraints(
                NSLayoutConstraint.FromVisualFormat(
                    "V:|-0-[scrollview]-0-|",
                    0,
                    new NSDictionary(),
                    NSDictionary.FromObjectsAndKeys(
                        new NSObject[] { Scrollview },
                        new NSObject[] { new NSString("scrollview") }
                    )
                )
            );

            View.AddConstraints(
                NSLayoutConstraint.FromVisualFormat(
                    "H:|-0-[scrollview]-0-|",
                    0,
                    new NSDictionary(),
                    NSDictionary.FromObjectsAndKeys(
                        new NSObject[] { Scrollview },
                        new NSObject[] { new NSString("scrollview") }
                    )
                )
            );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            UpdateUI();

            if (PageControl != null)
            {
                PageControl.Pages = controllers.Count;
                PageControl.CurrentPage = 0;
            }
        }

        [Action("NextPage")]
        public void NextPage()
        {
            if ((CurrentPage + 1) < controllers.Count)
            {
                walkDelegate?.WalkthroughNextButtonPressed();
                GotoPage(CurrentPage + 1);
            }
        }

        [Action("PrevPage")]
        public void PreviousPage()
        {
            if (CurrentPage > 0)
            {
                walkDelegate?.WalkthroughPrevButtonPressed();
                GotoPage(CurrentPage - 1);
            }
        }

        [Action("Close")]
        public void Close()
        {
            walkDelegate?.WalkthroughCloseButtonPressed();
        }

        [Action("PageControlDidTouch")]
        public void PageControlDidTouch()
        {
            if (PageControl != null)
            {
                GotoPage(PageControl.CurrentPage);
            }
        }

        private void GotoPage(nint page)
        {
            if (page < controllers.Count)
            {
                var frame = Scrollview.Frame;

                frame.X = page * frame.Size.Width;

                Scrollview.ScrollRectToVisible(frame, true);
            }
        }

        public void AddViewController(UIViewController vc)
        {
            controllers.Add(vc);

            // Setup the viewController view
            vc.View.TranslatesAutoresizingMaskIntoConstraints = false;
            Scrollview.AddSubview(vc.View);

            var metridDict = new NSDictionary("w", vc.View.Bounds.Size.Width, "h", vc.View.Bounds.Size.Height);
            var viewDict = new NSDictionary("view", vc.View);

            // Constraints

            // - Generic cnst
            vc.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[view(h)]", 0, metridDict, viewDict));
            vc.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[view(w)]", 0, metridDict, viewDict));
            Scrollview.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-0-[view]|", 0, null, viewDict));

            // cnst for position: 1st element

            if (controllers.Count == 1)
            {
                Scrollview.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-0-[view]", 0, null, viewDict));
            }
            else
            {
                var previousVC = controllers[controllers.Count - 2];
                var previousView = previousVC?.View;

                if (previousView != null)
                {
                    var prevDict = new NSDictionary("previousView", previousView, "view", vc.View);
                    Scrollview.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[previousView]-0-[view]", 0, null, prevDict));

                    if (lastViewConstraint != null)
                    {
                        Scrollview.RemoveConstraints(lastViewConstraint);
                    }

                    lastViewConstraint = NSLayoutConstraint.FromVisualFormat("H:[view]-0-|", 0, null, viewDict);
                    Scrollview.AddConstraints(lastViewConstraint);
                }
            }
        }

        /** 
		  Update the UI to reflect the current walkthrough status
		 **/
        private void UpdateUI()
        {
            // Get the current page

            PageControl.CurrentPage = CurrentPage;

            // Notify delegate about the new page

            walkDelegate?.WalkthroughPageDidChange(CurrentPage);

            // Hide/Show navigation buttons

            if (NextButton != null)
            {
                if (CurrentPage == controllers.Count - 1)
                {
                    NextButton.Hidden = true;
                }

                else
                {
                    NextButton.Hidden = false;
                }
            }

            if (PreviousButton != null)
            {
                if (CurrentPage == 0)
                {
                    PreviousButton.Hidden = true;
                }
                else
                {
                    PreviousButton.Hidden = false;
                }
            }
        }

        // MARK: - Scrollview Delegate -
        [Export("scrollViewDidScroll:")]
        public void Scrolled(UIScrollView scrollView)
        {
            for (int i = 0; i < controllers.Count; i++)
            {
                var vc = controllers[i] as IBWWalkthroughPage;
                if (vc != null)
                {
                    var mx = ((Scrollview.ContentOffset.X + View.Bounds.Size.Width) - (View.Bounds.Size.Width * (i))) / View.Bounds.Size.Width;

                    // While sliding to the "next" slide (from right to left), the "current" slide changes its offset from 1.0 to 2.0 while the "next" slide changes it from 0.0 to 1.0
                    // While sliding to the "previous" slide (left to right), the current slide changes its offset from 1.0 to 0.0 while the "previous" slide changes it from 2.0 to 1.0
                    // The other pages update their offsets whith values like 2.0, 3.0, -2.0... depending on their positions and on the status of the walkthrough
                    // This value can be used on the previous, current and next page to perform custom animations on page's subviews.

                    // print the mx value to get more info.
                    //System.Diagnostics.Debug.Print($"{i}:{mx}");

                    // We animate only the previous, current and next page

                    if (mx < 2 && mx > -2.0)
                    {
                        vc.WalkThroughDidScroll((float)Scrollview.ContentOffset.X, (float)mx);
                    }
                }
            }
        }

        [Export("scrollViewDidEndDecelerating:")]
        public void DecelerationEnded(UIScrollView scrollView)
        {
            UpdateUI();
        }

        [Export("scrollViewDidEndScrollingAnimation:")]
        public void ScrollAnimationEnded(UIScrollView scrollView)
        {
            UpdateUI();
        }

        private void adjustOffsetForTransition()
        {
            var currentPage = this.CurrentPage;
            var popTime = new DispatchTime(DispatchTime.Now, (long)(NSEC_PER_SEC * 0.1) / NSEC_PER_SEC));
            DispatchQueue.MainQueue.DispatchAfter(popTime, () =>
             {
                 GotoPage(currentPage);
             });
        }

        public override void WillTransitionToTraitCollection(UITraitCollection traitCollection, IUIViewControllerTransitionCoordinator coordinator)
        {
            adjustOffsetForTransition();
        }

        public override void ViewWillTransitionToSize(CoreGraphics.CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            adjustOffsetForTransition();
        }
    }
}

