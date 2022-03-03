namespace ASPNETCore2JwtAuthentication.Common;

public static class ServerPath
{
    public static string GetProjectPath(Assembly startupAssembly)
    {
        if (startupAssembly == null)
        {
            throw new ArgumentNullException(nameof(startupAssembly));
        }

        var projectName = startupAssembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(projectName))
        {
            throw new InvalidOperationException("Couldn't find the assembly name.");
        }

        var applicationBasePath = AppContext.BaseDirectory;
        var directoryInfo = new DirectoryInfo(applicationBasePath);
        do
        {
            directoryInfo = directoryInfo.Parent;
            if (directoryInfo is null)
            {
                break;
            }

            var projectDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName));
            if (projectDirectoryInfo.Exists
                && new FileInfo(Path.Combine(projectDirectoryInfo.FullName, projectName, $"{projectName}.csproj"))
                    .Exists)
            {
                return Path.Combine(projectDirectoryInfo.FullName, projectName);
            }
        } while (directoryInfo.Parent != null);

        throw new InvalidOperationException(
            $"Project root could not be located using the application root {applicationBasePath}.");
    }
}