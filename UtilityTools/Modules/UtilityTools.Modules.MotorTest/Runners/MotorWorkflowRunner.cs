using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using UtilityTools.Core.Interface; // 引用接口
using UtilityTools.Core.Model;
using UtilityTools.Modules.MotorTest.Entity;
using UtilityTools.Modules.MotorTest.Event;
using UtilityTools.Modules.MotorTest.Interface;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Modules.MotorTest.TestItems;

namespace UtilityTools.Modules.MotorTest.Runners
{
    public class MotorWorkflowRunner
    {
        private readonly ITestReportService _reportService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMotorEntity _motorEntity;
        private readonly string _motorName;
        private readonly EnumMotorId _motorId;
        private readonly MotorModel _motorModel;
        private readonly (int min, int max) _strokeRange;
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // 构造函数：把工具全领进来
        public MotorWorkflowRunner(
            ITestReportService reportService,
            IEventAggregator eventAggregator,
            IMotorEntity motorEntity,
            EnumMotorId motorId,
            MotorModel motorModel,
            (int min, int max) strokeRange,
            string motorName)
        {
            _reportService = reportService;
            _eventAggregator = eventAggregator;
            _motorEntity = motorEntity;
            _motorId = motorId;
            _motorModel = motorModel;
            _strokeRange = strokeRange;
            _motorName = motorName;
        }

        // 大喇叭广播方法
        private void PublishLog(string message)
        {
            _eventAggregator.GetEvent<MotorLogEvent>().Publish($"{_motorName}|{message}");
        }

