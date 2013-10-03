namespace Localization.Net.Maintenance
{
    public class LocalizedTextState
    {
        public LocalizedText Text { get; set; }

        public LocalizedTextStatus Status { get; set; }
    }

    public enum LocalizedTextStatus : int
    {
        Unchanged = 0,
        Changed = 1,
        New = 2,
        Unused = 4
    }

}
