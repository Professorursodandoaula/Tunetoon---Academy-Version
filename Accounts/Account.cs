using System.Text.Json.Serialization;
using Tunetoon.Utilities;
namespace Tunetoon.Accounts
{
    public abstract class Account : AsyncNotifyPropertyChanged
    {
        [JsonIgnore]
        private bool loginWanted;
        public bool LoginWanted
        {
            get { return loginWanted; }
            set { loginWanted = value; NotifyPropertyChanged("loginWanted"); }
        }
        public string Toon { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        [JsonIgnore]
        public bool EndWanted { get; set; }
        [JsonIgnore]
        private bool loggedIn;
        [JsonIgnore]
        public bool LoggedIn
        {
            get { return loggedIn; }
            set { loggedIn = value; NotifyPropertyChanged("loggedIn"); }
        }

        // Posição da janela na tela (-1 = sem posição definida)
        public int WindowSlot { get; set; } = -1;

        // Prioridade de login (1, 2, 3... — 0 = sem prioridade)
        public int LoginPriority { get; set; } = 0;

        // Monitor onde a janela abre (0 = Monitor 1, 1 = Monitor 2)
        public int LoginMonitor { get; set; } = 0;

        public bool CanLogin()
        {
            return !LoggedIn && LoginWanted && Username != null && Password != null;
        }
    }
}
