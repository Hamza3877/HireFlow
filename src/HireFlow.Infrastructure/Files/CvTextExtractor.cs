using System.Text;
using DocumentFormat.OpenXml.Packaging;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using UglyToad.PdfPig;

namespace HireFlow.Infrastructure.Files;

// Reads plain text out of a CV so the match score can look for skills.
// PdfPig reads PDF files, Open XML SDK reads DOCX files. Both are free.
public class CvTextExtractor : ICvTextExtractor
{
    public string Extract(Stream file, string extension)
    {
        try
        {
            return extension.ToLowerInvariant() == ".pdf" ? ReadPdf(file) : ReadDocx(file);
        }
        catch (Exception)
        {
            throw new BusinessRuleException("We could not read your CV. Please upload a valid PDF or DOCX.");
        }
    }

    private static string ReadPdf(Stream file)
    {
        var text = new StringBuilder();
        using var document = PdfDocument.Open(file);
        foreach (var page in document.GetPages())
        {
            text.Append(page.Text).Append(' ');
        }
        return text.ToString();
    }

    private static string ReadDocx(Stream file)
    {
        using var document = WordprocessingDocument.Open(file, false);
        return document.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
    }
}
