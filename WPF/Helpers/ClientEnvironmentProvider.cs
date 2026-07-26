using System.Net;
using System.Net.Sockets;
using System.Reflection;
using BusinessObjects.DTOs;

namespace WPF.Helpers;

public static class ClientEnvironmentProvider
{
    public static ClientEnvironmentDto Capture()
    {
        return new ClientEnvironmentDto
        {
            MachineName = Normalize(Environment.MachineName),
            WindowsUser = Normalize(Environment.UserName),
            IpAddress = GetLocalIpAddress(),
            OsVersion = Normalize(Environment.OSVersion.VersionString),
            AppVersion = Normalize(Assembly.GetEntryAssembly()?.GetName().Version?.ToString()),
            DeviceType = "Windows Desktop"
        };
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var address = host.AddressList
                .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                .Select(address => address.ToString())
                .FirstOrDefault(address => !address.StartsWith("127.", StringComparison.OrdinalIgnoreCase));

            return Normalize(address);
        }
        catch
        {
            return "Unknown";
        }
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();
    }
}
