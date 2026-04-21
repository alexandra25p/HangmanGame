namespace HangmanGame.Models
{
    public class SaveFile
    {
        public string FileName { get; set; }     // Calea completă către fișierul .json
        public string DisplayName { get; set; }  // Numele pe care îl vede utilizatorul (ex: "Jocul Meu")
        public string SaveDate { get; set; }     // Data formatată ca text (ex: "21/04/2026 14:30")
    }
}