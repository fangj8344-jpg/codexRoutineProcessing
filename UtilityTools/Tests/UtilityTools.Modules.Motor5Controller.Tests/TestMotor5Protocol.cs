
using System.Text;
using UtilityTools.Core.Protocol;
using UtilityTools.Modules.Motor5Controller.Protocol;

namespace UtilityTools.Modules.Motor5Controller.Tests
{
    public class Tests
    {
        public static string HexStr(byte[] bytes, string sep = " ")
        {
            return string.Join(sep, bytes.Select(x => x.ToString("X2")));
        }


        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestProtocol()
        {
            byte[] bytes = Motor5Protocol.SetMotorOnCmd(EnumMotorId.MOTOR_5);
            Assert.That(bytes.Length, Is.EqualTo(64));
            Console.WriteLine(HexStr(bytes));
            Console.WriteLine(Encoding.ASCII.GetString(bytes));
        }


        [Test]
        public void TestParser()
        {
            byte[] bytes = Motor5Protocol.SetMotorOnCmd(EnumMotorId.MOTOR_5);

            var parser = new Motor5ProtocolParser();
            List<Motor5DataPacket> respList = new();

            parser.PacketReceivedEvent += (object? sender, Motor5DataPacket packet) => { respList.Add(packet); };
            parser.ReceiveBytes(new byte[256]);
            parser.ReceiveBytes(new byte[] { 0x00, 0x11, 0x33 }); // 制造一些垃圾数据
            parser.ReceiveBytes(bytes);
            // 异步通知的需要sleep才能拿到结果
            Thread.Sleep(100);

            Assert.True(respList.Count == 1);
            var resp = respList[0];

            Assert.True(resp.CmdType == EnumMotor5CmdType.W_MOTOR_ON);
            Assert.True(resp.MotorId == EnumMotorId.MOTOR_5);

        }
    }
}