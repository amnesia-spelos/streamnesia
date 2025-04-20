namespace Streamnesia.Core;

public interface ICommandPreprocessor
{
    string PreprocessCommand(string command);

    string StringEscape(string str);
}
