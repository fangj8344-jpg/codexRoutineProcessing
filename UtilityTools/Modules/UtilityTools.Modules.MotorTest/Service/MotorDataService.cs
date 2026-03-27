using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Modules.MotorTest.SQLite;

namespace UtilityTools.Modules.MotorTest.Service
{
    public static class MotorDataService
    {
        // ==========================================
        // 1. 数据库加载（核心业务逻辑隔离）
        // ==========================================
        public static async Task LoadFromSqliteAsync(
            ObservableCollection<FiveAxisModel> motors,
            EnumMotorAxisType axisType,
            int headIndex,
            int loadSize)
        {
            if (motors == null || motors.Count == 0) return;

            int pointNumber, speedNumber;
            List<PlotViewPointMessage> points;
            List<PlotViewSpeedMessage> speeds;

            // 根据轴类型决定查哪个库
            if (axisType == EnumMotorAxisType.TwoAxisMotor)
            {
                using var db = new MotorDbContext("TwoMotorTetsMessages.db");
                pointNumber = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
                speedNumber = await SpliteOperate.GetPlotViewSpeedMessageCountAsync(db);
                points = await SpliteOperate.GetPlotViewPointMessagesAsync(db, headIndex, loadSize > pointNumber ? pointNumber : loadSize);
                speeds = await SpliteOperate.GetPlotViewSpeedMessagesAsync(db, headIndex, loadSize > speedNumber ? speedNumber : loadSize);
            }
            else
            {
                using var db = new MotorDbContext("FiveMotorTetsMessages.db");
                pointNumber = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
                speedNumber = await SpliteOperate.GetPlotViewSpeedMessageCountAsync(db);
                points = await SpliteOperate.GetPlotViewPointMessagesAsync(db, headIndex, loadSize > pointNumber ? pointNumber : loadSize);
                speeds = await SpliteOperate.GetPlotViewSpeedMessagesAsync(db, headIndex, loadSize > speedNumber ? speedNumber : loadSize);
            }

            // 核心：分发数据并执行“安全换水”
            foreach (var motor in motors)
            {
                // 筛选出属于当前轴的数据
                var motorPoints = points.Where(m => m.MotorModelAxis == motor.MotorModel.MotorModelAxis).ToList();
                var motorSpeeds = speeds.Where(m => m.MotorModelAxis == motor.MotorModel.MotorModelAxis).ToList();

                // 执行安全填充（不改变引用地址，不卡界面）
                motor.MotorModel.PointList.Clear();
                foreach (var p in motorPoints) motor.MotorModel.PointList.Add(p);

                motor.MotorModel.SpeedList.Clear();
                foreach (var s in motorSpeeds) motor.MotorModel.SpeedList.Add(s);
            }
        }

        // ==========================================
        // 2. JSON 序列化（存文件）
        // ==========================================
        public static void SaveToJson(string path, FiveAxisModel motor)
        {
            if (motor == null) return;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string id = motor.MotorModel.MotorParams.MotorModelID.ToString();

            File.WriteAllText(Path.Combine(path, $"{id}Point.json"), JsonConvert.SerializeObject(motor.MotorModel.PointList));
            File.WriteAllText(Path.Combine(path, $"{id}Speed.json"), JsonConvert.SerializeObject(motor.MotorModel.SpeedList));
            File.WriteAllText(Path.Combine(path, $"{id}testMessage.json"), JsonConvert.SerializeObject(motor.MotorTestMessages));
        }

        // ==========================================
        // 3. JSON 反序列化（读文件）
        // ==========================================
        public static void LoadFromJson(string path, FiveAxisModel motor)
        {
            if (motor == null) return;

            string id = motor.MotorModel.MotorParams.MotorModelID.ToString();
            string pointPath = Path.Combine(path, $"{id}Point.json");
            string speedPath = Path.Combine(path, $"{id}Speed.json");
            string msgPath = Path.Combine(path, $"{id}testMessage.json");

            if (File.Exists(pointPath))
            {
                var data = JsonConvert.DeserializeObject<List<PlotViewPointMessage>>(File.ReadAllText(pointPath));
                motor.MotorModel.PointList.Clear();
                if (data != null) foreach (var p in data) motor.MotorModel.PointList.Add(p);
            }

            if (File.Exists(speedPath))
            {
                var data = JsonConvert.DeserializeObject<List<PlotViewSpeedMessage>>(File.ReadAllText(speedPath));
                motor.MotorModel.SpeedList.Clear();
                if (data != null) foreach (var s in data) motor.MotorModel.SpeedList.Add(s);
            }

            if (File.Exists(msgPath))
            {
                var data = JsonConvert.DeserializeObject<ObservableCollection<MotorTestMessage>>(File.ReadAllText(msgPath));
                motor.MotorTestMessages.Clear();
                if (data != null) foreach (var m in data) motor.MotorTestMessages.Add(m);
            }
        }
    }
}
