using Application.Common.Interfaces;

namespace API.Services;

/// <summary>
/// Сервис для хэширования паролей и проверки их соответствия хэшу
/// на основе алгоритма BCrypt.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Вычисляет BCrypt-хэш указанного пароля.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <returns>
    /// Строка с BCrypt-хэшем переданного пароля.
    /// </returns>
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    /// <summary>
    /// Проверяет соответствие пароля в открытом виде его BCrypt-хэшу.
    /// </summary>
    /// <param name="password">Пароль в открытом виде для проверки.</param>
    /// <param name="hash">BCrypt-хэш, с которым производится сравнение.</param>
    /// <returns>
    /// <see langword="true"/> если пароль соответствует хэшу;
    /// иначе <see langword="false"/>.
    /// </returns>
    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}