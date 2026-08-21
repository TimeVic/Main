namespace TimeTracker.Business.Common.Constants.Storage;

public static class FileAcceptConstants
{
    public const string Default = ".pdf,.doc,.docx,.xls,.xlsx,.csv,.txt,.md,.png,.jpg,.jpeg,.gif,.webp,.zip,.7z,.tar,.gz,image/*";

    public const string Csv = ".csv,text/csv,application/vnd.ms-excel";

    public const string Notes = ".pdf,.doc,.docx,.xls,.xlsx,.csv,.txt,.md,.png,.jpg,.jpeg,.gif,.webp,.svg,.zip,.7z,.json,.xml,image/*";

    public const string Tasks = ".pdf,.doc,.docx,.xls,.xlsx,.csv,.txt,.md,.png,.jpg,.jpeg,.gif,.webp,.svg,.zip,.7z,.json,.xml,.log,image/*";

    public const string Images = "image/jpeg,image/png,image/gif,image/webp,image/bmp,.jpg,.jpeg,.png,.gif,.webp,.bmp";

    public const int MaxDefaultFileSizeInMb = 50;

    public const long MaxDefaultFileSize = MaxDefaultFileSizeInMb * 1024 * 1024; // 50 MB

    public const int MaxCsvFileSizeInMb = 10;

    public const long MaxCsvFileSize = MaxCsvFileSizeInMb * 1024 * 1024; // 10 MB
}
