using System;
namespace BWWalkthrough
{
    public interface IBWWalkthroughPage
    {
        void WalkThroughDidScroll(float position, float offset);
    }
}

