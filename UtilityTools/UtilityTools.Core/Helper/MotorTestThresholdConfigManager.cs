using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using UtilityTools.Core.Model;

namespace UtilityTools.Core.Helper
{
    /// <summary>
    /// 电机测试阈值配置管理（与 TB 配置放同一目录）。
    /// </summary>
    public static class MotorTestThresholdConfigManager
    {
        private static readonly string ConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
        private static readonly string StageConfigFolder = Path.Combine(ConfigFolder, "MotorTestThresholds");
        public const string DefaultStageKey = "SAMPLESTANDARD";

        public static MotorTestThresholdConfigModel Current { get; private set; } = new MotorTestThresholdConfigModel();

        public static void LoadConfig()
        {
            try
            {
                EnsureFolders();
                var defaultConfig = ResolveForStage(DefaultStageKey, out _, out _);
                Current = CloneWithoutStageMap(defaultConfig);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"读取电机测试阈值配置失败，已回退默认值: {ex.Message}");
                Current = new MotorTestThresholdConfigModel();
                SaveForStage(DefaultStageKey, Current);
            }
        }

        public static void SaveConfig()
        {
            SaveForStage(DefaultStageKey, Current);
        }

        public static MotorTestThresholdConfigModel ResolveForStage(string stageType)
        {
            return ResolveForStage(stageType, out _, out _);
        }

        public static MotorTestThresholdConfigModel ResolveForStage(string stageType, out string resolvedStageKey, out bool usedFallback)
        {
            try
            {
                EnsureFolders();
                string requestedKey = NormalizeStageKey(stageType);

                if (TryLoadStageConfig(requestedKey, out var stageConfig))
                {
                    resolvedStageKey = requestedKey;
                    usedFallback = false;
                    return stageConfig;
                }

                if (TryLoadStageConfig(DefaultStageKey, out var defaultConfig))
                {
                    resolvedStageKey = DefaultStageKey;
                    usedFallback = requestedKey != DefaultStageKey;
                    return defaultConfig;
                }

                // 全新环境：生成一份标准20默认配置。
                var freshDefault = new MotorTestThresholdConfigModel();
                SaveForStage(DefaultStageKey, freshDefault);
                resolvedStageKey = DefaultStageKey;
                usedFallback = requestedKey != DefaultStageKey;
                return CloneWithoutStageMap(freshDefault);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"按型号读取电机阈值失败，回退默认值: {ex.Message}");
                resolvedStageKey = DefaultStageKey;
                usedFallback = true;
                return new MotorTestThresholdConfigModel();
            }
        }

        public static void SaveForStage(string stageType, MotorTestThresholdConfigModel stageConfig)
        {
            try
            {
                EnsureFolders();
                string key = NormalizeStageKey(stageType);
                string path = GetStageConfigPath(key);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(stageConfig, options);
                File.WriteAllText(path, jsonString);

                if (key == DefaultStageKey)
                {
                    Current = CloneWithoutStageMap(stageConfig);
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"保存电机测试阈值配置失败: {ex.Message}");
            }
        }

        public static string NormalizeStageKey(string stageType)
        {
            if (string.IsNullOrWhiteSpace(stageType))
                return DefaultStageKey;

            return stageType.Trim().ToUpperInvariant();
        }

        private static void EnsureFolders()
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);
            if (!Directory.Exists(StageConfigFolder))
                Directory.CreateDirectory(StageConfigFolder);
        }

        private static string GetStageConfigPath(string stageKey)
        {
            return Path.Combine(StageConfigFolder, $"{stageKey}.json");
        }

        private static bool TryLoadStageConfig(string stageKey, out MotorTestThresholdConfigModel config)
        {
            config = new MotorTestThresholdConfigModel();
            string path = GetStageConfigPath(stageKey);
            if (!File.Exists(path))
                return false;

            string json = File.ReadAllText(path);
            config = JsonSerializer.Deserialize<MotorTestThresholdConfigModel>(json) ?? new MotorTestThresholdConfigModel();
            return true;
        }

        private static MotorTestThresholdConfigModel CloneWithoutStageMap(MotorTestThresholdConfigModel src)
        {
            var axisRanges = new Dictionary<string, AxisPulseRange>(StringComparer.OrdinalIgnoreCase);
            if (src.FullTravelAxisRanges != null)
            {
                foreach (var kv in src.FullTravelAxisRanges)
                {
                    if (string.IsNullOrWhiteSpace(kv.Key) || kv.Value == null)
                        continue;
                    axisRanges[kv.Key.Trim().ToUpperInvariant()] = new AxisPulseRange
                    {
                        MinPulse = kv.Value.MinPulse,
                        MaxPulse = kv.Value.MaxPulse
                    };
                }
            }

            return new MotorTestThresholdConfigModel
            {
                MovementMinDistancePulse = src.MovementMinDistancePulse,
                EncoderMinDeltaPulse = src.EncoderMinDeltaPulse,
                LimitAccuracyMaxDiffPulse = src.LimitAccuracyMaxDiffPulse,
                LinearStdDevMaxUm = src.LinearStdDevMaxUm,
                SmoothnessStdDevMaxUm = src.SmoothnessStdDevMaxUm,
                FullTravelMinPulse = src.FullTravelMinPulse,
                FullTravelMaxPulse = src.FullTravelMaxPulse,
                FullTravelAxisRanges = axisRanges
            };
        }
    }
}
