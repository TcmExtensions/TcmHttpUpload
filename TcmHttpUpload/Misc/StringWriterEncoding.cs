#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: StringWriterWithEncoding
// ---------------------------------------------------------------------------------
//	Date Created	: March 13, 2013
//	Author			: Rob van Oostenrijk
// ---------------------------------------------------------------------------------
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.IO;
using System.Text;

namespace TcmHttpUpload.Misc
{
	public class StringWriterEncoding : StringWriter
	{
		private readonly Encoding mEncoding;

		/// <summary>
		/// Initializes a new instance of the <see cref="StringWriterEncoding"/> class.
		/// </summary>
		public StringWriterEncoding() : base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StringWriterEncoding"/> class.
		/// </summary>
		/// <param name="formatProvider">An <see cref="T:System.IFormatProvider" /> object that controls formatting.</param>
		public StringWriterEncoding(IFormatProvider formatProvider) : base(formatProvider)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StringWriterEncoding"/> class.
		/// </summary>
		/// <param name="stringBuilder">The string builder.</param>
		public StringWriterEncoding(StringBuilder stringBuilder) : base(stringBuilder)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StringWriterEncoding"/> class.
		/// </summary>
		/// <param name="stringBuilder">The string builder.</param>
		/// <param name="formatProvider">The format provider.</param>
		public StringWriterEncoding(StringBuilder stringBuilder, IFormatProvider formatProvider) : base(stringBuilder, formatProvider)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StringWriterEncoding"/> class.
		/// </summary>
		/// <param name="newEncoding">The new encoding.</param>
		public StringWriterEncoding(Encoding encoding) : base()
		{
			mEncoding = encoding;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StringWriterEncoding"/> class.
		/// </summary>
		/// <param name="formatProvider">The format provider.</param>
		/// <param name="newEncoding">The new encoding.</param>
		public StringWriterEncoding(IFormatProvider formatProvider, Encoding encoding) : base(formatProvider)
		{
			mEncoding = encoding;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StringWriterEncoding"/> class.
		/// </summary>
		/// <param name="stringBuilder">The string builder.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <param name="newEncoding">The new encoding.</param>
		public StringWriterEncoding(StringBuilder stringBuilder, IFormatProvider formatProvider, Encoding encoding) : base(stringBuilder, formatProvider)
		{
			mEncoding = encoding;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StringWriterEncoding"/> class.
		/// </summary>
		/// <param name="stringBuilder">The string builder.</param>
		/// <param name="newEncoding">The new encoding.</param>
		public StringWriterEncoding(StringBuilder stringBuilder, Encoding encoding) : base(stringBuilder)
		{
			mEncoding = encoding;
		}

		/// <summary>
		/// Gets the <see cref="T:System.Text.Encoding" /> in which the output is written.
		/// </summary>
		/// <returns>The Encoding in which the output is written.</returns>
		public override Encoding Encoding
		{
			get
			{
				return mEncoding ?? base.Encoding;
			}
		}
	}
}
