using System.Text.Json;
using Nowy.Auth.Common.Models;
using Nowy.Auth.Contract.Models;
using Nowy.Auth.Contract.Services;

namespace Nowy.Auth.Client.Services;

internal class RestNowyAuthStateProvider : INowyAuthStateProvider
{
    public RestNowyAuthState State { get; set; }
    INowyAuthState INowyAuthStateProvider.State => this.State;
}
