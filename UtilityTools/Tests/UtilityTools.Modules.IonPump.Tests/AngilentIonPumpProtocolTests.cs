using System.Collections;
using System.Windows.Markup;
using UtilityTools.Core.Helper;
using UtilityTools.Modules.IonPump.Protocol;

namespace UtilityTools.Modules.IonPump.Tests
{
    public class AngilentIonPumpProtocolTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestReqPack()
        {
            byte[] bytes = AgilentIonPumpProtocol.QueryPressureCmd();
            Assert.That(StrHelper.HexStr(bytes), Is.EqualTo("02 80 38 31 32 30 30 03 42 38"));
        }


        [Test]
        public void TestFindHeader()
        {
            byte[] bytes = AgilentIonPumpProtocol.QueryPressureCmd();
            var pos = Helper.FindHeaderPos(bytes, 0, bytes.Length);
            Assert.That(pos, Is.EqualTo(0));

            byte[] case2 = { Helper.STX, Helper.ADDR, 0x00, 0x01, Helper.STX, Helper.ADDR };
            Assert.That(Helper.FindHeaderPos(case2, 1, case2.Length - 1), Is.EqualTo(4));
        }


        [Test]
        public void TestCrc()
        {
            byte[] bytes = AgilentIonPumpProtocol.QueryPressureCmd();
            Assert.True(Helper.IsCrcValid(bytes, 0, bytes.Length));
        }

        [Test]
        public void TestWriteRespParser()
        {
            byte[] data = { 0x02, 0x80, 0x06, 0x03, 0x38, 0x35 };
            var parser = new RespParser();
            List<Resp> respList = new();
            parser.RespReceivedEvent += (object? sender, Resp resp) => { respList.Add(resp); };

            parser.ReceiveBytes(data);

            Assert.True(respList.Count == 1);
            var resp = respList[0];
            Assert.True(resp.CmdType == EnumCmdType.WRITE);
            Assert.True(resp.GetWriteRespType() == EnumWriteRespType.SUCCESS);

            parser.ReceiveBytes(data);
            Assert.True(respList.Count == 2);
            resp = respList[1];
            Assert.True(resp.CmdType == EnumCmdType.WRITE);
            Assert.True(resp.GetWriteRespType() == EnumWriteRespType.SUCCESS);
        }

        [Test]
        public void TestReadRespParser()
        {
            byte[] bytes = AgilentIonPumpProtocol.QueryPressureCmd();
            int length = Helper.SearchReadRespPacket(bytes, 0, bytes.Length);
            Assert.True(length == bytes.Length);

            var parser = new RespParser();
            List<Resp> respList = new();
            parser.RespReceivedEvent += (object? sender, Resp resp) => { respList.Add(resp); };

            parser.ReceiveBytes(bytes);

            Assert.True(respList.Count == 1);
            var resp = respList[0];
            Assert.True(resp.CmdType == EnumCmdType.READ);

            var packet = resp.GetReadResp();
            Assert.That(packet.GetDataStr(), Is.EqualTo("0"));
            Assert.That(packet.GetWinType(), Is.EqualTo(EnumWinType.R_PRESSURE));
        }
    }
}