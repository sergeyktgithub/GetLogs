using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using GetLogsClient.ListBoxContent;
using GetLogsClient.Models;
using GetLogsClient.NetServices;
using NetCom;
using NetComModels;
using ZipExtension;
using Msg = GetLogsClient.ListBoxContent.Msg;

namespace GetLogsClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private bool _radioButtonIlogsChecked;
        private bool _radioButtonLogsChecked;
        private DateTime _fromDateTime;
        private DateTime _toDateTime;
        private string _saveDirectory;
        private int _accIdIndex;
        private string _accIdText;
        private LastState _lastState = new LastState();
        private AboutLogs _aboutLogs;
        private AboutILogs _aboutILogs;
        private LoaderFile _loaderFile;
        private CancellationTokenSource _cts;
        private string _accIdSelected;
        private int _selectedPatternIndex;
        private string _patternText;
        private string _selectedPattern;
        private bool _buttonLoadLogsEnabled;
        private bool _buttonGiveGlanceEnabled;
        private Msg _selectedMsg;
        private PreparedArchive _preparedArchive;
        private MsgUdpListener _msgUdpListener;

        public bool RadioButtonIlogsChecked
        {
            get => _radioButtonIlogsChecked;
            set => SetProperty(ref _radioButtonIlogsChecked, value, nameof(RadioButtonIlogsChecked));
        }

        public bool RadioButtonLogsChecked
        {
            get => _radioButtonLogsChecked;
            set => SetProperty(ref _radioButtonLogsChecked, value, nameof(RadioButtonLogsChecked));
        }

        public bool ButtonGiveGlanceEnabled
        {
            get => _buttonGiveGlanceEnabled;
            set => SetProperty(ref _buttonGiveGlanceEnabled, value, nameof(ButtonGiveGlanceEnabled));
        }

        public bool ButtonLoadLogsEnabled
        {
            get => _buttonLoadLogsEnabled;
            set => SetProperty(ref _buttonLoadLogsEnabled, value, nameof(ButtonLoadLogsEnabled));
        }

        public DateTime FromDateTime
        {
            get => _fromDateTime;
            set => SetProperty(ref _fromDateTime, value, nameof(FromDateTime));
        }

        public DateTime ToDateTime  
        {
            get => _toDateTime;
            set => SetProperty(ref _toDateTime, value, nameof(ToDateTime));
        }

        public string SaveDirectory
        {
            get => _saveDirectory;
            set => SetProperty(ref _saveDirectory, value, nameof(SaveDirectory));
        }

        public ObservableCollection<string> AccIdList { get; set; }

        public int AccIdIndex
        {
            get => _accIdIndex;
            set => SetProperty(ref _accIdIndex, value, nameof(AccIdIndex));
        }

        public string AccIdText
        {
            get => _accIdText;
            set => SetProperty(ref _accIdText, value, nameof(AccIdText), () =>
            {
                AccIdList.Add(value);
            }, () => !string.IsNullOrEmpty(value) && !AccIdList.Contains(value));
        }

        public string AccIdSelected
        {
            get => _accIdSelected;
            set => SetProperty(ref _accIdSelected, value);
        }

        public ObservableCollection<string> PatternList { get; set; }

        public int SelectedPatternIndex
        {
            get => _selectedPatternIndex;
            set => SetProperty(ref _selectedPatternIndex, value, nameof(SelectedPatternIndex));
        }

        public string PatternText
        {
            get => _patternText;
            set => SetProperty(ref _patternText, value, nameof(PatternText), () =>
            {
                PatternList.Add(value);
            }, () => !string.IsNullOrEmpty(value) && !PatternList.Contains(value));
        }

        public string SelectedPattern
        {
            get => _selectedPattern;
            set => SetProperty(ref _selectedPattern, value);
        }

        private string GetLogsClientPath { get; set; }

        private string LastStateJsonFilePath { get; set; }

        public ObservableCollection<Msg> MsgList { get; set; }

        public Msg SelectedMsg
        {
            get => _selectedMsg;
            set => SetProperty(ref _selectedMsg, value);
        }

        public MainWindowViewModel()
        {
            _cts = new CancellationTokenSource();

            GetLogsClientPath = Environment.GetEnvironmentVariable("APPDATA") + "\\GetLogsClient";
            LastStateJsonFilePath = Environment.GetEnvironmentVariable("APPDATA") + "\\GetLogsClient\\LastState.json";

            AccIdList = new ObservableCollection<string>();
            PatternList = new ObservableCollection<string>();

            ButtonLoadLogsEnabled = false;
            ButtonGiveGlanceEnabled = true;

            RadioButtonLogsChecked = true;

            MsgList = new ObservableCollection<Msg>();
        }

        public void ExternalInitializer()
        {
            ReadLastState();
            InitializeNetworkServices();
        }

        public void ViewFileInExternalEditor()
        {
            if (SelectedMsg != null && SelectedMsg is FileLogMsg fileLog && File.Exists(fileLog.Path))
            {
                var ext = Path.GetExtension(fileLog.Path);
                if (ext == ".txt")
                {
                    var filePath = "\"" + fileLog.Path.Replace("/", "\\") + "\"";
                    Process.Start("C:\\Program Files (x86)\\Notepad++\\notepad++.exe", filePath);
                }
                
                if (ext == ".ilog")
                {
                    var filePath = "\"" + fileLog.Path.Replace("/", "\\") + "\"";
                    var argument = "--path=" + filePath;
                    Process.Start("E:\\programming\\NeoLab\\GetSeqLogs_Base\\IlogPlayer\\bin\\Debug\\netcoreapp3.1\\IlogPlayer.exe", argument);
                }
            }
        }

        public async void DownloadArchive()
        {
            AddMsg("Загружаю ...");

            try
            {
                if (string.IsNullOrEmpty(SaveDirectory))
                {
                    AddMsg("Не задан путь сохранения");
                    return;
                }

                if (_preparedArchive != null && _preparedArchive.IsEmpty == false)
                {
                    var archNameWithoutExt = Path.GetFileNameWithoutExtension(_preparedArchive.Name);
                    var archDir = Path.Combine(SaveDirectory, archNameWithoutExt ?? throw new InvalidOperationException());
                    var archPath = Path.Combine(archDir, _preparedArchive.Name);

                    Directory.CreateDirectory(archDir);

                    await _loaderFile.FileRequestAsync(_preparedArchive.FullPath, archPath, _preparedArchive.Source);

                    AddMsg($"Извлечение архива: {archPath}");
                    var files = Zip.Extract(archPath);

                    foreach (var file in files)
                    {
                        AddMsg(new FileLogMsg(file));
                    }
                }
                else
                {
                    ButtonGiveGlanceEnabled = false;
                    AddMsg("Нечего загружать повторите поиск");
                }
            }
            catch (Exception ex)
            {
                AddMsg($"Ошибка: {ex.Message}");
            }
        }

        public async void GiveGlanceLogFiles()
        {
            try
            {
                ButtonGiveGlanceEnabled = false;

                AddMsg("Ждемс ...");

                var pattern = "";
                if (!string.IsNullOrEmpty(PatternText))
                {
                    pattern = PatternText;
                }

                var accId = AccIdSelected;
                if (string.IsNullOrEmpty(accId))
                {
                    AddMsg($"AccId не задан");
                    return;
                }

                ButtonLoadLogsEnabled = false;

                if (RadioButtonLogsChecked)
                {
                    _preparedArchive = await _aboutLogs.CheckExistLogsAsync(pattern, accId, FromDateTime, ToDateTime, 1000 * 60);
                }

                if (RadioButtonIlogsChecked)
                {
                    _preparedArchive = await _aboutILogs.CheckExistLogsAsync(accId, FromDateTime, ToDateTime, 1000 * 60);
                }

                if (_preparedArchive != null && _preparedArchive.IsEmpty == false)
                {
                    foreach (var file in _preparedArchive.Files)
                    {
                        AddMsg(new FileLogMsg(file));
                    }

                    var totalSizeMb = _preparedArchive.TotalSize / 1024 / 1024;

                    if (string.IsNullOrEmpty(pattern))
                        AddMsg($"Найдено файл(ов): {_preparedArchive.Files.Count}, Размер = {totalSizeMb} Mb, From = {FromDateTime}, To = {ToDateTime}");
                    else
                        AddMsg($"Найдено файл(ов): {_preparedArchive.Files.Count}, Размер = {totalSizeMb} Mb, Pattern = {pattern}, From = {FromDateTime}, To = {ToDateTime}");

                    ButtonLoadLogsEnabled = true;
                }
                else
                {
                    AddMsg($"Не удалось найти: Pattern = {pattern}, From = {FromDateTime}, To = {ToDateTime}");
                    _preparedArchive = null;
                }
            }
            catch (Exception ex)
            {
                AddMsg(ex.Message);
            }
            finally
            {
                ButtonGiveGlanceEnabled = true;
            }
        }

        public void Dispose()
        {
            SaveState();

            _cts?.Dispose();
            _loaderFile?.Dispose();
            _msgUdpListener?.Dispose();
        }

        private void ReadLastState()
        {
            if (File.Exists(LastStateJsonFilePath) == false)
            {
                AddMsg($"Не удалось загрузить последнее состояние, файл не найден {LastStateJsonFilePath}");
                return;
            }

            var lastState = JsonSerializer.Deserialize<LastState>(File.ReadAllText(LastStateJsonFilePath));

            if (lastState.AccIdList != null)
            {
                foreach (var item in lastState.AccIdList)
                {
                    AccIdList.Add(item);
                }
            }

            AccIdIndex = lastState.AccIdIndex < 0 ? 0 : lastState.AccIdIndex;

            if (lastState.PatternList != null)
            {
                foreach (var item in lastState.PatternList)
                {
                    PatternList.Add(item);
                }
            }

            SelectedPatternIndex = lastState.SelectedPatternIndex < 0 ? 0 : lastState.SelectedPatternIndex;

            FromDateTime = lastState.FromDateTime;
            ToDateTime = lastState.ToDateTime;
            SaveDirectory = string.IsNullOrEmpty(lastState.SavePath) ? "" : lastState.SavePath;

            UpdateLastStateConfig();
        }

        private void SaveState()
        {
            if (_lastState == null)
            {
                return;
            }

            UpdateLastStateConfig();

            var json = JsonSerializer.Serialize(_lastState);

            if (!Directory.Exists(GetLogsClientPath))
                Directory.CreateDirectory(GetLogsClientPath);

            File.WriteAllText(LastStateJsonFilePath, json);
        }

        private void InitializeNetworkServices()
        {
            _msgUdpListener = new MsgUdpListener(GlobalProperties.ClientMsgPort, _cts.Token);
            _msgUdpListener.Start();

            var srcIp = NetHelper.GetLocalIpAddress();
            var srcMsgEndPoint = new IPEndPoint(srcIp, GlobalProperties.ClientMsgPort);
            var srcFileEndPoint = new IPEndPoint(srcIp, NetHelper.GetRandomServerPort());
            var destBroadcastEndPoint = new IPEndPoint(IPAddress.Parse("10.99.132.255"), GlobalProperties.ServerMsgPort);

            _aboutLogs = new AboutLogs(new TwoEndPoints(srcMsgEndPoint, destBroadcastEndPoint), _msgUdpListener);
            _aboutILogs = new AboutILogs(new TwoEndPoints(srcMsgEndPoint, destBroadcastEndPoint), _msgUdpListener);

            _loaderFile = new LoaderFile(srcMsgEndPoint, srcFileEndPoint, _msgUdpListener, _cts.Token);
            _loaderFile.BeginDownloadEvent += (sender, str) => AddMsg(str);
            _loaderFile.EndDownloadEvent += (sender, str) => AddMsg(str);
            _loaderFile.ErrorEvent += (sender, str) => AddMsg(str);
        }

        private void UpdateLastStateConfig()
        {
            _lastState.AccIdList = AccIdList.ToArray();
            _lastState.AccIdIndex = AccIdIndex;

            _lastState.PatternList = PatternList.ToArray();
            _lastState.SelectedPatternIndex = SelectedPatternIndex;

            _lastState.FromDateTime = _fromDateTime;
            _lastState.ToDateTime = _toDateTime;
            _lastState.SavePath = SaveDirectory;
        }

        private void AddMsg(Msg msg)
        {
            MsgList.Insert(0, msg);
        }

        private void AddMsg(string msg)
        {
            AddMsg(new Msg($"[{DateTime.Now:T}] : {msg}"));
        }
    }
}