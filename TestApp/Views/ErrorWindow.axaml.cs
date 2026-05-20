using System;
using Avalonia.Controls;

namespace TestApp;

public partial class ErrorWindow : Window
{
    public ErrorWindow(string errorMessage)
    {
        InitializeComponent();
        ErrorMessageTextBox.Text = errorMessage;
    }
}