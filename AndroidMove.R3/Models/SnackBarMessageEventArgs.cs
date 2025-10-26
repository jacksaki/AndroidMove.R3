namespace AndroidMove.R3.Models
{
    public class SnackBarMessageEventArgs(string message) : EventArgs
    {
        public string Message => message;
    }
}
