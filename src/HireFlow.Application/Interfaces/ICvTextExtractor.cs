namespace HireFlow.Application.Interfaces;

// Reads the plain text out of a PDF or DOCX file.
public interface ICvTextExtractor
{
    string Extract(Stream file, string extension);
}
