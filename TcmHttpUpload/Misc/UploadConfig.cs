#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: Upload Configuration
// ---------------------------------------------------------------------------------
//	Date Created	: November 16, 2013
//	Author			: Rob van Oostenrijk
// ---------------------------------------------------------------------------------
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Configuration;

namespace TcmHttpUpload.Misc
{
	public class UploadConfig : ConfigurationSection
	{
		private static UploadConfig mUploadConfig = null;

		public static UploadConfig Instance
		{
			get
			{
				if (mUploadConfig == null)
					mUploadConfig = ConfigurationManager.GetSection("TcmHttpUpload") as UploadConfig;

				return mUploadConfig;
			}
		}

		[ConfigurationProperty("incomingFolder", IsRequired = true, DefaultValue = null)]
		public String IncomingFolder
		{
			get
			{
				return base["incomingFolder"] as String;
			}
		}

		[ConfigurationProperty("temporaryFolder", IsRequired = true, DefaultValue = null)]
		public String TemporaryFolder
		{
			get
			{
				return base["temporaryFolder"] as String;
			}
		}

		[ConfigurationProperty("maximumSize", IsRequired = true, DefaultValue = null)]
		public int MaximumSize
		{
			get
			{
				return (int)base["maximumSize"];
			}
		}

		[ConfigurationProperty("maxStateAge", IsRequired = false, DefaultValue = -1)]
		public int MaxStateAge
		{
			get
			{
				return (int)base["maxStateAge"];
			}
		}		
	}
}
