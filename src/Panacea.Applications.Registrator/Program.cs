using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanaceaLib;

namespace PanaceaRegistrator
{
	class Program
	{
		[STAThread]
		public static void Main()
		{
			var app = new App();


#if DEBUG
			app.Run();
#else
           
            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
            }
#endif

		}
	}
}
