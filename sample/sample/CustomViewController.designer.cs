// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace sample
{
	[Register ("CustomViewController")]
	partial class CustomViewController
	{
		[Outlet]
		UIKit.UIImageView imageView { get; set; }

		[Outlet]
		UIKit.UILabel textLabel { get; set; }

		[Outlet]
		UIKit.UILabel titleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (titleLabel != null) {
				titleLabel.Dispose ();
				titleLabel = null;
			}

			if (textLabel != null) {
				textLabel.Dispose ();
				textLabel = null;
			}

			if (imageView != null) {
				imageView.Dispose ();
				imageView = null;
			}
		}
	}
}
