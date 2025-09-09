using Microsoft.Extensions.Options;

namespace KulturSanatPortal.Web.Auth;
public class BasicAdminAuth(IOptions<AdminAuthOptions> opts) : IAdminAuth
{
    private readonly AdminAuthOptions _o = opts.Value;

    public bool Validate(string username, string password)
        => string.Equals(username, _o.Username, StringComparison.Ordinal)
        && string.Equals(password, _o.Password, StringComparison.Ordinal);
}
