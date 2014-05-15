#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: HttpUpload
// ---------------------------------------------------------------------------------
//	Date Created	: February 28, 2013
//	Author			: Rob van Oostenrijk
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Xml;
using TcmHttpUpload.Logging;
using TcmHttpUpload.Misc;

namespace TcmHttpUpload
{
	/// <summary>
	/// <see cref="HttpUpload"/> implements a HTTP Uploader compatible with SDL Tridion 2011 (and newer hopefully).
	/// </summary>
	public class HttpUpload : IHttpHandler
	{
		private const String META_XML = "meta.xml";
		
		private static String mIncomingFolder = String.Empty;		
		private static int mMaximumSize = -1;		

		public static String IncomingFolder
		{
			get
			{
				return mIncomingFolder;
			}
		}

		static HttpUpload()
		{
			String incomingFolder = Path.GetFullPath(UploadConfig.Instance.IncomingFolder);

			if (!String.IsNullOrEmpty(incomingFolder))
			{
				Directory.CreateDirectory(incomingFolder);

				if (Directory.Exists(incomingFolder))
					mIncomingFolder = Path.GetFullPath(incomingFolder);
			}

			mMaximumSize = UploadConfig.Instance.MaximumSize;

			Logger.Info("TcmHttpUpload initialized with settings\n\tIncoming Folder: {0}\n\tMaximum Size: {1}", mIncomingFolder, mMaximumSize);
		}

		/// <summary>
		/// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
		/// </summary>
		/// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
		public bool IsReusable
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void ProcessRequest(HttpContext context)
		{
			HttpResponse response = context.Response;

			// Ensure cd_transport service always gets fresh data
			response.Cache.SetCacheability(HttpCacheability.NoCache);
			response.Cache.SetNoServerCaching();
			response.Cache.SetNoStore();

			HandleRequest(context.Request, context.Response);
		}

		/// <summary>
		/// Handles the file upload.
		/// </summary>
		/// <param name="request"><see cref="T:System.Web.HttpRequest"/></param>
		/// <param name="response"><see cref="T:System.Web.HttpResponse"/></param>
		private void HandleFileUpload(HttpRequest request, HttpResponse response)
		{
			// Handle multiple file uploads
			foreach (String key in request.Files)
			{
				HttpPostedFile postedFile = request.Files[key];

				// Process only files smaller then the configured maximum size
				if (postedFile.ContentLength <= mMaximumSize)
				{
					try
					{
						Logger.Info("FileUpload - Package '{0}'.", postedFile.FileName);

						String package = Path.GetFileName(postedFile.FileName);
						String upload = Path.GetFileNameWithoutExtension(package) + ".tmp";
						String uploadPath = Path.Combine(mIncomingFolder, upload);
						String packagePath = Path.Combine(mIncomingFolder, package);

						Logger.Debug("FileUpload - Receiving '{0}' into '{1}'.", package, upload);

						postedFile.SaveAs(uploadPath);

						Logger.Debug("FileUpload - Moving temporary upload to '{0}'.", package);

						File.Move(uploadPath, packagePath);
					}
					catch (Exception ex)
					{
						Logger.Error("FileUpload - {0}", ex, ex.Message);
					}

					response.Write("Upload successful");
				}
				else
				{
					response.StatusCode = (int)HttpStatusCode.RequestEntityTooLarge;
					response.Write("Maximum filesize exceeded");
				}
			}
		}

		/// <summary>
		/// Handles creating a list of files in the incoming directory based on the given extension.
		/// </summary>
		/// <param name="response"><see cref="T:System.Web.HttpResponse"/></param>
		/// <param name="listExtension">File extension to filter on</param>
		private void HandleList(HttpResponse response, String listExtension)
		{
			Logger.Info("List - Requested file list for '{0}'.", listExtension);

#if DOTNET4
            foreach (String file in Directory.EnumerateFiles(mIncomingFolder, "*" + listExtension, SearchOption.TopDirectoryOnly))
#else
			foreach (String file in Directory.GetFiles(mIncomingFolder, "*" + listExtension, SearchOption.TopDirectoryOnly))
#endif
			{
				response.Write(Path.GetFileName(file) + ":");
			}
		}

