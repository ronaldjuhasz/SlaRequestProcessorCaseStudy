// Resolves input folder path from command-line args or uses default.
public class InputPathResolver
{
	public const string DefaultInputFolder = "./inputFiles";
	// Returns first arg if provided, otherwise default folder.
	public static string DecidePathToFiles(string[] args)
	{
		return args.Length > 0 ? args[0] : DefaultInputFolder;
	}
}