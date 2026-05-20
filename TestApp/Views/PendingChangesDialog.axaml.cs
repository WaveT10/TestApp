using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TestApp;

public partial class PendingChangesDialog : Window
{
    public PendingChangesDialog()
    {
        InitializeComponent();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!e.IsProgrammatic)
        {
            Close(PendingChangesDialogResult.Cancel);
        }

        base.OnClosing(e);
    }

    private void OnSaveAndProceedClick(object? sender, RoutedEventArgs e) => Close(PendingChangesDialogResult.SaveAndProceed);
    private void OnProceedWithoutSavingClick(object? sender, RoutedEventArgs e) => Close(PendingChangesDialogResult.ProceedWithoutSaving);
    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(PendingChangesDialogResult.Cancel);

}