namespace OrderIntegration.Api.Domain.Exceptions;

/// <summary>
/// Excepción base para errores de negocio.
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }

    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Excepción para conflictos de negocio (devuelve 409).
/// </summary>
public class ConflictException : BusinessException
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>
/// Excepción para recursos no encontrados (devuelve 404).
/// </summary>
public class NotFoundException : BusinessException
{
    public string ResourceType { get; }
    public object ResourceId { get; }

    public NotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} con ID {resourceId} no encontrado.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public NotFoundException(string message) : base(message)
    {
        ResourceType = "Resource";
        ResourceId = "unknown";
    }
}
