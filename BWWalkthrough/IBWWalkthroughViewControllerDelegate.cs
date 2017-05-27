using System;
namespace BWWalkthrough
{
    public interface IBWWalkthroughViewControllerDelegate
    {
        void WalkthroughCloseButtonPressed();
        void WalkthroughNextButtonPressed();
        void WalkthroughPrevButtonPressed();
        void WalkthroughPageDidChange(int pageNumber);
    }
}

