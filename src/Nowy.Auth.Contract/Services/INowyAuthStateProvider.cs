using Nowy.Auth.Contract.Models;

namespace Nowy.Auth.Contract.Services;

public interface INowyAuthStateProvider
{
    INowyAuthState State { get; }
}