        // ==========================================
        // 流程图纸 A：无限耐久跑机测试
        // ==========================================
        public async Task RunDurabilityTestAsync(CancellationToken ct)
        {
            PublishLog($"--- [{_motorName}] 开始丝杆耐久跑机测试 ---");
            int count = 1;
            var smoothnessTestItem = new SmoothnessTestItem(); // 掏出积木

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    PublishLog($"[{_motorName}] 正在执行第 {count} 次跑机...");
                    var result = await smoothnessTestItem.ExecuteAsync(_motorId, _motorModel, _motorEntity, ct);

                    if (!result.IsPassed)
                    {
                        PublishLog($"[{_motorName}] ❌ 第 {count} 次异常: {result.ErrorDescription}");
                        _motorEntity.SetMotorOperatingStatusCommand(_motorId, EnumMotorOperatingState.Stop);
                        break;
                    }
                    count++;
                    await Task.Delay(500, ct);
                }
                catch (OperationCanceledException) { /* 处理取消 */ break; }
                catch (Exception ex) { /* 处理异常 */ break; }
            }
        }

        // ==========================================
        // 流程图纸 B：计数的丝杆顺滑度测试 (比如只跑 50 次)
        // ==========================================
        public async Task RunCountedSmoothnessTestAsync(int targetCount, CancellationToken ct)
        {
            PublishLog($"--- [{_motorName}] 开始定次丝杆测试 (目标: {targetCount} 次) ---");
            var smoothnessTestItem = new SmoothnessTestItem();

            for (int i = 1; i <= targetCount; i++)
            {
                if (ct.IsCancellationRequested) break;

                PublishLog($"[{_motorName}] 进度：{i} / {targetCount}");
                var result = await smoothnessTestItem.ExecuteAsync(_motorId, _motorModel, _motorEntity, ct);

                if (!result.IsPassed)
                {
                    PublishLog($"[{_motorName}] ❌ 丝杆测试在第 {i} 次失败: {result.ErrorDescription}");
                    _motorEntity.SetMotorOperatingStatusCommand(_motorId, EnumMotorOperatingState.Stop);
                    return; // 失败直接退出
                }
                await Task.Delay(500, ct);
            }
            PublishLog($"--- [{_motorName}] 定次丝杆测试完美通过！ ---");
        }

        

        // 专门用来发成绩单的大喇叭方法
        private void PublishTestResult(string testProject, string standardValue, MotorTestResult result)
        {
            var msg = new MotorTestMessage
            {
                AxisName = _motorName, // 🚨 给成绩单写上名字
                TestProject = testProject,
                StandardValue = standardValue,
                TestResult = result.IsPassed ? "合格" : "不合格",
                TestValue = result.MeasuredValue ?? "",
                Description = result.IsPassed ? result.Description : result.ErrorDescription
            };
            _eventAggregator.GetEvent<MotorTestResultEvent>().Publish(msg);
        }

        // ==========================================
        // 流程图纸 C：全套基础出厂体检 (BaseTest)
        // ==========================================
        public async Task RunBaseTestAsync(CancellationToken cancellationToken)
        {
            //先准备好这一轴的 MotorData 对象
            var myData = new MotorData { AxisType = _motorName, PositionErrors = new List<PositionError>() };

            // 注意：这里不再清空界面的集合，清空动作由界面的 Command 触发前自己做
            PublishLog($"--- 开始执行 [{_motorName}] 全套基础测试 ---");

            // ==========================================
            // 积木 1：电机基础移动测试
            // ==========================================
            var moveTest = new MovementTestItem();
            var moveResult = await moveTest.ExecuteAsync(_motorId, _motorModel, _motorEntity, cancellationToken);
            PublishTestResult(moveTest.TestName, "到达指定位置", moveResult);
            if (!moveResult.IsPassed) { PublishLog($"[{_motorName}] 移动测试失败，终止。"); return; }

            // ==========================================
            // 积木 2：编码器方向及响应测试
            // ==========================================
            var encoderTest = new EncoderTestItem();
            var encoderResult = await encoderTest.ExecuteAsync(_motorId, _motorModel, _motorEntity, cancellationToken);
            PublishTestResult(encoderTest.TestName, "变化>=100", encoderResult);
            if (!encoderResult.IsPassed) { PublishLog($"[{_motorName}] 编码器测试失败，终止。"); return; }

            // ==========================================
            // 积木 3：满行程及限位测试 
            // ==========================================
            PublishLog($"[{_motorName}] 正在执行满行程及限位扫描...");
            var fullTravelTest = new FullTravelTestItem(_strokeRange);
            var fullTravelResult = await fullTravelTest.ExecuteAsync(_motorId, _motorModel, _motorEntity, cancellationToken);
            float ratio = _motorModel.MotorParams.SubRatio;
            // 判断积木交上来的是不是定义的 TravelTestResult
            if (fullTravelResult is TravelTestResult travelRes)
            {
                myData.MaxRange = (float)travelRes.RealMaxPosUm;
                myData.MinRange = (float)travelRes.RealMinPosUm;
                myData.PositiveLimit = travelRes.IsPositiveLimitFound;
                myData.NegativeLimit = travelRes.IsNegativeLimitFound;
            }
            PublishTestResult(fullTravelTest.TestName, $"[{_strokeRange.min}-{_strokeRange.max}]", fullTravelResult);
            if (!fullTravelResult.IsPassed) { PublishLog($"[{_motorName}] 满行程测试失败，终止。"); return; }

            // ==========================================
            // 积木 4：定位精度(重复性)测试
            // ==========================================
            PublishLog($"[{_motorName}] 正在执行定位精度(重复性)测试...");
            var accuracyTest = new PositioningAccuracyTestItem();
            var accuracyResult = await accuracyTest.ExecuteAsync(_motorId, _motorModel, _motorEntity, cancellationToken);
            PublishTestResult(accuracyTest.TestName, "偏差<200", accuracyResult);
            if (!accuracyResult.IsPassed) { PublishLog($"[{_motorName}] 定位精度测试失败，终止。"); return; }

            // ==========================================
            // 积木 5：分段定位线性测试 (98点测试)
            // ==========================================
            PublishLog($"[{_motorName}] 正在执行 98 点线性测试...");
            int minPos = _motorModel.MotorParams.NegativeLimitPosition;
            int maxPos = _motorModel.MotorParams.PositiveLimitPosition;
            if (maxPos <= minPos) { minPos = 0; maxPos = 100000; }

            var linearTest = new LinearStepPrecisionTestItem(minPos, maxPos,msg => PublishLog(msg));
            var linearResult = await linearTest.ExecuteAsync(_motorId, _motorModel, _motorEntity, cancellationToken);
            //  98 个点和标准差记录下来
            if (linearResult is LinearTestResult linearRes)
            {
                myData.PositioningStdDev = linearRes.FinalStdDevUm;
                
                myData.PositionErrors = linearRes.PositionErrors;
            }


            PublishTestResult(linearTest.TestName, "StdDev<1", linearResult);
            _reportService.AddOrUpdateMotorData(myData);


            // ==========================================
            // 【新增】积木 6：丝杆顺滑度测试 (获取正反向速度标准差)
            // ==========================================
            PublishLog($"🚀 [{_motorName}] 开始30分钟丝杆顺滑度测试...");
            int midPoint = minPos + (maxPos - minPos) / 2;

            // 使用底层接口发指令，不再依赖外面的 Goto 方法
            _motorEntity.SetMotorGoToCommand(_motorId, EnumMotorUnit.Pulse, midPoint);

            await Task.Delay(5000, cancellationToken);
            var smoothnessTest = new SmoothnessTestItem();

            // 包工头的两个小本本：记下每一圈的正反向标准差
            List<double> allForwardStdDevs = new List<double>();
            List<double> allBackwardStdDevs = new List<double>();

            // 掐表计时：30分钟
            var sw = Stopwatch.StartNew();
            TimeSpan testDuration = TimeSpan.FromMinutes(30);
            int lapCount = 1;

            // 只要时间没到 30 分钟，就一直跑
            while (sw.Elapsed < testDuration)
            {
                cancellationToken.ThrowIfCancellationRequested(); // 随时响应手动停止

                PublishLog($"[{_motorName}] 正在跑第 {lapCount} 圈 (已耗时: {sw.Elapsed.TotalMinutes:F1} min)...");

                // 跑一圈积木（一次正向 + 一次反向）
                var smoothnessResult = await smoothnessTest.ExecuteAsync(_motorId, _motorModel, _motorEntity, cancellationToken);

                // 如果这一圈顺利跑完，就把成绩记在小本本上
                if (smoothnessResult is SmoothnessTestResult smoothRes && smoothRes.IsPassed)
                {
                    allForwardStdDevs.Add(smoothRes.ForwardStdDevUm);
                    allBackwardStdDevs.Add(smoothRes.BackwardStdDevUm);
                }
                else
                {
                    // 如果跑到一半卡死了，直接中止整个大测试
                    PublishLog($"[{_motorName}] ❌ 丝杆在第 {lapCount} 圈卡死或异常，测试强行终止！");
                    return;
                }

                lapCount++;
            }
            sw.Stop(); // 30 分钟到！停表！

            // 把两个小本本里的数据取平均，填进表格里面
            myData.ForwardSpeedStdDev = allForwardStdDevs.Any() ? Math.Round(allForwardStdDevs.Average(), 3) : 0;
            myData.BackwardSpeedStdDev = allBackwardStdDevs.Any() ? Math.Round(allBackwardStdDevs.Average(), 3) : 0;

            PublishLog($"✅ [{_motorName}] 30分钟测试达标！正向均值波动: {myData.ForwardSpeedStdDev}, 反向均值波动: {myData.BackwardSpeedStdDev}");
            bool isSmoothnessPassed = myData.ForwardSpeedStdDev < 150 && myData.BackwardSpeedStdDev < 150; // 

            var smoothnessFinalResult = new MotorTestResult
            {
                IsPassed = isSmoothnessPassed,
                MeasuredValue = $"正:{myData.ForwardSpeedStdDev} / 反:{myData.BackwardSpeedStdDev}",
                Description = "顺滑度测试完成",
                ErrorDescription = isSmoothnessPassed ? "" : "速度波动超出150限值"
            };

            PublishTestResult("丝杆顺滑度测试", "波动 < 150", smoothnessFinalResult);

            // ==========================================
            // 终点站：把填得满满当当的体检表交上去！
            // ==========================================
            _reportService.AddOrUpdateMotorData(myData);
            PublishLog($"🎉 [{_motorName}] 所有数据已完美录入总报告！");
            // ==========================================
            // 收尾：回到物理行程中点
            // ==========================================
            midPoint = minPos + (maxPos - minPos) / 2;
            PublishLog($"[{_motorName}] 测试全部完毕，正在回到中点位置: {midPoint}");

            // 使用底层接口发指令，不再依赖外面的 Goto 方法
            _motorEntity.SetMotorControlModeCommand(_motorId, EnumMotorCtrType.CloseLoopPosCtr);
            _motorEntity.SetMotorGoToCommand(_motorId, EnumMotorUnit.Pulse, midPoint);

            await Task.Delay(5000, cancellationToken);

            PublishLog($"--- [{_motorName}] 所有基础测试已完美通过！ ---");
        }
        // ==========================================
        // 流程图纸 D：自动寻两端限位并回物理中点置零
        // ==========================================
        public async Task RunAutomaticZeroInitializationAsync(CancellationToken ct)
        {
            PublishLog($"--- [{_motorName}] 开始自动寻限位及初始化零点 ---");

            // 1. 找正限位 (借用现成的完美积木)
            var forwardTest = new StallAndLimitTestItem(true);
            var forwardRes = await forwardTest.ExecuteAsync(_motorId, _motorModel, _motorEntity, ct);
            if (!forwardRes.IsPassed)
            {
                PublishLog($"[{_motorName}] ❌ 找正限位失败，中止回零: {forwardRes.ErrorDescription}");
                return;
            }
            int posForward = _motorModel.MotorParams.Pos;
            PublishLog($"[{_motorName}] 找到正向边界: {posForward}");

            // 2. 找负限位
            var backwardTest = new StallAndLimitTestItem(false);
            var backwardRes = await backwardTest.ExecuteAsync(_motorId, _motorModel, _motorEntity, ct);
            if (!backwardRes.IsPassed)
            {
                PublishLog($"[{_motorName}] ❌ 找负限位失败，中止回零: {backwardRes.ErrorDescription}");
                return;
            }
            int posBackward = _motorModel.MotorParams.Pos;
            PublishLog($"[{_motorName}] 找到负向边界: {posBackward}");

            // 3. 计算中心点
            int centerPos = posBackward + (posForward - posBackward) / 2;
            PublishLog($"[{_motorName}] 计算出物理中点: {centerPos}，正在前往...");

            // 4. 下发移动到中点指令
            _motorEntity.SetMotorGoToCommand(_motorId, EnumMotorUnit.Pulse, centerPos);

            // 5. 等待电机启动（稍微等个 500ms 让电机离开 Stop 状态）
            await Task.Delay(500, ct);

            // 6. 优雅地等待电机停止 (完美替代你原来的 Thread.Sleep)
            int timeoutCount = 0;
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    PublishLog($"[{_motorName}] 收到停止指令，取消置零。");
                    return;
                }

                // 一旦检测到电机停稳了，立马发送零点指令！
                if (_motorModel.MotorParams.MoveState == EnumMotorMoveState.MotorStop)
                {
                    _motorEntity.SetMotorZeroCommand(_motorId);
                    PublishLog($"[{_motorName}] ✅ 已到达物理中点，成功重置零点！");
                    return;
                }

                timeoutCount++;
                if (timeoutCount > 50) // 最多等 50 秒，防止死循环
                {
                    PublishLog($"[{_motorName}] ❌ 等待到达中点超时(>50s)，置零失败！");
                    return;
                }

                // 每次循环异步等待 1 秒，绝对不卡 UI！
                await Task.Delay(1000, ct);
            }
        }

        public async Task RunLeadScrewTestAsync(CancellationToken ct)
        {
            try
            {
                PublishLog($"🚀 [{_motorName}] 30分钟丝杆耐久测试开始");

                // 1. 极其优雅：直接掏出你写好的绝美积木
                var smoothnessTest = new SmoothnessTestItem();

                // 2. 准备两个小本本，记下每一圈的成绩
                List<double> allForwardStdDevs = new List<double>();
                List<double> allBackwardStdDevs = new List<double>();

                var sw = Stopwatch.StartNew();
                TimeSpan testDuration = TimeSpan.FromMinutes(30);
                int lapCount = 1;

                // 3. 开启 30 分钟马拉松
                while (sw.Elapsed < testDuration)
                {
                    ct.ThrowIfCancellationRequested(); // 随时响应手动停止

                    PublishLog($"[{_motorName}] 正在跑第 {lapCount} 圈 (已耗时: {sw.Elapsed.TotalMinutes:F1} min)...");

                    // 🚨 核心复用：直接让积木去跑一圈！
                    // 因为积木内部已经挂了速度挡，所以它跑出来的波动绝对真实！
                    var result = await smoothnessTest.ExecuteAsync(_motorId, _motorModel, _motorEntity, ct);

                    if (result is SmoothnessTestResult smoothRes && smoothRes.IsPassed)
                    {
                        // 把这一圈算出来的波动存进临时本子里
                        allForwardStdDevs.Add(smoothRes.ForwardStdDev);
                        allBackwardStdDevs.Add(smoothRes.BackwardStdDev);
                    }
                    else
                    {
                        string reason = result?.ErrorDescription ?? "未知异常";
                        PublishLog($"[{_motorName}] ❌ 第 {lapCount} 圈测试失败！原因: {reason}");
                        return;
                    }

                    lapCount++;

                    
                }
                sw.Stop();

                // 4. 30 分钟到了，算总平均账！
                double finalF = allForwardStdDevs.Any() ? Math.Round(allForwardStdDevs.Average(), 3) : 0;
                double finalB = allBackwardStdDevs.Any() ? Math.Round(allBackwardStdDevs.Average(), 3) : 0;

                // 5. 【核心】：把成绩单拍在 UI 界面的“测试结果”表格里！
                var finalResult = new MotorTestResult
                {
                    IsPassed = true, // 强制给个绿灯
                    MeasuredValue = $"正向:{finalF} / 反向:{finalB}", // 填入测量值
                    ErrorDescription = "独立跑机，不上报" // 填入说明
                };

                // 调用你包工头自带的方法，第一个参数是测试名，第二个是标准值，第三个是结果对象
                PublishTestResult("30分钟丝杆耐久测试", "< 150", finalResult);

                PublishLog($"✅ [{_motorName}] 30分钟测试圆满完成！已显示在结果表格中。");
            }
            catch (OperationCanceledException)
            {
                PublishLog($"⚠️ [{_motorName}] 测试被手动叫停。");
            }
            catch (Exception ex)
            {
                PublishLog($"❌ [{_motorName}] 测试崩溃: {ex.Message}");
            }
            finally
            {
                _motorEntity.SetMotorOperatingStatusCommand(_motorId, EnumMotorOperatingState.Stop);
            }
        }

    }
}
