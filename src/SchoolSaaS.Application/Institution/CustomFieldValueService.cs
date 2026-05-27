using System.Text.Json;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Institution;

public static class CustomFieldValueValidator
{
    public static Result ValidateValue(CustomFieldDefinition definition, string? value)
    {
        if (definition.IsRequired && string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure(
                "custom_fields.required",
                $"Field '{definition.Label}' is required.");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Success();
        }

        return definition.DataType switch
        {
            CustomFieldDataType.Text => Result.Success(),
            CustomFieldDataType.Number => decimal.TryParse(value, out _)
                ? Result.Success()
                : Result.Failure("custom_fields.invalid_number", $"'{definition.Label}' must be a number."),
            CustomFieldDataType.Date => DateOnly.TryParse(value, out _)
                ? Result.Success()
                : Result.Failure("custom_fields.invalid_date", $"'{definition.Label}' must be a date (YYYY-MM-DD)."),
            CustomFieldDataType.Boolean => bool.TryParse(value, out _)
                ? Result.Success()
                : Result.Failure("custom_fields.invalid_boolean", $"'{definition.Label}' must be true or false."),
            CustomFieldDataType.Select => ValidateSelect(definition, value),
            _ => Result.Failure("custom_fields.unsupported_type", $"Unsupported field type for '{definition.Label}'.")
        };
    }

    public static Result ValidateAllDefinitionsHaveValues(
        IReadOnlyList<CustomFieldDefinition> definitions,
        IReadOnlyDictionary<string, string?> submitted)
    {
        foreach (var definition in definitions.Where(d => d.IsRequired && d.IsActive))
        {
            submitted.TryGetValue(definition.FieldKey, out var value);
            var result = ValidateValue(definition, value);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return Result.Success();
    }

    private static Result ValidateSelect(CustomFieldDefinition definition, string value)
    {
        if (string.IsNullOrWhiteSpace(definition.OptionsJson))
        {
            return Result.Failure("custom_fields.invalid_select", $"'{definition.Label}' has no configured options.");
        }

        try
        {
            var options = JsonSerializer.Deserialize<string[]>(definition.OptionsJson) ?? [];
            return options.Contains(value, StringComparer.OrdinalIgnoreCase)
                ? Result.Success()
                : Result.Failure("custom_fields.invalid_option", $"'{value}' is not allowed for '{definition.Label}'.");
        }
        catch (JsonException)
        {
            return Result.Failure("custom_fields.invalid_select", $"'{definition.Label}' has invalid option configuration.");
        }
    }
}

public sealed class CustomFieldValueService(ICustomFieldRepository repository)
{
    public async Task<Result> SetValuesAsync(
        Guid tenantId,
        CustomFieldEntityType entityType,
        Guid entityId,
        IReadOnlyDictionary<string, string?>? customFields,
        CancellationToken cancellationToken)
    {
        var definitions = await repository.ListDefinitionsAsync(tenantId, entityType, activeOnly: true, cancellationToken);
        if (definitions.Count == 0)
        {
            return customFields is null or { Count: 0 }
                ? Result.Success()
                : Result.Failure("custom_fields.unknown", "No custom fields are configured for this entity type.");
        }

        var submitted = customFields ?? new Dictionary<string, string?>();
        var definitionByKey = definitions.ToDictionary(d => d.FieldKey, StringComparer.OrdinalIgnoreCase);

        foreach (var (key, _) in submitted)
        {
            if (!definitionByKey.ContainsKey(key))
            {
                return Result.Failure("custom_fields.unknown", $"Custom field '{key}' is not defined.");
            }
        }

        var requiredCheck = CustomFieldValueValidator.ValidateAllDefinitionsHaveValues(definitions, submitted);
        if (requiredCheck.IsFailure)
        {
            return requiredCheck;
        }

        var upsertItems = new List<(CustomFieldDefinition Definition, string? Value)>();
        foreach (var definition in definitions)
        {
            if (!submitted.TryGetValue(definition.FieldKey, out var value))
            {
                continue;
            }

            var validation = CustomFieldValueValidator.ValidateValue(definition, value);
            if (validation.IsFailure)
            {
                return validation;
            }

            upsertItems.Add((definition, string.IsNullOrWhiteSpace(value) ? null : value.Trim()));
        }

        await repository.UpsertValuesAsync(tenantId, entityType, entityId, upsertItems, cancellationToken);
        return Result.Success();
    }

    public async Task<IReadOnlyList<CustomFieldValueDto>> GetValuesAsync(
        Guid tenantId,
        CustomFieldEntityType entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var values = await repository.ListValuesForEntityAsync(tenantId, entityType, entityId, cancellationToken);
        return values
            .Select(v => new CustomFieldValueDto(
                v.Definition.FieldKey,
                v.Definition.Label,
                v.Definition.DataType.ToString(),
                v.Value))
            .ToList();
    }
}

public sealed record CustomFieldValueDto(
    string FieldKey,
    string Label,
    string DataType,
    string? Value);
