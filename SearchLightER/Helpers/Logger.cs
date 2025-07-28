using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace naget.Helpers
{
	public sealed class Logger
	{
		/// <summary>
		/// ログレベル
		/// </summary>
		public enum LogLevel
		{
			ERROR,
			WARN,
			INFO,
			DEBUG
		}

		/// <summary>
		/// ログの出力レベル
		/// </summary>
		public LogLevel Level { get; set; } = LogLevel.DEBUG;
		/// <summary>
		/// ログファイルの出力先フォルダー
		/// </summary>
		public string LogDirName { get; set; } = Path.Join(App.ConfigFolder, "Log");
		/// <summary>
		/// ログファイルのファイル名
		/// </summary>
		public string LogFileName { get; set; } = "naget_Debug";
		/// <summary>
		/// ログファイルの最大サイズ (バイト単位)
		/// </summary>
		public int LogFileMaxSize { get; set; } = 1024 * 1024 * 10; // 10MB
		/// <summary>
		/// ログファイルの保存期間 (日数)
		/// </summary>
		public int LogFilePeriod { get; set; } = 30; // 日数
		/// <summary>
		/// ログファイルを保存するかどうか
		/// </summary>
		public bool IsLogFileEnabled { get; set; } = true;

		private static Logger? singleton;
		private readonly string logFilePath;
		private readonly object lockObj = new object();
		private StreamWriter stream;

		/// <summary>
		/// インスタンスを生成する
		/// </summary>
		public static Logger GetInstance()
		{
			if (singleton == null)
			{
				singleton = new Logger();
			}
			return singleton;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		private Logger()
		{
			logFilePath = Path.Join(LogDirName, LogFileName + ".log");

			// ログファイルを生成する
			CreateLogfile(new FileInfo(logFilePath));
		}

		/// <summary>
		/// ERRORレベルのログを出力する
		/// </summary>
		/// <param name="msg">メッセージ</param>
		public void Error(string msg)
		{
			if (LogLevel.ERROR <= Level)
			{
				Out(LogLevel.ERROR, msg);
			}
		}

		/// <summary>
		/// ERRORレベルのスタックトレースログを出力する
		/// </summary>
		/// <param name="ex">例外オブジェクト</param>
		public void Error(Exception ex)
		{
			if (LogLevel.ERROR <= Level)
			{
				Out(LogLevel.ERROR, ex.Message + Environment.NewLine + ex.StackTrace);
			}
		}

		/// <summary>
		/// WARNレベルのログを出力する
		/// </summary>
		/// <param name="msg">メッセージ</param>
		public void Warn(string msg)
		{
			if (LogLevel.WARN <= Level)
			{
				Out(LogLevel.WARN, msg);
			}
		}

		/// <summary>
		/// INFOレベルのログを出力する
		/// </summary>
		/// <param name="msg">メッセージ</param>
		public void Info(string msg)
		{
			if (LogLevel.INFO <= Level)
			{
				Out(LogLevel.INFO, msg);
			}
		}

		/// <summary>
		/// DEBUGレベルのログを出力する
		/// </summary>
		/// <param name="msg">メッセージ</param>
		public void Debug(string msg)
		{
			if (LogLevel.DEBUG <= Level)
			{
				Out(LogLevel.DEBUG, msg);
			}
		}

		/// <summary>
		/// ログを出力する
		/// </summary>
		/// <param name="level">ログレベル</param>
		/// <param name="msg">メッセージ</param>
		private void Out(LogLevel level, string msg)
		{
			int tid = Environment.CurrentManagedThreadId;
			string fullMsg = string.Format("[{0}][{1}][{2}] {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), tid, level.ToString(), msg);

			// デバッグログへ出力する
			System.Diagnostics.Debug.WriteLine(fullMsg);

			// ログファイルへ出力する
			if (IsLogFileEnabled)
			{
				lock (lockObj)
				{
					stream.WriteLine(fullMsg);

					FileInfo logFile = new FileInfo(logFilePath);
					if (LogFileMaxSize < logFile.Length)
					{
						// ログファイルを圧縮する
						CompressLogFile();

						// 古いログファイルを削除する
						DeleteOldLogFile();
					}
				}
			}
		}

		/// <summary>
		/// ログファイルを生成する
		/// </summary>
		/// <param name="logFile">ファイル情報</param>
		private void CreateLogfile(FileInfo logFile)
		{
			System.Diagnostics.Debug.WriteLine(LogLevel.DEBUG, $"Log File Path: {logFile.FullName}");

			if (!Directory.Exists(logFile.DirectoryName))
			{
				Directory.CreateDirectory(logFile.DirectoryName);
			}

			stream = new StreamWriter(logFile.FullName, true, Encoding.UTF8)
			{
				AutoFlush = true
			};
		}

		/// <summary>
		/// ログファイルを圧縮する
		/// </summary>
		private void CompressLogFile()
		{
			stream.Close();
			string oldFilePath = LogDirName + LogFileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
			File.Move(logFilePath, oldFilePath + ".log");

			FileStream inStream = new FileStream(oldFilePath + ".log", FileMode.Open, FileAccess.Read);
			FileStream outStream = new FileStream(oldFilePath + ".gz", FileMode.Create, FileAccess.Write);
			GZipStream gzStream = new GZipStream(outStream, CompressionMode.Compress);

			int size = 0;
			byte[] buffer = new byte[LogFileMaxSize + 1000];
			while (0 < (size = inStream.Read(buffer, 0, buffer.Length)))
			{
				gzStream.Write(buffer, 0, size);
			}

			inStream.Close();
			gzStream.Close();
			outStream.Close();

			File.Delete(oldFilePath + ".log");
			CreateLogfile(new FileInfo(logFilePath));
		}

		/// <summary>
		/// 古いログファイルを削除する
		/// </summary>
		private void DeleteOldLogFile()
		{
			Regex regex = new Regex(LogFileName + @"_(\d{14}).*\.gz");
			DateTime retentionDate = DateTime.Today.AddDays(-LogFilePeriod);
			string[] filePathList = Directory.GetFiles(LogDirName, LogFileName + "_*.gz", SearchOption.TopDirectoryOnly);
			foreach (string filePath in filePathList)
			{
				Match match = regex.Match(filePath);
				if (match.Success)
				{
					DateTime logCreatedDate = DateTime.ParseExact(match.Groups[1].Value.ToString(), "yyyyMMddHHmmss", null);
					if (logCreatedDate < retentionDate)
					{
						File.Delete(filePath);
					}
				}
			}
		}
	}
}
