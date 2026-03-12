namespace SlaRequestProcessorCaseStudy.Tests;

// Verifies command-line input path selection.
public class InputPathResolverTests
{
	[Fact]
	// No args should use default input folder.
	public void DecidePathToFiles_NoArgs_ReturnsDefault()
	{
		// Arrange
		string[] args = Array.Empty<string>();

		// Act
		string result = InputPathResolver.DecidePathToFiles(args);

		// Assert
		Assert.Equal(InputPathResolver.DefaultInputFolder, result);
	}


	[Fact]
	// One arg should override default folder.
	public void DecidePathToFiles_OneArg_ReturnsThatArg()
	{
		// Arrange
		string[] args = { "./CustomFolder" };

		// Act
		string result = InputPathResolver.DecidePathToFiles(args);

		// Assert
		Assert.Equal("./CustomFolder", result);
	}
}