using Nowy.Auth.Contract.Models;
using Nowy.Auth.Contract.Services;

namespace Nowy.Auth.Server.Services;

internal class DefaultNowyAuthStateProvider : INowyAuthStateProvider
{
    public DefaultNowyAuthState State { get; set; }
    INowyAuthState INowyAuthStateProvider.State => this.State;
}
