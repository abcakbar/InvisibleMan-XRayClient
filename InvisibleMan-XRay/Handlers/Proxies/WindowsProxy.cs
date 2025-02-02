using Microsoft.Win32;

namespace InvisibleManXRay.Handlers.Proxies
{
    using Services;
    using Models;
    using Values;

    public class WindowsProxy : IProxy
    {
        private bool isCanceled;

        private const string INTERNET_SETTINGS = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
        private const string PROXY_ENABLE = "ProxyEnable";
        private const string PROXY_SERVER = "ProxyServer";

        private LocalizationService LocalizationService => ServiceLocator.Get<LocalizationService>();

        public Status Enable(string address, int port)
        {
            RegistryKey registry = GetInternetSettingsRegistry();

            try
            {
                registry.SetValue(PROXY_ENABLE, 1);
                registry.SetValue(PROXY_SERVER, $"{address}:{port}");

                if (isCanceled)
                    return CancelStatus();
                
                return new Status(
                    code: Code.SUCCESS,
                    subCode: SubCode.SUCCESS,
                    content: null
                );
            }
            catch
            {
                return new Status(
                    code: Code.ERROR,
                    subCode: SubCode.CANT_PROXY,
                    content: LocalizationService.GetTerm(Localization.CANT_PROXY_SYSTEM)
                );
            }
        }

        public void Disable()
        {
            isCanceled = false;
            RegistryKey registry = GetInternetSettingsRegistry();
            registry.SetValue(PROXY_ENABLE, 0);
        }

        public void Cancel()
        {
            isCanceled = true;
        }

        private RegistryKey GetInternetSettingsRegistry()
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey(INTERNET_SETTINGS, true);
            if (registry == null)
                registry = Registry.CurrentUser.CreateSubKey(INTERNET_SETTINGS);
            
            return registry;
        }

        private Status CancelStatus()
        {
            isCanceled = false;

            return new Status(
                code: Code.INFO,
                subCode: SubCode.CANCELED,
                content: null
            );
        }
    }
}