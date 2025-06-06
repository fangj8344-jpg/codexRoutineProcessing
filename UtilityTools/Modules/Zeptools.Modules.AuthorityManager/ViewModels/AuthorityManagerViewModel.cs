using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text;
using System.Text.Json;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;

namespace Zeptools.Modules.AuthorityManager.ViewModels
{
    public class AuthorityManagerViewModel : RegionViewModelBase
    {
        private AuthorityModel? _authority = null;

        public AuthorityModel? Authority
        {
            get { return _authority; }
            set
            {
                _authority = value;
                RaisePropertyChanged();
                SaveAuthorityCommand.RaiseCanExecuteChanged();
            }
        }

        private string _privateKey;
        public string PrivateKey
        {
            get => _privateKey;
            set { _privateKey = value; RaisePropertyChanged(); }
        }

        private string _publicKey;
        public string PublicKey
        {
            get => _publicKey;
            set { _publicKey = value; RaisePropertyChanged(); }
        }

        public DelegateCommand LoadAuthorityCommand { get; }
        public DelegateCommand SaveAuthorityCommand { get; }
        public DelegateCommand GenerateKeyPairCommand { get; }

        public AuthorityManagerViewModel(IContainerProvider containerProvider)
            : base(containerProvider)
        {
            LoadAuthorityCommand = new DelegateCommand(OnLoadAuthority);
            SaveAuthorityCommand = new DelegateCommand(OnSaveAuthority, () => { return Authority != null; });
            GenerateKeyPairCommand = new DelegateCommand(OnGenerateKeyPair);
        }

        private void OnGenerateKeyPair()
        {
            var keypair = Sm2Method.GenerateKeyPair();
            PrivateKey = keypair.PriKey;
            PublicKey = keypair.PubKey;
        }

        private void OnLoadAuthority()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            // 设置默认扩展名
            dialog.DefaultExt = "info";
            // 设置文件类型过滤器
            dialog.Filter = "Text files (*.info)|*.info|All files (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;
                var data = File.ReadAllBytes(path);
                var jsonData = CompressMethod.DecompressDeflate(data);
                try
                {
                    Authority = JsonSerializer.Deserialize<AuthorityModel>(jsonData);
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error(ex);
                }
            }
        }

        private void OnSaveAuthority()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            // 设置默认文件名
            dialog.FileName = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.au";
            // 设置默认扩展名
            dialog.DefaultExt = "info";
            // 设置文件类型过滤器
            dialog.Filter = "Text files (*.au)|*.au|All files (*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;
                // 将对象序列化为 JSON 字符串
                string jsonString = JsonSerializer.Serialize(Authority, new JsonSerializerOptions { WriteIndented = true });
                var data = CompressMethod.CompressDeflate(jsonString);

                File.WriteAllBytes(path, data);
            }
        }
    }
}