using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBM
{
	public enum AibmAlertMessageType
    {
		Enemy,
		LowOnItems,
    }

	public class AibmAlertMessages {
		public AibmAlertMessageType type;
		public string message;
		public long targetId;
	}
}
