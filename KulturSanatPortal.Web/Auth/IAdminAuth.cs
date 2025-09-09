namespace KulturSanatPortal.Web.Auth;
public interface IAdminAuth
{
    bool Validate(string username, string password);
}
