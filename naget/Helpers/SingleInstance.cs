using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using naget.Services;

namespace naget.Helpers;

/// <summary>
/// 二重起動を防ぎ、既存インスタンスへ設定画面の表示を要求する
/// </summary>
public static class SingleInstance
{
	private const string ShowSettingsCommand = "show-settings";

	// 昇格状態が異なるインスタンスへは通知できないため、待ち時間を過ぎたら終了する
	private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(5);

	// パッケージ版と非パッケージ版、ユーザーごとに別インスタンスとして扱う
	private static readonly string Name = "naget-" + (PackageHelper.GetPackageFamilyName() ?? "desktop") + "-" + Environment.UserName;

	private static Mutex? mutex;
	private static CancellationTokenSource? listener;

	/// <summary>
	/// 唯一のインスタンスとして起動できた場合に true を返す (既存インスタンスには設定画面の表示を要求する)
	/// </summary>
	public static bool Acquire()
	{
		DateTime limit = DateTime.UtcNow + WaitTime;

		while (true)
		{
			mutex?.Dispose();

			mutex = new Mutex(false, Name, out bool createdNew);
			if (createdNew) return true;

			// 再起動中など相手がまだ応答できない場合は、終了を待って再試行する
			if (Notify() || limit < DateTime.UtcNow)
			{
				mutex.Dispose();
				return false;
			}

			Thread.Sleep(200);
		}
	}

	/// <summary>
	/// 既存インスタンスからの表示要求の受付を開始する
	/// </summary>
	public static void Listen()
	{
		if (listener != null) return;

		CancellationTokenSource source = new();
		listener = source;
		_ = Task.Run(() => ListenAsync(source.Token));
	}

	/// <summary>
	/// 表示要求の受付を終了する (再起動時に新しいインスタンスへ譲る)
	/// </summary>
	public static void StopListening()
	{
		listener?.Cancel();
	}

	/// <summary>
	/// インスタンスの保持を解放する (再起動時に新しいインスタンスへ譲る)
	/// </summary>
	public static void Release()
	{
		mutex?.Dispose();
		mutex = null;
	}

	/// <summary>
	/// 既存インスタンスへ設定画面の表示を要求する
	/// </summary>
	private static bool Notify()
	{
		try
		{
			using NamedPipeClientStream client = new(".", Name, PipeDirection.Out);
			client.Connect(500);

			using StreamWriter writer = new(client);
			writer.WriteLine(ShowSettingsCommand);
			writer.Flush();

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static async Task ListenAsync(CancellationToken token)
	{
		bool warned = false;

		while (!token.IsCancellationRequested)
		{
			try
			{
				await using NamedPipeServerStream server = new(Name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
				await server.WaitForConnectionAsync(token);

				using StreamReader reader = new(server);
				string? command = await reader.ReadLineAsync(token);
				if (command == ShowSettingsCommand)
				{
					App.Logger.Debug("Single instance: show settings requested.");
					Dispatcher.UIThread.Post(WindowService.ShowSettingsWindow);
				}
			}
			catch (Exception ex)
			{
				if (token.IsCancellationRequested) return;

				if (!warned)
				{
					App.Logger.Warn($"Single instance listener error: {ex.Message}");
					warned = true;
				}

				await Task.Delay(200, CancellationToken.None);
			}
		}
	}
}
