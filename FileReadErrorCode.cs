using System.ComponentModel;


namespace TeamCreation
{
    public enum FileReadErrorCode
    {
        [Description("No Errors Found")]
        OK = 0,
        [Description("File Not Found")]
        InvalidFilePath,
        [Description("CSV Files Only")]
        InvalidFileType,
        [Description("Player Name Missing")]
        BlankName,
        [Description("Invalid Rating")]
        InvalidRating
    }
}
