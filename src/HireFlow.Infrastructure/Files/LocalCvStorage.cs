using HireFlow.Application.Dtos;
using HireFlow.Application.Exceptions;
using HireFlow.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HireFlow.Infrastructure.Files;

// Checks uploaded CVs (type, size, real file content) and saves them in a private folder
// outside the web root, using a random file name so nobody can guess or overwrite files.
public class LocalCvStorage : ICvStorage
{
    private const long MaxBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = { ".pdf", ".docx" };

    private static readonly string[] AllowedContentTypes =
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private readonly string _folder;

    public LocalCvStorage(IConfiguration configuration)
    {
        var configured = configuration["Storage:CvFolder"] ?? "cv-files";
        _folder = Path.GetFullPath(configured);
        Directory.CreateDirectory(_folder);
    }

    public async Task<string> SaveAsync(CvUpload cv)
    {
        if (cv.Length <= 0)
        {
            throw new BusinessRuleException("Please attach your CV.");
        }
        if (cv.Length > MaxBytes)
        {
            throw new BusinessRuleException("The CV must be 5 MB or smaller.");
        }

        var extension = Path.GetExtension(cv.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new BusinessRuleException("Only PDF and DOCX files are allowed.");
        }
        if (!AllowedContentTypes.Contains(cv.ContentType.ToLowerInvariant()))
        {
            throw new BusinessRuleException("The file type is not allowed. Upload a PDF or DOCX.");
        }

        var storedName = Guid.NewGuid().ToString("N") + extension;
        var fullPath = Path.Combine(_folder, storedName);

        await using (var target = new FileStream(fullPath, FileMode.CreateNew))
        {
            await cv.Content.CopyToAsync(target);
        }

        // The file name can be faked, so also check the first bytes of the real content.
        if (!StartsWithExpectedBytes(fullPath, extension))
        {
            File.Delete(fullPath);
            throw new BusinessRuleException("The file content does not match a real PDF or DOCX.");
        }

        return storedName;
    }

    public Stream Open(string storedFileName)
    {
        // GetFileName removes any folder parts, which blocks path traversal like "..\..\secret".
        var safeName = Path.GetFileName(storedFileName);
        var fullPath = Path.Combine(_folder, safeName);
        if (!File.Exists(fullPath))
        {
            throw new NotFoundException("The CV file was not found.");
        }
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    // PDF files start with "%PDF". DOCX files are ZIP files and start with "PK".
    private static bool StartsWithExpectedBytes(string path, string extension)
    {
        var header = new byte[4];
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var read = stream.Read(header, 0, header.Length);
        if (read < 4)
        {
            return false;
        }

        if (extension == ".pdf")
        {
            return header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46;
        }
        return header[0] == 0x50 && header[1] == 0x4B;
    }
}
