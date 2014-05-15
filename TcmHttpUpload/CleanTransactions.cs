#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: CleanTransactions
// ---------------------------------------------------------------------------------
//	Date Created	: February 28, 2013
//	Author			: Rob van Oostenrijk
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.IO;
using TcmHttpUpload.Logging;
using TcmHttpUpload.Misc;

namespace TcmHttpUpload
{
	public static class CleanTransactions
	{
		private const String TRIDION_FILE_PREFIX = "tcm*";

		private static String mTemporaryFolder = String.Empty;
		private static int mMaxStateAge = -1;

		static CleanTransactions()
		{
			String tempFolder = Path.GetFullPath(UploadConfig.Instance.TemporaryFolder);

			if (!String.IsNullOrEmpty(tempFolder) && Directory.Exists(tempFolder))
				mTemporaryFolder = tempFolder;

			mMaxStateAge = UploadConfig.Instance.MaxStateAge;

			// Clean up left-over transactions on initialization
			if (mMaxStateAge != -1)
				Execute();

			Logger.Info("CleanTransactions initialized with settings\n\tTemporary Folder: {0}\n\tMax. State Age: {1}", mTemporaryFolder, mMaxStateAge);
		}

		private static void CleanDirectory(String Path)
		{
			if (Directory.Exists(Path))
			{
				DirectoryInfo folderInfo = new DirectoryInfo(Path);

#if DOTNET4
				foreach (DirectoryInfo dirInfo in folderInfo.EnumerateDirectories(TRIDION_FILE_PREFIX))
#else
				foreach (DirectoryInfo dirInfo in folderInfo.GetDirectories(TRIDION_FILE_PREFIX))
#endif
				{
					if (DateTime.Now.Subtract(dirInfo.LastWriteTime).TotalMinutes > mMaxStateAge)
					{
						Logger.Debug("CleanupTransactions - Removing folder '{0}'.", dirInfo.Name);

						try
						{
							dirInfo.Delete(true);
						}
						catch (Exception ex)
						{
							Logger.Error("CleanupTransactions - Error deleting {0}", ex, ex.Message);
						}
					}
				}

#if DOTNET4
                foreach (FileInfo fileInfo in folderInfo.EnumerateFiles())
#else
				foreach (FileInfo fileInfo in folderInfo.GetFiles())
#endif
				{
					if (DateTime.Now.Subtract(fileInfo.LastWriteTime).TotalMinutes > mMaxStateAge)
					{
						Logger.Debug("CleanupTransactions - Removing file '{0}'.", fileInfo.Name);

						try
						{
							fileInfo.Delete();
						}
						catch (Exception ex)
						{
							Logger.Error("CleanupTransactions - Error deleting {0}", ex, ex.Message);
						}
					}
				}
			}
		}

		/// <summary>
		/// Clean up left-over transaction files
		/// </summary>
		public static void Execute()
		{
			if (mMaxStateAge != -1 && !String.IsNullOrEmpty(HttpUpload.IncomingFolder))
			{
				Logger.Info("CleanupTransactions - Executing cleanup of left-over processing files");

				try
				{
					DirectoryInfo incomingInfo = new DirectoryInfo(HttpUpload.IncomingFolder);

					// Clear all left-over state.xml files
#if DOTNET4
					foreach (FileInfo stateInfo in incomingInfo.EnumerateFiles(TRIDION_FILE_PREFIX, SearchOption.TopDirectoryOnly))
#else
					foreach (FileInfo stateInfo in incomingInfo.GetFiles(TRIDION_FILE_PREFIX, SearchOption.TopDirectoryOnly))
#endif
					{
						if (DateTime.Now.Subtract(stateInfo.LastWriteTime).TotalMinutes > mMaxStateAge)
						{
							Logger.Warning("CleanupTransactions - Removing state '{0}'.", stateInfo.Name);

							try
							{
								stateInfo.Delete();
							}
							catch (Exception ex)
							{
								Logger.Error("CleanupTransactions - Error deleting {0}", ex, ex.Message);
							}
						}
					}

					// Clean transaction folder                    
					CleanDirectory(Path.Combine(HttpUpload.IncomingFolder, "Transaction"));

					// Clean zip folder
					CleanDirectory(Path.Combine(HttpUpload.IncomingFolder, "Zip"));

					// Clean Tridion temporary folder
					if (!String.IsNullOrEmpty(mTemporaryFolder) && Directory.Exists(mTemporaryFolder))
						CleanDirectory(mTemporaryFolder);
				}
				catch (Exception ex)
				{
					Logger.Error("CleanupTransactions - {0}", ex, ex.Message);
				}
			}
		}
	}
}