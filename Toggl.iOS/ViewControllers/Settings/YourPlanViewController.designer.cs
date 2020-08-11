// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.iOS.ViewControllers.Settings
{
	[Register ("YourPlanViewController")]
	partial class YourPlanViewController
	{
		[Outlet]
		UIKit.UILabel LoginToYourAccountLabel { get; set; }

		[Outlet]
		UIKit.UILabel PlanExpirationDateLabel { get; set; }

		[Outlet]
		UIKit.UILabel PlanNameLabel { get; set; }

		[Outlet]
		UIKit.UIStackView StackView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (PlanExpirationDateLabel != null) {
				PlanExpirationDateLabel.Dispose ();
				PlanExpirationDateLabel = null;
			}

			if (PlanNameLabel != null) {
				PlanNameLabel.Dispose ();
				PlanNameLabel = null;
			}

			if (LoginToYourAccountLabel != null) {
				LoginToYourAccountLabel.Dispose ();
				LoginToYourAccountLabel = null;
			}

			if (StackView != null) {
				StackView.Dispose ();
				StackView = null;
			}
		}
	}
}
