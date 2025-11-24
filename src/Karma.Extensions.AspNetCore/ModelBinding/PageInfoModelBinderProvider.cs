// -----------------------------------------------------------------------
// <copyright file="PageInfoModelBinderProvider.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Karma.Extensions.AspNetCore.ModelBinding
{
  /// <summary>
  /// Provides a model binder for the <see cref="PageInfo"/> type.
  /// </summary>
  /// <remarks>This provider determines whether the requested model type is assignable to <see
  /// cref="PageInfo"/>. If so, it returns a <see cref="PageInfoModelBinder"/> instance configured with binders for
  /// string and unsigned integer types. Otherwise, it returns <see langword="null"/>.</remarks>
  public sealed class PageInfoModelBinderProvider : IModelBinderProvider
  {
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
      ArgumentNullException.ThrowIfNull(context);

      if (!context.Metadata.ModelType.IsAssignableTo(typeof(PageInfo)))
      {
        return null;
      }

      ModelMetadata stringMetadata = context.MetadataProvider.GetMetadataForType(typeof(string));
      IModelBinder stringBinder = context.CreateBinder(stringMetadata);

      ModelMetadata uintMetadata = context.MetadataProvider.GetMetadataForType(typeof(uint));
      IModelBinder uintBinder = context.CreateBinder(uintMetadata);

      return new PageInfoModelBinder(stringBinder, uintBinder);
    }
  }

  /// <summary>
  /// Provides model binding for the <see cref="PageInfo"/> type, supporting binding of pagination-related properties
  /// such as <c>After</c>, <c>Before</c>, <c>Offset</c>, and <c>Limit</c>.
  /// </summary>
  /// <remarks>This model binder attempts to bind the <see cref="PageInfo"/> properties from the incoming
  /// request data, such as query string parameters, route data, or form data. 
  /// <para>The <c>After</c> property supports a fallback binding to "Cursor" if the standard property name fails.</para>
  /// <para>The <c>After</c> and <c>Before</c> properties are bound as strings, while the <c>Offset</c> and 
  /// <c>Limit</c> properties are bound as unsigned integers.</para>
  /// <para>The binding succeeds if at least one property is successfully bound.</para></remarks>
  public sealed class PageInfoModelBinder : IModelBinder
  {
    private const string CURSOR_FALLBACK_NAME = "Cursor";
    private readonly IModelBinder _stringBinder;
    private readonly IModelBinder _uintBinder;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageInfoModelBinder"/> class with the specified model binders.
    /// </summary>
    /// <param name="stringBinder">The model binder used for binding string values.</param>
    /// <param name="uintBinder">The model binder used for binding unsigned integer values.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stringBinder"/> or <paramref name="uintBinder"/> is <see langword="null"/>.</exception>
    public PageInfoModelBinder(IModelBinder stringBinder, IModelBinder uintBinder)
    {
      _stringBinder = stringBinder ?? throw new ArgumentNullException(nameof(stringBinder));
      _uintBinder = uintBinder ?? throw new ArgumentNullException(nameof(uintBinder));
    }

    /// <inheritdoc />
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
      // Check if the binding context is null
      ArgumentNullException.ThrowIfNull(bindingContext);

      string modelName = bindingContext.ModelName ?? QueryParameterNames.Page;

      // Get the value from the request (e.g., from query string, route data, form data)
      ModelBindingResult afterValueResult = await TryBindStrongModelAsync(bindingContext, _stringBinder, nameof(PageInfo.After));
      if (!afterValueResult.IsModelSet)
      {
        string cursorValueModelName = ModelNames.CreateIndexModelName(modelName, CURSOR_FALLBACK_NAME);
        afterValueResult = await TryBindStrongModelAsync(bindingContext, _stringBinder, nameof(PageInfo.After), cursorValueModelName);
      }

      ModelBindingResult beforeValueResult = await TryBindStrongModelAsync(bindingContext, _stringBinder, nameof(PageInfo.Before));
      ModelBindingResult offsetValueResult = await TryBindStrongModelAsync(bindingContext, _uintBinder, nameof(PageInfo.Offset));
      ModelBindingResult limitValueResult = await TryBindStrongModelAsync(bindingContext, _uintBinder, nameof(PageInfo.Limit));

      bool isOffsetValid = IsValidBinding(bindingContext, offsetValueResult, modelName, nameof(PageInfo.Offset));
      bool isLimitValid = IsValidBinding(bindingContext, limitValueResult, modelName, nameof(PageInfo.Limit));
      bool hasValidBinding = isOffsetValid || isLimitValid || afterValueResult.IsModelSet || beforeValueResult.IsModelSet;

      if (!hasValidBinding || bindingContext.ModelState.ErrorCount > 0)
      {
        // If no properties were bound, fail the binding - there isn't enough information to create a PageInfo
        bindingContext.Result = ModelBindingResult.Failed();
        return;
      }

      var model = new PageInfo(
        after: GetStringValue(afterValueResult),
        before: GetStringValue(beforeValueResult),
        offset: GetUintValue(offsetValueResult),
        limit: GetUintValue(limitValueResult));

      bindingContext.Result = ModelBindingResult.Success(model);
    }

    private static void AddModelError(ModelBindingContext bindingContext, string modelName, string propertyName, string? attemptedValue)
    {
      string propertyModelName = ModelNames.CreateIndexModelName(modelName, propertyName);
      string errorMessage = propertyName switch
      {
        nameof(PageInfo.Offset) => $"The value '{attemptedValue}' is not valid for {propertyName}. {propertyName} must be a non-negative integer.",
        nameof(PageInfo.Limit) => $"The value '{attemptedValue}' is not valid for {propertyName}. {propertyName} must be a positive integer (greater than 0).",
        _ => $"The value '{attemptedValue}' is not valid for {propertyName}."
      };

      _ = bindingContext.ModelState.TryAddModelError(propertyModelName, errorMessage);
    }

    private static bool HasParameterValue(ModelBindingContext bindingContext, string modelName, string propertyName)
    {
      string propertyModelName = ModelNames.CreateIndexModelName(modelName, propertyName);
      ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(propertyModelName);
      return valueProviderResult != ValueProviderResult.None && !string.IsNullOrEmpty(valueProviderResult.FirstValue);
    }

    private static string? GetParameterValue(ModelBindingContext bindingContext, string modelName, string propertyName)
    {
      string propertyModelName = ModelNames.CreateIndexModelName(modelName, propertyName);
      ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(propertyModelName);
      return valueProviderResult.FirstValue;
    }

    private static bool IsValidBinding(ModelBindingContext bindingContext, ModelBindingResult modelBindingResult, string modelName, string propertyName)
    {
      if (modelBindingResult.IsModelSet)
      {
        return true;
      }

      if (HasParameterValue(bindingContext, modelName, propertyName))
      {
        string propertyModelName = ModelNames.CreateIndexModelName(modelName, propertyName);

        // Clear any existing errors for this property to avoid duplicates
        if (bindingContext.ModelState.TryGetValue(propertyModelName, out ModelStateEntry? modelStateEntry))
        {
          modelStateEntry.Errors.Clear();
        }

        AddModelError(bindingContext, modelName, propertyName, GetParameterValue(bindingContext, modelName, propertyName));
      }

      return false;
    }

    private static string GetStringValue(ModelBindingResult result) =>
      CastOrDefault<string>(result.Model) ?? string.Empty;

    private static uint GetUintValue(ModelBindingResult result) =>
      CastOrDefault<uint>(result.Model);

    private static TModel? CastOrDefault<TModel>(object? model) =>
      (model is TModel tModel) ? tModel : default;

    private static Task<ModelBindingResult> TryBindStrongModelAsync(
        ModelBindingContext bindingContext,
        IModelBinder binder,
        string propertyName)
    {
      string modelName = bindingContext.ModelName ?? QueryParameterNames.Page;
      string propertyModelName = ModelNames.CreateIndexModelName(modelName, propertyName);
      return TryBindStrongModelAsync(bindingContext, binder, propertyName, propertyModelName);
    }

    private static async Task<ModelBindingResult> TryBindStrongModelAsync(
        ModelBindingContext bindingContext,
        IModelBinder binder,
        string propertyName,
        string propertyModelName)
    {
      ModelMetadata propertyModelMetadata = bindingContext.ModelMetadata.Properties[propertyName]!;

      using (bindingContext.EnterNestedScope(
          modelMetadata: propertyModelMetadata,
          fieldName: propertyName,
          modelName: propertyModelName,
          model: null))
      {
        await binder.BindModelAsync(bindingContext);

        return bindingContext.Result;
      }
    }
  }
}
