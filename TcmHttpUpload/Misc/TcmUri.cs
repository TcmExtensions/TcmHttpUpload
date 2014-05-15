#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: TcmUri
// ---------------------------------------------------------------------------------
//	Date Created	: February 28, 2013
//	Author			: Rob van Oostenrijk
// ---------------------------------------------------------------------------------
//
// Based on https://code.google.com/p/tridion-2011-power-tools/source/browse/trunk/PowerTools.Model/Utils/TcmUri.cs?r=633
// Thanks to pkjaer.sdl
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Text.RegularExpressions;

namespace TcmHttpUpload.Misc
{
	/// <summary>
	/// Initializes a new instance of the TcmUri class.
	/// </summary>
	public class TcmUri
	{
		private static readonly Regex mTcmRegEx = new Regex(@"tcm:(\d+)-(\d+)-?(\d*)-?v?(\d*)");

		private int mItemId;
		private int mItemType;
		private int mPublicationId;
		private int mVersion;
		
		/// <summary>
		/// <see cref="TcmUri" /> publication identifier
		/// </summary>
		/// <value>
		/// Publication identifier
		/// </value>
		public int PublicationId
		{
			get
			{
				return mPublicationId;
			}
			set
			{
				mPublicationId = value;
			}
		}

		/// <summary>
		/// <see cref="TcmUri" /> item identifier
		/// </summary>
 		/// <value>
		/// Item identifier
		/// </value>
		public int ItemId
		{
			get
			{
				return mItemId;
			}
			set
			{
				mItemId = value;
			}
		}

		/// <summary>
		/// <see cref="TcmUri" /> item type
		/// </summary>
		/// <value>
		/// Item type
		/// </value>
		public int ItemType
		{
			get
			{
				return mItemType;
			}
			set
			{
				mItemType = value;
			}
		}

		/// <summary>
		/// <see cref="TcmUri" /> item version
		/// </summary>
		/// <value>
		/// Item version
		/// </value>
		public int Version
		{
			get
			{
				return mVersion;
			}
			set
			{
				mVersion = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the TcmUri class.
		/// </summary>
		/// <param name="uri">The string representation of the TCM URI.</param>
		public TcmUri(String uri)
		{
			if (!Parse(uri, out mPublicationId, out mItemId, out mItemType, out mVersion))
				throw new Exception(String.Format("Invalid TcmUri {0}", uri));
		}

		/// <summary>
		/// Initializes a new instance of the TcmUri class.
		/// </summary>
		/// <param name="publicationId">The ID of the Publication the item belongs to.</param>
		/// <param name="itemId">The ID of the item itself.</param>
		/// <param name="itemType">The type of item (e.g. 16 for a Component)</param>
		public TcmUri(int publicationId, int itemId, int itemType): this(publicationId, itemId, itemType, 0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the TcmUri class.
		/// </summary>
		/// <param name="publicationId">The ID of the Publication the item belongs to.</param>
		/// <param name="itemId">The ID of the item itself.</param>
		/// <param name="itemType">The type of item (e.g. 16 for a Component)</param>
		/// <param name="version">The specific version of the item to retrieve.</param>
		public TcmUri(int publicationId, int itemId, int itemType, int version)
			: this(String.Format("tcm:{0}-{1}-{2}-v{3}", publicationId, itemId, itemType, version))
		{
		}

		/// <summary>
		/// Returns the string representation of the TCM URI.
		/// </summary>
		public override String ToString()
		{
			if (Version > 0)
				return String.Format("tcm:{0}-{1}-{2}-v{3}", mPublicationId, mItemId, mItemType, mVersion);

			if (ItemType == 16)
				return String.Format("tcm:{0}-{1}", mPublicationId, mItemId);

			return String.Format("tcm:{0}-{1}-{2}", mPublicationId, mItemId, mItemType);
		}

		/// <summary>
		/// Check if a given string is a valid TCM URI.
		/// </summary>
		/// <param name="uri">The string representation of a TCM URI (e.g. "tcm:2-255-32").</param>
		/// <returns><code>true</code> if the string is valid as a TCM URI; <code>false</code> otherwise.</returns>
		public static bool IsValid(String uri)
		{
			int publicationId;
			int itemId;
			int itemType;
			int version;

			return Parse(uri, out publicationId, out itemId, out itemType, out version);
		}

		/// <summary>
		/// Returns the equivalent of a <code>null</code> value for a TCM URI.
		/// </summary>
		public static TcmUri UriNull
		{
			get
			{
				return new TcmUri("tcm:0-0-0");
			}
		}

		/// <summary>
		/// Converts the string representation of a TCM URI to integers representing the different parts of the URI.
		/// </summary>
		/// <param name="input">The string to parse.</param>
		/// <param name="publicationId">The ID of the Publication the item belongs to.</param>
		/// <param name="itemId">The ID of the item itself.</param>
		/// <param name="itemType">The type of the item (e.g. 16 for a Component)</param>
		/// <param name="version">The version of the item. Defaults to 0 which is the current version.</param>
		/// <returns><code>true</code> if the parsing succeeded; <code>false</code> otherwise.</returns>
		protected static bool Parse(String input, out int publicationId, out int itemId, out int itemType, out int version)
		{
			publicationId = 0;
			itemId = 0;
			itemType = 0;
			version = 0;

			if (String.IsNullOrEmpty(input))
				return false;

			Match m = mTcmRegEx.Match(input);

			if (!m.Success)
				return false;

			try
			{
				publicationId = Convert.ToInt32(m.Groups[1].Value);
				itemId = Convert.ToInt32(m.Groups[2].Value);
				version = 0;
				itemType = 16;

				if (m.Groups.Count > 3)
				{
					itemType = Convert.ToInt32(m.Groups[3].Value);

					if (m.Groups.Count > 4)
						version = Convert.ToInt32(m.Groups[4].Value);
				}

				if (publicationId == 0 && itemId == 0 && itemType == 0 && version == 0)
					return true;

				return publicationId > -1 && itemId > 0 && itemType > 0 && version > -1;
			}
			catch (FormatException)
			{
			}
			catch (OverflowException)
			{
			}

			return false;
		}
	}
}
