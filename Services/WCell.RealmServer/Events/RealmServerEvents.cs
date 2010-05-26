using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Login;

namespace WCell.RealmServer
{
	public partial class RealmServer
	{
		public event Action<RealmStatus> StatusChanged;
	}
}