		/// <summary>
		/// Handles a batch state request from the Tridion service
		/// </summary>
		/// <param name="response">The response.</param>
		/// <param name="batchFiles">The batch files.</param>
		private void HandleBatch(HttpResponse response, String batchFiles)
		{
			Logger.Info("Batch - BatchFiles '{0}'.", batchFiles);

			response.ContentType = "text/xml";
			response.ContentEncoding = Encoding.UTF8;

			XmlWriterSettings settings = new XmlWriterSettings()
			{
				Encoding = Encoding.UTF8,
				Indent = false,
				NewLineHandling = NewLineHandling.None,
				OmitXmlDeclaration = false
			};

			using (XmlWriter xw = XmlTextWriter.Create(response.Output, settings))
			{
				xw.WriteStartDocument();
				xw.WriteStartElement("DeployerTransactions");

				foreach (String batchFile in batchFiles.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
				{
					String filePath = Path.Combine(mIncomingFolder, batchFile);

					if (File.Exists(filePath))
					{
						try
						{
							using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
							{
								using (XmlReader xr = XmlTextReader.Create(fs))
								{
									xr.MoveToContent();

									if (xr.ReadState == ReadState.Interactive && xr.HasAttributes)
									{
										xw.WriteStartElement("DeployerTransaction");

										// Copy all existing attributes
										xr.MoveToFirstAttribute();
										xw.WriteAttributes(xr, false);

										// Move back to the element
										xr.MoveToElement();
										xr.Read();

										while (!xr.EOF)
										{
											xr.Read();

											if (xr.NodeType == XmlNodeType.Element)
												xw.WriteNode(xr, false);
										}

										xw.WriteEndElement();
									}
								}
							}
						}
						catch (Exception ex)
						{
							Logger.Warning("Batch - Error reading state '{0}'.", ex, batchFile);
						}
					}
					else
					{
						Logger.Warning("Batch - Missing state '{0}'.", batchFile);
					}
				}

				xw.WriteEndElement();
				xw.WriteEndDocument();
			}
		}

		/// <summary>
		/// Handles "fileName=" requests from the cd_transport service
		/// </summary>
		/// <param name="response"><see cref="T:System.Web.HttpResponse"/></param>
		/// <param name="fileName">fileName</param>
		/// <param name="action">action to take, either empty or "remove"</param>
		private void HandleFileRequest(HttpResponse response, String fileName, String action)
		{
			String mappedFile = TcmUri.IsValid(fileName) ? Path.Combine(mIncomingFolder, fileName + ".state.xml") : Path.Combine(mIncomingFolder, fileName);

			Logger.Info("FileRequest - fileName '{0}', action '{1}'.", fileName, action);

			if (File.Exists(mappedFile))
			{
				if (String.Equals(action, "remove", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						Logger.Info("FileRequest - removing file '{0}'.", fileName);
						File.Delete(mappedFile);
					}
					catch (Exception ex)
					{
						Logger.Error("FileRequest - removing file", ex, ex.Message);
					}

					response.Write("File removed");

					return;
				}
				else
				{
					if (String.Equals(Path.GetExtension(mappedFile), ".xml", StringComparison.OrdinalIgnoreCase))
					{
						response.ContentType = "text/xml";

						if (String.Equals(fileName, META_XML, StringComparison.OrdinalIgnoreCase))
						{
							CleanTransactions.Execute();

							Logger.Debug("FileRequest - meta.xml requested");

							String metaXml = HttpContext.Current.Cache[META_XML] as String;

							if (String.IsNullOrEmpty(metaXml))
							{
								metaXml = File.ReadAllText(mappedFile, Encoding.UTF8);
								HttpContext.Current.Cache.Insert(META_XML, metaXml, new CacheDependency(mappedFile));
							}
							else
							{
								Logger.Info("FileRequest - Sending cached meta.xml");
							}

							response.Write(metaXml);
							return;
						}
					}

					Logger.Info("FileRequest - sending file {0}", fileName);

					response.WriteFile(mappedFile);
					return;
				}
			}

			Logger.Warning("FileRequest - Not found or invalid '{0}'.", mappedFile);

			response.StatusCode = (int)HttpStatusCode.NoContent;
			response.Write("No Content");
		}

		/// <summary>
		/// Returns the XML for the requested transaction and removes any state files associated with it.
		/// </summary>
		/// <param name="response"><see cref="T:System.Web.HttpResponse"/></param>
		/// <param name="transaction">Transaction TcmUri</param>
		private void HandleTransactionRequest(HttpResponse response, String transaction)
		{
			Logger.Info("FileTransaction - Requested '{0}'.", transaction);

			if (TcmUri.IsValid(transaction))
			{
				String mappedFile = Path.Combine(mIncomingFolder, transaction + ".xml");

				if (File.Exists(mappedFile))
				{
					response.ContentType = "text/xml";
					response.WriteFile(mappedFile);

					try
					{
						String stateFile = Path.Combine(mIncomingFolder, transaction + ".state.xml");
						Logger.Debug("Removing transaction state xml '{0}'.", stateFile);

						if (File.Exists(stateFile))
							File.Delete(stateFile);
					}
					catch (Exception ex)
					{
						Logger.Error("FileTransaction - Removing '{0}'.", ex, ex.Message);
					}

					return;
				}

				Logger.Warning("FileTransaction - Not found transaction file '{0}'.", mappedFile);
			}

			Logger.Warning("FileTransaction - Invalid transaction '{0}'.", transaction);

			response.StatusCode = (int)HttpStatusCode.NoContent;
			response.Write("No Content");
		}

		private void HandleRequest(HttpRequest request, HttpResponse response)
		{
			if (String.IsNullOrEmpty(mIncomingFolder))
			{
				response.Write("Error: No HTTPS incoming path found. Check if the Content Deployer is configured correctly.");
				return;
			}

			if (mMaximumSize == -1)
			{
				response.Write("Error: No maximum package size found. Check if the Content Deployer is configured correctly.");
				return;
			}

			String fileName = request.Params["fileName"] as String;
			String action = request.Params["action"] as String;
			String transaction = request.Params["transactionid"] as String;
			String extension = request.Params["extension"] as String;
			String batchFiles = request.Params["batchFiles"] as String;

			// Handle file uploads
			if (request.Files.Count > 0)
			{
				HandleFileUpload(request, response);
				return;
			}

			// Handle list requests
			if (String.Equals(action, "list", StringComparison.OrdinalIgnoreCase))
			{
				HandleList(response, extension);
				return;
			}

			// Handle batch requests
			if (String.Equals(action, "batch", StringComparison.OrdinalIgnoreCase))
			{
				HandleBatch(response, batchFiles);
				return;
			}

			// Handle transaction requests
			if (!String.IsNullOrEmpty(transaction))
			{
				HandleTransactionRequest(response, transaction);
				return;
			}

			// Handle file requests
			if (!String.IsNullOrEmpty(fileName))
			{
				HandleFileRequest(response, fileName, action);
				return;
			}

			response.Write("TcmHttpUpload");
		}
	}
}