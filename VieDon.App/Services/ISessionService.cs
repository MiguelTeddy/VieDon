using VieDon.Core.Models;

namespace VieDon.App.Services;

public interface ISessionService
{
    UserSpace? SelectedSpace { get; set; }
    UserProfile? CurrentUser { get; set; }
    void Clear();
}

public sealed class SessionService : ISessionService
{
    public UserSpace? SelectedSpace { get; set; }
    public UserProfile? CurrentUser { get; set; }

    public void Clear()
    {
        SelectedSpace = null;
        CurrentUser = null;
    }
}
