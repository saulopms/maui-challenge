using MauiDexChallenge.App.Commands;
using MauiDexChallenge.App.Services;
using MauiDexChallenge.Shared.Enums;

namespace MauiDexChallenge.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IDexSubmissionService _submissionService;
    private string _apiBaseUrl;
    private string _statusMessage = "Ready to send a DEX payload.";
    private string _statusTitle = "Idle";
    private string _lastSubmissionSummary = "No requests have been sent yet.";
    private bool _isBusy;

    public MainViewModel(IDexSubmissionService submissionService, AppSettings settings)
    {
        _submissionService = submissionService;
        _apiBaseUrl = settings.BaseUrl;

        SendMachineACommand = new AsyncCommand(() => SendAsync(MachineType.A), () => !IsBusy);
        SendMachineBCommand = new AsyncCommand(() => SendAsync(MachineType.B), () => !IsBusy);
    }

    public string ApiBaseUrl
    {
        get => _apiBaseUrl;
        set => SetProperty(ref _apiBaseUrl, value);
    }

    public string StatusTitle
    {
        get => _statusTitle;
        private set => SetProperty(ref _statusTitle, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string LastSubmissionSummary
    {
        get => _lastSubmissionSummary;
        private set => SetProperty(ref _lastSubmissionSummary, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                SendMachineACommand.RaiseCanExecuteChanged();
                SendMachineBCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncCommand SendMachineACommand { get; }

    public AsyncCommand SendMachineBCommand { get; }

    private async Task SendAsync(MachineType machine)
    {
        try
        {
            IsBusy = true;
            StatusTitle = "Sending";
            StatusMessage = $"Posting DEX report for Machine {machine.ToApiValue()}...";

            var response = await _submissionService.SubmitAsync(ApiBaseUrl, machine, CancellationToken.None);

            StatusTitle = "Success";
            StatusMessage = response.Message;
            LastSubmissionSummary =
                $"Machine {response.Machine} stored at {response.DexDateTime:yyyy-MM-dd HH:mm:ss} with {response.LaneCount} lane meters.";
        }
        catch (Exception ex)
        {
            StatusTitle = "Error";
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
