using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using WCell.Util.Commands;

namespace WCell.Intercommunication.DataTypes
{
	[Serializable]
	public class BufferedCommandResponse : IBufferedCommandResponse
	{
		public BufferedCommandResponse()
		{
			Replies = new List<string>(3);
		}

		public BufferedCommandResponse(params string[] replies)
		{
			Replies = replies.ToList();
		}

		public List<string> Replies
		{
			get;
			set;
		}
	}
}
