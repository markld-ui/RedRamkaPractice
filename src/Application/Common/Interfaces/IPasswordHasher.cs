namespace Application.Common.Interfaces;

/// <summary>
/// Интерфейс сервиса для хэширования паролей и проверки их соответствия хэшу.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Вычисляет хэш указанного пароля.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <returns>Строка с хэшем переданного пароля.</returns>
    string Hash(string password);

    /// <summary>
    /// Проверяет соответствие пароля в открытом виде его хэшу.
    /// </summary>
    /// <param name="password">Пароль в открытом виде для проверки.</param>
    /// <param name="hash">Хэш, с которым производится сравнение.</param>
    /// <returns>
    /// <see langword="true"/> если пароль соответствует хэшу;
    /// иначе <see langword="false"/>.
    /// </returns>
    bool Verify(string password, string hash);
}