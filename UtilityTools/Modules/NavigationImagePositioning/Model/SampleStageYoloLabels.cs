namespace UtilityTools.Modules.NavigationImagePositioning.Model
{
    /// <summary>
    /// 与 YOLO 训练 data.yaml 一致：索引 0=screw，1=sample_stage（见项目 ONNX 说明文档）。
    /// </summary>
    public static class SampleStageYoloLabels
    {
        public const string Screw = "screw";
        public const string SampleStage = "sample_stage";

        public static string[] All { get; } = { Screw, SampleStage };
    }
}
