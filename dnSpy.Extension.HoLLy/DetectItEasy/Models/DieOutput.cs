namespace HoLLy.dnSpyExtension.DetectItEasy.Models
{
    public class DieOutput
    {
        public Detection[] Detects { get; set; } = new Detection[0];
        public string Filetype { get; set; } = string.Empty;
    }
}