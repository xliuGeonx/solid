using System;

using Cairo;

namespace SolidV.MVC
{
	public class ShapeViewer : Viewer
	{
		public ShapeViewer()
		{
		}

		public ShapeViewer(View parent) : base(parent) {}
		
		public override void DrawItem(Context context, object item)
		{
			context.Rectangle((item as Shape).Rectangle);
		}
	}
}