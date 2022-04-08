namespace ClientWebAppService.PosProfile.Services.Credentials
{
    /// <summary>
    ///     Resolve PosCredentialsService for concrete pos type
    /// </summary>
    public interface IPosCredentialsServiceResolver
    {
        IPosCredentialsService<T> Resolve<T>(T _);
    }
}