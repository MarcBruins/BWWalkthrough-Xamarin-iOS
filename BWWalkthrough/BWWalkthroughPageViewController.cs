using System;
using System.Collections.Generic;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BWWalkthrough
{
	[Register("BWWalkthroughPageViewController")]
	public class BWWalkthroughPageViewController : UIViewController, IBWWalkthroughPage
	{
		private WalkthroughAnimationType animation = WalkthroughAnimationType.Linear;

		private List<CGPoint> subsWeights;

		private List<int> notAnimatableViews = new List<int>();// Array of views' tags that should not be animated during the scroll/transition

		private CGPoint _speed = new CGPoint(0, 0);
		[Export("Speed"), Browsable(true)]
		public CGPoint Speed
		{
			get
			{
				return _speed;
			}
			set
			{
				_speed = value;
			}
		}

		[Export("SpeedVariance"), Browsable(true)]
		public CGPoint SpeedVariance
		{
			get;
			set;
		} = new CGPoint(0, 0);

		[Export("AnimationType"), Browsable(true)]
		public String AnimationType
		{
			get { return this.AnimationType.ToString(); }
			set { this.AnimationType = value; }
		}

		[Export("AnimateAlpha"), Browsable(true)]
		public bool AnimateAlpha
		{
			get;
			set;
		} = false;


		//TODO: A comma separated list of tags that you don't want to animate during the transition/scroll 
		[Export("StaticTags"), Browsable(true)]
		public string StaticTags
		{
			get;
			set;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			this.View.Layer.MasksToBounds = true;

			subsWeights = new List<CGPoint>();

			foreach (var v in View.Subviews)
			{
				_speed.X += SpeedVariance.X;
				_speed.Y += SpeedVariance.Y;

				if (!notAnimatableViews.Contains((int)v.Tag))
				{
					subsWeights.Add(Speed);
				}
			}

		}
		public void WalkThroughDidScroll(float position, float offset)
		{
			for (int i = 0; i < subsWeights.Count; i++)
			{
				switch (animation)
				{
					case WalkthroughAnimationType.Linear:
						animationLinear(i, offset);
						break;
					case WalkthroughAnimationType.InOut:
						animationInOut(i, offset);
						break;
					case WalkthroughAnimationType.Zoom:
						animationZoom(i, offset);
						break;
					case WalkthroughAnimationType.Curve:
						animationCurve(i, offset);
						break;
				}

				// Animate alpha
				if (AnimateAlpha)
					animationAlpha(i, offset);
			}
		}

		void animationAlpha(int i, float offset)
		{
			var cView = View.Subviews[i];
			if (cView != null)
			{
				var mutableOffset = offset;
				if (mutableOffset > 1.0)
				{
					mutableOffset = (float)(1.0 + (1.0 - mutableOffset));
				}
				cView.Alpha = (mutableOffset);
			}
		}

		void animationCurve(int i, float offset)
		{
			var transform = CATransform3D.Identity;

			var x = (float)(1.0 - offset) * 10;
			transform = CATransform3D.MakeTranslation((System.nfloat)((Math.Pow(x, 3) - (x * 25)) * subsWeights[i].X), (System.nfloat)((Math.Pow(x, 3) - (x * 20)) * subsWeights[i].Y), 0);
			applyTransform(i, transform);
		}

		void animationZoom(int i, float offset)
		{
			var transform = CATransform3D.Identity;
			var tmpOffset = offset;

			if (tmpOffset > 1.0)
			{
					tmpOffset = (float)(1.0 + (1.0 - tmpOffset));
			}
			var scale = (float)(1.0 - tmpOffset);
			//TODO: Check --> CATransform3DScale(transform, 1 - scale, 1 - scale, 1.0);
			transform = CATransform3D.MakeScale(1 - scale, 1 - scale, (System.nfloat)1.0);
			applyTransform(i, transform);
		}

		void animationInOut(int i, float offset)
		{
			var transform = CATransform3D.Identity;
			var tmpOffset = offset;

			if (tmpOffset > 1.0)
			{
				tmpOffset = (float)(1.0 + (1.0 - tmpOffset));
			}
			transform = CATransform3D.MakeTranslation((System.nfloat)((1.0 - tmpOffset) * subsWeights[i].X * 100), (System.nfloat)((1.0 - tmpOffset) * subsWeights[i].Y * 100), 0);
			applyTransform(i, transform);
		}

		void animationLinear(int i, float offset)
		{
			var transform = CATransform3D.Identity;
			var mx = (float)(1.0 - offset) * 100;

			transform = CATransform3D.MakeTranslation(mx * subsWeights[i].X, mx * subsWeights[i].Y, 0);
			applyTransform(i, transform);
		}

		void applyTransform(int i, CATransform3D transform)
		{
			var subview = View.Subviews[i];

			if (!notAnimatableViews.Contains((int)subview.Tag))
			{
				View.Subviews[i].Layer.Transform = transform;
			}
		}
	}
}

