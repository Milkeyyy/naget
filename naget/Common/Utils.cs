namespace naget.Common;

public static class Utils
{
	public static int ConvertToInt(string str, int def = 0)
	{
		try
		{
			return int.Parse(str);
		}
		catch
		{
			return def != 0 ? def : 0;
		}
	}
}
