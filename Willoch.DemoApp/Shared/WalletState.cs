namespace Willoch.DemoApp.Shared
{
    public record WalletState(
        string[] accounts, 
        bool isConnected,   

        /** True if isConnected and mm is unlocked by entering password */
        bool isUnlocked, 
        bool initialized, 
        bool isPermanentlyDisconnected);
}