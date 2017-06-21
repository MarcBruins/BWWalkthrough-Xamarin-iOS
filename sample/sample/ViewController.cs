using System;
using BWWalkthrough;
using UIKit;

namespace sample
{
    public partial class ViewController : UIViewController, IBWWalkthroughViewControllerDelegate
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            btnShow.TouchUpInside += (s, e) => { ShowWalkthrough(); };
        }

        BWWalkthroughViewController walkthrough;

        public void ShowWalkthrough()
        {
            // Get view controllers and build the walkthrough
            var stb = UIStoryboard.FromName("Walkthrough", null);

            walkthrough = stb.InstantiateViewController("walk") as BWWalkthroughViewController;
            var page_zero = stb.InstantiateViewController("walk0");
            var page_one = stb.InstantiateViewController("walk1");
            var page_two = stb.InstantiateViewController("walk2");
            var page_three = stb.InstantiateViewController("walk3");

            // Attach the pages to the master
            walkthrough.walkDelegate = this;

            walkthrough.AddViewController(page_one);
            walkthrough.AddViewController(page_two);
            walkthrough.AddViewController(page_three);
            walkthrough.AddViewController(page_zero);

            walkthrough.CurrentPage = 2;

            this.PresentViewController(walkthrough, true, null);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public void WalkthroughCloseButtonPressed()
        {
            this.DismissViewController(true, null);
        }

        public void WalkthroughNextButtonPressed()
        {
            //		throw new NotImplementedException();
        }

        public void WalkthroughPrevButtonPressed()
        {
            //	throw new NotImplementedException();
        }

        public void WalkthroughPageDidChange(int pageNumber)
        {
            //	throw new NotImplementedException();
        }
    }
}

