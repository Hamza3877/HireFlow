using HireFlow.Application.Dtos;

namespace HireFlow.Application.Interfaces;

// Checks and stores uploaded CV files.
public interface ICvStorage
{
    // Validates the file (type, size) and saves it. Returns the stored file name.
    Task<string> SaveAsync(CvUpload cv);

    Stream Open(string storedFileName);
}
