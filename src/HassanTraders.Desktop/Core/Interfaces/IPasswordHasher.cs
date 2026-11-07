namespace HassanTraders.Desktop.Services.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
