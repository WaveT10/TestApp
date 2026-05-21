using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using System.IO;
using TestApp.Services;
using Avalonia.Input.Platform;
using TestApp.Abstractions;

namespace TestApp
{
    public partial class FunctionsViewModel : ObservableValidator
    {
        private const double AcceptedDistanceNearMovedPoint = 5;
        private readonly IModelConverterFactory _modelConverterFactory;
        private readonly IClipboard? _clipboard;
        private readonly IPendingChangesDialogService _pendingChangesDialogService;
        private Functions _oldFunctionsModel = new Functions();
        private Functions _functionsModel = new Functions();
        private Function? _movedFunction;
        private Point? _movedPoint;
        private string? _activeFilePath = null;
        [ObservableProperty]
        private int? _selectedFunctionNumber;
        [ObservableProperty]
        private FunctionViewModel _selectedFunction;
        [ObservableProperty]
        private bool _isFunctionSelected;
        [ObservableProperty]
        private ObservableCollection<FunctionViewModel> _functions = new ObservableCollection<FunctionViewModel>();
        [ObservableProperty]
        private ObservableCollection<ISeries> _lineSeriesList = new ObservableCollection<ISeries>();

        public FunctionsViewModel(IModelConverterFactory modelConverterFactory, IClipboard? clipboard, IPendingChangesDialogService pendingChangesDialogService)
        {
            _modelConverterFactory = modelConverterFactory;
            _clipboard = clipboard;
            _pendingChangesDialogService = pendingChangesDialogService;
        }

        private bool CanNotSave => _oldFunctionsModel.Equals(_functionsModel) || _functionsModel.AllFunctionsEmpty;

        [RelayCommand]
        private void AddFunction()
        {
            var function = _functionsModel.AddFunction();
            AddFunction(function);
        }

        [RelayCommand]
        private void AddInversedFunction(int functionNumber)
        {
            var function = _functionsModel.AddInversedFunction(functionNumber);
            AddFunction(function);
        }

        [RelayCommand]
        private void RemoveFunction(int functionNumber)
        {
            var isSelectedItemRemoved = SelectedFunctionNumber == functionNumber;
            _functionsModel.Remove(functionNumber);
            var function = GetFunction(functionNumber);
            LineSeriesList.Remove(function.LineSeries);
            Functions.Remove(function);
            SelectedFunctionNumber = isSelectedItemRemoved && !_functionsModel.Empty ? _functionsModel.LastFunctionNumber : SelectedFunctionNumber;
            
        }

        [RelayCommand]
        public void StartMovingFunctionPoint(PointerCommandArgs args)
        {
            var chart = (ICartesianChartView)args.Chart;
            var position = chart.ScalePixelsToData(args.PointerPosition);
            _movedFunction = _functionsModel.GetLastFunctionWithNearPoint(position.X, position.Y, AcceptedDistanceNearMovedPoint);

            if (_movedFunction is not null)
            {
                _movedPoint = _movedFunction.GetLastNearPoint(position.X, position.Y, AcceptedDistanceNearMovedPoint);
                SelectedFunctionNumber = _movedFunction.Number;
            }
        }

        [RelayCommand]
        public void MoveFunctionPoint(PointerCommandArgs args)
        {
            if (_movedFunction is null || _movedPoint is null)
            {
                return;
            }

            var chart = (ICartesianChartView)args.Chart;
            var position = chart.ScalePixelsToData(args.PointerPosition);
            var functionViewModel = GetFunction(_movedFunction.Number);
            functionViewModel.UpdatePoint(_movedPoint, position.X, position.Y, AcceptedDistanceNearMovedPoint);
        }

        [RelayCommand]
        public void StopMovingFunctionPoint(PointerCommandArgs args)
        {
            _movedFunction = null;
            _movedPoint = null;
        }

        [RelayCommand]
        private async Task LoadFunctions(IStorageProvider storageProvider)
        {
            if (!await ProceedWithSavingFunctions(storageProvider))
            {
                return;
            }

            using var file = await storageProvider.OpenFunctionsFilePickerAsync();

            if (file is null)
            {
                return;
            }

            var content = await file.Open();
            _modelConverterFactory.Get(file.GetExtension()).ToFunctions(_functionsModel, content, overwriteIfEmptyText: true);
            _oldFunctionsModel = _functionsModel.Clone();
            Functions = new ObservableCollection<FunctionViewModel>(_functionsModel.List
                                                                                   .OrderBy(function => function.Number)
                                                                                   .Select(function => new FunctionViewModel(function, _clipboard, _modelConverterFactory)));
            LineSeriesList = new ObservableCollection<ISeries>(Functions.Select(function => function.LineSeries));
            SelectedFunctionNumber = _functionsModel.LastFunctionNumber;
            _activeFilePath = file.TryGetLocalPath();
        }

        [RelayCommand]
        private async Task<bool> SaveFunctions(IStorageProvider storageProvider)
        {
            if (CanNotSave)
            {
                return true;
            }

            if (_activeFilePath is null)
            {
                return await SaveFunctionsAs(storageProvider);
            }

            var extension = Path.GetExtension(_activeFilePath);
            var content = _modelConverterFactory.Get(extension).FromFunctions(_functionsModel);
            await File.WriteAllTextAsync(_activeFilePath, content);
            _oldFunctionsModel = _functionsModel.Clone();
            return true;
        }

        [RelayCommand]
        private async Task<bool> SaveFunctionsAs(IStorageProvider storageProvider)
        {
            using var file = await storageProvider.SaveFunctionsFilePickerAsync();

            if (file is null)
            {
                return false;
            }

            var content = _modelConverterFactory.Get(file.GetExtension()).FromFunctions(_functionsModel);
            await file.Save(content);
            _oldFunctionsModel = _functionsModel.Clone();
            _activeFilePath = file.TryGetLocalPath();
            return true;
        }

        public async Task<bool> ProceedWithSavingFunctions(IStorageProvider storageProvider)
        {
            if (CanNotSave)
            {
                return true;
            }

            var pendingChangesResult = await _pendingChangesDialogService.Show();

            switch (pendingChangesResult)
            {
                case PendingChangesDialogResult.Cancel:
                    return false;
                case PendingChangesDialogResult.SaveAndProceed:
                    return await SaveFunctions(storageProvider);
                case PendingChangesDialogResult.ProceedWithoutSaving:
                    return true;
            }

            return false;
        }

        partial void OnSelectedFunctionNumberChanged(int? value)
        {
            if (value.HasValue)
            {
                SelectedFunction = GetFunction(value.Value);
                IsFunctionSelected = true;
                return;
            }

            IsFunctionSelected = false;
        }

        private void AddFunction(Function function)
        {
            var functionViewModel = new FunctionViewModel(function, _clipboard, _modelConverterFactory);            
            Functions.Add(functionViewModel);
            LineSeriesList.Add(functionViewModel.LineSeries);
            SelectedFunctionNumber = functionViewModel.Number;
        }

        private FunctionViewModel GetFunction(int functionNumber)
        {
            return Functions.Single(function => function.Number == functionNumber);
        }
    }
}