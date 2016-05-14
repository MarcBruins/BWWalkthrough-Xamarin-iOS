using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace BWWalkthrough
{
	public enum WalkthroughAnimationType
	{
		Linear,
		Curve,
		Zoom,
		InOut
	}

	public class BWWalkthroughViewController : UIViewController, IUIScrollViewDelegate
	{
		public IBWWalkthroughViewControllerDelegate walkDelegate;

		[Outlet]
		public UIPageControl pageControl { get; set; }

		[Outlet]
		public UIPageControl nextButton { get; set; }

		[Outlet]
		public UIPageControl prevButton { get; set; }

		[Outlet]
		public UIPageControl closeButton { get; set; }

		public UIScrollView scrollview { get; set;}

		private List<UIViewController> controllers = new List<UIViewController>(); 
		private NSLayoutConstraint[] lastViewConstraint;


		public int CurrentPage
		{
			get
			{
				var page =  (int)Math.Ceiling(scrollview.ContentOffset.X / View.Bounds.Size.Width);
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

		public BWWalkthroughViewController(NSCoder coder) : base(coder)
		{
			scrollview = new UIScrollView();
			scrollview.ShowsHorizontalScrollIndicator = false;
			scrollview.ShowsVerticalScrollIndicator = false;
			scrollview.PagingEnabled = true;
		}

		public BWWalkthroughViewController(string nibName, NSBundle bundle):base(nibName, bundle)
		{
			scrollview = new UIScrollView();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			pageControl.TouchUpInside += (sender,e) =>
			{
				this.PageControlDidTouch();
			};

			scrollview.Delegate = this;
			scrollview.TranslatesAutoresizingMaskIntoConstraints = false;

			View.InsertSubview(scrollview, 0);


			View.AddConstraints(
				NSLayoutConstraint.FromVisualFormat(
					"V:|-0-[scrollview]-0-|",
					0,
					new NSDictionary(),
					NSDictionary.FromObjectsAndKeys(
						new NSObject[] { scrollview },
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
						new NSObject[] { scrollview },
						new NSObject[] { new NSString("scrollview") }
					)
				)
			);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			if (pageControl != null)
			{
				pageControl.Pages = controllers.Count;
				pageControl.CurrentPage = 0;
			}
		}


		[Action]
		public void NextPage()
		{
			if ((CurrentPage + 1) < controllers.Count) {
				walkDelegate?.WalkthroughNextButtonPressed();
				gotoPage(CurrentPage + 1);
			}
		}

		[Action]
		public void PreviousPage()
		{
			if (CurrentPage > 0) {
				walkDelegate?.WalkthroughPrevButtonPressed();
				gotoPage(CurrentPage - 1);
			}
		}

		[Action]
		public void Close(object sender)
		{
			walkDelegate?.WalkthroughCloseButtonPressed();
		}

		[Action]
		public void PageControlDidTouch()
		{
			if (pageControl != null)
			{
				gotoPage(pageControl.CurrentPage);
			}
		}

		private void gotoPage(nint page)
		{
			if (page < controllers.Count){
				var frame = scrollview.Frame;

				frame.X = page * frame.Size.Width;

				scrollview.ScrollRectToVisible(frame, true);
			}
		}


		public void AddViewController(UIViewController vc){
        
			controllers.Add(vc);

			// Setup the viewController view

			vc.View.TranslatesAutoresizingMaskIntoConstraints = false;
			scrollview.AddSubview(vc.View);

			var metridDict = new NSDictionary("w", vc.View.Bounds.Size.Width, "h", vc.View.Bounds.Size.Height);
			var viewDict = new NSDictionary("view", vc.View);

			// Constraints

			// - Generic cnst

			vc.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[view(h)]", 0, metridDict, viewDict));
			vc.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[view(w)]", 0, metridDict, viewDict));
			scrollview.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-0-[view]|", 0, null, viewDict));


			// cnst for position: 1st element

			if (controllers.Count == 1)
			{
				scrollview.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-0-[view]", 0, null, viewDict));
			}
			else
			{
				var previousVC = controllers[controllers.Count - 2];
				var previousView = previousVC?.View;

				if (previousView != null)
				{
					var prevDict = new NSDictionary("previousView", previousView,"view", vc.View);
					scrollview.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[previousView]-0-[view]", 0, null, prevDict));

					if (lastViewConstraint != null)
					{
						scrollview.RemoveConstraints(lastViewConstraint);
					}


					lastViewConstraint = NSLayoutConstraint.FromVisualFormat("H:[view]-0-|", 0, null, viewDict);
					scrollview.AddConstraints(lastViewConstraint);
				}
			}
		}
    }

}

