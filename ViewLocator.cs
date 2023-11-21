using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

using AvaloniaSourceGeneratedViewLocation.ViewModels;
using AvaloniaSourceGeneratedViewLocation.Views;

namespace AvaloniaSourceGeneratedViewLocation
{
    public partial class ViewLocator  : IDataTemplate
    {
        public readonly Dictionary<Type, Func<Control>> Locator = new();
        
        public Control Build(object? data)
        {
            string  pos  = nameof(ViewLocator);
            string? name = data?.GetType().Name;

            Debug.WriteLine($"[[{name}]]");

            if (data is null) return new TextBlock { Text = "No VM provided" };
            else if (data is MainWindowViewModel)
            {
                return new MainWindow();
            }

            if (Locator.TryGetValue(data.GetType(), out var factory))
            {
                Debug.WriteLine($"Success: {pos}: {name}");
                return factory.Invoke() ?? new TextBlock { Text = $"VM Not Registered: {data.GetType().Name}" };
            }
            else
            {
                Debug.WriteLine($"In: {pos}: {name}");
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object? data)
        {
            Debug.WriteLine($"In: {nameof(ViewLocator)}: {data?.GetType().Name}");
            return data is ViewModelBase;
        }
    }
}